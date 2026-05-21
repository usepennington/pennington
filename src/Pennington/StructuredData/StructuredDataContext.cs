namespace Pennington.StructuredData;

/// <summary>
/// Per-request context passed to <see cref="IHasStructuredData.GetStructuredData"/>
/// so a front matter implementation can build canonical URLs and apply
/// site-level fallbacks it does not own (e.g. the BlogSite author).
/// </summary>
public sealed record StructuredDataContext
{
    /// <summary>Absolute canonical URL for the page, including base URL and trailing slash.</summary>
    public required string CanonicalUrl { get; init; }

    /// <summary>
    /// Site-level author fallback. Honored when the front matter has no
    /// author of its own and the template has a default author configured
    /// (BlogSite's <c>BlogSiteOptions.AuthorName</c>). May be null.
    /// </summary>
    public string? FallbackAuthorName { get; init; }
}
