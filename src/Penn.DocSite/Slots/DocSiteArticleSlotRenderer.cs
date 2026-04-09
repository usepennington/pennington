namespace Pennington.DocSite.Slots;

using Pennington.DocSite.Services;
using Pennington.DocSite.Slots.Components;
using Pennington.Islands;
using Pennington.Routing;

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

        var area = contentResolver.ResolveCurrentArea(url);
        var navInfo = await contentResolver.GetNavigationInfoForAreaAsync(url, area);

        return new Dictionary<string, object?>
        {
            [nameof(DocSiteArticle.Title)] = resolved.Title,
            [nameof(DocSiteArticle.HtmlContent)] = resolved.Html,
            [nameof(DocSiteArticle.PreviousPageName)] = navInfo?.PreviousPage?.Title,
            [nameof(DocSiteArticle.PreviousPageHref)] = navInfo?.PreviousPage?.Route.CanonicalPath.Value,
            [nameof(DocSiteArticle.NextPageName)] = navInfo?.NextPage?.Title,
            [nameof(DocSiteArticle.NextPageHref)] = navInfo?.NextPage?.Route.CanonicalPath.Value,
            [nameof(DocSiteArticle.FallbackRequestedLocale)] = resolved.FallbackRequestedDisplayName,
            [nameof(DocSiteArticle.FallbackDefaultLocale)] = resolved.FallbackDefaultDisplayName,
        };
    }
}
