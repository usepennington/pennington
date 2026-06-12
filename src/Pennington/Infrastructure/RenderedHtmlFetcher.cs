namespace Pennington.Infrastructure;

using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

/// <summary>
/// Fetches fully rendered page HTML from the running app and exposes a section
/// of it (via CSS selector) as an AngleSharp <see cref="IElement"/>.
/// <para>
/// <see cref="Pipeline.SiteProjection"/> (its sole consumer) uses this to get
/// post-pipeline HTML — i.e., after Markdig extensions, Razor SSR, xref
/// resolution, locale rewriting, and any other middleware have run. The
/// pre-pipeline <c>IContentRenderer</c> path misses Razor pages entirely and
/// misses request-pipeline transforms for everything else.
/// </para>
/// <para>
/// Requests are dispatched via <see cref="IInProcessHttpDispatcher"/>, which
/// delivers them in-memory through <c>TestServer</c> (build mode + integration
/// tests) or over Kestrel's listening socket (dev mode). The middleware pipeline
/// runs identically in either case. Every fetch carries
/// <see cref="CorpusFetchScope.HeaderName"/> so the served request can fail fast
/// (instead of deadlocking) if its render path awaits the projection — a future
/// consumer of this fetcher outside the projection inherits that header and its
/// tripwire semantics.
/// </para>
/// </summary>
public sealed class RenderedHtmlFetcher
{
    private readonly IInProcessHttpDispatcher _dispatcher;
    private readonly ILogger<RenderedHtmlFetcher> _logger;

    /// <summary>Initializes the fetcher with the in-process dispatcher and a logger.</summary>
    public RenderedHtmlFetcher(IInProcessHttpDispatcher dispatcher, ILogger<RenderedHtmlFetcher> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Fetches <paramref name="path"/> from the running app, parses the response
    /// body as HTML, and returns the element matching <paramref name="selector"/>.
    /// When <paramref name="selector"/> is null or no match is found, returns
    /// <see cref="IDocument.Body"/>. Returns null on non-success responses.
    /// </summary>
    public async Task<IElement?> FetchContentAsync(string path, string? selector, CancellationToken ct = default)
    {
        using var client = _dispatcher.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(CorpusFetchScope.HeaderName, "1");

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(path, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RenderedHtmlFetcher failed to GET {Path}", path);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug("RenderedHtmlFetcher: {Path} returned {StatusCode}", path, (int)response.StatusCode);
            return null;
        }

        var html = await response.Content.ReadAsStringAsync(ct);

        // A browsing context per call: AngleSharp's IBrowsingContext is not safe for
        // concurrent OpenAsync, and SearchArtifactService now fetches in parallel.
        var browsingContext = BrowsingContext.New(Configuration.Default);
        var document = await browsingContext.OpenAsync(req => req.Content(html), ct);

        if (!string.IsNullOrEmpty(selector))
        {
            var match = document.QuerySelector(selector);
            if (match is not null)
            {
                return match;
            }

            _logger.LogWarning(
                "RenderedHtmlFetcher: selector '{Selector}' did not match any element in {Path}; falling back to <body>",
                selector, path);
        }

        return document.Body;
    }
}