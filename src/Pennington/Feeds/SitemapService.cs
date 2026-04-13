namespace Pennington.Feeds;

using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Content;
using FrontMatter;
using Infrastructure;
using Pipeline;

/// <summary>
/// Generates sitemap XML for the /sitemap.xml endpoint.
/// Uses <see cref="AsyncLazy{T}"/> for lazy, thread-safe computation.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — trusts IContentService for fresh metadata.
/// <para>
/// Enumerates every <see cref="IContentService.DiscoverAsync"/> result. For
/// markdown sources, the parser is invoked so <see cref="IFrontMatter.Date"/> metadata
/// (lastmod) and <see cref="IFrontMatter.IsDraft"/> filtering apply. For programmatic /
/// Razor page sources, the route is emitted with no extra metadata — those
/// content types are rarely dateable and forcing every programmatic generator
/// to run at sitemap-build time would be expensive.
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
public sealed class SitemapService
{
    private readonly AsyncLazy<string> _sitemapLazy;

    public SitemapService(IServiceProvider serviceProvider, SitemapBuilder builder)
    {
        _sitemapLazy = new AsyncLazy<string>(() => BuildSitemapAsync(serviceProvider, builder));
    }

    public Task<string> GetSitemapXmlAsync() => _sitemapLazy.Value;

    private static async Task<string> BuildSitemapAsync(IServiceProvider sp, SitemapBuilder builder)
    {
        var contentServices = sp.GetServices<IContentService>();
        var localization = sp.GetRequiredService<LocalizationOptions>();
        // Parser is only required for MarkdownFileSource entries. Sites that
        // only expose programmatic / Razor sources never register one, so
        // resolve optionally and fall back to metadata-less entries.
        var parser = sp.GetService<IContentParser>();
        var candidates = new List<SitemapCandidate>();

        foreach (var service in contentServices)
        {
            await foreach (var discovered in service.DiscoverAsync())
            {
                // Skip non-HTML outputs (SPA page-data JSON, static assets, etc.).
                // The SPA navigation content service surfaces /_spa-data/*.json
                // routes through DiscoverAsync — those are transport for the
                // SPA shell and must not appear as canonical URLs.
                var outputFile = discovered.Route.OutputFile.Value;
                var ext = Path.GetExtension(outputFile);
                if (!string.IsNullOrEmpty(ext)
                    && !ext.Equals(".html", StringComparison.OrdinalIgnoreCase)
                    && !ext.Equals(".htm", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // RedirectSource-backed items either are explicit redirects
                // (no canonical value) or are framework-internal placeholder
                // routes like /_spa-data/*.json (which the extension check
                // above already caught). Skip them.
                if (discovered.Source is RedirectSource) continue;

                IFrontMatter? metadata = null;

                // Only markdown sources go through the parser — it's the only
                // source type that parses front matter from the file system.
                // Programmatic/Razor sources surface metadata at generation /
                // render time, which is too expensive to run here just for
                // lastmod. Those entries get emitted with route + no metadata.
                if (discovered.Source is MarkdownFileSource && parser is not null)
                {
                    var parseResult = await parser.ParseAsync(discovered);
                    if (parseResult is ParsedItem parsed)
                    {
                        metadata = parsed.Metadata;
                    }
                    else
                    {
                        // Parse failed entirely — skip. (The page probably
                        // won't render either, so omitting it is defensible.)
                        continue;
                    }
                }

                candidates.Add(new SitemapCandidate(discovered.Route, metadata));
            }
        }

        var entries = builder.Build(candidates);

        if (localization.IsMultiLocale)
        {
            var localeUrlMap = BuildLocaleUrlMap(candidates, localization);
            return SerializeToXmlWithHreflang(entries, localeUrlMap, localization, builder.CanonicalBase);
        }

        return SerializeToXml(entries);
    }

    private static string SerializeToXml(IReadOnlyList<SitemapEntry> entries)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset",
            entries.Select(entry =>
            {
                var url = new XElement(ns + "url",
                    new XElement(ns + "loc", entry.Url.Value));

                if (entry.LastModified.HasValue)
                    url.Add(new XElement(ns + "lastmod", entry.LastModified.Value.ToString("yyyy-MM-dd")));

                if (entry.ChangeFrequency != null)
                    url.Add(new XElement(ns + "changefreq", entry.ChangeFrequency));

                if (entry.Priority.HasValue)
                    url.Add(new XElement(ns + "priority", entry.Priority.Value.ToString("F1")));

                return url;
            }));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);
        return doc.Declaration + Environment.NewLine + doc;
    }

    private static string SerializeToXmlWithHreflang(
        IReadOnlyList<SitemapEntry> entries,
        Dictionary<string, List<(string Locale, string Url)>> localeUrlMap,
        LocalizationOptions localization,
        Routing.UrlPath canonicalBase)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        var urlset = new XElement(ns + "urlset",
            new XAttribute(XNamespace.Xmlns + "xhtml", xhtml.NamespaceName),
            entries.Select(entry =>
            {
                var url = new XElement(ns + "url",
                    new XElement(ns + "loc", entry.Url.Value));

                if (entry.LastModified.HasValue)
                    url.Add(new XElement(ns + "lastmod", entry.LastModified.Value.ToString("yyyy-MM-dd")));

                // Find the content-relative URL for this entry to look up alternates
                var entryPath = entry.Url.Value;
                if (canonicalBase.Value != "/" && entryPath.StartsWith(canonicalBase.Value))
                    entryPath = entryPath[canonicalBase.Value.Length..];

                var contentRelative = StripLocalePrefix(entryPath, localization);

                if (localeUrlMap.TryGetValue(contentRelative, out var alternates) && alternates.Count > 1)
                {
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

                return url;
            }));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);
        return doc.Declaration + Environment.NewLine + doc;
    }

    /// <summary>
    /// Builds a map from content-relative URL to list of (locale, URL) pairs.
    /// </summary>
    private static Dictionary<string, List<(string Locale, string Url)>> BuildLocaleUrlMap(
        List<SitemapCandidate> candidates,
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
        if (string.IsNullOrEmpty(trimmed)) return "/";

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