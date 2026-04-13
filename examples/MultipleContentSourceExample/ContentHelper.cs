namespace MultipleContentSourceExample;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Helper for querying markdown content across multiple content sources.
/// </summary>
public sealed class ContentHelper(
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
}

/// <summary>
/// A rendered page with front matter, HTML content, and outline.
/// </summary>
public record RenderedPage<T>(T FrontMatter, string Html, OutlineEntry[] Outline)
    where T : IFrontMatter;