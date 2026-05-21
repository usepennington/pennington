namespace Pennington.StructuredData;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Serializes any <see cref="JsonLdEntity"/> subclass to a JSON string safe
/// for embedding in a <c>&lt;script type="application/ld+json"&gt;</c> tag.
/// </summary>
public static class JsonLdSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Serializes <paramref name="entity"/> to JSON-LD. The concrete runtime
    /// type is used so subclass-only properties are included.
    /// </summary>
    public static string Serialize(JsonLdEntity entity) =>
        EscapeForScriptTag(JsonSerializer.Serialize(entity, entity.GetType(), Options));

    /// <summary>
    /// Escapes sequences that would prematurely close a script tag when the
    /// JSON is embedded inline in HTML.
    /// </summary>
    private static string EscapeForScriptTag(string json) =>
        json.Replace("</", "<\\/");
}
