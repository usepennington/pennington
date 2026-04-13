namespace Pennington.Islands;

using System.Collections.Immutable;
using Pipeline;

public record SpaEnvelope(
    string Title,
    string? Description,
    SocialMetadata? Social,
    ImmutableDictionary<string, string> Islands
);