namespace Pennington.Content;

using Pipeline;
using Routing;

/// <summary>
/// Default <see cref="IPageResolver"/> over the registered content services, parser, and renderer.
/// </summary>
public sealed class PageResolver : IPageResolver
{
    private readonly IReadOnlyList<IContentService> _services;
    private readonly IContentParser _parser;
    private readonly IContentRenderer _renderer;

    /// <summary>Creates the resolver from the registered content services, parser, and renderer.</summary>
    public PageResolver(IEnumerable<IContentService> services, IContentParser parser, IContentRenderer renderer)
    {
        _services = services.ToList();
        _parser = parser;
        _renderer = renderer;
    }

    /// <inheritdoc/>
    public async Task<RenderedItem?> ResolveAsync(UrlPath requested)
    {
        await foreach (var discovered in _services.DiscoverAllAsync())
        {
            if (!discovered.Route.CanonicalPath.Matches(requested))
            {
                continue;
            }

            var parsed = await _parser.ParseAsync(discovered);
            if (parsed.Value is not ParsedItem parsedItem)
            {
                continue;
            }

            var rendered = await _renderer.RenderAsync(parsedItem);
            if (rendered.Value is RenderedItem renderedItem)
            {
                return renderedItem;
            }
        }

        return null;
    }
}
