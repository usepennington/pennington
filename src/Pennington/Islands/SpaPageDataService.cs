namespace Pennington.Islands;

using Pennington.Routing;

/// <summary>Assembles SPA page data by coordinating island renderers.</summary>
public sealed class SpaPageDataService
{
    private readonly IEnumerable<IIslandRenderer> _renderers;
    private readonly RenderContext _renderContext;

    public SpaPageDataService(IEnumerable<IIslandRenderer> renderers, RenderContext renderContext)
    {
        _renderers = renderers;
        _renderContext = renderContext;
    }

    /// <summary>Get the SPA envelope for a given route. Returns null if no renderers produce content.</summary>
    public async Task<SpaEnvelopeDto?> GetPageDataAsync(ContentRoute route, string title, string? description = null)
    {
        var islands = new Dictionary<string, string>();

        foreach (var renderer in _renderers)
        {
            var html = await renderer.RenderAsync(route, _renderContext);
            if (!string.IsNullOrEmpty(html))
            {
                islands[renderer.IslandName] = html;
            }
        }

        if (islands.Count == 0) return null;

        return new SpaEnvelopeDto(title, description, islands);
    }
}
