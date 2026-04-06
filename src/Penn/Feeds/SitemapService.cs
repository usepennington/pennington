namespace Penn.Feeds;

using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.FrontMatter;
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
        // Sitemap only needs routes and metadata — no rendering required.
        // Use IContentService.GetContentTocEntriesAsync() for route discovery
        // and DiscoverAsync() + parse for IDateable metadata.
        var contentServices = sp.GetServices<IContentService>();
        var entries = ImmutableList.CreateBuilder<SitemapEntry>();

        foreach (var service in contentServices)
        {
            var tocItems = await service.GetContentTocEntriesAsync();
            foreach (var item in tocItems)
            {
                var absoluteUrl = item.Route.AbsoluteUrl(builder.CanonicalBase);
                entries.Add(new SitemapEntry(absoluteUrl, LastModified: null, ChangeFrequency: null, Priority: null));
            }
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
}
