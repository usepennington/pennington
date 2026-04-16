namespace Pennington.Pipeline;

/// <summary>Open Graph / social card metadata for a page.</summary>
/// <param name="Description">Page description.</param>
/// <param name="ImageUrl">URL of the social preview image.</param>
/// <param name="Type">Open Graph type (e.g., "article", "website").</param>
/// <param name="PublishedTime">Publication timestamp for articles.</param>
/// <param name="Author">Author name for articles.</param>
public record SocialMetadata(
    string? Description,
    string? ImageUrl,
    string? Type,
    DateTime? PublishedTime,
    string? Author
);
