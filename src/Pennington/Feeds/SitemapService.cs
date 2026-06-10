namespace Pennington.Feeds;

using System.Xml.Linq;
using Content;
using FrontMatter;
using Infrastructure;
using Localization;
using Pipeline;

/// <summary>
/// Generates sitemap XML for the /sitemap.xml endpoint.
/// Uses <see cref="AsyncLazy{T}"/> for lazy, thread-safe computation.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — trusts IContentService for fresh metadata.
/// <para>
/// Enumerates every <see cref="IContentService.DiscoverAsync"/> result. Markdown
/// sources carry the front matter their content service already parsed at
/// discovery time, so <see cref="IFrontMatter.Date"/> (lastmod) is read straight
/// off <see cref="DiscoveredItem.Metadata"/> — no re-parse, and no risk of
/// applying one source's front-matter type to another's files. Programmatic /
/// Razor page sources surface metadata only at render time and carry none on the
/// discovered item, so their routes are emitted with no extra metadata.
/// </para>
/// <para>
/// <b>Filtering philosophy.</b> Sitemap, search index, and llms.txt each
/// answer a different question, so they intentionally run different
/// filtering paths:
/// </para>
/// <list type="bullet">
///   <item><b>Sitemap</b> (this service) — every canonical HTML URL a
///     crawler should index. Sourced from <see cref="IContentService.DiscoverAsync"/>
///     because Razor / programmatic routes with no TOC entry still need
///     to appear. Per-page <c>search:</c> / <c>llms:</c> opt-outs are
///     <i>not</i> honored here: those are search UX preferences, not
///     SEO directives.</item>
///   <item><b>Search index / llms.txt</b> — enumerates
///     <see cref="IContentService.GetIndexableEntriesAsync"/>, which
///     carries <see cref="ContentTocItem.ExcludeFromSearch"/> /
///     <see cref="ContentTocItem.ExcludeFromLlms"/> flags sourced from
///     <c>search:</c> / <c>llms:</c> front-matter.</item>
/// </list>
/// <para>
/// Keep these two paths distinct — collapsing them would either leak
/// search opt-outs into the sitemap (bad for SEO) or force every
/// Razor/programmatic source to grow a TOC entry purely so it shows up
/// in the sitemap.
/// </para>
/// </summary>
public sealed class SitemapService : IFileWatchAware
{
    private readonly AsyncLazy<string> _sitemapLazy;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>
    /// Initializes the service and prepares lazy sitemap generation driven by the provided builder.
    /// </summary>
    public SitemapService(
        IEnumerable<IContentService> contentServices,
        LocalizationOptions localization,
        SitemapBuilder builder)
    {
        _sitemapLazy = new AsyncLazy<string>(
            () => BuildSitemapAsync(contentServices, localization, builder));
    }

    /// <summary>
    /// Returns the serialized sitemap XML, generating it on first access and caching the result.
    /// </summary>
    public Task<string> GetSitemapXmlAsync() => _sitemapLazy.Task;

