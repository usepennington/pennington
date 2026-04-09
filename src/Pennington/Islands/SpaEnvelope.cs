namespace Pennington.Islands;

using System.Collections.Immutable;
using Pennington.Pipeline;

public record SpaEnvelope(
    string Title,
    string? Description,
    SocialMetadata? Social,
    ImmutableDictionary<string, string> Islands
);
