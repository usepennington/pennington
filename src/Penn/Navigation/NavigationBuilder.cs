namespace Penn.Navigation;

using System.Collections.Immutable;
using Penn.Content;
using Penn.Routing;

public sealed class NavigationBuilder
{
    /// <summary>
    /// Build a navigation tree from flat TOC items.
    /// </summary>
    public ImmutableList<NavigationTreeItem> BuildTree(
        IReadOnlyList<ContentTocItem> items,
        ContentRoute? currentRoute = null)
    {
        return BuildLevel(items, [], currentRoute);
    }

    /// <summary>
    /// Build NavigationInfo for a specific route within the tree.
    /// </summary>
    public NavigationInfo BuildNavigationInfo(
        IReadOnlyList<ContentTocItem> items,
        ContentRoute currentRoute)
    {
        var tree = BuildTree(items, currentRoute);
        var flatList = Flatten(tree);

        var currentIndex = flatList.FindIndex(n => n.Route.CanonicalPath.Matches(currentRoute.CanonicalPath));

        var previous = currentIndex > 0 ? flatList[currentIndex - 1] : null;
        var next = currentIndex >= 0 && currentIndex < flatList.Count - 1 ? flatList[currentIndex + 1] : null;

        var breadcrumbs = BuildBreadcrumbs(tree, currentRoute);
        var currentItem = currentIndex >= 0 ? flatList[currentIndex] : null;

        return new NavigationInfo(
            SectionName: currentItem?.Section,
            SectionRoute: null,
            Breadcrumbs: breadcrumbs,
            PageTitle: currentItem?.Title ?? "",
            PreviousPage: previous,
            NextPage: next
        );
    }

    private ImmutableList<NavigationTreeItem> BuildLevel(
        IReadOnlyList<ContentTocItem> allItems,
        string[] parentParts,
        ContentRoute? currentRoute)
    {
        var depth = parentParts.Length;

        var itemsAtLevel = allItems
            .Where(item => item.HierarchyParts.Length == depth + 1
                        && PartsMatch(item.HierarchyParts, parentParts))
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var builder = ImmutableList.CreateBuilder<NavigationTreeItem>();

        foreach (var item in itemsAtLevel)
        {
            var children = BuildLevel(allItems, item.HierarchyParts, currentRoute);
            var isSelected = currentRoute != null
                && item.Route.CanonicalPath.Matches(currentRoute.CanonicalPath);
            var isExpanded = isSelected || children.Any(c => c.IsSelected || c.IsExpanded);

            builder.Add(new NavigationTreeItem(
                Title: item.Title,
                Route: item.Route,
                Order: item.Order,
                Section: item.Section,
                IsSelected: isSelected,
                IsExpanded: isExpanded,
                Children: children
            ));
        }

        return builder.ToImmutable();
    }

    private static bool PartsMatch(string[] itemParts, string[] parentParts)
    {
        if (itemParts.Length < parentParts.Length) return false;
        for (var i = 0; i < parentParts.Length; i++)
        {
            if (!string.Equals(itemParts[i], parentParts[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Depth-first flattening for prev/next computation.
    /// </summary>
    private static List<NavigationTreeItem> Flatten(ImmutableList<NavigationTreeItem> items)
    {
        var result = new List<NavigationTreeItem>();
        foreach (var item in items)
        {
            result.Add(item);
            result.AddRange(Flatten(item.Children));
        }
        return result;
    }

    /// <summary>
    /// Build breadcrumb trail from root to selected item.
    /// </summary>
    private static ImmutableList<BreadcrumbItem> BuildBreadcrumbs(
        ImmutableList<NavigationTreeItem> tree,
        ContentRoute currentRoute)
    {
        var path = new List<BreadcrumbItem>();
        FindPath(tree, currentRoute, path);
        return [.. path];
    }

    private static bool FindPath(
        ImmutableList<NavigationTreeItem> items,
        ContentRoute target,
        List<BreadcrumbItem> path)
    {
        foreach (var item in items)
        {
            if (item.Route.CanonicalPath.Matches(target.CanonicalPath))
            {
                path.Add(new BreadcrumbItem(item.Title, item.Route));
                return true;
            }
            if (item.IsExpanded)
            {
                path.Add(new BreadcrumbItem(item.Title, item.Route));
                if (FindPath(item.Children, target, path))
                    return true;
                path.RemoveAt(path.Count - 1);
            }
        }
        return false;
    }
}
