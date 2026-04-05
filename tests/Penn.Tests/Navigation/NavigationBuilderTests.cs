using System.Collections.Immutable;
using Penn.Content;
using Penn.Navigation;
using Penn.Routing;

namespace Penn.Tests.Navigation;

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
        Section: null,
        Locale: null
    );

    private readonly NavigationBuilder _builder = new();

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
    public void BuildTree_EmptyInput_ProducesEmptyTree()
    {
        var tree = _builder.BuildTree([]);

        tree.ShouldBeEmpty();
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

    [Fact]
    public void BuildNavigationInfo_SectionPreserved()
    {
        var tocItem = new ContentTocItem(
            Title: "API Docs",
            Route: MakeRoute("/docs/api"),
            Order: 1,
            HierarchyParts: ["API Docs"],
            Section: "documentation",
            Locale: null
        );

        var info = _builder.BuildNavigationInfo([tocItem], MakeRoute("/docs/api"));

        info.SectionName.ShouldBe("documentation");
        info.PageTitle.ShouldBe("API Docs");
    }
}
