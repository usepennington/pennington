namespace Pennington.Pipeline;

/// <summary>A content tag with a display name and URL-safe slug.</summary>
/// <param name="Name">Human-readable tag name.</param>
/// <param name="Slug">URL-safe slug used in tag routes.</param>
public record Tag(string Name, string Slug);