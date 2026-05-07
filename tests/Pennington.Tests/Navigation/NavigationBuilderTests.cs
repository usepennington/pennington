using Pennington.Content;
using Pennington.Navigation;
using Pennington.Routing;

namespace Pennington.Tests.Navigation;

public class NavigationBuilderTests
{
    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static ContentTocItem MakeTocItem(string title, string path, int order, params string[] hierarchy) => new(
        Title: title,
        Route: MakeRoute(path),
        Order: order,
        HierarchyParts: hierarchy,
        SectionLabel: null,
        Locale: null
    );

    private readonly NavigationBuilder _builder = new();

    [Fact]
    public void BuildTree_AreaIndexWithEmptyHierarchy_AppearsFirstAsOverview()
    {
        // Simulates GetTocItemsForAreaAsync for area="guides":
        // guides/index.md has HierarchyParts stripped to [], while peers retain
        // their depth-1 paths. The overview entry must appear first in the sidebar
        // regardless of its Order value or child Order values.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Guides Overview", "/guides/", 0),
            MakeTocItem("Installation", "/guides/installation", 10, "installation"),
            MakeTocItem("First Project", "/guides/first-project", 20, "first-project"),
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(3);
        tree[0].Title.ShouldBe("Guides Overview");
        tree[0].Route.CanonicalPath.Value.ShouldBe("/guides/");
        tree[1].Title.ShouldBe("Installation");
        tree[2].Title.ShouldBe("First Project");
    }

