namespace Pennington.StructuredData;

using System.Text.Json.Serialization;

/// <summary>schema.org BreadcrumbList describing a page's place in the navigation tree.</summary>
public sealed record JsonLdBreadcrumbList : JsonLdEntity
{
    /// <inheritdoc />
    [JsonPropertyName("@type")]
    public override string Type => "BreadcrumbList";

    /// <summary>Ordered crumb items.</summary>
    [JsonPropertyName("itemListElement")]
    public required IReadOnlyList<JsonLdBreadcrumbItem> Items { get; init; }
}

/// <summary>A single rung in a <see cref="JsonLdBreadcrumbList"/>.</summary>
public sealed record JsonLdBreadcrumbItem
{
    /// <summary>schema.org @type literal.</summary>
    [JsonPropertyName("@type")]
    public string Type => "ListItem";

    /// <summary>1-based position of the item in the crumb trail.</summary>
    [JsonPropertyName("position")]
    public required int Position { get; init; }

    /// <summary>Display name for this crumb.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>URL the crumb links to. Omitted when null (typically the current page).</summary>
    [JsonPropertyName("item")]
    public string? Url { get; init; }
}
