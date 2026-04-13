using CooklangSharp.Models;

namespace RecipeExample.Models;

public class RecipeContentPage
{
    public RecipeContentPage(
        Recipe recipe,
        RecipeFrontMatter frontMatter,
        string fileName,
        string url,
        string originalContent)
    {
        Recipe = recipe;
        FrontMatter = frontMatter;
        FileName = fileName;
        Url = url;
        OriginalContent = originalContent;
    }

    public Recipe Recipe { get; }
    public RecipeFrontMatter FrontMatter { get; }
    public string FileName { get; }
    public string Url { get; }
    public string OriginalContent { get; }

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(FrontMatter.Title) ? FrontMatter.Title :
        !string.IsNullOrWhiteSpace(FileName) ? FileName.Replace("-", " ").Replace("_", " ") :
        "Unknown Recipe";

    public string Slug => FileName.ToLowerInvariant();
}