namespace Penn.Navigation;

using System.Collections.Immutable;
using Penn.Routing;

public record NavigationInfo(
    string? SectionName,
    ContentRoute? SectionRoute,
    ImmutableList<BreadcrumbItem> Breadcrumbs,
    string PageTitle,
    NavigationTreeItem? PreviousPage,
    NavigationTreeItem? NextPage
);
