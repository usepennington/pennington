namespace BeyondCookFormatExample;

using CooklangSharp;
using Microsoft.AspNetCore.Components.Web;
using Pennington.Pipeline;

/// <summary>
/// Renders a parsed <c>.cook</c> recipe by binding the CooklangSharp model to the <see cref="RecipeView"/> Razor
/// component. All markup lives in the component; this renderer only parses the body and supplies the parameters.
/// The <see cref="RazorContentRenderer{TComponent}"/> base owns the Blazor <c>HtmlRenderer</c> dispatch, heading
/// anchors, and outline extraction.
/// </summary>
public sealed class CookContentRenderer : RazorContentRenderer<RecipeView>
{
    /// <summary>Creates the renderer over the Blazor <c>HtmlRenderer</c> resolved from DI.</summary>
    public CookContentRenderer(HtmlRenderer renderer) : base(renderer)
    {
    }

    /// <inheritdoc/>
    protected override IReadOnlyDictionary<string, object?> BuildParameters(ParsedItem item)
    {
        var parsed = CooklangParser.Parse(item.RawMarkdown);
        if (parsed.Recipe is not { } recipe)
        {
            throw new InvalidOperationException(
                $"Cooklang parse failed: {string.Join("; ", parsed.Diagnostics.Select(d => d.Message))}");
        }

        return new Dictionary<string, object?>
        {
            [nameof(RecipeView.Recipe)] = recipe,
            [nameof(RecipeView.FrontMatter)] = (CookFrontMatter)item.Metadata,
        };
    }
}
