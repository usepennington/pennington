---
title: "Create Custom Content Service"
description: "Implement IContentService to integrate non-markdown content sources with Penn's pipeline"
uid: "penn.guides.custom-content-service"
order: 2100
---

Penn's built-in `MarkdownContentService<T>` handles markdown files. When your content lives in a database, an API, a collection of `.cook` files, or some other format entirely, you implement `IContentService` to bring it into the pipeline. This guide walks through the full interface, the union types that carry content through the pipeline, and a complete working implementation.

## Understanding IContentService

The `IContentService` interface defines five methods and two properties. Every registered implementation contributes to the unified site during both development and static generation.

```csharp
public interface IContentService
{
    IAsyncEnumerable<DiscoveredItem> DiscoverAsync();
    Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync();
    Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync();
    Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync();
    Task<ImmutableList<CrossReference>> GetCrossReferencesAsync();

    string DefaultSection { get; }
    int SearchPriority { get; }
}
```

The methods:

- **`DiscoverAsync()`** -- Yields `DiscoveredItem` values, each pairing a `ContentRoute` (where the content lives in the URL space) with a `ContentSource` (how to obtain the content). Called at the start of every pipeline run.
- **`GetContentTocEntriesAsync()`** -- Returns navigation entries. Each `ContentTocItem` positions content in the table-of-contents tree.
- **`GetCrossReferencesAsync()`** -- Returns `CrossReference` values that map UIDs to routes. Other content can then use `xref:your.uid` to link to your pages.
- **`GetContentToCopyAsync()`** -- Returns `ContentToCopy` pairs (source file, output path) for static assets like images. Penn copies these during static generation.
- **`GetContentToCreateAsync()`** -- Returns `ContentToCreate` entries for dynamically generated files (search indexes, sitemaps, RSS feeds). Each carries a `Func<Task<byte[]>>` that Penn calls at build time.

The properties:

- **`DefaultSection`** -- The navigation section this service's content belongs to. Used when individual items don't specify their own section.
- **`SearchPriority`** -- Relative ranking weight in search results. Lower values rank higher.

Penn iterates over all registered `IContentService` implementations and aggregates results. There is no priority or override mechanism between services -- just aggregation.

## The Pipeline and Union Types

Content flows through four stages, modeled as cases of the `ContentItem` union:

```csharp
public record DiscoveredItem(ContentRoute Route, ContentSource Source);
public record ParsedItem(ContentRoute Route, IFrontMatter Metadata, string RawMarkdown);
public record RenderedItem(ContentRoute Route, IFrontMatter Metadata, RenderedContent Content);
public record FailedItem(ContentRoute Route, ContentError Error);

public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem);
```

Your `IContentService` produces `DiscoveredItem` values. The pipeline advances them through parsing and rendering. The compiler enforces exhaustive matching on the union, so nothing gets silently dropped.

Each `DiscoveredItem` carries a `ContentSource` that tells the pipeline what kind of content it's dealing with:

```csharp
public record MarkdownFileSource(FilePath Path);
public record RazorPageSource(string ComponentType);
public record RedirectSource(UrlPath TargetUrl);
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

public union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource);
```

For custom content services, `ProgrammaticSource` is the one you want. It wraps an `IProgrammaticContentGenerator` that Penn calls when it needs actual content. The other three cases are used internally by `MarkdownContentService`, `RazorPageContentService`, and the redirect system.

For a deeper look at how the pipeline processes these types, see <xref:penn.under-the-hood.content-processing-pipeline>.

## Building a Recipe Content Service

Here's a complete `IContentService` implementation that serves recipes from an in-memory collection. In production, the data source could be a database, an external API, or flat files in a custom format.

Start with a model and a front matter type:

```csharp
public record Recipe(
    string Slug,
    string Name,
    string Category,
    string Summary,
    string[] Ingredients,
    string[] Steps,
    string? Uid = null,
    int SortOrder = 0);

public record RecipeFrontMatter : IFrontMatter, IDescribable, ICrossReferenceable
{
    public string Title { get; init; } = "Untitled";
    public string? Description { get; init; }
    public string? Uid { get; init; }
}
```

