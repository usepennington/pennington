namespace BlogKitchenSinkExample;

using System.Text.Json.Serialization;
using Pennington.BlogSite;
using Pennington.StructuredData;

/// <summary>
/// Demonstrates extending Pennington's JSON-LD surface with a user-defined
/// schema.org type. <see cref="JsonLdRecipe"/> derives from
/// <see cref="JsonLdEntity"/>, attributes its fields with
/// <see cref="JsonPropertyNameAttribute"/>, and serializes through the
/// same <see cref="JsonLdSerializer"/> the framework uses — no framework
/// changes required. Pass an instance to the <c>&lt;StructuredData&gt;</c>
/// component's <c>Entities</c> parameter to emit it in the page head.
/// </summary>
public sealed record JsonLdRecipe : JsonLdEntity
{
    /// <inheritdoc />
    [JsonPropertyName("@type")]
    public override string Type => "Recipe";

    /// <summary>Recipe name.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>Short description of the dish.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Servings count, e.g. "4 servings".</summary>
    [JsonPropertyName("recipeYield")]
    public string? RecipeYield { get; init; }

    /// <summary>Prep duration as an ISO 8601 duration, e.g. "PT15M".</summary>
    [JsonPropertyName("prepTime")]
    public string? PrepTime { get; init; }

    /// <summary>Cook duration as an ISO 8601 duration, e.g. "PT30M".</summary>
    [JsonPropertyName("cookTime")]
    public string? CookTime { get; init; }

    /// <summary>One-line ingredient strings, with amount and unit baked in.</summary>
    [JsonPropertyName("recipeIngredient")]
    public required IReadOnlyList<string> Ingredients { get; init; }

    /// <summary>Step text, one entry per instruction.</summary>
    [JsonPropertyName("recipeInstructions")]
    public required IReadOnlyList<string> Instructions { get; init; }
}

/// <summary>
/// Helper that projects a <see cref="BlogSiteFrontMatter"/> into a sample
/// <see cref="JsonLdRecipe"/>. In a real recipe site the values would come
/// from the post's own front matter type rather than the BlogSite default —
/// this helper exists so the example exercises the same serialization path
/// the docs site teaches.
/// </summary>
public static class StructuredDataBuilder
{
    /// <summary>Returns the example recipe as serialized JSON-LD.</summary>
    public static string BuildSampleRecipeJson() =>
        JsonLdSerializer.Serialize(SampleRecipe);

    /// <summary>A hand-authored sample recipe used for documentation snippets.</summary>
    public static JsonLdRecipe SampleRecipe { get; } = new()
    {
        Name = "Weeknight pasta with garlic and oil",
        Description = "A pantry-friendly pasta with toasted garlic, olive oil, and chili.",
        RecipeYield = "4 servings",
        PrepTime = "PT5M",
        CookTime = "PT15M",
        Ingredients =
        [
            "1 lb spaghetti",
            "6 cloves garlic, thinly sliced",
            "1/2 cup extra-virgin olive oil",
            "1 tsp red pepper flakes",
            "Salt to taste",
        ],
        Instructions =
        [
            "Bring a pot of salted water to a boil and cook the spaghetti to al dente.",
            "While the pasta cooks, warm the olive oil and garlic in a skillet over low heat until the garlic just turns golden.",
            "Add the red pepper flakes and a ladle of pasta water; swirl to emulsify.",
            "Drain the pasta, toss with the oil, and serve immediately.",
        ],
    };
}
