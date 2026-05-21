namespace Pennington.BlogSite.StructuredData;

using System.Text.Json.Serialization;
using Pennington.StructuredData;

/// <summary>schema.org Article emitted in the head of BlogSite post pages.</summary>
public sealed record JsonLdArticle : JsonLdEntity
{
    /// <inheritdoc />
    [JsonPropertyName("@type")]
    public override string Type => "Article";

    /// <summary>Post headline.</summary>
    [JsonPropertyName("headline")]
    public required string Headline { get; init; }

    /// <summary>Canonical URL of the post.</summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>Short description. Omitted when null.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Publication date. Omitted when null; serialized as <c>yyyy-MM-ddTHH:mm:ssZ</c>.</summary>
    [JsonPropertyName("datePublished")]
    [JsonConverter(typeof(JsonLdDateConverter))]
    public DateTime? DatePublished { get; init; }

    /// <summary>Author entity. Omitted when null.</summary>
    [JsonPropertyName("author")]
    public JsonLdPerson? Author { get; init; }
}

/// <summary>schema.org Person — author on a BlogSite post.</summary>
public sealed record JsonLdPerson
{
    /// <summary>schema.org @type literal.</summary>
    [JsonPropertyName("@type")]
    public string Type => "Person";

    /// <summary>Display name of the author.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
