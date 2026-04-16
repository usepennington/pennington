namespace Pennington.Islands;

using System.Collections.Immutable;
using Pipeline;

/// <summary>SPA page envelope aggregating page-level metadata and rendered island fragments.</summary>
/// <param name="Title">Page title.</param>
/// <param name="Description">Optional page description.</param>
/// <param name="Social">Optional social/Open Graph metadata.</param>
/// <param name="Islands">Map of island name to rendered HTML fragment.</param>
public record SpaEnvelope(
    string Title,
    string? Description,
    SocialMetadata? Social,
    ImmutableDictionary<string, string> Islands
);
