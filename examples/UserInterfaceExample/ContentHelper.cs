namespace UserInterfaceExample;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Helper for querying markdown content pages and building navigation.
/// </summary>
public sealed class ContentHelper
{
    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly NavigationBuilder _navBuilder;

    public ContentHelper(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer,
        NavigationBuilder navBuilder)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
        _navBuilder = navBuilder;
    }

    public async Task<(DocsFrontMatter FrontMatter, string Html, OutlineEntry[] Outline)?> GetPageByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');

        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (!item.Route.CanonicalPath.Matches(new UrlPath(url))) continue;
                if (item.Source is not MarkdownFileSource source) continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<DocsFrontMatter>(content);
                var fm = parsed.Metadata ?? new DocsFrontMatter();

                var parsedItem = new ParsedItem(item.Route, fm, parsed.Body);
                var rendered = await _renderer.RenderAsync(parsedItem);
                if (rendered is RenderedItem renderedItem)
                {
                    return (fm, renderedItem.Content.Html, renderedItem.Content.Outline);
                }
            }
        }

        return null;
    }

    public async Task<ImmutableList<NavigationTreeItem>> GetNavigationTocAsync(string currentPath)
    {
        var tocItems = new List<ContentTocItem>();
        foreach (var service in _services)
        {
            var items = await service.GetContentTocEntriesAsync();
            tocItems.AddRange(items);
        }

        var currentRoute = new ContentRoute
        {
            CanonicalPath = new UrlPath(currentPath),
            OutputFile = new FilePath($"{currentPath.TrimStart('/')}/index.html"),
        };

        return _navBuilder.BuildTree(tocItems, currentRoute);
    }
}