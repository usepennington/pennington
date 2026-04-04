---
title: "Create Custom Content Service"
description: "Implement IContentService to integrate non-markdown content sources with Penn's pipeline"
uid: "penn.guides.custom-content-service"
order: 2100
---

Penn's built-in `MarkdownContentService<T>` handles markdown files. But sometimes your content lives in a database, an API, or a collection of YAML files describing recipes you'll never actually cook. This guide shows you how to implement <xref:T:Penn.Content.IContentService> to bring any content source into Penn's pipeline.

## Understanding IContentService

The <xref:T:Penn.Content.IContentService> interface is how Penn discovers and provides content. It defines five methods and two properties:

- `DiscoverAsync()` -- Returns an `IAsyncEnumerable<DiscoveredItem>` of all content this service is responsible for
- `GetContentTocEntriesAsync()` -- Navigation entries for the table of contents
- `GetCrossReferencesAsync()` -- Cross-references for `xref:` resolution
- `GetContentToCopyAsync()` -- Static files to copy to output
- `GetContentToCreateAsync()` -- Dynamically generated files (search indexes, etc.)
- `DefaultSection` -- The navigation section for this service's content
- `SearchPriority` -- Relative ranking in search results

Penn iterates over all registered `IContentService` implementations during both development and static generation. Every service contributes to the unified site. There is no priority or override mechanism -- just aggregation.

## The Pipeline and Union Types

Penn v2 uses C# 15 union types to model content as it flows through the pipeline. Understanding <xref:T:Penn.Pipeline.ContentItem> is key:

```csharp
// The four stages of content
public record DiscoveredItem(ContentRoute Route, ContentSource Source);
public record ParsedItem(ContentRoute Route, IFrontMatter Metadata, string RawMarkdown);
public record RenderedItem(ContentRoute Route, IFrontMatter Metadata, RenderedContent Content);
public record FailedItem(ContentRoute Route, ContentError Error);

// The union -- compiler enforces exhaustive matching
public union ContentItem(DiscoveredItem, ParsedItem, RenderedItem, FailedItem);
```

Your `IContentService` produces <xref:T:Penn.Pipeline.DiscoveredItem> values. Each carries a `ContentRoute` (where) and a <xref:T:Penn.Pipeline.ContentSource> (what):

```csharp
public record MarkdownFileSource(FilePath Path);
public record RazorPageSource(string ComponentType);
public record RedirectSource(UrlPath TargetUrl);
public record ProgrammaticSource(IProgrammaticContentGenerator Generator);

public union ContentSource(MarkdownFileSource, RazorPageSource, RedirectSource, ProgrammaticSource);
```

For custom content services, `ProgrammaticSource` is your friend. It wraps an `IProgrammaticContentGenerator` that Penn calls when it needs the actual content.

## Basic Implementation

Here's a custom content service that serves recipes from an in-memory collection. In production, this could be a database, an API, or a filing cabinet you've painstakingly digitized:

```csharp
using System.Collections.Immutable;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

public class RecipeContentService : IContentService
{
    private readonly List<Recipe> _recipes;

    public RecipeContentService(IRecipeRepository repository)
    {
        _recipes = repository.GetAll().ToList();
    }

    public string DefaultSection => "recipes";
    public int SearchPriority => 5;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var recipe in _recipes)
        {
            var route = ContentRouteFactory.FromUrl(
                new UrlPath($"/recipes/{recipe.Slug}"));

            var generator = new RecipeContentGenerator(recipe);
            var source = new ContentSource(new ProgrammaticSource(generator));

            yield return new DiscoveredItem(route, source);
        }

        await Task.CompletedTask;
    }

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var recipe in _recipes)
        {
            var route = ContentRouteFactory.FromUrl(
                new UrlPath($"/recipes/{recipe.Slug}"));

            builder.Add(new ContentTocItem(
                Title: recipe.Name,
                Route: route,
                Order: recipe.SortOrder,
                HierarchyParts: ["recipes", recipe.Category, recipe.Slug],
                Section: DefaultSection
            ));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var recipe in _recipes.Where(r => r.Uid is not null))
        {
            var route = ContentRouteFactory.FromUrl(
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

## The IProgrammaticContentGenerator

The `ProgrammaticSource` variant of <xref:T:Penn.Pipeline.ContentSource> wraps an `IProgrammaticContentGenerator`. Penn calls `GenerateAsync` when it actually needs the content -- not at discovery time. This is important for performance: discovery should be fast; content generation can be lazy.

`GenerateAsync` returns a `ProgrammaticContent` union -- either `TextProgrammaticContent` (for HTML/markdown) or `BinaryProgrammaticContent` (for images, PDFs, etc.):

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
        };

        var markdown = $"""
            ## Ingredients

            {string.Join("\n", _recipe.Ingredients.Select(i => $"- {i}"))}

            ## Instructions

            {string.Join("\n", _recipe.Steps.Select((s, i) => $"{i + 1}. {s}"))}
            """;

        var content = new TextProgrammaticContent(frontMatter, markdown);
        return Task.FromResult<ProgrammaticContent>(content);
    }
}
```

> [!NOTE]
> If your `TextProgrammaticContent` has a `ContentType` of `"text/html"`, Penn treats the `RawContent` as already-rendered HTML. If it's anything else (or omitted), Penn processes it as markdown through the standard rendering pipeline. Choose wisely.

## Service Registration

Register your custom content service in `Program.cs` as an `IContentService`:

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

Penn collects all `IContentService` registrations from the DI container. Your custom service sits alongside `MarkdownContentService<T>` registrations from `AddMarkdownContent` calls. There's no special ceremony required.

## Caching and File Watching

For content services backed by external data, you may want cache invalidation when files change. Penn's `FileWatchDependencyFactory<T>` manages a cached instance that auto-invalidates when `IFileWatcher` detects changes:

```csharp
// Register with file-watch support
builder.Services.AddSingleton<FileWatchDependencyFactory<RecipeContentService>>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<FileWatchDependencyFactory<RecipeContentService>>().GetInstance());
```

When any watched file changes, `FileWatchDependencyFactory` disposes the existing instance and creates a fresh one on next access. This is the same pattern Penn's built-in content services use. It is not sophisticated. It is reliable.

## Understanding Hierarchy Parts

The `ContentTocItem` includes hierarchy parts -- an array of strings defining where the entry sits in the navigation tree. This is independent of URL structure:

```csharp
// Top-level "Recipes" entry
new ContentTocItem("Recipes", route, 100, ["recipes"], "recipes")

// Recipes -> Desserts -> "Chocolate Cake"
new ContentTocItem("Chocolate Cake", route, 200,
    ["recipes", "desserts", "chocolate-cake"], "recipes")

// Recipes -> Mains -> "Pad Thai"
new ContentTocItem("Pad Thai", route, 300,
    ["recipes", "mains", "pad-thai"], "recipes")
```

This gives you full control over navigation without coupling it to URL paths. The hierarchy parts define nesting; the `Section` property groups items into navigation regions.

## Performance Considerations

- **Discovery should be fast**: `DiscoverAsync()` is called frequently. Keep it lightweight -- enumerate routes, don't generate content.
- **Content generation is lazy**: `IProgrammaticContentGenerator.GenerateAsync()` is called on-demand. This is where expensive work belongs.
- **Cache when possible**: Use `FileWatchDependencyFactory<T>` or your own caching to avoid recreating expensive objects on every request.
- **Return empty immutable lists, not null**: Penn uses `ImmutableList<T>.Empty` throughout. Follow the convention.
