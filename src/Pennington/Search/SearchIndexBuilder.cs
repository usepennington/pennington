namespace Pennington.Search;

using Content;
using DeweySearch;
using FrontMatter;
using Infrastructure;

/// <summary>
/// Maps a page's <see cref="HeadingSection"/>s onto DeweySearch <see cref="SearchDocument"/> records —
/// one record per heading (plus a page-lead record) so search results are heading-level and
/// deep-link to anchors. This is the Pennington-specific adapter: it builds anchor URLs, the
/// page→heading breadcrumb trail, and maps the content model (section label, tags, content area)
/// onto DeweySearch's open facet dictionary. Draft filtering is handled upstream by each
/// <c>IContentService</c>'s TOC builder.
/// </summary>
public sealed class SearchIndexBuilder
{
    private readonly SearchIndexOptions _options;
    private readonly LocalizationOptions _localization;

    /// <summary>Creates the adapter from the search options and localization (for content-area derivation).</summary>
    public SearchIndexBuilder(SearchIndexOptions options, LocalizationOptions localization)
    {
        _options = options;
        _localization = localization;
    }

    /// <summary>
    /// Builds a DeweySearch <see cref="SearchDocument"/> for one section of a page. The lead section
    /// becomes the page record (page URL, page title, page description); each heading section
    /// becomes an anchored record carrying the page→heading breadcrumb trail.
    /// </summary>
    /// <param name="toc">The page's table-of-contents entry (title, description, section, tags).</param>
    /// <param name="section">The heading section to map (or the page-lead section).</param>
    /// <param name="metadata">
    /// The page's record front matter, when available. When it implements
    /// <see cref="IHasSearchFacets"/>, its custom facet axes are emitted alongside the built-in
    /// section/tag/area dimensions. Pass <c>null</c> for pages with no record (the built-in facets
    /// still apply).
    /// </param>
    public SearchDocument BuildSection(ContentTocItem toc, HeadingSection section, IFrontMatter? metadata = null)
    {
        var pageUrl = toc.Route.CanonicalPath.Value;
        var area = Area(pageUrl);

        // Full trail shown/grouped by the client: page title first, then ancestor headings.
        string[] crumbs = section.IsLead ? [] : [toc.Title, .. section.Crumbs];

        var url = section.IsLead || string.IsNullOrEmpty(section.AnchorId)
            ? pageUrl
            : $"{pageUrl}#{section.AnchorId}";

        return new SearchDocument(
            Url: url,
            Title: section.IsLead ? toc.Title : section.Title,
            Description: section.IsLead ? toc.Description : null,
            Headings: string.Join(' ', crumbs), // index page title + ancestor headings at heading boost
            Body: section.Text,
            Priority: PriorityFor(area),
            Facets: BuildFacets(toc, area, metadata),
            Crumbs: crumbs);
    }

    // Per-area boost (derived from the host's area order) lets comparable matches in earlier areas
    // outrank later ones; areas without an explicit priority fall back to the default.
    private int PriorityFor(string? area) =>
        area is not null && _options.AreaPriorities.TryGetValue(area, out var p)
            ? p
            : _options.DefaultPriority;

    // Maps the enabled Pennington facet dimensions onto DeweySearch's open facet dictionary. A
    // dimension is present only when enabled in options and the page actually carries a value,
    // which is exactly how DeweySearch decides a facet exists. Facets are page-level — every section of
    // a page shares them.
    private Dictionary<string, string[]>? BuildFacets(ContentTocItem toc, string? area, IFrontMatter? metadata)
    {
        var facets = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (FacetEnabled(SearchFacetField.Section) && !string.IsNullOrEmpty(toc.SectionLabel))
        {
            facets["section"] = [toc.SectionLabel];
        }

        if (FacetEnabled(SearchFacetField.Tags) && toc.Tags.Length > 0)
        {
            var tags = CleanValues(toc.Tags);
            if (tags.Length > 0)
            {
                facets["tag"] = tags;
            }
        }

        if (FacetEnabled(SearchFacetField.Area) && area is not null)
        {
            facets["area"] = [area];
        }

        // Custom facet axes declared on the record's front matter. Emitted unconditionally — they
        // are the service author's explicit opt-in, not gated by SearchFacetField — but they never
        // overwrite a built-in dimension, so section/tag/area stay authoritative. The reserved-name
        // guard holds even when a built-in is disabled or absent for this page (so it was never
        // added to the dict), which a plain ContainsKey check would miss.
        if (metadata is IHasSearchFacets faceted)
        {
            foreach (var (axis, values) in faceted.SearchFacets)
            {
                if (string.IsNullOrWhiteSpace(axis) || ReservedFacetKeys.Contains(axis) || facets.ContainsKey(axis))
                {
                    continue;
                }

                var cleaned = CleanValues(values);
                if (cleaned.Length > 0)
                {
                    facets[axis] = cleaned;
                }
            }
        }

        return facets.Count > 0 ? facets : null;
    }

    private static string[] CleanValues(IEnumerable<string> values) =>
        values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray();

    // The built-in facet dimension names, reserved so a custom IHasSearchFacets axis can never
    // occupy them — even on a page/config where the built-in itself produced no value.
    private static readonly HashSet<string> ReservedFacetKeys = new(StringComparer.Ordinal) { "section", "tag", "area" };

    private bool FacetEnabled(SearchFacetField field) => (_options.Facets & field) != 0;

    /// <summary>The content area: the first URL segment after any non-default locale prefix, or null for the site root.</summary>
    private string? Area(string url)
    {
        var trimmed = url.Trim('/');
        if (trimmed.Length == 0)
        {
            return null;
        }

        var segments = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var start = 0;
        if (_localization.IsMultiLocale
            && segments.Length > 0
            && _localization.Locales.ContainsKey(segments[0])
            && !string.Equals(segments[0], _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            start = 1;
        }

        return start < segments.Length ? segments[start] : null;
    }
}
