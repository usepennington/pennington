using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CooklangSharp;
using Penn.Content;
using Penn.Pipeline;
using Penn.Routing;
using RecipeExample.Models;
using YamlDotNet.Serialization;

namespace RecipeExample;

public interface IRecipeContentService : IContentService
{
    Task<RecipeContentPage?> GetRecipeByUrlOrDefault(string url);
    Task<ImmutableList<RecipeContentPage>> GetAllRecipesAsync();
}

public partial class RecipeContentService : IRecipeContentService
{
    public string DefaultSection => "recipes";
    public int SearchPriority => 10;

    private readonly string _recipePath;
    private readonly string _filePattern;
    private readonly IDeserializer _yamlDeserializer;
    private ConcurrentDictionary<string, RecipeContentPage>? _cache;

    public RecipeContentService(string recipePath, string filePattern = "*.cook")
    {
        _recipePath = recipePath;
        _filePattern = filePattern;
        _yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
    }

    private async Task<ConcurrentDictionary<string, RecipeContentPage>> GetCacheAsync()
    {
        if (_cache != null) return _cache;

        var recipes = new ConcurrentDictionary<string, RecipeContentPage>();

        if (!Directory.Exists(_recipePath))
            return _cache = recipes;

        var recipeFiles = Directory.GetFiles(_recipePath, _filePattern);

        foreach (var filePath in recipeFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var url = $"/recipes/{fileName}";

                var (frontMatter, recipeContent) = ParseFrontMatter(content);
                var parseResult = CooklangParser.Parse(recipeContent);

                if (parseResult.Recipe != null)
                {
                    var recipePage = new RecipeContentPage(
                        parseResult.Recipe, frontMatter, fileName, url, content);
                    recipes.TryAdd(url, recipePage);
                }
            }
            catch
            {
                // Skip invalid recipe files
            }
        }

        return _cache = recipes;
    }

    private (RecipeFrontMatter FrontMatter, string Content) ParseFrontMatter(string content)
    {
        var frontMatter = new RecipeFrontMatter();
        var recipeContent = content;

        var match = FrontMatterRegex().Match(content);
        if (match.Success)
        {
            var yamlContent = match.Groups[1].Value;
            recipeContent = match.Groups[2].Value;

            try
            {
                frontMatter = _yamlDeserializer.Deserialize<RecipeFrontMatter>(yamlContent);
            }
            catch
            {
                frontMatter = new RecipeFrontMatter();
            }
        }

        return (frontMatter, recipeContent);
    }

    public async Task<RecipeContentPage?> GetRecipeByUrlOrDefault(string url)
    {
        var data = await GetCacheAsync();
        return data.GetValueOrDefault(url.TrimEnd('/'));
    }

    public async Task<ImmutableList<RecipeContentPage>> GetAllRecipesAsync()
    {
        var data = await GetCacheAsync();
        return data.Values.ToImmutableList();
    }

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var data = await GetCacheAsync();
        foreach (var (url, page) in data)
        {
            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath(url),
                OutputFile = new FilePath($"{url.TrimStart('/')}/index.html"),
            };
            var source = new ContentSource(new RazorPageSource(page.DisplayName));
            yield return new DiscoveredItem(route, source);
        }
    }

    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var data = await GetCacheAsync();
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var (url, page) in data)
        {
            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath(url),
                OutputFile = new FilePath($"{url.TrimStart('/')}/index.html"),
            };
            var hierarchyParts = url.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            builder.Add(new ContentTocItem(
                Title: page.DisplayName,
                Route: route,
                Order: 0,
                HierarchyParts: hierarchyParts,
                Section: "recipes",
                Locale: null));
        }

        return builder.ToImmutable();
    }

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    [GeneratedRegex(@"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n(.*)",
        RegexOptions.Singleline | RegexOptions.Multiline)]
    private static partial Regex FrontMatterRegex();
}
