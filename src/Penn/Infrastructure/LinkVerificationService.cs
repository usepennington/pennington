namespace Penn.Infrastructure;

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Penn.Generation;
using Penn.Routing;

/// <summary>
/// Verifies internal links in rendered HTML against known routes.
/// Extracts href and src attributes and classifies each as valid, broken, or external.
/// Does not make HTTP requests — this is purely static analysis.
/// </summary>
public sealed partial class LinkVerificationService
{
    private readonly HashSet<string> _knownPaths;

    /// <summary>
    /// Create with the set of all known page canonical paths.
    /// </summary>
    public LinkVerificationService(IEnumerable<ContentRoute> knownRoutes)
    {
        _knownPaths = new HashSet<string>(
            knownRoutes.Select(r => NormalizePath(r.CanonicalPath.Value)),
            StringComparer.OrdinalIgnoreCase);
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
            return new LinkCheckResult(new ExternalLink(sourcePage, url));

        // Anchor-only links (#something)
        if (url.StartsWith('#'))
            return new LinkCheckResult(new ValidLink(sourcePage, url));

        // Internal links — check against known routes
        var normalizedUrl = NormalizePath(url.Split('#')[0].Split('?')[0]);

        if (_knownPaths.Contains(normalizedUrl))
            return new LinkCheckResult(new ValidLink(sourcePage, url));

        return new LinkCheckResult(new BrokenLinkResult(sourcePage, url, linkType, "Page not found"));
    }

    /// <summary>
    /// Extract all href and src URLs from HTML.
    /// </summary>
    internal static List<(string Url, LinkType Type)> ExtractLinks(string html)
    {
        var results = new List<(string, LinkType)>();

        foreach (var match in HrefRegex().EnumerateMatches(html))
        {
            var url = ExtractAttributeValue(html, match);
            if (!string.IsNullOrWhiteSpace(url))
                results.Add((url, LinkType.Internal));
        }

        foreach (var match in SrcRegex().EnumerateMatches(html))
        {
            var url = ExtractAttributeValue(html, match);
            if (!string.IsNullOrWhiteSpace(url))
                results.Add((url, LinkType.Image));
        }

        return results;
    }

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
