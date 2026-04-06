namespace Penn.Feeds;

using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.Infrastructure;
using Penn.Pipeline;

/// <summary>
/// Generates and caches the sitemap XML for the /sitemap.xml endpoint.
/// Invalidates when content files change.
/// </summary>
public sealed class SitemapService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SitemapBuilder _builder;
    private string? _cachedXml;

    public SitemapService(
        IServiceProvider serviceProvider,
        SitemapBuilder builder,
        IFileWatcher fileWatcher)
    {
        _serviceProvider = serviceProvider;
        _builder = builder;
        fileWatcher.SubscribeToChanges(() => _cachedXml = null);
    }

    public async Task<string> GetSitemapXmlAsync()
    {
        if (_cachedXml != null)
            return _cachedXml;

        var renderedItems = new List<RenderedItem>();
        var contentServices = _serviceProvider.GetServices<IContentService>();
        var parser = _serviceProvider.GetRequiredService<IContentParser>();
        var renderer = _serviceProvider.GetRequiredService<IContentRenderer>();

        foreach (var service in contentServices)
        {
            await foreach (var discovered in service.DiscoverAsync())
            {
                var parseResult = await parser.ParseAsync(discovered);
                if (parseResult is not ParsedItem parsed) continue;

                var renderResult = await renderer.RenderAsync(parsed);
                if (renderResult is RenderedItem rendered)
                    renderedItems.Add(rendered);
            }
        }

        var entries = _builder.Build(renderedItems);
        _cachedXml = SerializeToXml(entries);
        return _cachedXml;
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
