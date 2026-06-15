namespace Pennington.DocSite.Api;

/// <summary>Per-registration options passed to <see cref="ApiReferenceServiceExtensions.AddApiReference"/>.</summary>
public sealed class ApiReferenceRegistrationOptions
{
    /// <summary>
    /// URL prefix under which this reference tree's pages are served. Leading and
    /// trailing slashes are normalized automatically. Examples: <c>"/reference/api/"</c>,
    /// <c>"/api/"</c>, <c>"/api/spectre-cli/"</c>. Defaults to <c>"/reference/api/"</c>.
    /// </summary>
    public string RoutePrefix { get; set; } = "/reference/api/";

    /// <summary>
    /// Sidebar title for the single TOC entry pointing at this registration's index
    /// page. Set to <see langword="null"/> to suppress the TOC entry entirely (the
    /// pages still publish). Defaults to <c>"API reference"</c>.
    /// </summary>
    public string? TocTitle { get; set; } = "API reference";

    /// <summary>
    /// Section label the TOC entry is grouped under in the sidebar. Leave <see langword="null"/>
    /// to keep the entry unsectioned. Set to a string that matches another page's
    /// <c>sectionLabel</c> to join that section.
    /// </summary>
    public string? TocSectionLabel { get; set; }

    /// <summary>
    /// Search priority for every page under this tree's <see cref="RoutePrefix"/>, registered into
    /// <c>SearchIndexOptions.PrefixPriorities</c> so it populates the index <c>p</c> field. Lower
    /// ranks later. Defaults to <c>3</c> — below typical prose so generated reference pages don't
    /// bury conceptual articles that match the same term.
    /// </summary>
    public int SearchPriority { get; set; } = 3;
}