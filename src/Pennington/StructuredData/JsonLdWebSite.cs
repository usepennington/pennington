namespace Pennington.StructuredData;

using System.Text.Json.Serialization;

/// <summary>schema.org WebSite — site-level identity, emitted on the home page.</summary>
public sealed record JsonLdWebSite : JsonLdEntity
{
    /// <inheritdoc />
    [JsonPropertyName("@type")]
    public override string Type => "WebSite";

    /// <summary>Site name.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>Site canonical URL.</summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>Short site description. Omitted when null.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
