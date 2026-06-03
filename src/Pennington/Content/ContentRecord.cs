namespace Pennington.Content;

using FrontMatter;
using Routing;

/// <summary>
/// A source-agnostic, routable content record — the unit every discovery channel reads. Pairs a
/// canonical <see cref="Routing.ContentRoute"/> with its typed <see cref="IFrontMatter"/>,
/// independent of whether the page came from markdown, a Razor page, or a custom
/// <see cref="IContentService"/> / endpoint.
/// <para>
/// Taxonomy, search faceting, and structured-data emission all consume records, so a service that
/// projects records through <see cref="IContentService.GetRecordsAsync"/> gets the same
/// browse-by-field, custom-facet, and JSON-LD behavior that markdown has. The capabilities each
/// pillar reads — <see cref="ITaggable"/>, <see cref="ISectionable"/>,
/// <see cref="StructuredData.IHasStructuredData"/>, <see cref="Search.IHasSearchFacets"/> — ride on
/// <see cref="Metadata"/>.
/// </para>
/// </summary>
/// <param name="Route">Canonical route the record is served at.</param>
/// <param name="Metadata">Typed front matter carrying the record's title, dates, indexing flags, and capability mixins.</param>
public record ContentRecord(ContentRoute Route, IFrontMatter Metadata);
