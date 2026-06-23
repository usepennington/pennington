namespace Pennington.Content;

using Pipeline;
using Routing;

/// <summary>
/// Default <see cref="IPageResolver"/> over the registered content services, parser, and renderer.
/// </summary>
public sealed class PageResolver : IPageResolver
{
    private readonly IReadOnlyList<IContentService> _services;
    private readonly IContentParser? _parser;
    private readonly IContentRenderer _renderer;

    /// <summary>
    /// Creates the resolver from the registered content services and renderer. The
    /// <paramref name="parser"/> is optional: a bare host that registers no markdown source has no
    /// <see cref="IContentParser"/>, so <see cref="ResolveAsync"/> resolves nothing.
    /// </summary>
    public PageResolver(
        IEnumerable<IContentService> services,
        IContentRenderer renderer,
        IContentParser? parser = null)
    {
        _services = services.ToList();
        _renderer = renderer;
        _parser = parser;
    }

    /// <inheritdoc/>
    public async Task<RenderedItem?> ResolveAsync(UrlPath requested)
    {
        // No markdown parser registered (bare host): markdown routes can't be resolved here.
        // Razor @page routes are matched by Blazor endpoint routing before this fallback, and
        // custom IContentService sources serve their own HTML, so there's nothing to render.
        if (_parser is null)
        {
            return null;
        }

        await foreach (var discovered in _services.DiscoverAllAsync())
        {
            if (!discovered.Route.CanonicalPath.Matches(requested))
            {
                continue;
            }

            // Llms-only routes (*.llms.md) contribute to llms.txt and its sidecar markdown but
            // never produce an HTML page — the projection renders them in-process, so serving them
            // here would expose agent-only content to humans. Decline (don't return) so a real page
            // from another service at the same slug can still win; otherwise the request 404s.
            if (discovered.Source is LlmsOnlySource)
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
