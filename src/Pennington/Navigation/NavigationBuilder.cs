namespace Pennington.Navigation;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Content;
using Routing;

/// <summary>Builds hierarchical navigation trees and related navigation metadata from flat TOC entries.</summary>
/// <remarks>
/// Composing service with a per-instance memo cache (<see cref="ConcurrentDictionary{TKey,TValue}"/>
/// keyed by locale + items fingerprint). Registered via <c>AddFileWatched&lt;NavigationBuilder&gt;()</c>
/// so the cache is dropped when content files change; trees rebuilt on next access reflect the
/// fresh TOC.
/// </remarks>
public sealed class NavigationBuilder
{
    private static readonly ContentRoute EmptySectionRoute = new()
    {
        CanonicalPath = new UrlPath(""),
        OutputFile = new FilePath("")
    };

    // Cache of structural (unselected) trees keyed by (locale, fingerprint of input items).
    // Populated lazily per unique (items, locale) combination; cleared when the enclosing
    // FileWatchDependencyFactory drops this instance on a file-change signal.
    private readonly ConcurrentDictionary<CacheKey, ImmutableList<NavigationTreeItem>> _structuralCache = new();

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
        var structural = GetOrBuildStructural(items, locale);
        return currentRoute is null
            ? structural
            : StampSelection(structural, currentRoute);
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
            SectionName: currentItem?.SectionLabel,
            SectionRoute: null,
            Breadcrumbs: breadcrumbs,
            PageTitle: currentItem?.Title ?? "",
            PreviousPage: previous,
            NextPage: next
        );
    }

    private ImmutableList<NavigationTreeItem> GetOrBuildStructural(
        IReadOnlyList<ContentTocItem> items, string? locale)
    {
        var key = ComputeCacheKey(items, locale);
        return _structuralCache.GetOrAdd(key, _ =>
        {
            // SearchOnly entries are indexed for search/llms but excluded from the
            // rendered navigation tree. Filter here so the structural cache holds
            // only the items that will appear in the sidebar.
            var localised = FilterByLocale(items, locale);
            var filtered = localised.Where(i => !i.SearchOnly).ToList();
            return BuildLevel(filtered, depth: 0, isRoot: true);
        });
    }

    private static ImmutableList<NavigationTreeItem> BuildLevel(
        IReadOnlyList<ContentTocItem> items, int depth, bool isRoot)
    {
        // Partition the incoming slice in a single pass:
        //  - overview: only at the root, items with no hierarchy parts (area index)
        //  - atLevel:  items whose depth in the tree matches this level
        //  - deeper:   items nested below this level, grouped later by their next segment
        List<ContentTocItem>? atLevel = null;
        List<ContentTocItem>? deeper = null;
        ContentTocItem? overview = null;

        foreach (var item in items)
        {
            var len = item.HierarchyParts.Length;
            if (isRoot && len == 0)
            {
                overview ??= item;
            }
            else if (len == depth + 1)
            {
                (atLevel ??= []).Add(item);
            }
            else if (len > depth + 1)
            {
                (deeper ??= []).Add(item);
            }
        }

        // Sort + dedup direct items. DistinctBy(CanonicalPath) is a defensive pass
        // against two content sources registering overlapping subtrees (e.g. a
        // catch-all DocFrontMatter source and a specialized ChangelogFrontMatter
        // source both walking `Content/changelog/`). The pipeline emits a
        // diagnostic warning for that misconfiguration, but dedup here prevents
        // the visible symptom (every sidebar listing each overlapping page twice).
        var atLevelSorted = atLevel is null
            ? (IReadOnlyList<ContentTocItem>)Array.Empty<ContentTocItem>()
            : atLevel
                .OrderBy(i => i.Order)
                .ThenBy(i => i.Title, StringComparer.OrdinalIgnoreCase)
                .DistinctBy(i => i.Route.CanonicalPath.Value, StringComparer.OrdinalIgnoreCase)
                .ToList();

        // Group deeper items by their segment at this depth so recursion only
        // touches the items belonging to that subtree instead of rescanning the
        // full list.
        ILookup<string, ContentTocItem>? deeperByKey = deeper is null
            ? null
            : deeper.ToLookup(i => i.HierarchyParts[depth], StringComparer.OrdinalIgnoreCase);

        var directNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in atLevelSorted)
            directNames.Add(item.HierarchyParts[depth]);

        var builder = ImmutableList.CreateBuilder<NavigationTreeItem>();

        // At the tree root, an item with empty HierarchyParts represents the
        // "overview" page for the tree — typically an area's index.md whose
        // hierarchy was stripped by GetTocItemsForAreaAsync. Include it so the
        // landing page is reachable from the sidebar, not only from the area
        // tab in the top nav. Order=int.MinValue pins it to the top.
        if (overview is not null)
        {
            builder.Add(new NavigationTreeItem(
                Title: overview.Title,
                Route: overview.Route,
                Order: int.MinValue,
                SectionLabel: overview.SectionLabel,
                IsSelected: false,
                IsExpanded: false,
                Children: []
            ));
        }

        // Auto-created section nodes for subtrees without a direct landing item
        if (deeperByKey is not null)
        {
            foreach (var group in deeperByKey)
            {
                if (directNames.Contains(group.Key)) continue;

                var children = BuildLevel(group.ToList(), depth + 1, isRoot: false);
                var minOrder = children.Count > 0 ? children.Min(c => c.Order) : int.MaxValue;

                builder.Add(new NavigationTreeItem(
                    Title: FormatSectionTitle(group.Key),
                    Route: EmptySectionRoute,
                    Order: minOrder,
                    SectionLabel: null,
                    IsSelected: false,
                    IsExpanded: false,
                    Children: children
                ));
            }
        }

        // Direct items and their descendants
        foreach (var item in atLevelSorted)
        {
            IReadOnlyList<ContentTocItem> childItems = deeperByKey is not null
                && deeperByKey.Contains(item.HierarchyParts[depth])
                ? deeperByKey[item.HierarchyParts[depth]].ToList()
                : Array.Empty<ContentTocItem>();
            var children = BuildLevel(childItems, depth + 1, isRoot: false);

            builder.Add(new NavigationTreeItem(
                Title: item.Title,
                Route: item.Route,
                Order: item.Order,
                SectionLabel: item.SectionLabel,
                IsSelected: false,
                IsExpanded: false,
                Children: children
            ));
        }

        return builder
            .OrderBy(i => i.Order)
            .ThenBy(i => i.Title, StringComparer.OrdinalIgnoreCase)
            .ToImmutableList();
    }

    /// <summary>
    /// Walks the cached structural tree and produces a tree with IsSelected/IsExpanded
    /// stamped along the path to <paramref name="currentRoute"/>. Subtrees that don't
    /// contain the selection are returned by reference — the cache is shared across
    /// page renders, so only the selected ancestor chain allocates new records.
    /// </summary>
    private static ImmutableList<NavigationTreeItem> StampSelection(
        ImmutableList<NavigationTreeItem> tree, ContentRoute currentRoute)
    {
        var builder = ImmutableList.CreateBuilder<NavigationTreeItem>();
        var anyChanged = false;
        foreach (var node in tree)
        {
            var stamped = StampNode(node, currentRoute);
            if (!ReferenceEquals(node, stamped)) anyChanged = true;
            builder.Add(stamped);
        }
        return anyChanged ? builder.ToImmutable() : tree;
    }

    private static NavigationTreeItem StampNode(NavigationTreeItem node, ContentRoute currentRoute)
    {
        var isSelected = node.Route.CanonicalPath.Matches(currentRoute.CanonicalPath);

        if (node.Children.Count == 0)
        {
            return isSelected
                ? node with { IsSelected = true, IsExpanded = false }
                : node;
        }

        ImmutableList<NavigationTreeItem>.Builder? childBuilder = null;
        var anyChildExpanded = false;
        for (var i = 0; i < node.Children.Count; i++)
        {
            var original = node.Children[i];
            var stamped = StampNode(original, currentRoute);
            if (!ReferenceEquals(original, stamped))
            {
                if (childBuilder is null)
                {
                    childBuilder = ImmutableList.CreateBuilder<NavigationTreeItem>();
                    for (var j = 0; j < i; j++) childBuilder.Add(node.Children[j]);
                }
                childBuilder.Add(stamped);
            }
            else
            {
                childBuilder?.Add(stamped);
            }
            if (stamped.IsSelected || stamped.IsExpanded) anyChildExpanded = true;
        }

        var isExpanded = isSelected || anyChildExpanded;

        if (!isSelected && !isExpanded && childBuilder is null)
            return node;

        return node with
        {
            IsSelected = isSelected,
            IsExpanded = isExpanded,
            Children = childBuilder?.ToImmutable() ?? node.Children
        };
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
        return string.Join(' ', folderName.Split('-')
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
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

    private static CacheKey ComputeCacheKey(IReadOnlyList<ContentTocItem> items, string? locale)
    {
        var hash = new HashCode();
        foreach (var item in items)
        {
            hash.Add(item.Title);
            hash.Add(item.Route.CanonicalPath.Value, StringComparer.OrdinalIgnoreCase);
            hash.Add(item.Order);
            hash.Add(item.SectionLabel);
            hash.Add(item.Locale);
            hash.Add(item.SearchOnly);
            hash.Add(item.HierarchyParts.Length);
            foreach (var part in item.HierarchyParts)
                hash.Add(part, StringComparer.OrdinalIgnoreCase);
        }
        return new CacheKey(locale, items.Count, hash.ToHashCode());
    }

    private readonly record struct CacheKey(string? Locale, int Count, int Fingerprint);
}
