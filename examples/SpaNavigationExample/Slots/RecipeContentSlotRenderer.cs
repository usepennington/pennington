using Pennington.Islands;
using Pennington.Routing;
using SpaNavigationExample.Slots.Components;

namespace SpaNavigationExample.Slots;

/// <summary>
/// Custom slot renderer that wraps the rendered markdown with a title header.
/// Demonstrates how sites delegate presentation to a Razor component
/// while keeping data-fetching logic in the renderer.
/// </summary>
public class RecipeContentSlotRenderer(
    ContentHelper contentHelper,
    ComponentRenderer renderer) : RazorIslandRenderer<RecipeContent>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var url = route.CanonicalPath.Value;
        var result = await contentHelper.GetPageByUrlAsync(url);
        if (result is null) return null;

        return new Dictionary<string, object?>
        {
            [nameof(RecipeContent.Title)] = result.Value.FrontMatter.Title,
            [nameof(RecipeContent.HtmlContent)] = result.Value.Html,
        };
    }
}
