namespace Pennington.DocSite.StructuredData;

using System.Text.Json.Serialization;
using Pennington.StructuredData;

/// <summary>schema.org Article emitted in the head of DocSite content pages.</summary>
public sealed record JsonLdArticle : JsonLdEntity
{
    /// <inheritdoc />
    [JsonPropertyName("@type")]
    public override string Type => "Article";

    /// <summary>Article headline.</summary>
    [JsonPropertyName("headline")]
    public required string Headline { get; init; }

    /// <summary>Canonical URL of the article.</summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>Short description of the article. Omitted when null.</summary>
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

/// <summary>schema.org Person — author or contributor on a DocSite article.</summary>
public sealed record JsonLdPerson
{
    /// <summary>schema.org @type literal.</summary>
    [JsonPropertyName("@type")]
    public string Type => "Person";

    /// <summary>Display name of the person.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
