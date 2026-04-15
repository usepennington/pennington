namespace Pennington.Infrastructure;

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Generation;
using Routing;

/// <summary>
/// Verifies internal links in rendered HTML against known routes.
/// Extracts href and src attributes and classifies each as valid, broken, or external.
/// Does not make HTTP requests — this is purely static analysis.
/// </summary>
public sealed partial class LinkVerificationService
{
    private readonly HashSet<string> _knownPaths;
    private readonly string _basePrefix;

    /// <summary>
    /// Create with the set of all known page canonical paths, the static asset
    /// paths the engine copied into the output tree, and the base URL the
    /// surrounding site was rendered with. The base URL is used to strip a common
    /// prefix (e.g. <c>/preview</c>) from extracted hrefs before comparing against
    /// the unprefixed canonical <see cref="ContentRoute.CanonicalPath"/>, and before
    /// applying the framework-asset prefix check for <c>/_content/</c> / <c>/_framework/</c>
    /// / <c>/_blazor/</c>. Passing <c>"/"</c> (the default) disables prefix stripping.
    ///
    /// <paramref name="copiedAssetPaths"/> are relative output paths produced by
    /// <see cref="IContentService.GetContentToCopyAsync"/> (e.g. <c>media/sample.svg</c>);
    /// they get normalized into absolute root-relative URLs (<c>/media/sample.svg</c>)
    /// and added to the known-paths set so that <c>&lt;img src&gt;</c> references to
    /// assets the engine just copied aren't flagged as broken.
    /// </summary>
    public LinkVerificationService(
        IEnumerable<ContentRoute> knownRoutes,
        IEnumerable<string>? copiedAssetPaths = null,
        string baseUrl = "/")
    {
        _knownPaths = new HashSet<string>(
            knownRoutes.Select(r => NormalizePath(r.CanonicalPath.Value)),
            StringComparer.OrdinalIgnoreCase);
        if (copiedAssetPaths != null)
        {
            foreach (var asset in copiedAssetPaths)
            {
                if (string.IsNullOrWhiteSpace(asset)) continue;
                var normalized = asset.Replace('\\', '/');
                if (!normalized.StartsWith('/')) normalized = "/" + normalized;
                _knownPaths.Add(NormalizePath(normalized));
            }
        }
        _basePrefix = (baseUrl ?? "/").TrimEnd('/');
    }

