namespace Pennington.Search;

/// <summary>
/// Capability interface for an <see cref="FrontMatter.IFrontMatter"/> type that declares custom
/// search facet axes beyond the built-in section/tag/area dimensions. Each entry becomes a facet
/// dimension on the page's search records, so the client can filter on it.
/// </summary>
/// <remarks>
/// A capability mixin — implement it alongside <see cref="FrontMatter.IFrontMatter"/> the same way
/// as <see cref="FrontMatter.ITaggable"/> or <see cref="StructuredData.IHasStructuredData"/>.
/// Facets declared here are emitted unconditionally; they are not gated by
/// <see cref="SearchFacetField"/>, which only governs the built-in dimensions.
/// </remarks>
public interface IHasSearchFacets
{
    /// <summary>
    /// Custom facet axes for this page: each key is a facet dimension (for example <c>"company"</c>
    /// or <c>"language"</c>), each value the page's membership values within that dimension. Return
    /// an empty dictionary to contribute none.
    /// </summary>
    IReadOnlyDictionary<string, string[]> SearchFacets { get; }
}
