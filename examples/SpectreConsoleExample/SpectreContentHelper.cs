namespace SpectreConsoleExample;

using System.Collections.Immutable;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Navigation;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Helper for querying Spectre.Console documentation content across multiple sections.
/// </summary>
public sealed class SpectreContentHelper(
    IEnumerable<IContentService> services,
    FrontMatterParser parser,
    IContentRenderer renderer,
    NavigationBuilder navBuilder)
{
    /// <summary>
    /// Get rendered content for a specific URL.
    /// </summary>
    public async Task<RenderedPage<T>?> GetRenderedPageAsync<T>(string url)
        where T : IFrontMatter, new()
    {
        url = "/" + url.Trim('/');

        foreach (var service in services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (!item.Route.CanonicalPath.Matches(new UrlPath(url))) continue;
                if (item.Source is not MarkdownFileSource source) continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = parser.Parse<T>(content);
                var fm = parsed.Metadata ?? new T();
                var parsedItem = new ParsedItem(item.Route, fm, parsed.Body);
                var rendered = await renderer.RenderAsync(parsedItem);
                if (rendered is RenderedItem renderedItem)
                {
                    return new RenderedPage<T>(fm, renderedItem.Content.Html, renderedItem.Content.Outline);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Get all pages matching a URL prefix, parsed with the given front matter type.
    /// </summary>
    public async Task<List<PageInfo<T>>> GetAllPagesAsync<T>(string urlPrefix)
        where T : IFrontMatter, new()
    {
        var pages = new List<PageInfo<T>>();
        foreach (var service in services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (!item.Route.CanonicalPath.Value.StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (item.Source is not MarkdownFileSource source) continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = parser.Parse<T>(content);
                var fm = parsed.Metadata ?? new T();
                pages.Add(new PageInfo<T>(item.Route.CanonicalPath.Value, fm));
            }
        }

        return pages;
    }

    /// <summary>
    /// Get navigation tree for a specific section.
    /// </summary>
    public async Task<ImmutableList<NavigationTreeItem>> GetNavigationAsync(string section, string? currentUrl = null)
    {
        var tocItems = new List<ContentTocItem>();
        foreach (var service in services.Where(s =>
                     string.Equals(s.DefaultSection, section, StringComparison.OrdinalIgnoreCase)))
        {
            var items = await service.GetContentTocEntriesAsync();
            tocItems.AddRange(items);
        }

        ContentRoute? currentRoute = currentUrl != null
            ? new ContentRoute
            {
                CanonicalPath = new UrlPath(currentUrl),
                OutputFile = new FilePath("")
            }
            : null;

        return navBuilder.BuildTree(tocItems, currentRoute);
    }

    /// <summary>
    /// Get navigation info (breadcrumbs, prev/next) for a URL within a section.
    /// </summary>
    public async Task<NavigationInfo?> GetNavigationInfoAsync(string url, string section)
    {
        var tocItems = new List<ContentTocItem>();
        foreach (var service in services.Where(s =>
                     string.Equals(s.DefaultSection, section, StringComparison.OrdinalIgnoreCase)))
        {
            var items = await service.GetContentTocEntriesAsync();
            tocItems.AddRange(items);
        }

        if (tocItems.Count == 0) return null;

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(url),
            OutputFile = new FilePath(url.TrimStart('/') + "/index.html")
        };

        return navBuilder.BuildNavigationInfo(tocItems, route);
    }
}

/// <summary>
/// A rendered page with front matter, HTML content, and outline.
/// </summary>
public record RenderedPage<T>(T FrontMatter, string Html, OutlineEntry[] Outline)
    where T : IFrontMatter;

/// <summary>
/// A discovered page with its URL and front matter.
/// </summary>
public record PageInfo<T>(string Url, T FrontMatter) where T : IFrontMatter;