    private static async Task<string> BuildSitemapAsync(
        IEnumerable<IContentService> contentServices,
        LocalizationOptions localization,
        SitemapBuilder builder)
    {
        var candidates = new List<SitemapCandidate>();

        await foreach (var discovered in contentServices.DiscoverAllAsync())
        {
            // Skip non-HTML outputs (custom JSON feeds, static assets, etc.).
            // Programmatic content services can surface non-HTML routes through
            // DiscoverAsync — those are transport, not canonical URLs.
            var outputFile = discovered.Route.OutputFile.Value;
            var ext = Path.GetExtension(outputFile);
            if (!string.IsNullOrEmpty(ext)
                && !ext.Equals(".html", StringComparison.OrdinalIgnoreCase)
                && !ext.Equals(".htm", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // RedirectSource items are explicit redirects — no canonical value.
            // LlmsOnlySource items have no HTML page at all — they only exist as
            // llms.txt sidecars and shouldn't be advertised to crawlers. Skip both.
            //
            // EndpointSource is deliberately NOT skipped: those routes — custom
            // IContentService pages, AddTaxonomy term pages — serve real canonical
            // HTML at a stable URL and belong in the sitemap. Transport endpoints
            // that happen to use EndpointSource (JSON feeds, data routes) emit a
            // non-HTML output file and are already dropped by the extension check
            // above, so they never reach this line.
            if (discovered.Source.Value is RedirectSource or LlmsOnlySource)
            {
                continue;
            }

            // Markdown sources carry the front matter their content service
            // parsed at discovery time (correct front-matter type, no re-parse);
            // programmatic / Razor sources carry none. SitemapBuilder tolerates
            // null metadata and re-filters drafts/redirects defensively. A
            // markdown page whose front matter failed to parse arrives with
            // null metadata — it still renders and is served, so it's emitted
            // with no <lastmod> rather than dropped.
            candidates.Add(new SitemapCandidate(discovered.Route, discovered.Metadata));
        }

        var entries = builder.Build(candidates);

        if (localization.IsMultiLocale)
        {
            // Feed only the publishable candidates so hreflang alternates can never
            // point at a draft / scheduled / redirect URL that Build() dropped.
            var localeUrlMap = BuildLocaleUrlMap(builder.Publishable(candidates), localization);
            return SerializeToXml(entries, localeUrlMap, localization, builder.CanonicalBase);
        }

        return SerializeToXml(entries);
    }

    // One serializer for both the plain and multi-locale cases. Passing a localeUrlMap
    // switches on the xhtml namespace and per-entry hreflang alternates.
    private static string SerializeToXml(
        IReadOnlyList<SitemapEntry> entries,
        Dictionary<string, List<(string Locale, string Url)>>? localeUrlMap = null,
        LocalizationOptions? localization = null,
        Routing.UrlPath canonicalBase = default)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        var urlset = new XElement(ns + "urlset",
            localeUrlMap is not null
                ? new XAttribute(XNamespace.Xmlns + "xhtml", xhtml.NamespaceName)
                : null,
            entries.Select(entry =>
            {
                var url = new XElement(ns + "url",
                    new XElement(ns + "loc", entry.Url.Value));

                if (entry.LastModified.HasValue)
                {
                    url.Add(new XElement(ns + "lastmod", entry.LastModified.Value.ToString("yyyy-MM-dd")));
                }

                if (localeUrlMap is not null)
                {
                    AddHreflangAlternates(url, entry, xhtml, localeUrlMap, localization!, canonicalBase);
                }

                return url;
            }));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);
        return doc.Declaration + Environment.NewLine + doc;
    }

    private static void AddHreflangAlternates(
        XElement url,
        SitemapEntry entry,
        XNamespace xhtml,
        Dictionary<string, List<(string Locale, string Url)>> localeUrlMap,
        LocalizationOptions localization,
        Routing.UrlPath canonicalBase)
    {
        // Find the content-relative URL for this entry to look up alternates.
        var entryPath = entry.Url.Value;
        if (canonicalBase.Value != "/" && entryPath.StartsWith(canonicalBase.Value))
        {
            entryPath = entryPath[canonicalBase.Value.Length..];
        }

        var contentRelative = StripLocalePrefix(entryPath, localization);

        if (!localeUrlMap.TryGetValue(contentRelative, out var alternates) || alternates.Count <= 1)
        {
            return;
        }

        foreach (var (locale, localeUrl) in alternates)
        {
            var hreflang = localization.Locales.TryGetValue(locale, out var info)
                ? info.HtmlLang ?? locale
                : locale;

            url.Add(new XElement(xhtml + "link",
                new XAttribute("rel", "alternate"),
                new XAttribute("hreflang", hreflang),
                new XAttribute("href", (canonicalBase / new Routing.UrlPath(localeUrl)).Value)));
        }
    }

    /// <summary>
    /// Builds a map from content-relative URL to list of (locale, URL) pairs.
    /// </summary>
    private static Dictionary<string, List<(string Locale, string Url)>> BuildLocaleUrlMap(
        IReadOnlyList<SitemapCandidate> candidates,
        LocalizationOptions localization)
    {
        var map = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            var url = candidate.Route.CanonicalPath.Value;
            var locale = candidate.Route.Locale is { Length: > 0 } loc ? loc : localization.DefaultLocale;
            var contentRelative = StripLocalePrefix(url, localization);

            if (!map.TryGetValue(contentRelative, out var list))
            {
                list = [];
                map[contentRelative] = list;
            }

            list.Add((locale, url));
        }

        return map;
    }

    private static string StripLocalePrefix(string url, LocalizationOptions localization)
    {
        var trimmed = url.Trim('/');
        if (string.IsNullOrEmpty(trimmed))
        {
            return "/";
        }

        var firstSlash = trimmed.IndexOf('/');
        var firstSegment = firstSlash >= 0 ? trimmed[..firstSlash] : trimmed;

        if (!string.IsNullOrEmpty(firstSegment)
            && localization.Locales.ContainsKey(firstSegment)
            && !string.Equals(firstSegment, localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            var remainder = firstSlash >= 0 ? trimmed[(firstSlash + 1)..] : "";
            return string.IsNullOrEmpty(remainder) ? "/" : $"/{remainder}";
        }

        return $"/{trimmed}";
    }
}