Now the content service:

```csharp
using System.Collections.Immutable;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

public class RecipeContentService : IContentService
{
    private readonly IRecipeRepository _repository;
    private List<Recipe>? _recipes;

    public RecipeContentService(IRecipeRepository repository)
    {
        _repository = repository;
    }

    public string DefaultSection => "recipes";
    public int SearchPriority => 5;

    private List<Recipe> Recipes => _recipes ??= _repository.GetAll().ToList();

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var recipe in Recipes)
        {
            var route = ContentRouteFactory.FromCustom(
                new UrlPath($"/recipes/{recipe.Slug}"));

            var generator = new RecipeContentGenerator(recipe);
            ContentSource source = new ProgrammaticSource(generator);

            yield return new DiscoveredItem(route, source);
        }

        await Task.CompletedTask;
    }

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var recipe in Recipes)
        {
            var route = ContentRouteFactory.FromCustom(
                new UrlPath($"/recipes/{recipe.Slug}"));

            builder.Add(new ContentTocItem(
                Title: recipe.Name,
                Route: route,
                Order: recipe.SortOrder,
                HierarchyParts: ["recipes", recipe.Category, recipe.Slug],
                Section: DefaultSection,
                Locale: null));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var recipe in Recipes.Where(r => r.Uid is not null))
        {
            var route = ContentRouteFactory.FromCustom(
                new UrlPath($"/recipes/{recipe.Slug}"));
            builder.Add(new CrossReference(recipe.Uid!, recipe.Name, route));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
}
```

Several things to note:

- **`ContentRouteFactory.FromCustom`** builds a `ContentRoute` from a URL path. It handles trailing slashes and output file paths (`recipes/pad-thai/index.html`) automatically. Use `FromCustom` for non-markdown content; use `FromUrl` when you don't need to attach a source file.
- **`ProgrammaticSource`** wraps the generator. Penn calls `GenerateAsync` later -- not during discovery.
- **Empty methods return `ImmutableList<T>.Empty`**, not `null`. Penn expects non-null returns throughout.
- **`ContentTocItem` takes six arguments**: `Title`, `Route`, `Order`, `HierarchyParts`, `Section`, and `Locale`. All six are required. Pass `null` for `Locale` when your content isn't localized.

## IProgrammaticContentGenerator

The `ProgrammaticSource` case of `ContentSource` wraps an `IProgrammaticContentGenerator`. Penn calls `GenerateAsync` when it needs the content -- not at discovery time. This separation matters for performance: discovery should be fast; content generation can be lazy.

```csharp
public interface IProgrammaticContentGenerator
{
    Task<ProgrammaticContent> GenerateAsync(ContentRoute route);
}
```

`GenerateAsync` returns a `ProgrammaticContent` union with two cases:

```csharp
public record TextProgrammaticContent(
    IFrontMatter? Metadata,
    string RawContent,
    string ContentType = "text/html");

public record BinaryProgrammaticContent(
    Func<Task<byte[]>> ByteGenerator,
    string ContentType);

public union ProgrammaticContent(TextProgrammaticContent, BinaryProgrammaticContent);
```

### Text content

For the recipe service, the generator produces markdown that Penn processes through the standard rendering pipeline:

```csharp
public class RecipeContentGenerator : IProgrammaticContentGenerator
{
    private readonly Recipe _recipe;

    public RecipeContentGenerator(Recipe recipe) => _recipe = recipe;

    public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
    {
        var frontMatter = new RecipeFrontMatter
        {
            Title = _recipe.Name,
            Description = _recipe.Summary,
            Uid = _recipe.Uid,
        };

        var markdown = $"""
            ## Ingredients

            {string.Join("\n", _recipe.Ingredients.Select(i => $"- {i}"))}

            ## Instructions

            {string.Join("\n", _recipe.Steps.Select((s, i) => $"{i + 1}. {s}"))}
            """;

        ProgrammaticContent content = new TextProgrammaticContent(
            frontMatter, markdown, "text/markdown");
        return Task.FromResult(content);
    }
}
```

