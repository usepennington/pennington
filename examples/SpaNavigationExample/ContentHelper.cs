namespace SpaNavigationExample;

using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Simple helper for querying markdown content pages.
/// </summary>
public sealed class ContentHelper
{
    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;

    public ContentHelper(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
    }

    public async Task<List<(ContentRoute Route, RecipeFrontMatter FrontMatter)>> GetAllPagesAsync()
    {
        var pages = new List<(ContentRoute, RecipeFrontMatter)>();
        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (item.Source is MarkdownFileSource source)
                {
                    var content = await File.ReadAllTextAsync(source.Path.Value);
                    var parsed = _parser.Parse<RecipeFrontMatter>(content);
                    var fm = parsed.Metadata ?? new RecipeFrontMatter();
                    pages.Add((item.Route, fm));
                }
            }
        }

        return pages;
    }

    public async Task<(RecipeFrontMatter FrontMatter, string Html)?> GetPageByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');

        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (!item.Route.CanonicalPath.Matches(new UrlPath(url))) continue;
                if (item.Source is not MarkdownFileSource source) continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<RecipeFrontMatter>(content);
                var fm = parsed.Metadata ?? new RecipeFrontMatter();

                var parsedItem = new ParsedItem(item.Route, fm, parsed.Body);
                var rendered = await _renderer.RenderAsync(parsedItem);
                if (rendered is RenderedItem renderedItem)
                {
                    return (fm, renderedItem.Content.Html);
                }
            }
        }

        return null;
    }
}
