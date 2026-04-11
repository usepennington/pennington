namespace Pennington.Navigation;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Routing;

public sealed class NavigationBuilder
{
    /// <summary>
    /// Build a navigation tree from flat TOC items.
    /// When <paramref name="locale"/> is specified, filters to that locale and
    /// strips the locale prefix from hierarchy parts.
    /// </summary>
    public ImmutableList<NavigationTreeItem> BuildTree(
        IReadOnlyList<ContentTocItem> items,
        ContentRoute? currentRoute = null,
        string? locale = null)
    {
        var filtered = FilterByLocale(items, locale);
        return BuildLevel(filtered, [], currentRoute);
    }

    /// <summary>
    /// Build NavigationInfo for a specific route within the tree.
    /// </summary>
    public NavigationInfo BuildNavigationInfo(
        IReadOnlyList<ContentTocItem> items,
        ContentRoute currentRoute,
        string? locale = null)
    {
        var tree = BuildTree(items, currentRoute, locale);
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

        // Find items exactly at this level. DistinctBy(CanonicalPath) is a defensive
        // pass against two content sources registering overlapping subtrees (e.g. a
        // catch-all DocFrontMatter source and a specialized ChangelogFrontMatter
        // source both walking `Content/changelog/`). The pipeline emits a diagnostic
        // warning for that misconfiguration, but dedup here prevents the visible
        // symptom (every sidebar listing each overlapping page twice).
        var itemsAtLevel = allItems
            .Where(item => item.HierarchyParts.Length == depth + 1
                        && PartsMatch(item.HierarchyParts, parentParts))
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .DistinctBy(item => item.Route.CanonicalPath.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Find distinct section names at this depth that have deeper descendants
        // but no direct item (auto-create folder nodes)
        var sectionsWithDescendants = allItems
            .Where(item => item.HierarchyParts.Length > depth + 1
                        && PartsMatch(item.HierarchyParts, parentParts))
            .Select(item => item.HierarchyParts[depth])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(section => !itemsAtLevel.Any(i =>
                string.Equals(i.HierarchyParts[depth], section, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var builder = ImmutableList.CreateBuilder<NavigationTreeItem>();

        // Add auto-created section nodes first
        foreach (var section in sectionsWithDescendants)
        {
            var sectionParts = parentParts.Append(section).ToArray();
            var children = BuildLevel(allItems, sectionParts, currentRoute);
            var isExpanded = children.Any(c => c.IsSelected || c.IsExpanded);

            // Find the minimum order among children to use for section ordering
            var minOrder = children.Any() ? children.Min(c => c.Order) : int.MaxValue;

            // Create a section title from the folder name
            var sectionTitle = FormatSectionTitle(section);

            // Use an empty route for section headers (they're not navigable)
            var sectionRoute = new ContentRoute
            {
                CanonicalPath = new UrlPath(""),
                OutputFile = new FilePath("")
            };

            builder.Add(new NavigationTreeItem(
                Title: sectionTitle,
                Route: sectionRoute,
                Order: minOrder,
                Section: null,
                IsSelected: false,
                IsExpanded: isExpanded,
                Children: children
            ));
        }

        // Add direct items
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

        // Sort all items by order, then title
        return builder.OrderBy(i => i.Order).ThenBy(i => i.Title, StringComparer.OrdinalIgnoreCase).ToImmutableList();
    }

    /// <summary>
    /// Filters TOC items by locale and strips locale prefix from hierarchy parts.
    /// Items with null locale are locale-agnostic and pass all filters.
    /// </summary>
    private static IReadOnlyList<ContentTocItem> FilterByLocale(
        IReadOnlyList<ContentTocItem> items, string? locale)
    {
        if (locale == null) return items;

        return items
            .Where(i => i.Locale == null
                || string.Equals(i.Locale, locale, StringComparison.OrdinalIgnoreCase))
            .Select(i =>
            {
                // Strip locale prefix from hierarchy parts for non-default locales
                if (i.HierarchyParts.Length > 0
                    && string.Equals(i.HierarchyParts[0], locale, StringComparison.OrdinalIgnoreCase)
                    && i.Locale != null)
                {
                    return i with { HierarchyParts = i.HierarchyParts[1..] };
                }
                return i;
            })
            .ToList();
    }

    private static string FormatSectionTitle(string folderName)
    {
        // Convert kebab-case to title case: "getting-started" → "Getting Started"
        return string.Join(' ', folderName.Split('-')
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
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
