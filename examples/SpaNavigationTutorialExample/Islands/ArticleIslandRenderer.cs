namespace SpaNavigationTutorialExample.Islands;

using Pennington.Islands;
using Pennington.Routing;
using Components;

public class ArticleIslandRenderer(
    ContentHelper contentHelper,
    ComponentRenderer renderer) : RazorIslandRenderer<ArticleContent>(renderer)
{
    public override string IslandName => "article";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var url = route.CanonicalPath.Value;
        var result = await contentHelper.GetPageByUrlAsync(url);
        if (result is null) return null;

        return new Dictionary<string, object?>
        {
            [nameof(ArticleContent.Title)] = result.Value.FrontMatter.Title,
            [nameof(ArticleContent.HtmlContent)] = result.Value.Html,
        };
    }
}