    [Fact]
    public void BuildTree_AreaIndexSelected_MarksOverviewAsCurrent()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Guides Overview", "/guides/", 0),
            MakeTocItem("Installation", "/guides/installation", 10, "installation"),
        };

        var tree = _builder.BuildTree(items, MakeRoute("/guides/"));

        tree[0].IsSelected.ShouldBeTrue();
        tree[1].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public void BuildTree_NoAreaIndex_OmitsOverview()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Installation", "/guides/installation", 10, "installation"),
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Installation");
    }

    [Fact]
    public void BuildTree_SimpleFlatList_ReturnsNoNesting()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Alpha", "/alpha", 1, "Alpha"),
            MakeTocItem("Beta", "/beta", 2, "Beta"),
            MakeTocItem("Gamma", "/gamma", 3, "Gamma"),
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(3);
        tree[0].Title.ShouldBe("Alpha");
        tree[1].Title.ShouldBe("Beta");
        tree[2].Title.ShouldBe("Gamma");
        tree.ShouldAllBe(n => n.Children.Count == 0);
    }

    [Fact]
    public void BuildTree_NestedHierarchy_CreatesParentChildRelationships()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Auth", "/docs/api/auth", 1, "Docs", "API", "Auth"),
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Docs");
        tree[0].Children.Count.ShouldBe(1);
        tree[0].Children[0].Title.ShouldBe("API");
        tree[0].Children[0].Children.Count.ShouldBe(1);
        tree[0].Children[0].Children[0].Title.ShouldBe("Auth");
        tree[0].Children[0].Children[0].Children.ShouldBeEmpty();
    }

    [Fact]
    public void BuildTree_Ordering_SortsByOrderThenTitle()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Zebra", "/zebra", 1, "Zebra"),
            MakeTocItem("Apple", "/apple", 1, "Apple"),
            MakeTocItem("First", "/first", 0, "First"),
            MakeTocItem("Middle", "/middle", 1, "Middle"),
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(4);
        tree[0].Title.ShouldBe("First");
        tree[1].Title.ShouldBe("Apple");
        tree[2].Title.ShouldBe("Middle");
        tree[3].Title.ShouldBe("Zebra");
    }

    [Fact]
    public void BuildTree_SelectionState_CorrectItemIsSelectedAndAncestorsExpanded()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Auth", "/docs/api/auth", 1, "Docs", "API", "Auth"),
            MakeTocItem("Guide", "/guide", 2, "Guide"),
        };

        var currentRoute = MakeRoute("/docs/api/auth");
        var tree = _builder.BuildTree(items, currentRoute);

        // Auth should be selected
        var auth = tree[0].Children[0].Children[0];
        auth.IsSelected.ShouldBeTrue();

        // API and Docs should be expanded (ancestors)
        tree[0].IsExpanded.ShouldBeTrue();
        tree[0].IsSelected.ShouldBeFalse();
        tree[0].Children[0].IsExpanded.ShouldBeTrue();
        tree[0].Children[0].IsSelected.ShouldBeFalse();

        // Guide is neither selected nor expanded
        tree[1].IsSelected.ShouldBeFalse();
        tree[1].IsExpanded.ShouldBeFalse();
    }

    [Fact]
    public void BuildTree_PrevNextNavigation_ReturnsCorrectNeighbors()
    {
        // Tree: A, B -> [B1, B2], C
        var items = new List<ContentTocItem>
        {
            MakeTocItem("A", "/a", 1, "A"),
            MakeTocItem("B", "/b", 2, "B"),
            MakeTocItem("B1", "/b/b1", 1, "B", "B1"),
            MakeTocItem("B2", "/b/b2", 2, "B", "B2"),
            MakeTocItem("C", "/c", 3, "C"),
        };

        var currentRoute = MakeRoute("/b/b1");
        var info = _builder.BuildNavigationInfo(items, currentRoute);

        // B1 is current, prev=B, next=B2
        info.PageTitle.ShouldBe("B1");
        info.PreviousPage.ShouldNotBeNull();
        info.PreviousPage!.Title.ShouldBe("B");
        info.NextPage.ShouldNotBeNull();
        info.NextPage!.Title.ShouldBe("B2");
    }

    [Fact]
    public void BuildTree_Breadcrumbs_ShowsPathFromRootToSelected()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Auth", "/docs/api/auth", 1, "Docs", "API", "Auth"),
        };

        var currentRoute = MakeRoute("/docs/api/auth");
        var info = _builder.BuildNavigationInfo(items, currentRoute);

        info.Breadcrumbs.Count.ShouldBe(3);
        info.Breadcrumbs[0].Title.ShouldBe("Docs");
        info.Breadcrumbs[1].Title.ShouldBe("API");
        info.Breadcrumbs[2].Title.ShouldBe("Auth");
    }

    [Fact]
    public void BuildTree_NoCurrentRoute_NothingSelectedOrExpanded()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
        };

        var tree = _builder.BuildTree(items);

        tree[0].IsSelected.ShouldBeFalse();
        tree[0].IsExpanded.ShouldBeFalse();
        tree[0].Children[0].IsSelected.ShouldBeFalse();
        tree[0].Children[0].IsExpanded.ShouldBeFalse();
    }

    [Fact]
    public void BuildNavigationInfo_ReturnsCorrectPageTitlePrevNextAndBreadcrumbs()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Home", "/", 0, "Home"),
            MakeTocItem("About", "/about", 1, "About"),
            MakeTocItem("Contact", "/contact", 2, "Contact"),
        };

        var currentRoute = MakeRoute("/about");
        var info = _builder.BuildNavigationInfo(items, currentRoute);

        info.PageTitle.ShouldBe("About");
        info.PreviousPage.ShouldNotBeNull();
        info.PreviousPage!.Title.ShouldBe("Home");
        info.NextPage.ShouldNotBeNull();
        info.NextPage!.Title.ShouldBe("Contact");
        info.Breadcrumbs.Count.ShouldBe(1);
        info.Breadcrumbs[0].Title.ShouldBe("About");
    }

    [Fact]
    public void BuildNavigationInfo_FirstItem_HasNoPrevious()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("First", "/first", 1, "First"),
            MakeTocItem("Second", "/second", 2, "Second"),
        };

        var info = _builder.BuildNavigationInfo(items, MakeRoute("/first"));

        info.PreviousPage.ShouldBeNull();
        info.NextPage.ShouldNotBeNull();
        info.NextPage!.Title.ShouldBe("Second");
    }

    [Fact]
    public void BuildNavigationInfo_LastItem_HasNoNext()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("First", "/first", 1, "First"),
            MakeTocItem("Second", "/second", 2, "Second"),
        };

        var info = _builder.BuildNavigationInfo(items, MakeRoute("/second"));

        info.PreviousPage.ShouldNotBeNull();
        info.PreviousPage!.Title.ShouldBe("First");
        info.NextPage.ShouldBeNull();
    }

    [Fact]
    public void BuildTree_ChildrenOrderedWithinParent()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Parent", "/parent", 1, "Parent"),
            MakeTocItem("Zulu", "/parent/zulu", 2, "Parent", "Zulu"),
            MakeTocItem("Alpha", "/parent/alpha", 1, "Parent", "Alpha"),
            MakeTocItem("Bravo", "/parent/bravo", 1, "Parent", "Bravo"),
        };

        var tree = _builder.BuildTree(items);

        tree[0].Children.Count.ShouldBe(3);
        tree[0].Children[0].Title.ShouldBe("Alpha");
        tree[0].Children[1].Title.ShouldBe("Bravo");
        tree[0].Children[2].Title.ShouldBe("Zulu");
    }

    // --- Locale filtering tests ---

    private static ContentTocItem MakeLocaleTocItem(string title, string path, int order, string? locale, params string[] hierarchy) => new(
        Title: title,
        Route: new ContentRoute
        {
            CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
            OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
            Locale = locale ?? ""
        },
        Order: order,
        HierarchyParts: hierarchy,
        SectionLabel: null,
        Locale: locale
    );

    [Fact]
    public void BuildTree_WithLocale_FiltersToMatchingLocale()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide EN", "/guide", 1, "en", "guide"),
            MakeLocaleTocItem("Guide FR", "/fr/guide", 1, "fr", "fr", "guide"),
        };

        var tree = _builder.BuildTree(items, locale: "en");
        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Guide EN");
    }

    [Fact]
    public void BuildTree_WithLocale_StripsLocalePrefixFromHierarchy()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide FR", "/fr/guide", 1, "fr", "fr", "guide"),
        };

        var tree = _builder.BuildTree(items, locale: "fr");
        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Guide FR");
        // Hierarchy was ["fr", "guide"], after stripping it's ["guide"] — renders as top-level
    }

    [Fact]
    public void BuildTree_WithLocale_NullLocaleItemsPassFilter()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide EN", "/guide", 1, "en", "guide"),
            MakeTocItem("About (agnostic)", "/about", 2, "about"), // locale = null
        };

        var tree = _builder.BuildTree(items, locale: "en");
        tree.Count.ShouldBe(2);
    }

    [Fact]
    public void BuildTree_NullLocale_IncludesAll()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide EN", "/guide", 1, "en", "guide"),
            MakeLocaleTocItem("Guide FR", "/fr/guide", 1, "fr", "fr", "guide"),
        };

        var tree = _builder.BuildTree(items, locale: null);
        tree.Count.ShouldBe(2);
    }

    [Fact]
    public void BuildTree_DuplicateCanonicalPaths_DedupedDefensively()
    {
        // Shape of the NorthwindHandbookExample misconfiguration: two content
        // sources both register a TOC entry for /changelog/v2-0-0/. The engine
        // should warn via MarkdownSourceOverlapDetector, but NavigationBuilder
        // also collapses the duplicate here so the sidebar isn't double-listed.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Changelog v2.0.0 (generic)", "/changelog/v2-0-0", 1, "changelog", "v2-0-0"),
            MakeTocItem("Changelog v2.0.0 (specialized)", "/changelog/v2-0-0", 1, "changelog", "v2-0-0"),
            MakeTocItem("Changelog v2.0.1", "/changelog/v2-0-1", 2, "changelog", "v2-0-1"),
        };

        var tree = _builder.BuildTree(items);

        // One auto-created "Changelog" section with two children (v2-0-0, v2-0-1),
        // not three.
        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Changelog");
        tree[0].Children.Count.ShouldBe(2);
        tree[0].Children.Select(c => c.Route.CanonicalPath.Value)
            .ShouldBe(["/changelog/v2-0-0/", "/changelog/v2-0-1/"], ignoreOrder: true);
    }

    [Fact]
    public void BuildTree_RepeatedCalls_ReuseStructuralSubtrees()
    {
        // Structural tree is cached per (locale, input fingerprint). Branches
        // off the selection path must be returned by reference — otherwise
        // every page render reallocates the full tree.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Guide", "/guide", 2, "Guide"),
            MakeTocItem("Guide Setup", "/guide/setup", 1, "Guide", "Setup"),
        };

        // Two renders against the same current route — Guide subtree has no
        // selection under it, so it reuses the cached structural node.
        var a = _builder.BuildTree(items, MakeRoute("/docs/api"));
        var b = _builder.BuildTree(items, MakeRoute("/docs/api"));
        ReferenceEquals(a[1], b[1]).ShouldBeTrue();
        ReferenceEquals(a[1].Children[0], b[1].Children[0]).ShouldBeTrue();

        // Two renders against different current routes — the Docs branch is
        // still structurally shared in the "no selection here" case.
        var onGuide = _builder.BuildTree(items, MakeRoute("/guide/setup"));
        var onApi = _builder.BuildTree(items, MakeRoute("/docs/api"));
        ReferenceEquals(onGuide[0], onApi[0]).ShouldBeFalse(); // Docs differs — API is selected under it in onApi
        // But the unselected leaf under Guide in onApi is the same structural
        // reference as in the no-route render.
        var bare = _builder.BuildTree(items);
        ReferenceEquals(onApi[1].Children[0], bare[1].Children[0]).ShouldBeTrue();
    }

    [Fact]
    public void BuildTree_SelectionChanges_TrackedAcrossCalls()
    {
        // Confirms the cached structural tree correctly re-stamps selection
        // when currentRoute changes between calls.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Auth", "/docs/api/auth", 1, "Docs", "API", "Auth"),
            MakeTocItem("Guide", "/guide", 2, "Guide"),
        };

        var onAuth = _builder.BuildTree(items, MakeRoute("/docs/api/auth"));
        onAuth[0].Children[0].Children[0].IsSelected.ShouldBeTrue();
        onAuth[1].IsSelected.ShouldBeFalse();

        var onGuide = _builder.BuildTree(items, MakeRoute("/guide"));
        onGuide[0].Children[0].Children[0].IsSelected.ShouldBeFalse();
        onGuide[0].IsExpanded.ShouldBeFalse();
        onGuide[1].IsSelected.ShouldBeTrue();

        // Back to no selection → whole tree collapses.
        var bare = _builder.BuildTree(items);
        bare[0].IsExpanded.ShouldBeFalse();
        bare[0].Children[0].Children[0].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public void BuildTree_DuplicateCanonicalPaths_CaseInsensitive_Deduped()
    {
        // Canonical paths are compared case-insensitively — the second entry
        // (capitalized `/Alpha`) should still dedup against the first.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("alpha lower", "/alpha", 1, "alpha"),
            MakeTocItem("alpha upper", "/Alpha", 2, "Alpha"),
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(1);
    }

    [Fact]
    public void BuildTree_SearchOnlyItem_FilteredFromTree()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Visible", "/guides/visible", 10, "visible"),
            MakeTocItem("Hidden", "/guides/hidden", 20, "hidden") with { SearchOnly = true },
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Visible");
    }

    [Fact]
    public void BuildTree_SearchOnlyItem_DoesNotCreateAutoSection()
    {
        // A SearchOnly item with hierarchy "guides/faq/q1" should not cause a synthetic
        // "faq" auto-section node to appear when no other guides/faq/* items exist.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Installation", "/guides/installation", 10, "installation"),
            MakeTocItem("Hidden FAQ", "/guides/faq/q1", 20, "faq", "q1") with { SearchOnly = true },
        };

        var tree = _builder.BuildTree(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Installation");
    }

    [Fact]
    public void BuildTree_AllSearchOnly_ProducesEmptyTree()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("A", "/a", 10, "a") with { SearchOnly = true },
            MakeTocItem("B", "/b", 20, "b") with { SearchOnly = true },
        };

        var tree = _builder.BuildTree(items);

        tree.ShouldBeEmpty();
    }

    [Fact]
    public void BuildTree_SearchOnlyChangeBetweenCalls_DoesNotReturnStaleCache()
    {
        // Cache key includes SearchOnly, so toggling the flag invalidates the cached tree.
        var visible = MakeTocItem("Topic", "/topic", 10, "topic");
        var hidden = visible with { SearchOnly = true };

        var first = _builder.BuildTree([visible]);
        var second = _builder.BuildTree([hidden]);

        first.Count.ShouldBe(1);
        second.ShouldBeEmpty();
    }
}