    /// <summary>
    /// Verify all links found in a page's rendered HTML.
    /// </summary>
    public ImmutableList<LinkCheckResult> VerifyLinks(ContentRoute sourcePage, string html)
    {
        var links = ExtractLinks(html);
        var results = ImmutableList.CreateBuilder<LinkCheckResult>();

        foreach (var (url, linkType) in links)
        {
            var result = ClassifyLink(sourcePage, url, linkType);
            results.Add(result);
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// Find internal page links in HTML that are missing a trailing slash.
    /// Returns the list of offending URLs.
    /// </summary>
    public static ImmutableList<string> FindLinksWithoutTrailingSlash(string html)
    {
        var links = ExtractLinks(html);
        var results = ImmutableList.CreateBuilder<string>();

        foreach (var (url, _) in links)
        {
            if (IsExternalUrl(url) || url.StartsWith('#'))
                continue;

            var pathOnly = url.Split('#')[0].Split('?')[0];
            if (string.IsNullOrEmpty(pathOnly) || pathOnly == "/")
                continue;

            // Skip URLs that look like files (have an extension in the last segment)
            var lastSegment = pathOnly.Split('/')[^1];
            if (lastSegment.Contains('.'))
                continue;

            if (!pathOnly.EndsWith('/'))
                results.Add(url);
        }

        return results.ToImmutable();
    }

    private LinkCheckResult ClassifyLink(ContentRoute sourcePage, string url, LinkType linkType)
    {
        // External links (http://, https://, //, mailto:, tel:)
        if (IsExternalUrl(url))
            return new ExternalLink(sourcePage, url);

        // Anchor-only links (#something)
        if (url.StartsWith('#'))
            return new ValidLink(sourcePage, url);

        // Strip query string and fragment
        var pathOnly = url.Split('#')[0].Split('?')[0];

        // Strip the site's base URL prefix (if any) so downstream checks can compare
        // against canonical, unprefixed paths. In pass B (base `/preview/`) this turns
        // `/preview/_content/Pennington.UI/scripts.js` into `/_content/Pennington.UI/scripts.js`
        // so the framework-asset bypass below catches it, and turns `/preview/about/`
        // into `/about/` so the known-paths lookup succeeds.
        if (_basePrefix.Length > 0 && pathOnly.StartsWith(_basePrefix, StringComparison.OrdinalIgnoreCase))
        {
            pathOnly = pathOnly[_basePrefix.Length..];
            if (pathOnly.Length == 0) pathOnly = "/";
        }

        // Framework-managed static asset paths — not content routes, skip verification
        if (pathOnly.StartsWith("/_content/", StringComparison.OrdinalIgnoreCase) ||
            pathOnly.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase) ||
            pathOnly.StartsWith("/_blazor/", StringComparison.OrdinalIgnoreCase))
            return new ValidLink(sourcePage, url);

        // Internal links — check against known routes
        var normalizedUrl = NormalizePath(pathOnly);

        if (_knownPaths.Contains(normalizedUrl))
            return new ValidLink(sourcePage, url);

        return new BrokenLinkResult(sourcePage, url, linkType, "Page not found");
    }

    /// <summary>
    /// Extract all href and src URLs from HTML. Strips <c>&lt;code&gt;</c> and
    /// <c>&lt;pre&gt;</c> content first so href/src patterns that appear as
    /// text inside syntax-highlighted code samples are not mistaken for real
    /// links — the highlighter splits long string literals across span
    /// boundaries, and the attribute regex would otherwise match across them.
    /// </summary>
    internal static List<(string Url, LinkType Type)> ExtractLinks(string html)
    {
        var stripped = StripCodeAndPre(html);
        var results = new List<(string, LinkType)>();

        foreach (var match in HrefRegex().EnumerateMatches(stripped))
        {
            var url = ExtractAttributeValue(stripped, match);
            if (!string.IsNullOrWhiteSpace(url))
                results.Add((url, LinkType.Internal));
        }

        foreach (var match in SrcRegex().EnumerateMatches(stripped))
        {
            var url = ExtractAttributeValue(stripped, match);
            if (!string.IsNullOrWhiteSpace(url))
                results.Add((url, LinkType.Image));
        }

        return results;
    }

    private static string StripCodeAndPre(string html)
        => CodeAndPreRegex().Replace(html, "");

    [GeneratedRegex("""<(code|pre|script|style)\b[^>]*>.*?</\1\s*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CodeAndPreRegex();

    private static string ExtractAttributeValue(string html, ValueMatch match)
    {
        var segment = html.AsSpan(match.Index, match.Length);
        // Find the value between quotes
        var eqPos = segment.IndexOf('=');
        if (eqPos < 0) return "";
        var rest = segment[(eqPos + 1)..].Trim();
        if (rest.Length < 2) return "";
        var quote = rest[0];
        if (quote is not '"' and not '\'') return "";
        var endQuote = rest[1..].IndexOf(quote);
        if (endQuote < 0) return "";
        return rest[1..(endQuote + 1)].ToString();
    }

    private static bool IsExternalUrl(string url)
        => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("//")
        || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string path)
    {
        var s = path.TrimEnd('/');
        if (s.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase))
            s = s[..^"/index.html".Length];
        if (string.IsNullOrEmpty(s)) s = "/";
        return s;
    }

    [GeneratedRegex("""href\s*=\s*["'][^"']*["']""", RegexOptions.IgnoreCase)]
    private static partial Regex HrefRegex();

    [GeneratedRegex("""src\s*=\s*["'][^"']*["']""", RegexOptions.IgnoreCase)]
    private static partial Regex SrcRegex();
}