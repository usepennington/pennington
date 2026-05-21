using System.Text.Json;
using System.Text.Json.Serialization;
using Pennington.StructuredData;

namespace Pennington.Tests.StructuredData;

public class JsonLdDateConverterTests
{
    [Fact]
    public void Write_UtcDate_EmitsZSuffixedFormat()
    {
        var date = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc);
        var json = JsonSerializer.Serialize(new Wrapper { Date = date });

        ExtractDate(json).ShouldBe("2026-03-15T14:30:00Z");
    }

    [Fact]
    public void Write_UnspecifiedKindDate_StillEmitsZSuffixedFormat()
    {
        var date = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var json = JsonSerializer.Serialize(new Wrapper { Date = date });

        ExtractDate(json).ShouldBe("2026-03-15T14:30:00Z");
    }

    [Fact]
    public void Write_LocalKindDate_EmitsClockTimeWithZSuffix()
    {
        // The converter does not adjust for timezone — it preserves clock time
        // and stamps Z, matching the wire format the framework has always
        // produced. Schema.org consumers treat the value as a label, not a
        // timestamp arithmetic input.
        var date = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Local);
        var json = JsonSerializer.Serialize(new Wrapper { Date = date });

        ExtractDate(json).ShouldBe("2026-03-15T14:30:00Z");
    }

    [Fact]
    public void Read_RoundTripsAsUtc()
    {
        const string json = """{"Date":"2026-03-15T14:30:00Z"}""";
        var wrapper = JsonSerializer.Deserialize<Wrapper>(json);

        wrapper.ShouldNotBeNull();
        wrapper.Date.ShouldBe(new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc));
        wrapper.Date.Kind.ShouldBe(DateTimeKind.Utc);
    }

    private static string ExtractDate(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("Date").GetString()!;
    }

    private sealed record Wrapper
    {
        [JsonConverter(typeof(JsonLdDateConverter))]
        public DateTime Date { get; init; }
    }
}
