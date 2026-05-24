namespace Pennington.Infrastructure;

using Generation;

/// <summary>
/// Caches GET responses from the in-process crawl in <see cref="BuildHtmlCache"/> so the
/// disk-write pass and the search/llms.txt sidecars share one render per URL. Installed by
/// <see cref="HttpDispatcher"/> as the outer handler over the TestServer/Kestrel client.
/// Non-GET requests and the 404 sentinel pass straight through uncached.
/// </summary>
public sealed class CachingHttpHandler : DelegatingHandler
{
    private readonly BuildHtmlCache _cache;

    /// <summary>Initializes the handler over the shared <paramref name="cache"/>.</summary>
    public CachingHttpHandler(BuildHtmlCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get ||
            request.RequestUri is null ||
            request.RequestUri.AbsolutePath == OutputGenerationService.NotFoundGeneratorPath)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var cached = await _cache.GetOrAddAsync(
            request.RequestUri.PathAndQuery,
            async () =>
            {
                using var response = await base.SendAsync(request, cancellationToken);
                return await CachedResponse.CaptureAsync(response, cancellationToken);
            });

        return cached.ToHttpResponseMessage();
    }
}
