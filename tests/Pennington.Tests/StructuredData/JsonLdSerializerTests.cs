using System.Text.Json;
using Pennington.StructuredData;

namespace Pennington.Tests.StructuredData;

public class JsonLdSerializerTests
{
    [Fact]
    public void SerializeArticle_FullFields_ProducesValidJsonLd()
    {
        var article = new JsonLdArticle(
            Headline: "Test Post",
            Description: "A test description",
            Url: "https://example.com/blog/test/",
            DatePublished: new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            AuthorName: "Jane Doe"
        );

        var json = JsonLdSerializer.SerializeArticle(article);

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
    public void SerializeArticle_NullDescription_OmitsDescription()
    {
        var article = new JsonLdArticle("Title", null, "https://example.com/", null, null);

        var json = JsonLdSerializer.SerializeArticle(article);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("description", out _).ShouldBeFalse();
    }

    [Fact]
    public void SerializeArticle_NullDate_OmitsDatePublished()
    {
        var article = new JsonLdArticle("Title", "Desc", "https://example.com/", null, null);

        var json = JsonLdSerializer.SerializeArticle(article);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("datePublished", out _).ShouldBeFalse();
    }

    [Fact]
    public void SerializeArticle_NullAuthor_OmitsAuthor()
    {
        var article = new JsonLdArticle("Title", null, "https://example.com/", null, null);

        var json = JsonLdSerializer.SerializeArticle(article);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("author", out _).ShouldBeFalse();
    }

    [Fact]
    public void SerializeArticle_AllOptionalNull_OnlyRequiredFields()
    {
        var article = new JsonLdArticle("Title", null, "https://example.com/", null, null);

        var json = JsonLdSerializer.SerializeArticle(article);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("Article");
        root.GetProperty("headline").GetString().ShouldBe("Title");
        root.GetProperty("url").GetString().ShouldBe("https://example.com/");
        root.EnumerateObject().Count().ShouldBe(4);
    }

    [Fact]
    public void SerializeBreadcrumbList_MultipleItems_ProducesCorrectPositions()
    {
        var breadcrumbs = new JsonLdBreadcrumbList([
            new JsonLdBreadcrumbItem(1, "Home", "https://example.com/"),
            new JsonLdBreadcrumbItem(2, "Docs", "https://example.com/docs/"),
            new JsonLdBreadcrumbItem(3, "API", null),
        ]);

        var json = JsonLdSerializer.SerializeBreadcrumbList(breadcrumbs);

        json.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("BreadcrumbList");

        var items = root.GetProperty("itemListElement");
        items.GetArrayLength().ShouldBe(3);

        items[0].GetProperty("@type").GetString().ShouldBe("ListItem");
        items[0].GetProperty("position").GetInt32().ShouldBe(1);
        items[0].GetProperty("name").GetString().ShouldBe("Home");
        items[0].GetProperty("item").GetString().ShouldBe("https://example.com/");

        items[1].GetProperty("position").GetInt32().ShouldBe(2);
        items[1].GetProperty("name").GetString().ShouldBe("Docs");

        items[2].GetProperty("position").GetInt32().ShouldBe(3);
        items[2].GetProperty("name").GetString().ShouldBe("API");
        items[2].TryGetProperty("item", out _).ShouldBeFalse();
    }

    [Fact]
    public void SerializeBreadcrumbList_EmptyList_ReturnsNull()
    {
        var breadcrumbs = new JsonLdBreadcrumbList([]);

        JsonLdSerializer.SerializeBreadcrumbList(breadcrumbs).ShouldBeNull();
    }

    [Fact]
    public void SerializeWebSite_FullFields_ProducesValidJsonLd()
    {
        var webSite = new JsonLdWebSite("My Site", "https://example.com/", "A great site");

        var json = JsonLdSerializer.SerializeWebSite(webSite);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("WebSite");
        root.GetProperty("name").GetString().ShouldBe("My Site");
        root.GetProperty("url").GetString().ShouldBe("https://example.com/");
        root.GetProperty("description").GetString().ShouldBe("A great site");
    }

    [Fact]
    public void SerializeWebSite_NullDescription_OmitsDescription()
    {
        var webSite = new JsonLdWebSite("My Site", "https://example.com/", null);

        var json = JsonLdSerializer.SerializeWebSite(webSite);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("description", out _).ShouldBeFalse();
    }

    [Fact]
    public void SerializeArticle_ContentWithScriptTag_EscapesClosingTag()
    {
        var article = new JsonLdArticle(
            Headline: "Using </script> in content",
            Description: null,
            Url: "https://example.com/",
            DatePublished: null,
            AuthorName: null
        );

        var json = JsonLdSerializer.SerializeArticle(article);

        json.ShouldNotContain("</script>");
        json.ShouldContain("<\\/script>");

        // Should still parse as valid JSON after un-escaping
        var unescaped = json.Replace("<\\/", "</");
        using var doc = JsonDocument.Parse(unescaped);
        doc.RootElement.GetProperty("headline").GetString().ShouldBe("Using </script> in content");
    }

}
