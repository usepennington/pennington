namespace Pennington.Navigation;

using System.Collections.Immutable;

/// <summary>Page-scoped navigation context exposed to layouts and components.</summary>
/// <param name="SectionName">Label of the containing top-level section, or null if none.</param>
/// <param name="Breadcrumbs">Breadcrumb trail from the site root to the current page.</param>
/// <param name="PageTitle">Title of the current page.</param>
/// <param name="PreviousPage">Previous page in reading order, or null at the start.</param>
/// <param name="NextPage">Next page in reading order, or null at the end.</param>
public record NavigationInfo(
    string? SectionName,
    ImmutableList<BreadcrumbItem> Breadcrumbs,
    string PageTitle,
    NavigationTreeItem? PreviousPage,
    NavigationTreeItem? NextPage
);