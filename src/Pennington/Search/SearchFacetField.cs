namespace Pennington.Search;

/// <summary>
/// Facet dimensions the search index can surface for client-side filtering. Combine
/// with bitwise OR to enable several at once.
/// </summary>
[Flags]
public enum SearchFacetField
{
    /// <summary>No facets are generated.</summary>
    None = 0,

    /// <summary>Facet on <see cref="Content.ContentTocItem.SectionLabel"/>.</summary>
    Section = 1,

    /// <summary>Facet on page tags (front matter implementing <see cref="FrontMatter.ITaggable"/>).</summary>
    Tags = 2,

    /// <summary>Facet on the content area — the first URL segment after any locale prefix.</summary>
    Area = 4,
}
