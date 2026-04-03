using System.Collections.Immutable;
using Penn.Navigation;
using Penn.Routing;

namespace Penn.Tests.Navigation;

public class NavigationTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void NavigationTreeItem_CreateWithChildren_VerifiesTreeStructure()
    {
        var child1 = new NavigationTreeItem(
            "Child 1", MakeRoute("/parent/child1"), 1, null, false, false,
            ImmutableList<NavigationTreeItem>.Empty);

        var child2 = new NavigationTreeItem(
            "Child 2", MakeRoute("/parent/child2"), 2, null, false, false,
            ImmutableList<NavigationTreeItem>.Empty);

        var parent = new NavigationTreeItem(
            "Parent", MakeRoute("/parent"), 0, "docs", false, true,
            [child1, child2]);

        parent.Title.ShouldBe("Parent");
        parent.Children.Count.ShouldBe(2);
        parent.Children[0].Title.ShouldBe("Child 1");
        parent.Children[1].Title.ShouldBe("Child 2");
    }

    [Fact]
    public void NavigationTreeItem_LeafNode_HasEmptyChildren()
    {
        var leaf = new NavigationTreeItem(
            "Leaf", MakeRoute("/leaf"), 0, null, false, false,
            ImmutableList<NavigationTreeItem>.Empty);

        leaf.Children.ShouldBeEmpty();
    }

    [Fact]
    public void NavigationTreeItem_IsSelectedAndIsExpanded_FlagsPreserved()
    {
        var selected = new NavigationTreeItem(
            "Selected", MakeRoute("/selected"), 0, null, true, false,
            ImmutableList<NavigationTreeItem>.Empty);

        var expanded = new NavigationTreeItem(
            "Expanded", MakeRoute("/expanded"), 0, null, false, true,
            ImmutableList<NavigationTreeItem>.Empty);

        selected.IsSelected.ShouldBeTrue();
        selected.IsExpanded.ShouldBeFalse();

        expanded.IsSelected.ShouldBeFalse();
        expanded.IsExpanded.ShouldBeTrue();
    }

    [Fact]
    public void BreadcrumbItem_CreateWithRoute()
    {
        var route = MakeRoute("/docs/getting-started");
        var breadcrumb = new BreadcrumbItem("Getting Started", route);

        breadcrumb.Title.ShouldBe("Getting Started");
        breadcrumb.Route.ShouldNotBeNull();
        breadcrumb.Route.CanonicalPath.Value.ShouldBe("/docs/getting-started");
    }

    [Fact]
    public void BreadcrumbItem_CreateWithoutRoute_RouteIsNull()
    {
        var breadcrumb = new BreadcrumbItem("Current Page", null);

        breadcrumb.Title.ShouldBe("Current Page");
        breadcrumb.Route.ShouldBeNull();
    }

    [Fact]
    public void NavigationInfo_CreateWithAllFieldsPopulated()
    {
        var sectionRoute = MakeRoute("/docs");
        var prev = new NavigationTreeItem(
            "Previous", MakeRoute("/docs/prev"), 1, "docs", false, false,
            ImmutableList<NavigationTreeItem>.Empty);
        var next = new NavigationTreeItem(
            "Next", MakeRoute("/docs/next"), 3, "docs", false, false,
            ImmutableList<NavigationTreeItem>.Empty);

        var breadcrumbs = ImmutableList.Create(
            new BreadcrumbItem("Home", MakeRoute("/")),
            new BreadcrumbItem("Docs", MakeRoute("/docs")),
            new BreadcrumbItem("Current", null));

        var info = new NavigationInfo(
            SectionName: "Documentation",
            SectionRoute: sectionRoute,
            Breadcrumbs: breadcrumbs,
            PageTitle: "Current Page",
            PreviousPage: prev,
            NextPage: next);

        info.SectionName.ShouldBe("Documentation");
        info.SectionRoute.ShouldNotBeNull();
        info.SectionRoute.CanonicalPath.Value.ShouldBe("/docs");
        info.PageTitle.ShouldBe("Current Page");
        info.PreviousPage.ShouldNotBeNull();
        info.PreviousPage.Title.ShouldBe("Previous");
        info.NextPage.ShouldNotBeNull();
        info.NextPage.Title.ShouldBe("Next");
    }

    [Fact]
    public void NavigationInfo_NullPreviousAndNextPage()
    {
        var info = new NavigationInfo(
            SectionName: null,
            SectionRoute: null,
            Breadcrumbs: ImmutableList<BreadcrumbItem>.Empty,
            PageTitle: "Standalone Page",
            PreviousPage: null,
            NextPage: null);

        info.SectionName.ShouldBeNull();
        info.SectionRoute.ShouldBeNull();
        info.PreviousPage.ShouldBeNull();
        info.NextPage.ShouldBeNull();
    }

    [Fact]
    public void NavigationInfo_BreadcrumbsPreservedInOrder()
    {
        var breadcrumbs = ImmutableList.Create(
            new BreadcrumbItem("Home", MakeRoute("/")),
            new BreadcrumbItem("Section", MakeRoute("/section")),
            new BreadcrumbItem("Subsection", MakeRoute("/section/sub")),
            new BreadcrumbItem("Page", null));

        var info = new NavigationInfo(
            SectionName: null,
            SectionRoute: null,
            Breadcrumbs: breadcrumbs,
            PageTitle: "Page",
            PreviousPage: null,
            NextPage: null);

        info.Breadcrumbs.Count.ShouldBe(4);
        info.Breadcrumbs[0].Title.ShouldBe("Home");
        info.Breadcrumbs[1].Title.ShouldBe("Section");
        info.Breadcrumbs[2].Title.ShouldBe("Subsection");
        info.Breadcrumbs[3].Title.ShouldBe("Page");
    }
}