The `ContentType` parameter on `TextProgrammaticContent` controls how Penn handles the `RawContent`:

- `"text/html"` (the default) -- Penn treats the content as already-rendered HTML and skips the markdown pipeline.
- Any other value (e.g., `"text/markdown"`) -- Penn processes the content through the standard Parse and Render stages.

Choose based on whether your content source produces raw markup or markdown that needs processing.

### Binary content

For binary assets like generated images or PDFs, return `BinaryProgrammaticContent`. The pipeline pattern-matches the union and writes bytes directly, skipping Parse and Render entirely:

```csharp
public class ThumbnailGenerator : IProgrammaticContentGenerator
{
    private readonly string _sourceImagePath;
    private readonly int _width;

    public ThumbnailGenerator(string sourceImagePath, int width)
    {
        _sourceImagePath = sourceImagePath;
        _width = width;
    }

    public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
    {
        ProgrammaticContent content = new BinaryProgrammaticContent(
            ByteGenerator: async () =>
            {
                var bytes = await File.ReadAllBytesAsync(_sourceImagePath);
                return ResizeImage(bytes, _width);
            },
            ContentType: "image/webp");

        return Task.FromResult(content);
    }

    private static byte[] ResizeImage(byte[] source, int width)
    {
        // Image processing logic here
        return source;
    }
}
```

The `ByteGenerator` is a `Func<Task<byte[]>>`, so the actual byte generation is deferred until output time. This keeps discovery and pipeline processing fast even when generating hundreds of image variants.

## ContentTocItem and Hierarchy Parts

The `ContentTocItem` record controls where your content appears in the navigation tree:

```csharp
public record ContentTocItem(
    string Title,
    ContentRoute Route,
    int Order,
    string[] HierarchyParts,
    string? Section,
    string? Locale);
```

The `HierarchyParts` array defines the nesting structure. It is independent of the URL -- you control the navigation tree without coupling it to URL paths.

```csharp
// Top-level group header (no leaf page, just a category node)
new ContentTocItem("Recipes", route, 0,
    ["recipes"], "recipes", null)

// Recipes > Desserts > Chocolate Cake
new ContentTocItem("Chocolate Cake", route, 100,
    ["recipes", "desserts", "chocolate-cake"], "recipes", null)

// Recipes > Mains > Pad Thai
new ContentTocItem("Pad Thai", route, 200,
    ["recipes", "mains", "pad-thai"], "recipes", null)
```

The `NavigationBuilder` uses these parts to construct the tree. Items sharing the same prefix are grouped. The `Order` field controls sorting within each level -- lower values appear first.

The `Section` property groups items into navigation regions. The built-in `MarkdownContentService<T>` sets this from the `ISectionable` capability on front matter, falling back to the service's `DefaultSection`. Your custom service can use the same pattern or hardcode a section.

`Locale` enables multi-language navigation. Pass `null` for single-locale sites. When set, Penn builds separate navigation trees per locale.

## Service Registration

Register your custom content service as an `IContentService` in `Program.cs`:

```csharp
builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Recipe Site";
    penn.ContentRootPath = "Content";

    // Markdown docs alongside custom content
    penn.AddMarkdownContent<DocFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/docs";
        opts.BasePageUrl = "/docs";
    });
});

// Register the custom content service
builder.Services.AddSingleton<IContentService, RecipeContentService>();
```

Penn resolves all `IContentService` registrations from the DI container. Your custom service sits alongside the `MarkdownContentService<T>` instances that `AddMarkdownContent` creates. No special ceremony needed.

If your service needs constructor parameters beyond what DI can resolve, use the factory overload:

```csharp
builder.Services.AddSingleton<IContentService>(sp =>
{
    var repository = sp.GetRequiredService<IRecipeRepository>();
    return new RecipeContentService(repository);
});
```

You can also register the service under its own interface in addition to `IContentService`, so other parts of your application can consume it directly:

