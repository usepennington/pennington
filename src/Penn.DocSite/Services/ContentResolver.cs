namespace Penn.DocSite.Services;

using Penn.Content;
using Penn.FrontMatter;
using Penn.Navigation;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Resolves content pages by URL for the DocSite.
/// </summary>
public sealed class ContentResolver
{
    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly NavigationBuilder _navBuilder;

    public ContentResolver(
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

    /// <summary>
    /// Get rendered content for a URL. Returns null if not found.
    /// </summary>
    public async Task<ResolvedContent?> GetContentByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');

        // Find the discovered item matching this URL
        DiscoveredItem? found = null;
        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (item.Route.CanonicalPath.Matches(new UrlPath(url)))
                {
                    found = item;
                    break;
                }
            }

            if (found != null) break;
        }

        if (found == null) return null;

        // Parse
        var parseResult = await ParseItem(found);
        if (parseResult == null) return null;

        // Render
        var renderResult = await _renderer.RenderAsync(parseResult);
        if (renderResult is not RenderedItem rendered) return null;

        return new ResolvedContent(
            Route: rendered.Route,
            Title: rendered.Metadata.Title,
            Description: (rendered.Metadata as IDescribable)?.Description,
            Html: rendered.Content.Html,
            Outline: rendered.Content.Outline,
            Metadata: rendered.Metadata
        );
    }

    /// <summary>
    /// Get navigation info for a URL.
    /// </summary>
    public async Task<NavigationInfo?> GetNavigationInfoAsync(string url)
    {
        url = "/" + url.Trim('/');
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(url),
            OutputFile = new FilePath($"{url.TrimStart('/')}/index.html"),
        };

        var tocItems = new List<ContentTocItem>();
        foreach (var service in _services)
        {
            var items = await service.GetContentTocEntriesAsync();
            tocItems.AddRange(items);
        }

        return _navBuilder.BuildNavigationInfo(tocItems, route);
    }

    /// <summary>
    /// Get all TOC items for building the navigation tree.
    /// </summary>
    public async Task<IReadOnlyList<ContentTocItem>> GetTocItemsAsync()
    {
        var items = new List<ContentTocItem>();
        foreach (var service in _services)
        {
            items.AddRange(await service.GetContentTocEntriesAsync());
        }

        return items;
    }

    private async Task<ParsedItem?> ParseItem(DiscoveredItem item)
    {
        if (item.Source is not MarkdownFileSource source) return null;

        var content = await File.ReadAllTextAsync(source.Path.Value);
        var result = _parser.Parse<DocSiteFrontMatter>(content);
        var metadata = result.Metadata ?? new DocSiteFrontMatter();

        return new ParsedItem(item.Route, metadata, result.Body);
    }
}

public record ResolvedContent(
    ContentRoute Route,
    string Title,
    string? Description,
    string Html,
    OutlineEntry[] Outline,
    IFrontMatter Metadata
);
