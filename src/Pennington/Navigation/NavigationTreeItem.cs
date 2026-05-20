namespace Pennington.Navigation;

using System.Collections.Immutable;
using Routing;

/// <summary>A node in the hierarchical navigation tree.</summary>
/// <param name="Title">Display title for the node.</param>
/// <param name="Route">Route the node links to.</param>
/// <param name="Order">Sort order within its parent.</param>
/// <param name="SectionLabel">Optional section grouping label.</param>
/// <param name="IsSelected">True when the node matches the current route.</param>
/// <param name="IsExpanded">True when the node should render expanded (contains or is the current route).</param>
/// <param name="Children">Child nodes nested under this one.</param>
public record NavigationTreeItem(
    string Title,
    ContentRoute Route,
    int Order,
    string? SectionLabel,
    bool IsSelected,
    bool IsExpanded,
    ImmutableList<NavigationTreeItem> Children
);