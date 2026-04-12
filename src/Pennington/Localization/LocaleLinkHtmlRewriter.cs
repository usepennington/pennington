namespace Pennington.Localization;

using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;
using Pennington.Infrastructure;

/// <summary>
/// Rewrites internal page links to include the current locale prefix.
/// When the user is browsing in a non-default locale, links like
/// <c>/about</c> become <c>/fr/about</c> automatically, so components
/// don't need locale-aware URL helpers.
/// <para>
/// Skips external links, links already carrying a locale prefix, and
/// paths that look like static assets (file extensions, <c>/_content/</c>, etc.).
/// </para>
/// <para>
/// Runs after xref resolution (<see cref="Order"/> 10) but before
/// base-URL rewriting (<see cref="Order"/> 30), so locale detection and
/// prefixing operate on logical root-relative paths (<c>/about/</c>),
/// not paths already prefixed with the deployment base URL
/// (<c>/preview/about/</c>). BaseUrl is the outermost transport layer
/// and must apply last among URL rewriters.
/// </para>
/// </summary>
public sealed class LocaleLinkHtmlRewriter : IHtmlResponseRewriter
{
    private readonly LocalizationOptions _localization;

    public LocaleLinkHtmlRewriter(LocalizationOptions localization)
    {
        _localization = localization;
    }

    public int Order => 20;

    public bool ShouldApply(HttpContext context)
    {
        if (!_localization.IsMultiLocale) return false;

        // Only rewrite when we're serving a non-default locale.
        var locale = context.Items["Pennington.Locale"] as string;
        return locale is not null
            && !string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase);
    }

    public Task ApplyAsync(IDocument document, HttpContext context)
    {
        var locale = context.Items["Pennington.Locale"] as string ?? _localization.DefaultLocale;
        var baseUri = GetBaseUri(context);

        foreach (var anchor in document.QuerySelectorAll("a[href]"))
        {
            RewriteAnchorHref(anchor, locale, baseUri);
        }

        return Task.CompletedTask;
    }

    private void RewriteAnchorHref(IElement anchor, string locale, string baseUri)
    {
        var href = anchor.GetAttribute("href");
        if (string.IsNullOrEmpty(href)) return;

        // Skip language-switcher links — they intentionally point at specific locales.
        if (anchor.HasAttribute("data-locale")) return;

        string? path;
        string prefix = "";

        if (href.StartsWith("//") || href.StartsWith("mailto:") || href.StartsWith("tel:") || href.StartsWith("#"))
            return;

        if (href.StartsWith("http://") || href.StartsWith("https://"))
        {
            // Absolute URL — only rewrite if it points at our own site.
            if (!string.IsNullOrEmpty(baseUri) && href.StartsWith(baseUri, StringComparison.OrdinalIgnoreCase))
            {
                prefix = baseUri;
                path = href[baseUri.Length..];
                if (!path.StartsWith('/')) path = "/" + path;
            }
            else
            {
                return;
            }
        }
        else if (href.StartsWith('/'))
        {
            path = href;
        }
        else
        {
            // Relative path — leave as-is. MarkdownLinkResolver handles
            // source-file-relative resolution before we ever see the HTML.
            return;
        }

        if (!ShouldRewritePath(path, locale)) return;

        var newPath = $"/{locale}{path}";
        anchor.SetAttribute("href", prefix + newPath);
    }

    private bool ShouldRewritePath(string path, string locale)
    {
        // Already has this locale prefix.
        if (path.StartsWith($"/{locale}/", StringComparison.OrdinalIgnoreCase)
            || path.Equals($"/{locale}", StringComparison.OrdinalIgnoreCase))
            return false;

        // Already has ANY locale prefix (e.g., a language-switcher link that
        // slipped through without data-locale).
        foreach (var loc in _localization.Locales.Keys)
        {
            if (path.StartsWith($"/{loc}/", StringComparison.OrdinalIgnoreCase)
                || path.Equals($"/{loc}", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Framework / internal paths.
        if (path.StartsWith("/_", StringComparison.Ordinal))
            return false;

        // Has a file extension — likely a static asset.
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
