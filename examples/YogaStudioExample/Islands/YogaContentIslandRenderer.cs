namespace YogaStudioExample.Islands;

using Pennington.Islands;
using Pennington.Routing;
using YogaStudioExample.Islands.Components;
using YogaStudioExample.Models;
using YogaStudioExample.Services;

public class YogaContentIslandRenderer(
    ContentHelper contentHelper,
    ComponentRenderer renderer) : RazorIslandRenderer<YogaArticle>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var url = route.CanonicalPath.Value;

        // Try blog content first
        if (url.StartsWith("/blog/", StringComparison.OrdinalIgnoreCase) && url.Length > 6)
        {
            var result = await contentHelper.GetRenderedPageAsync<YogaBlogFrontMatter>(url);
            if (result.HasValue)
            {
                return new Dictionary<string, object?>
                {
                    [nameof(YogaArticle.Title)] = result.Value.FrontMatter.Title,
                    [nameof(YogaArticle.HtmlContent)] = result.Value.Html,
                    [nameof(YogaArticle.Author)] = result.Value.FrontMatter.Author,
                    [nameof(YogaArticle.Date)] = result.Value.FrontMatter.Date,
                };
            }
        }

        // Try page content
        var pageResult = await contentHelper.GetRenderedPageAsync<YogaFrontMatter>(url);
        if (pageResult.HasValue)
        {
            return new Dictionary<string, object?>
            {
                [nameof(YogaArticle.Title)] = pageResult.Value.FrontMatter.Title,
                [nameof(YogaArticle.HtmlContent)] = pageResult.Value.Html,
            };
        }

        return null;
    }
}
