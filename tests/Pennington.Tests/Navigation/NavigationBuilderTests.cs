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

    private readonly NavigationBuilder _builder = new(new FolderMetadataRegistry(Array.Empty<FolderMetadata>()));

    [Fact]
    public async Task BuildTree_AreaIndexWithEmptyHierarchy_AppearsFirstAsOverview()
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

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(3);
        tree[0].Title.ShouldBe("Guides Overview");
        tree[0].Route.CanonicalPath.Value.ShouldBe("/guides/");
        tree[1].Title.ShouldBe("Installation");
        tree[2].Title.ShouldBe("First Project");
    }

    [Fact]
    public async Task BuildTree_AreaIndexSelected_MarksOverviewAsCurrent()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Guides Overview", "/guides/", 0),
            MakeTocItem("Installation", "/guides/installation", 10, "installation"),
        };

        var tree = await _builder.BuildTreeAsync(items, MakeRoute("/guides/"));

        tree[0].IsSelected.ShouldBeTrue();
        tree[1].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public async Task BuildTree_NoAreaIndex_OmitsOverview()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Installation", "/guides/installation", 10, "installation"),
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Installation");
    }

    [Fact]
    public async Task BuildTree_SimpleFlatList_ReturnsNoNesting()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Alpha", "/alpha", 1, "Alpha"),
            MakeTocItem("Beta", "/beta", 2, "Beta"),
            MakeTocItem("Gamma", "/gamma", 3, "Gamma"),
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(3);
        tree[0].Title.ShouldBe("Alpha");
        tree[1].Title.ShouldBe("Beta");
        tree[2].Title.ShouldBe("Gamma");
        tree.ShouldAllBe(n => n.Children.Count == 0);
    }

    [Fact]
    public async Task BuildTree_NestedHierarchy_CreatesParentChildRelationships()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Auth", "/docs/api/auth", 1, "Docs", "API", "Auth"),
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Docs");
        tree[0].Children.Count.ShouldBe(1);
        tree[0].Children[0].Title.ShouldBe("API");
        tree[0].Children[0].Children.Count.ShouldBe(1);
        tree[0].Children[0].Children[0].Title.ShouldBe("Auth");
        tree[0].Children[0].Children[0].Children.ShouldBeEmpty();
    }

    [Fact]
    public async Task BuildTree_Ordering_SortsByOrderThenTitle()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Zebra", "/zebra", 1, "Zebra"),
            MakeTocItem("Apple", "/apple", 1, "Apple"),
            MakeTocItem("First", "/first", 0, "First"),
            MakeTocItem("Middle", "/middle", 1, "Middle"),
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(4);
        tree[0].Title.ShouldBe("First");
        tree[1].Title.ShouldBe("Apple");
        tree[2].Title.ShouldBe("Middle");
        tree[3].Title.ShouldBe("Zebra");
    }

    [Fact]
    public async Task BuildTree_SelectionState_CorrectItemIsSelectedAndAncestorsExpanded()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Auth", "/docs/api/auth", 1, "Docs", "API", "Auth"),
            MakeTocItem("Guide", "/guide", 2, "Guide"),
        };

        var currentRoute = MakeRoute("/docs/api/auth");
        var tree = await _builder.BuildTreeAsync(items, currentRoute);

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
    public async Task BuildTree_PrevNextNavigation_ReturnsCorrectNeighbors()
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
        var info = await _builder.BuildNavigationInfoAsync(items, currentRoute);

        // B1 is current, prev=B, next=B2
        info.PageTitle.ShouldBe("B1");
        info.PreviousPage.ShouldNotBeNull();
        info.PreviousPage!.Title.ShouldBe("B");
        info.NextPage.ShouldNotBeNull();
        info.NextPage!.Title.ShouldBe("B2");
    }

    [Fact]
    public async Task BuildTree_Breadcrumbs_ShowsPathFromRootToSelected()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
            MakeTocItem("Auth", "/docs/api/auth", 1, "Docs", "API", "Auth"),
        };

        var currentRoute = MakeRoute("/docs/api/auth");
        var info = await _builder.BuildNavigationInfoAsync(items, currentRoute);

        info.Breadcrumbs.Count.ShouldBe(3);
        info.Breadcrumbs[0].Title.ShouldBe("Docs");
        info.Breadcrumbs[1].Title.ShouldBe("API");
        info.Breadcrumbs[2].Title.ShouldBe("Auth");
    }

    [Fact]
    public async Task BuildTree_NoCurrentRoute_NothingSelectedOrExpanded()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs", "/docs", 1, "Docs"),
            MakeTocItem("API", "/docs/api", 1, "Docs", "API"),
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree[0].IsSelected.ShouldBeFalse();
        tree[0].IsExpanded.ShouldBeFalse();
        tree[0].Children[0].IsSelected.ShouldBeFalse();
        tree[0].Children[0].IsExpanded.ShouldBeFalse();
    }

    [Fact]
    public async Task BuildNavigationInfo_ReturnsCorrectPageTitlePrevNextAndBreadcrumbs()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Home", "/", 0, "Home"),
            MakeTocItem("About", "/about", 1, "About"),
            MakeTocItem("Contact", "/contact", 2, "Contact"),
        };

        var currentRoute = MakeRoute("/about");
        var info = await _builder.BuildNavigationInfoAsync(items, currentRoute);

        info.PageTitle.ShouldBe("About");
        info.PreviousPage.ShouldNotBeNull();
        info.PreviousPage!.Title.ShouldBe("Home");
        info.NextPage.ShouldNotBeNull();
        info.NextPage!.Title.ShouldBe("Contact");
        info.Breadcrumbs.Count.ShouldBe(1);
        info.Breadcrumbs[0].Title.ShouldBe("About");
    }

    [Fact]
    public async Task BuildNavigationInfo_FirstItem_HasNoPrevious()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("First", "/first", 1, "First"),
            MakeTocItem("Second", "/second", 2, "Second"),
        };

        var info = await _builder.BuildNavigationInfoAsync(items, MakeRoute("/first"));

        info.PreviousPage.ShouldBeNull();
        info.NextPage.ShouldNotBeNull();
        info.NextPage!.Title.ShouldBe("Second");
    }

    [Fact]
    public async Task BuildNavigationInfo_LastItem_HasNoNext()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("First", "/first", 1, "First"),
            MakeTocItem("Second", "/second", 2, "Second"),
        };

        var info = await _builder.BuildNavigationInfoAsync(items, MakeRoute("/second"));

        info.PreviousPage.ShouldNotBeNull();
        info.PreviousPage!.Title.ShouldBe("First");
        info.NextPage.ShouldBeNull();
    }

    [Fact]
    public async Task BuildTree_ChildrenOrderedWithinParent()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Parent", "/parent", 1, "Parent"),
            MakeTocItem("Zulu", "/parent/zulu", 2, "Parent", "Zulu"),
            MakeTocItem("Alpha", "/parent/alpha", 1, "Parent", "Alpha"),
            MakeTocItem("Bravo", "/parent/bravo", 1, "Parent", "Bravo"),
        };

        var tree = await _builder.BuildTreeAsync(items);

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
    public async Task BuildTree_WithLocale_FiltersToMatchingLocale()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide EN", "/guide", 1, "en", "guide"),
            MakeLocaleTocItem("Guide FR", "/fr/guide", 1, "fr", "fr", "guide"),
        };

        var tree = await _builder.BuildTreeAsync(items, locale: "en");
        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Guide EN");
    }

    [Fact]
    public async Task BuildTree_WithLocale_StripsLocalePrefixFromHierarchy()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide FR", "/fr/guide", 1, "fr", "fr", "guide"),
        };

        var tree = await _builder.BuildTreeAsync(items, locale: "fr");
        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Guide FR");
        // Hierarchy was ["fr", "guide"], after stripping it's ["guide"] — renders as top-level
    }

    [Fact]
    public async Task BuildTree_WithLocale_NullLocaleItemsPassFilter()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide EN", "/guide", 1, "en", "guide"),
            MakeTocItem("About (agnostic)", "/about", 2, "about"), // locale = null
        };

        var tree = await _builder.BuildTreeAsync(items, locale: "en");
        tree.Count.ShouldBe(2);
    }

    [Fact]
    public async Task BuildTree_NullLocale_IncludesAll()
    {
        var items = new List<ContentTocItem>
        {
            MakeLocaleTocItem("Guide EN", "/guide", 1, "en", "guide"),
            MakeLocaleTocItem("Guide FR", "/fr/guide", 1, "fr", "fr", "guide"),
        };

        var tree = await _builder.BuildTreeAsync(items, locale: null);
        tree.Count.ShouldBe(2);
    }

    [Fact]
    public async Task BuildTree_DuplicateCanonicalPaths_DedupedDefensively()
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

        var tree = await _builder.BuildTreeAsync(items);

        // One auto-created "Changelog" section with two children (v2-0-0, v2-0-1),
        // not three.
        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Changelog");
        tree[0].Children.Count.ShouldBe(2);
        tree[0].Children.Select(c => c.Route.CanonicalPath.Value)
            .ShouldBe(["/changelog/v2-0-0/", "/changelog/v2-0-1/"], ignoreOrder: true);
    }

    [Fact]
    public async Task BuildTree_RepeatedCalls_ReuseStructuralSubtrees()
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
        var a = await _builder.BuildTreeAsync(items, MakeRoute("/docs/api"));
        var b = await _builder.BuildTreeAsync(items, MakeRoute("/docs/api"));
        ReferenceEquals(a[1], b[1]).ShouldBeTrue();
        ReferenceEquals(a[1].Children[0], b[1].Children[0]).ShouldBeTrue();

        // Two renders against different current routes — the Docs branch is
        // still structurally shared in the "no selection here" case.
        var onGuide = await _builder.BuildTreeAsync(items, MakeRoute("/guide/setup"));
        var onApi = await _builder.BuildTreeAsync(items, MakeRoute("/docs/api"));
        ReferenceEquals(onGuide[0], onApi[0]).ShouldBeFalse(); // Docs differs — API is selected under it in onApi
        // But the unselected leaf under Guide in onApi is the same structural
        // reference as in the no-route render.
        var bare = await _builder.BuildTreeAsync(items);
        ReferenceEquals(onApi[1].Children[0], bare[1].Children[0]).ShouldBeTrue();
    }

    [Fact]
    public async Task BuildTree_SelectionChanges_TrackedAcrossCalls()
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

        var onAuth = await _builder.BuildTreeAsync(items, MakeRoute("/docs/api/auth"));
        onAuth[0].Children[0].Children[0].IsSelected.ShouldBeTrue();
        onAuth[1].IsSelected.ShouldBeFalse();

        var onGuide = await _builder.BuildTreeAsync(items, MakeRoute("/guide"));
        onGuide[0].Children[0].Children[0].IsSelected.ShouldBeFalse();
        onGuide[0].IsExpanded.ShouldBeFalse();
        onGuide[1].IsSelected.ShouldBeTrue();

        // Back to no selection → whole tree collapses.
        var bare = await _builder.BuildTreeAsync(items);
        bare[0].IsExpanded.ShouldBeFalse();
        bare[0].Children[0].Children[0].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public async Task BuildTree_DuplicateCanonicalPaths_CaseInsensitive_Deduped()
    {
        // Canonical paths are compared case-insensitively — the second entry
        // (capitalized `/Alpha`) should still dedup against the first.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("alpha lower", "/alpha", 1, "alpha"),
            MakeTocItem("alpha upper", "/Alpha", 2, "Alpha"),
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(1);
    }

    [Fact]
    public async Task BuildTree_SearchOnlyItem_FilteredFromTree()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Visible", "/guides/visible", 10, "visible"),
            MakeTocItem("Hidden", "/guides/hidden", 20, "hidden") with { SearchOnly = true },
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Visible");
    }

    [Fact]
    public async Task BuildTree_SearchOnlyItem_DoesNotCreateAutoSection()
    {
        // A SearchOnly item with hierarchy "guides/faq/q1" should not cause a synthetic
        // "faq" auto-section node to appear when no other guides/faq/* items exist.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Installation", "/guides/installation", 10, "installation"),
            MakeTocItem("Hidden FAQ", "/guides/faq/q1", 20, "faq", "q1") with { SearchOnly = true },
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Installation");
    }

    [Fact]
    public async Task BuildTree_AllSearchOnly_ProducesEmptyTree()
    {
        var items = new List<ContentTocItem>
        {
            MakeTocItem("A", "/a", 10, "a") with { SearchOnly = true },
            MakeTocItem("B", "/b", 20, "b") with { SearchOnly = true },
        };

        var tree = await _builder.BuildTreeAsync(items);

        tree.ShouldBeEmpty();
    }

    [Fact]
    public async Task BuildTree_SearchOnlyChangeBetweenCalls_DoesNotReturnStaleCache()
    {
        // Cache key includes SearchOnly, so toggling the flag invalidates the cached tree.
        var visible = MakeTocItem("Topic", "/topic", 10, "topic");
        var hidden = visible with { SearchOnly = true };

        var first = await _builder.BuildTreeAsync([visible]);
        var second = await _builder.BuildTreeAsync([hidden]);

        first.Count.ShouldBe(1);
        second.ShouldBeEmpty();
    }

    // ----------------------------------------------------------------------
    // _meta.yml folder-metadata overrides
    // ----------------------------------------------------------------------

    [Fact]
    public async Task BuildTree_FolderSidecarOrder_PositionsAutoSectionAheadOfSiblings()
    {
        // The "explanation" folder has no index.md and so today would inherit
        // min(children) — both folders' children carry the same low orders, so
        // the tiebreaker falls to alphabetical. A sidecar order: 1 should override
        // that and pin explanation ahead of zebra-area regardless of children.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Foo", "/explanation/foo", 10, "explanation", "foo"),
            MakeTocItem("Bar", "/zebra-area/bar", 5, "zebra-area", "bar"),
        };
        var registry = new FolderMetadataRegistry(new[]
        {
            new FolderMetadata("/explanation/", Title: null, Order: 1, LlmsDescription: null),
        });
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(items);

        tree.Count.ShouldBe(2);
        tree[0].Title.ShouldBe("Explanation");
        tree[1].Title.ShouldBe("Zebra Area");
    }

    [Fact]
    public async Task BuildTree_FolderSidecarTitle_OverridesFormatSectionTitle()
    {
        // "core-api" would normally render as "Core API" via FormatSectionTitle.
        // The sidecar title wins.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Page", "/core-api/page", 10, "core-api", "page"),
        };
        var registry = new FolderMetadataRegistry(new[]
        {
            new FolderMetadata("/core-api/", Title: "Foundations", Order: null, LlmsDescription: null),
        });
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(items);

        tree[0].Title.ShouldBe("Foundations");
    }

    [Fact]
    public async Task BuildTree_FolderSidecarOverridesIndexMdTitleAndOrder()
    {
        // The folder has an index.md whose front matter says order=99, title="Stale".
        // A _meta.yml sidecar at the same folder declares order=1, title="Fresh"; both wins.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Stale", "/explanation/", 99, "explanation"),
            MakeTocItem("Page", "/explanation/page", 10, "explanation", "page"),
            MakeTocItem("Other", "/other/", 1, "other"),
        };
        var registry = new FolderMetadataRegistry(new[]
        {
            new FolderMetadata("/explanation/", Title: "Fresh", Order: 1, LlmsDescription: null),
        });
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(items);

        // Fresh sorts ahead of Other (order 1 vs 1, then alphabetical Fresh < Other).
        tree[0].Title.ShouldBe("Fresh");
        tree[0].Route.CanonicalPath.Value.ShouldBe("/explanation/");
        tree[1].Title.ShouldBe("Other");
    }

    [Fact]
    public async Task BuildTree_FolderSidecarPartialFields_FallsBackPerField()
    {
        // Sidecar sets title but leaves order null → order falls through to the index.md's value.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Original", "/explanation/", 5, "explanation"),
            MakeTocItem("Page", "/explanation/page", 10, "explanation", "page"),
            MakeTocItem("Other", "/other/", 3, "other"),
        };
        var registry = new FolderMetadataRegistry(new[]
        {
            new FolderMetadata("/explanation/", Title: "Renamed", Order: null, LlmsDescription: null),
        });
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(items);

        // Other (order 3) comes before Renamed (order 5, from index.md).
        tree[0].Title.ShouldBe("Other");
        tree[1].Title.ShouldBe("Renamed");
    }

    [Fact]
    public async Task BuildTree_NoSidecar_EmergentMinChildrenOrderPreserved()
    {
        // Backwards-compat: with no sidecar entries, the existing min(children)
        // emergent rule still drives folder ordering.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Late", "/late-folder/page", 100, "late-folder", "page"),
            MakeTocItem("Early", "/early-folder/page", 1, "early-folder", "page"),
        };
        var registry = new FolderMetadataRegistry(Array.Empty<FolderMetadata>());
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(items);

        tree[0].Title.ShouldBe("Early Folder");
        tree[1].Title.ShouldBe("Late Folder");
    }

    [Fact]
    public async Task BuildTree_SidecarTieOnOrder_BreaksAlphabetically()
    {
        // Two sidecar folders with the same explicit order should still tie-break by title.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Page1", "/zebra/page", 10, "zebra", "page"),
            MakeTocItem("Page2", "/alpha/page", 10, "alpha", "page"),
        };
        var registry = new FolderMetadataRegistry(new[]
        {
            new FolderMetadata("/zebra/", Title: null, Order: 1, LlmsDescription: null),
            new FolderMetadata("/alpha/", Title: null, Order: 1, LlmsDescription: null),
        });
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(items);

        tree[0].Title.ShouldBe("Alpha");
        tree[1].Title.ShouldBe("Zebra");
    }

    [Fact]
    public async Task BuildTree_AreaStrippedHierarchy_LooksUpFullCanonicalPrefix()
    {
        // Simulates DocSite GetTocItemsForAreaAsync: the area slug "explanation"
        // has been stripped from HierarchyParts, but Route.CanonicalPath keeps
        // the full URL. The folder-metadata registry stores prefixes in canonical
        // form, so the lookup must derive the prefix from the canonical path.
        var areaStrippedItems = new List<ContentTocItem>
        {
            new(
                Title: "Pipeline",
                Route: MakeRoute("/explanation/core/pipeline"),
                Order: 10,
                HierarchyParts: ["core", "pipeline"],
                SectionLabel: null,
                Locale: null),
        };
        var registry = new FolderMetadataRegistry(new[]
        {
            new FolderMetadata("/explanation/core/", Title: "Core Architecture", Order: 1, LlmsDescription: null),
        });
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(areaStrippedItems);

        tree.Count.ShouldBe(1);
        tree[0].Title.ShouldBe("Core Architecture");
        tree[0].Order.ShouldBe(1);
    }

    [Fact]
    public async Task BuildTree_NestedFolderSidecar_AppliesAtMatchingDepth()
    {
        // A sidecar at /docs/core/ applies to that subfolder, not to /docs/.
        var items = new List<ContentTocItem>
        {
            MakeTocItem("Docs index", "/docs/", 1, "docs"),
            MakeTocItem("Core leaf", "/docs/core/leaf", 10, "docs", "core", "leaf"),
            MakeTocItem("Misc leaf", "/docs/misc/leaf", 10, "docs", "misc", "leaf"),
        };
        var registry = new FolderMetadataRegistry(new[]
        {
            new FolderMetadata("/docs/core/", Title: "Renamed Core", Order: 1, LlmsDescription: null),
        });
        var builder = new NavigationBuilder(registry);

        var tree = await builder.BuildTreeAsync(items);

        tree[0].Title.ShouldBe("Docs index");
        var docsChildren = tree[0].Children;
        docsChildren.Count.ShouldBe(2);
        docsChildren[0].Title.ShouldBe("Renamed Core");
        docsChildren[1].Title.ShouldBe("Misc");
    }
}