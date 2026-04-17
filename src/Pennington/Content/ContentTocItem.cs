namespace Pennington.Content;

using Routing;

/// <summary>
/// A table-of-contents entry for navigation.
/// </summary>
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