namespace Penn.Islands;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>JSON serialization for SPA envelopes.</summary>
public static class SpaEnvelopeSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(SpaEnvelopeDto envelope)
        => JsonSerializer.Serialize(envelope, Options);
}

/// <summary>DTO for JSON serialization (SpaEnvelope uses ImmutableDictionary which needs a simpler shape).</summary>
public record SpaEnvelopeDto(
    string Title,
    string? Description,
    Dictionary<string, string> Islands
);
