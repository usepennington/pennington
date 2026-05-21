namespace Pennington.StructuredData;

using System.Text.Json.Serialization;

/// <summary>
/// Base record for a schema.org JSON-LD entity. Subclass with
/// <see cref="JsonPropertyNameAttribute"/>-decorated properties and override
/// <see cref="Type"/> to declare a new schema.org type. Repeat the
/// <c>[JsonPropertyName("@type")]</c> attribute on the override —
/// <see cref="System.Text.Json"/> does not inherit it from the abstract base.
/// </summary>
public abstract record JsonLdEntity
{
    /// <summary>JSON-LD context. Defaults to schema.org; override for a different vocabulary.</summary>
    [JsonPropertyName("@context")]
    public string Context { get; init; } = "https://schema.org";

    /// <summary>schema.org type literal (e.g. "Article", "Recipe").</summary>
    [JsonPropertyName("@type")]
    public abstract string Type { get; }
}
