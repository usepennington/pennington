namespace Pennington.Navigation;

using System.Collections.Immutable;
using Routing;

public record NavigationInfo(
    string? SectionName,
    ContentRoute? SectionRoute,
    ImmutableList<BreadcrumbItem> Breadcrumbs,
    string PageTitle,
    NavigationTreeItem? PreviousPage,
    NavigationTreeItem? NextPage
);