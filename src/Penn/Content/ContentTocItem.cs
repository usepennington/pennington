namespace Pennington.Content;

using Pennington.Routing;

/// <summary>
/// A table-of-contents entry for navigation.
/// </summary>
public record ContentTocItem(
    string Title,
    ContentRoute Route,
    int Order,
    string[] HierarchyParts,
    string? Section,
    string? Locale
);
