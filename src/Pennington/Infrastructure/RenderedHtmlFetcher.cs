namespace Pennington.Infrastructure;

using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

/// <summary>
/// Fetches fully rendered page HTML from the running web host and exposes a
/// section of it (via CSS selector) as an AngleSharp <see cref="IElement"/>.
/// <para>
/// Both <c>LlmsTxtService</c> and <c>SearchIndexService</c> use this to get
/// post-pipeline HTML — i.e., after Markdig extensions, Razor SSR, xref
/// resolution, locale rewriting, and any other middleware have run. The
/// pre-pipeline <c>IContentRenderer</c> path misses Razor pages entirely and
/// misses request-pipeline transforms for everything else.
/// </para>
/// <para>
/// The base URL is resolved lazily per call by the <see cref="IHttpClientFactory"/>
/// named-client config registered in <c>AddPennington</c>, so this works
/// identically in dev-serve mode and in static-build mode (both run Kestrel).
/// http:// addresses are preferred over https:// to avoid dev-cert trust issues.
/// </para>
/// </summary>
public sealed class RenderedHtmlFetcher
{
    /// <summary>Named-client key used to resolve the configured <see cref="HttpClient"/>.</summary>
    public const string HttpClientName = "Pennington.RenderedHtml";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RenderedHtmlFetcher> _logger;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

    /// <summary>Initializes the fetcher with the HTTP client factory and a logger.</summary>
    public RenderedHtmlFetcher(IHttpClientFactory httpClientFactory, ILogger<RenderedHtmlFetcher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Fetches <paramref name="path"/> from the running host, parses the response
    /// body as HTML, and returns the element matching <paramref name="selector"/>.
    /// When <paramref name="selector"/> is null or no match is found, returns
    /// <see cref="IDocument.Body"/>. Returns null on non-success responses.
    /// </summary>
    public async Task<IElement?> FetchContentAsync(string path, string? selector, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

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
        var document = await _browsingContext.OpenAsync(req => req.Content(html), ct);

        if (!string.IsNullOrEmpty(selector))
        {
            var match = document.QuerySelector(selector);
            if (match is not null) return match;
            _logger.LogWarning(
                "RenderedHtmlFetcher: selector '{Selector}' did not match any element in {Path}; falling back to <body>",
                selector, path);
        }

        return document.Body;
    }
}