```csharp
var recipeService = new RecipeContentService(recipePath);
builder.Services.AddSingleton<IRecipeContentService>(recipeService);
builder.Services.AddSingleton<IContentService>(recipeService);
```

This dual-registration pattern is common when Razor components or API endpoints need to query the same data the content service exposes. See <xref:penn.guides.multiple-content-sources> for more on combining content services.

## Caching with FileWatchDependencyFactory

When your content service reads from the file system, you want cache invalidation during development. Penn's `FileWatchDependencyFactory<T>` manages a cached instance that auto-invalidates when `IFileWatcher` detects changes.

The simplest approach is `AddFileWatched`, which registers the factory and wires up transient resolution:

```csharp
builder.Services.AddFileWatched<IContentService, RecipeContentService>();
```

This registers a `FileWatchDependencyFactory<RecipeContentService>` as a singleton. The factory creates a `RecipeContentService` instance on first access and caches it. When any watched file changes, the factory disposes the old instance and creates a new one on next access.

For the file watcher to know about your content directory, your service constructor should register a watch via `IFileWatcher`:

```csharp
public class RecipeContentService : IContentService
{
    private readonly string _contentPath;

    public RecipeContentService(string contentPath, IFileWatcher fileWatcher)
    {
        _contentPath = contentPath;
        fileWatcher.AddPathWatch(_contentPath, "*.cook", (_, _) => { });
    }

    // ... rest of the implementation
}
```

`AddPathWatch` tells the central file watcher to monitor the directory. When a `.cook` file changes, `FileWatchDependencyFactory` disposes the current `RecipeContentService` and creates a fresh one. The callback parameter on `AddPathWatch` is for service-level notifications -- pass an empty lambda when the factory handles invalidation.

If you need the manual factory pattern instead (for dual-registration or other DI scenarios):

```csharp
builder.Services.AddSingleton<FileWatchDependencyFactory<RecipeContentService>>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<FileWatchDependencyFactory<RecipeContentService>>()
        .GetInstance());
```

This is the same pattern Penn's built-in services use. It is not sophisticated. It is reliable.

## Performance Considerations

**Keep discovery fast.** `DiscoverAsync()` runs on every pipeline execution. It should enumerate routes and create lightweight generator objects, not read files or query databases. Defer expensive work to `IProgrammaticContentGenerator.GenerateAsync()`.

**Content generation is lazy.** `GenerateAsync()` is called when the pipeline actually needs the content. During development, this happens on-demand per request. During static build, it happens once per page. Put expensive work here.

**Use `AsyncLazy<T>` for metadata caching.** If your TOC entries and cross-references require parsing files, cache the results. Penn's `MarkdownContentService<T>` uses `AsyncLazy<ImmutableList<T>>` to parse front matter once and share results across `GetContentTocEntriesAsync()` and `GetCrossReferencesAsync()`:

```csharp
private readonly AsyncLazy<ImmutableList<RecipeMetadata>> _metadataLazy;

public RecipeContentService(string contentPath)
{
    _metadataLazy = new AsyncLazy<ImmutableList<RecipeMetadata>>(
        LoadAllMetadataAsync);
}

public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
{
    var metadata = await _metadataLazy.Value;
    // Build TOC entries from cached metadata
}
```

**Return empty immutable lists, not null.** Penn calls all five methods on every service. Returning `null` will throw. Use `ImmutableList<ContentToCopy>.Empty` for methods your service doesn't need.

**Binary generators defer byte creation.** `BinaryProgrammaticContent` takes a `Func<Task<byte[]>>`, not a `byte[]`. The function is called at output time, so the pipeline can process hundreds of binary items without holding them all in memory simultaneously.

**Avoid duplicate URLs across services.** Penn aggregates content from all `IContentService` implementations without deduplication. If two services produce the same canonical path, you get undefined behavior during rendering and static generation. Check your URL schemes before registering multiple services. See <xref:penn.reference.front-matter-properties> for how front matter UIDs factor into cross-reference resolution.
