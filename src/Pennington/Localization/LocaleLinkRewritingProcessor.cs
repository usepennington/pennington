namespace Pennington.Localization;

using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;
using Pennington.Infrastructure;

/// <summary>
/// Response processor that rewrites internal page links to include the current
/// locale prefix. When the user is browsing in a non-default locale, links like
/// <c>/about</c> become <c>/fr/about</c> automatically, so components don't need
/// to use explicit locale-aware URL helpers.
/// <para>
/// Skips links that are external, already locale-prefixed, or point to static
/// assets (file extensions, <c>/_content/</c>, etc.).
/// </para>
/// </summary>
public sealed class LocaleLinkRewritingProcessor : IResponseProcessor
{
    private readonly LocalizationOptions _localization;
    private readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

    public LocaleLinkRewritingProcessor(LocalizationOptions localization)
    {
        _localization = localization;
    }

    /// <summary>
    /// Runs after xref resolution (10) but BEFORE base-URL rewriting (30) so that
    /// locale detection and prefixing operate on logical root-relative paths
    /// (<c>/about/</c>), not on paths already prefixed with the deployment base
    /// URL (<c>/preview/about/</c>). BaseUrlRewritingProcessor is the outermost
    /// transport layer and must apply last among the URL rewriters.
    /// </summary>
    public int Order => 20;

    public bool ShouldProcess(HttpContext context)
    {
        if (!_localization.IsMultiLocale) return false;

        var contentType = context.Response.ContentType ?? "";
        if (!contentType.Contains("text/html")) return false;
        if (context.Response.StatusCode is < 200 or >= 300) return false;

        // Only rewrite for non-default locales
        var locale = context.Items["Pennington.Locale"] as string;
        return locale is not null
            && !string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ProcessAsync(string responseBody, HttpContext context)
    {
        var locale = context.Items["Pennington.Locale"] as string ?? _localization.DefaultLocale;
        var document = await _browsingContext.OpenAsync(req => req.Content(responseBody));
        var baseUri = GetBaseUri(context);

        foreach (var anchor in document.QuerySelectorAll("a[href]"))
        {
            RewriteAnchorHref(anchor, locale, baseUri);
        }

        return document.ToHtml();
    }

    private void RewriteAnchorHref(IElement anchor, string locale, string baseUri)
    {
        var href = anchor.GetAttribute("href");
        if (string.IsNullOrEmpty(href)) return;

        // Skip language switcher links — they intentionally point to specific locales
        if (anchor.HasAttribute("data-locale")) return;

        // Extract path from the href (handle both absolute URLs and root-relative paths)
        string? path;
        string prefix = "";

        if (href.StartsWith("//") || href.StartsWith("mailto:") || href.StartsWith("tel:") || href.StartsWith("#"))
            return;

        if (href.StartsWith("http://") || href.StartsWith("https://"))
        {
            // Absolute URL — check if it's for our site
            if (!string.IsNullOrEmpty(baseUri) && href.StartsWith(baseUri, StringComparison.OrdinalIgnoreCase))
            {
                prefix = baseUri;
                path = href[baseUri.Length..];
                if (!path.StartsWith('/')) path = "/" + path;
            }
            else
            {
                return; // External link
            }
        }
        else if (href.StartsWith('/'))
        {
            path = href;
        }
        else
        {
            return; // Relative path — leave as-is
        }

        if (!ShouldRewritePath(path, locale)) return;

        var newPath = $"/{locale}{path}";
        anchor.SetAttribute("href", prefix + newPath);
    }

    private bool ShouldRewritePath(string path, string locale)
    {
        // Already has this locale prefix
        if (path.StartsWith($"/{locale}/", StringComparison.OrdinalIgnoreCase)
            || path.Equals($"/{locale}", StringComparison.OrdinalIgnoreCase))
            return false;

        // Already has ANY locale prefix (e.g., link to a different locale from language switcher)
        foreach (var loc in _localization.Locales.Keys)
        {
            if (path.StartsWith($"/{loc}/", StringComparison.OrdinalIgnoreCase)
                || path.Equals($"/{loc}", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Framework / internal paths
        if (path.StartsWith("/_", StringComparison.Ordinal))
            return false;

        // Has a file extension — likely a static asset
        var lastSegment = path.AsSpan();
        var lastSlash = lastSegment.LastIndexOf('/');
        if (lastSlash >= 0) lastSegment = lastSegment[(lastSlash + 1)..];
        if (lastSegment.Contains('.'))
            return false;

        return true;
    }

    private static string GetBaseUri(HttpContext context)
    {
        var request = context.Request;
        return $"{request.Scheme}://{request.Host}";
    }
}
