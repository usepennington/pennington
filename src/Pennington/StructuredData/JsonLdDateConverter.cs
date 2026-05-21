namespace Pennington.StructuredData;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Emits <see cref="DateTime"/> values in the JSON-LD wire format
/// <c>yyyy-MM-ddTHH:mm:ssZ</c> regardless of <see cref="DateTime.Kind"/>.
/// </summary>
public sealed class JsonLdDateConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ssZ";

    /// <summary>Reads a JSON-LD date string back to a UTC <see cref="DateTime"/>.</summary>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);

    /// <summary>Writes a <see cref="DateTime"/> as <c>yyyy-MM-ddTHH:mm:ssZ</c>.</summary>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
