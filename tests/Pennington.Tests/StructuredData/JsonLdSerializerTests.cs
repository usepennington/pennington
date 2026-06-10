using System.Text.Json;
using System.Text.Json.Serialization;
using Pennington.StructuredData;

namespace Pennington.Tests.StructuredData;

public class JsonLdSerializerTests
{
    [Fact]
    public void Serialize_FullArticle_ProducesValidJsonLd()
    {
        var article = new JsonLdArticle
        {
            Headline = "Test Post",
            Description = "A test description",
            Url = "https://example.com/blog/test/",
            DatePublished = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Author = new JsonLdPerson { Name = "Jane Doe" },
        };

        var json = JsonLdSerializer.Serialize(article);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("Article");
        root.GetProperty("headline").GetString().ShouldBe("Test Post");
        root.GetProperty("description").GetString().ShouldBe("A test description");
        root.GetProperty("url").GetString().ShouldBe("https://example.com/blog/test/");
        root.GetProperty("datePublished").GetString().ShouldBe("2026-03-15T00:00:00Z");
        root.GetProperty("author").GetProperty("@type").GetString().ShouldBe("Person");
        root.GetProperty("author").GetProperty("name").GetString().ShouldBe("Jane Doe");
    }

    [Fact]
    public void Serialize_NullDescription_OmitsDescription()
    {
        var article = new JsonLdArticle { Headline = "Title", Url = "https://example.com/" };

        var json = JsonLdSerializer.Serialize(article);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("description", out _).ShouldBeFalse();
    }

    [Fact]
    public void Serialize_NullDate_OmitsDatePublished()
    {
        var article = new JsonLdArticle { Headline = "Title", Url = "https://example.com/" };

        var json = JsonLdSerializer.Serialize(article);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("datePublished", out _).ShouldBeFalse();
    }

    [Fact]
    public void Serialize_NullAuthor_OmitsAuthor()
    {
        var article = new JsonLdArticle { Headline = "Title", Url = "https://example.com/" };

        var json = JsonLdSerializer.Serialize(article);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("author", out _).ShouldBeFalse();
    }

    [Fact]
    public void Serialize_AllOptionalNull_OnlyRequiredFields()
    {
        var article = new JsonLdArticle { Headline = "Title", Url = "https://example.com/" };

        var json = JsonLdSerializer.Serialize(article);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("Article");
        root.GetProperty("headline").GetString().ShouldBe("Title");
        root.GetProperty("url").GetString().ShouldBe("https://example.com/");
        root.EnumerateObject().Count().ShouldBe(4);
    }

    [Fact]
    public void Serialize_WebSite_FullFields()
    {
        var webSite = new JsonLdWebSite
        {
            Name = "My Site",
            Url = "https://example.com/",
            Description = "A great site",
        };

        var json = JsonLdSerializer.Serialize(webSite);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("WebSite");
        root.GetProperty("name").GetString().ShouldBe("My Site");
        root.GetProperty("url").GetString().ShouldBe("https://example.com/");
        root.GetProperty("description").GetString().ShouldBe("A great site");
    }

    [Fact]
    public void Serialize_WebSite_NullDescriptionOmitted()
    {
        var webSite = new JsonLdWebSite { Name = "My Site", Url = "https://example.com/" };

        var json = JsonLdSerializer.Serialize(webSite);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("description", out _).ShouldBeFalse();
    }

    [Fact]
    public void Serialize_ContentWithScriptTag_EscapesClosingTag()
    {
        var article = new JsonLdArticle
        {
            Headline = "Using </script> in content",
            Url = "https://example.com/",
        };

        var json = JsonLdSerializer.Serialize(article);

        json.ShouldNotContain("</script>");
        json.ShouldContain("<\\/script>");

        var unescaped = json.Replace("<\\/", "</");
        using var doc = JsonDocument.Parse(unescaped);
        doc.RootElement.GetProperty("headline").GetString().ShouldBe("Using </script> in content");
    }

    [Fact]
    public void Serialize_UserDefinedSubclass_IncludesAllProperties()
    {
        // A user defines their own JsonLdEntity subclass — the framework
        // serializer must walk the concrete runtime type, not the abstract
        // base, so the new fields appear in the JSON output.
        var custom = new TestSoftwareApplication
        {
            Name = "Penny",
            ApplicationCategory = "DeveloperApplication",
            OperatingSystem = "Cross-platform",
        };

        var json = JsonLdSerializer.Serialize(custom);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("SoftwareApplication");
        root.GetProperty("name").GetString().ShouldBe("Penny");
        root.GetProperty("applicationCategory").GetString().ShouldBe("DeveloperApplication");
        root.GetProperty("operatingSystem").GetString().ShouldBe("Cross-platform");
    }

    [Fact]
    public void Serialize_CustomContextOverride_RespectsInitializer()
    {
        var custom = new TestSoftwareApplication
        {
            Context = "https://example.com/custom",
            Name = "Penny",
            ApplicationCategory = null,
            OperatingSystem = null,
        };

        var json = JsonLdSerializer.Serialize(custom);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("@context").GetString().ShouldBe("https://example.com/custom");
    }

    private sealed record TestSoftwareApplication : JsonLdEntity
    {
        [JsonPropertyName("@type")]
        public override string Type => "SoftwareApplication";

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("applicationCategory")]
        public string? ApplicationCategory { get; init; }

        [JsonPropertyName("operatingSystem")]
        public string? OperatingSystem { get; init; }
    }
}
