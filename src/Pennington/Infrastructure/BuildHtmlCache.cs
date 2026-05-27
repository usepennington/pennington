namespace Pennington.Infrastructure;

using System.Collections.Concurrent;
using System.Net;
using Content;

/// <summary>
/// Process-lifetime cache of fully rendered in-process HTTP responses, keyed by request path.
/// <para>
/// The static build crawls itself: the disk-write pass, the search index, and the llms.txt
/// sidecar each self-fetch the same pages, so without sharing every page renders 2–3× through
/// the full middleware pipeline. Installed behind <see cref="HttpDispatcher"/> via
/// <see cref="CachingHttpHandler"/>, this collapses that to one render per URL — every consumer
/// replays the first render.
/// </para>
/// <para>
/// Eviction rides the existing <see cref="FileWatchDispatcher"/>: as an <see cref="IFileWatchAware"/>
/// with no scopes of its own, it is notified of every change another watcher already observes.
/// On notification, it consults <see cref="IContentService.GetAffectedRoutes"/> on every
/// registered content service and evicts only the affected keys — wholesale only on a
/// <see cref="ContentChangeImpactCases.Wildcard"/> report.
/// </para>
/// </summary>
public sealed class BuildHtmlCache : IFileWatchAware
{
    private readonly ConcurrentDictionary<string, Lazy<Task<CachedResponse>>> _entries = new(StringComparer.Ordinal);
    private readonly IEnumerable<IContentService> _contentServices;

    /// <summary>Initializes the cache with the content services it consults for affected routes on file change.</summary>
    public BuildHtmlCache(IEnumerable<IContentService> contentServices)
    {
        _contentServices = contentServices;
    }

    /// <summary>
    /// Returns the cached response for <paramref name="key"/>, invoking <paramref name="factory"/>
    /// exactly once on the first request for that key. Concurrent first-requests coalesce onto the
    /// same render; a faulted render is evicted so a later request can retry.
    /// </summary>
    public async Task<CachedResponse> GetOrAddAsync(string key, Func<Task<CachedResponse>> factory)
    {
        var lazy = _entries.GetOrAdd(key, _ => new Lazy<Task<CachedResponse>>(factory));
        try
        {
            return await lazy.Value;
        }
        catch
        {
            // Don't poison the cache with a transient failure — let the next caller re-render.
            _entries.TryRemove(new KeyValuePair<string, Lazy<Task<CachedResponse>>>(key, lazy));
            throw;
        }
    }

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change)
    {
        var wildcard = false;
        List<string>? affectedPaths = null;

        foreach (var service in _contentServices)
        {
            var impact = service.GetAffectedRoutes(change);
            switch (impact.Value)
            {
                case ContentChangeImpactCases.Wildcard:
                    wildcard = true;
                    break;
                case ContentChangeImpactCases.Routes routes:
                    foreach (var route in routes.Affected)
                    {
                        (affectedPaths ??= []).Add(route.CanonicalPath.Value);
                    }
                    break;
            }
            if (wildcard) break;
        }

        if (wildcard)
        {
            _entries.Clear();
            return FileWatchResponse.Refreshed;
        }

        if (affectedPaths is null || affectedPaths.Count == 0)
        {
            return FileWatchResponse.Refreshed;
        }

        // Cache keys are full PathAndQuery (e.g. "/foo/" or "/foo/?x=1"). Evict any key whose
        // path portion matches an affected route's canonical path — covers query-string variants.
        foreach (var key in _entries.Keys)
        {
            var pathPart = ExtractPath(key);
            foreach (var affected in affectedPaths)
            {
                if (string.Equals(pathPart, affected, StringComparison.OrdinalIgnoreCase))
                {
                    _entries.TryRemove(key, out _);
                    break;
                }
            }
        }

        return FileWatchResponse.Refreshed;
    }

    private static string ExtractPath(string pathAndQuery)
    {
        var queryIndex = pathAndQuery.IndexOf('?');
        return queryIndex < 0 ? pathAndQuery : pathAndQuery[..queryIndex];
    }
}

/// <summary>
/// A captured HTTP response — status, body, and headers — replayable as a fresh
/// <see cref="HttpResponseMessage"/> any number of times. Headers are preserved verbatim so
/// per-request signals the build relies on (the <c>X-Pennington-Diagnostic</c> headers,
/// <c>Location</c> on redirects, <c>Content-Type</c>) survive replay.
/// </summary>
/// <param name="Status">The response status code.</param>
/// <param name="Body">The response body bytes.</param>
/// <param name="ResponseHeaders">Captured response-level headers.</param>
/// <param name="ContentHeaders">Captured content-level headers.</param>
public sealed record CachedResponse(
    HttpStatusCode Status,
    byte[] Body,
    IReadOnlyList<KeyValuePair<string, string[]>> ResponseHeaders,
    IReadOnlyList<KeyValuePair<string, string[]>> ContentHeaders)
{
    /// <summary>Reads <paramref name="response"/> fully into a replayable <see cref="CachedResponse"/>.</summary>
    public static async Task<CachedResponse> CaptureAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsByteArrayAsync(ct);
        var responseHeaders = response.Headers
            .Select(h => new KeyValuePair<string, string[]>(h.Key, h.Value.ToArray()))
            .ToArray();
        var contentHeaders = response.Content.Headers
            .Select(h => new KeyValuePair<string, string[]>(h.Key, h.Value.ToArray()))
            .ToArray();
        return new CachedResponse(response.StatusCode, body, responseHeaders, contentHeaders);
    }

    /// <summary>Rebuilds a fresh <see cref="HttpResponseMessage"/> from the captured state.</summary>
    public HttpResponseMessage ToHttpResponseMessage()
    {
        var message = new HttpResponseMessage(Status)
        {
            Content = new ByteArrayContent(Body),
        };

        // Content headers must be set before adding response headers so Content-Type/Length land
        // on the right collection; TryAddWithoutValidation skips re-parsing already-valid values.
        foreach (var (name, values) in ContentHeaders)
        {
            message.Content.Headers.TryAddWithoutValidation(name, values);
        }

        foreach (var (name, values) in ResponseHeaders)
        {
            message.Headers.TryAddWithoutValidation(name, values);
        }

        return message;
    }
}
