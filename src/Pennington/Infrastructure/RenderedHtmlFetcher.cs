namespace Pennington.Infrastructure;

using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
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
/// The base URL is taken from <see cref="IServerAddressesFeature"/>, so this
/// works identically in dev-serve mode and in static-build mode (both run
/// Kestrel). http:// addresses are preferred over https:// to avoid dev-cert
/// trust issues.
/// </para>
/// </summary>
public sealed class RenderedHtmlFetcher : IDisposable
{
    private readonly IServer _server;
    private readonly ILogger<RenderedHtmlFetcher> _logger;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);
    private readonly Lazy<HttpClient> _client;

    public RenderedHtmlFetcher(IServer server, ILogger<RenderedHtmlFetcher> logger)
    {
        _server = server;
        _logger = logger;
        _client = new Lazy<HttpClient>(CreateClient);
    }

    private HttpClient CreateClient()
    {
        var addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses;
        if (addresses is null || addresses.Count == 0)
        {
            throw new InvalidOperationException(
                "RenderedHtmlFetcher requires the web host to be listening. " +
                "IServerAddressesFeature has no addresses — is the app started yet?");
        }

        // Prefer http:// to avoid dev-cert trust issues.
        var baseAddress = addresses.FirstOrDefault(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            ?? addresses.First();

        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        return new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
    }

    /// <summary>
    /// Fetches <paramref name="path"/> from the running host, parses the response
    /// body as HTML, and returns the element matching <paramref name="selector"/>.
    /// When <paramref name="selector"/> is null or no match is found, returns
    /// <see cref="IDocument.Body"/>. Returns null on non-success responses.
    /// </summary>
    public async Task<IElement?> FetchContentAsync(string path, string? selector, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.Value.GetAsync(path, ct);
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

    public void Dispose()
    {
        if (_client.IsValueCreated)
        {
            _client.Value.Dispose();
        }
    }
}