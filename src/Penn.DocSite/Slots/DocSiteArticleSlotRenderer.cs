namespace Penn.DocSite.Slots;

using Penn.DocSite.Services;
using Penn.DocSite.Slots.Components;
using Penn.Islands;
using Penn.Routing;

/// <summary>
/// Renders the article content island for SPA navigation.
/// </summary>
internal class DocSiteArticleSlotRenderer(
    ContentResolver contentResolver,
    ComponentRenderer renderer) : RazorIslandRenderer<DocSiteArticle>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var url = route.CanonicalPath.Value;
        var resolved = await contentResolver.GetContentByUrlAsync(url);
        if (resolved is null) return null;

        var navInfo = await contentResolver.GetNavigationInfoAsync(url);

        return new Dictionary<string, object?>
        {
            [nameof(DocSiteArticle.Title)] = resolved.Title,
            [nameof(DocSiteArticle.HtmlContent)] = resolved.Html,
            [nameof(DocSiteArticle.PreviousPageName)] = navInfo?.PreviousPage?.Title,
            [nameof(DocSiteArticle.PreviousPageHref)] = navInfo?.PreviousPage?.Route.NavigationPath.Value,
            [nameof(DocSiteArticle.NextPageName)] = navInfo?.NextPage?.Title,
            [nameof(DocSiteArticle.NextPageHref)] = navInfo?.NextPage?.Route.NavigationPath.Value,
        };
    }
}
