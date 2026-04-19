namespace Pennington.Islands;

using System.Text.Json;
using System.Text.Json.Serialization;
using Diagnostics;

/// <summary>JSON serialization for SPA envelopes.</summary>
public static class SpaEnvelopeSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>Serializes an SPA envelope DTO to JSON using the configured options.</summary>
    public static string Serialize(SpaEnvelopeDto envelope)
        => JsonSerializer.Serialize(envelope, Options);
}

/// <summary>DTO for JSON serialization (SpaEnvelope uses ImmutableDictionary which needs a simpler shape).</summary>
/// <param name="Title">Page title, surfaced in the browser tab and hero.</param>
/// <param name="Description">Short page description, when available.</param>
/// <param name="Islands">Island id to rendered HTML payload.</param>
/// <param name="Diagnostics">Per-request diagnostics emitted during rendering.</param>
/// <param name="Reload">When set, directs the SPA shell to perform a full reload instead of a client-side swap.</param>
public record SpaEnvelopeDto(
    string Title,
    string? Description,
    Dictionary<string, string> Islands,
    IReadOnlyList<Diagnostic>? Diagnostics = null,
    bool? Reload = null
);