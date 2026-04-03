namespace Penn.Islands;

using System.Collections.Immutable;
using Penn.Pipeline;

public record SpaEnvelope(
    string Title,
    string? Description,
    SocialMetadata? Social,
    ImmutableDictionary<string, string> Islands
);
