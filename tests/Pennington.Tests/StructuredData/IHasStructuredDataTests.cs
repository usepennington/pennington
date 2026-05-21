using System.Text.Json;
using System.Text.Json.Serialization;
using Pennington.BlogSite;
using Pennington.StructuredData;

namespace Pennington.Tests.StructuredData;

public class IHasStructuredDataTests
{
    [Fact]
    public void BlogSiteFrontMatter_ImplementsIHasStructuredData()
    {
        var fm = new BlogSiteFrontMatter { Title = "Post" };

        fm.ShouldBeAssignableTo<IHasStructuredData>();
    }

    [Fact]
    public void BlogSiteFrontMatter_EmitsArticleWithFrontMatterAuthor()
    {
        var fm = new BlogSiteFrontMatter
        {
            Title = "Post",
            Description = "Summary",
            Author = "Jamie",
            Date = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        var ctx = new StructuredDataContext
        {
            CanonicalUrl = "https://example.com/blog/post/",
            FallbackAuthorName = "Site Author",
        };

        var entity = fm.GetStructuredData(ctx).Single();
        var json = JsonLdSerializer.Serialize(entity);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@type").GetString().ShouldBe("Article");
        root.GetProperty("headline").GetString().ShouldBe("Post");
        root.GetProperty("description").GetString().ShouldBe("Summary");
        root.GetProperty("url").GetString().ShouldBe("https://example.com/blog/post/");
        root.GetProperty("datePublished").GetString().ShouldBe("2026-04-01T00:00:00Z");
        root.GetProperty("author").GetProperty("name").GetString().ShouldBe("Jamie");
    }

    [Fact]
    public void BlogSiteFrontMatter_UsesFallbackAuthorWhenFrontMatterAuthorEmpty()
    {
        var fm = new BlogSiteFrontMatter { Title = "Post", Author = "" };
        var ctx = new StructuredDataContext
        {
            CanonicalUrl = "https://example.com/blog/post/",
            FallbackAuthorName = "Site Author",
        };

        var entity = fm.GetStructuredData(ctx).Single();
        var json = JsonLdSerializer.Serialize(entity);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("author").GetProperty("name").GetString().ShouldBe("Site Author");
    }

    [Fact]
    public void BlogSiteFrontMatter_OmitsAuthorWhenBothAuthorAndFallbackEmpty()
    {
        var fm = new BlogSiteFrontMatter { Title = "Post", Author = "" };
        var ctx = new StructuredDataContext { CanonicalUrl = "https://example.com/blog/post/" };

        var entity = fm.GetStructuredData(ctx).Single();
        var json = JsonLdSerializer.Serialize(entity);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("author", out _).ShouldBeFalse();
    }

    [Fact]
    public void UserDefinedFrontMatter_CanImplementIHasStructuredData()
    {
        // Smoke test: any consumer can wire the capability to emit a
        // user-defined entity (here, a stand-in for JsonLdRecipe) without
        // touching the framework. This is the seam recipe/product/academic
        // sites use.
        var recipe = new TestRecipeFrontMatter
        {
            Title = "Pasta Aglio e Olio",
            Description = "Pantry pasta with toasted garlic and chili.",
            ServingsLabel = "4 servings",
        };

        var ctx = new StructuredDataContext { CanonicalUrl = "https://example.com/recipes/pasta/" };

        var entity = recipe.GetStructuredData(ctx).Single();
        var json = JsonLdSerializer.Serialize(entity);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("@type").GetString().ShouldBe("Recipe");
        root.GetProperty("name").GetString().ShouldBe("Pasta Aglio e Olio");
        root.GetProperty("description").GetString().ShouldBe("Pantry pasta with toasted garlic and chili.");
        root.GetProperty("recipeYield").GetString().ShouldBe("4 servings");
        root.GetProperty("url").GetString().ShouldBe("https://example.com/recipes/pasta/");
    }

    private sealed record TestRecipeFrontMatter : Pennington.FrontMatter.IFrontMatter, IHasStructuredData
    {
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public string? ServingsLabel { get; init; }

        public IEnumerable<JsonLdEntity> GetStructuredData(StructuredDataContext context)
        {
            yield return new TestRecipe
            {
                Name = Title,
                Description = Description,
                Url = context.CanonicalUrl,
                RecipeYield = ServingsLabel,
            };
        }
    }

    private sealed record TestRecipe : JsonLdEntity
    {
        [JsonPropertyName("@type")]
        public override string Type => "Recipe";

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("url")]
        public required string Url { get; init; }

        [JsonPropertyName("recipeYield")]
        public string? RecipeYield { get; init; }
    }
}
