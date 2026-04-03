namespace Penn.Navigation;

using System.Collections.Immutable;
using Penn.Routing;

public record NavigationTreeItem(
    string Title,
    ContentRoute Route,
    int Order,
    string? Section,
    bool IsSelected,
    bool IsExpanded,
    ImmutableList<NavigationTreeItem> Children
);
