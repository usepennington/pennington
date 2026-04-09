namespace SpaNavigationTutorialExample.Islands;

using System.Collections.Immutable;
using Penn.Content;
using Penn.Islands;
using Penn.Navigation;
using Penn.Routing;
using SpaNavigationTutorialExample.Islands.Components;

public class NavIslandRenderer(
    IEnumerable<IContentService> contentServices,
    NavigationBuilder navigationBuilder,
    ComponentRenderer renderer) : RazorIslandRenderer<SidebarNav>(renderer)
{
    public override string IslandName => "nav";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var tocItems = new List<ContentTocItem>();
        foreach (var service in contentServices)
        {
            var items = await service.GetContentTocEntriesAsync();
            tocItems.AddRange(items);
        }

        var tree = navigationBuilder.BuildTree(tocItems, route);

        return new Dictionary<string, object?>
        {
            [nameof(SidebarNav.NavItems)] = tree,
        };
    }
}
