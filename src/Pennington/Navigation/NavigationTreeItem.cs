namespace Pennington.Navigation;

using System.Collections.Immutable;
using Routing;

public record NavigationTreeItem(
    string Title,
    ContentRoute Route,
    int Order,
    string? SectionLabel,
    bool IsSelected,
    bool IsExpanded,
    ImmutableList<NavigationTreeItem> Children
);