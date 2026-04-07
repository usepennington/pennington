namespace Penn.Feeds;

using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.Infrastructure;

/// <summary>
/// Generates sitemap XML for the /sitemap.xml endpoint.
/// Uses <see cref="AsyncLazy{T}"/> for lazy, thread-safe computation.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — trusts IContentService for fresh metadata.
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
        var entries = ImmutableList.CreateBuilder<SitemapEntry>();
        var tocItems = new List<ContentTocItem>();

        foreach (var service in contentServices)
        {
            var items = await service.GetContentTocEntriesAsync();
            tocItems.AddRange(items);
            foreach (var item in items)
            {
                var absoluteUrl = item.Route.AbsoluteUrl(builder.CanonicalBase);
                entries.Add(new SitemapEntry(absoluteUrl, LastModified: null, ChangeFrequency: null, Priority: null));
            }
        }

        if (localization.IsMultiLocale)
        {
            var localeUrlMap = BuildLocaleUrlMap(tocItems, localization);
            return SerializeToXmlWithHreflang(entries.ToImmutable(), localeUrlMap, localization, builder.CanonicalBase);
        }

        return SerializeToXml(entries.ToImmutable());
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
        List<ContentTocItem> tocItems,
        LocalizationOptions localization)
    {
        var map = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in tocItems)
        {
            var url = item.Route.CanonicalPath.Value;
            var locale = item.Locale ?? localization.DefaultLocale;
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
