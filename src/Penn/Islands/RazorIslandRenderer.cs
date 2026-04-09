namespace Pennington.Islands;

using Microsoft.AspNetCore.Components;
using Pennington.Routing;

/// <summary>
/// Base class for island renderers that render a Razor component.
/// </summary>
public abstract class RazorIslandRenderer<TComponent>(
    ComponentRenderer renderer) : IIslandRenderer
    where TComponent : IComponent
{
    public abstract string IslandName { get; }

    /// <summary>
    /// Build parameter dictionary for the component, or null to skip this island.
    /// </summary>
    protected abstract Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route);

    public async Task<string> RenderAsync(ContentRoute route, RenderContext context)
    {
        var parameters = await BuildParametersAsync(route);
        if (parameters is null)
            return "";

        return await renderer.RenderComponentAsync<TComponent>(parameters);
    }
}
