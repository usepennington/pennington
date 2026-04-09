using Pennington.Islands;
using Pennington.Routing;
using SpaNavigationExample.Slots.Components;

namespace SpaNavigationExample.Slots;

/// <summary>
/// Custom slot renderer that produces a recipe metadata card for the sidebar.
/// Demonstrates a Razor-based slot renderer using <see cref="RazorIslandRenderer{TComponent}"/>
/// to delegate presentation to a <c>.razor</c> component.
/// </summary>
public class RecipeInfoSlotRenderer(
    ContentHelper contentHelper,
    ComponentRenderer renderer) : RazorIslandRenderer<RecipeInfoCard>(renderer)
{
    public override string IslandName => "recipe-info";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var url = route.CanonicalPath.Value;
        var result = await contentHelper.GetPageByUrlAsync(url);
        if (result is null) return null;

        var fm = result.Value.FrontMatter;

        // Index page has no recipe metadata.
        if (fm is { PrepTime: 0, CookTime: 0, Servings: 0 })
            return null;

        return new Dictionary<string, object?>
        {
            [nameof(RecipeInfoCard.PrepTime)] = fm.PrepTime,
            [nameof(RecipeInfoCard.CookTime)] = fm.CookTime,
            [nameof(RecipeInfoCard.Servings)] = fm.Servings,
            [nameof(RecipeInfoCard.Difficulty)] = fm.Difficulty,
            [nameof(RecipeInfoCard.Tags)] = fm.Tags,
        };
    }
}
