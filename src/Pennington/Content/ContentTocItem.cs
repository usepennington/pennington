namespace Pennington.Content;

using Routing;

/// <summary>
/// A table-of-contents entry for navigation.
/// </summary>
/// <param name="Title">Display title shown in navigation.</param>
/// <param name="Route">Content route the entry links to.</param>
/// <param name="Order">Sort order within the entry's section.</param>
/// <param name="HierarchyParts">Ancestor path segments used to place the entry in the tree.</param>
/// <param name="SectionLabel">Section label used for grouping, or <c>null</c> for the default bucket.</param>
/// <param name="Locale">Locale the entry belongs to, or <c>null</c> when the entry is locale-neutral.</param>
public record ContentTocItem(
    string Title,
    ContentRoute Route,
    int Order,
    string[] HierarchyParts,
    string? SectionLabel,
    string? Locale
)
{
    /// <summary>Front-matter description, surfaced as a boosted field in the search index.</summary>
    public string? Description { get; init; }

    /// <summary>When true, excluded from the search index.</summary>
    public bool ExcludeFromSearch { get; init; }

    /// <summary>When true, excluded from llms.txt.</summary>
    public bool ExcludeFromLlms { get; init; }
}