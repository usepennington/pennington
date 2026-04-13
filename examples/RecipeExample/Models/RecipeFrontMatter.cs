using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace RecipeExample.Models;

public class RecipeFrontMatter
{
    [YamlMember(Alias = "servings")]
    [JsonPropertyName("servings")]
    public string? Servings { get; set; }

    [YamlMember(Alias = "prep time")]
    [JsonPropertyName("prep_time")]
    public string? PrepTime { get; set; }

    [YamlMember(Alias = "cook time")]
    [JsonPropertyName("cook_time")]
    public string? CookTime { get; set; }

    [YamlMember(Alias = "rest time")]
    [JsonPropertyName("rest_time")]
    public string? RestTime { get; set; }

    [YamlMember(Alias = "tags")]
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [YamlMember(Alias = "title")]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [YamlMember(Alias = "description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}