---
title: "Create Custom Content Service"
description: "Implement a custom IContentService to handle specialized content sources and processing requirements"
uid: "docs.guides.custom-content-service"
order: 2100
---

This guide shows you how to create a custom `IContentService` implementation to integrate specialized content sources
with MyLittleContentEngine. Custom content services are useful when you need to pull content from databases, APIs, or
other non-file sources.

## Understanding IContentService

The `IContentService` interface defines how MyLittleContentEngine discovers and processes content. It provides five key
methods:

- `GetPagesToGenerateAsync()` - Returns all pages that should be generated
- `GetContentTocEntriesAsync()` - Returns table of contents entries with hierarchy information for navigation
- `GetContentToCopyAsync()` - Returns static assets to copy
- `GetCrossReferencesAsync()` - Returns cross-references for linking
- `GetContentToCreateAsync()` - Returns content that should be created dynamically during generation

`MarkdownContentService<TFrontMatter>` and `ApiReferenceContentService` are both built-in implementations of `IContentService` 
that handle Markdown files and API references, respectively. You can create your own implementation to handle content
from other sources, such as a database or an external API. Keep in mind that during development, the content service might
appear dynamic, but it's designed to be static for production builds. 

## Basic Implementation

Here's a minimal custom content service that loads content from a database:

```csharp
using System.Collections.Immutable;
using MyLittleContentEngine.Models;
using MyLittleContentEngine.Services.Content;
using MyLittleContentEngine.Services.Content.TableOfContents;

public class DatabaseContentService : IContentService
{
    private readonly IDbContext _dbContext;
    private readonly IMarkdownProcessor _markdownProcessor;

    public DatabaseContentService(IDbContext dbContext, IMarkdownProcessor markdownProcessor)
    {
        _dbContext = dbContext;
        _markdownProcessor = markdownProcessor;
    }

    public async Task<ImmutableList<PageToGenerate>> GetPagesToGenerateAsync()
    {
        var articles = await _dbContext.Articles
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.PublishedDate)
            .ToListAsync();

        var pages = articles.Select(article => new PageToGenerate
        {
            Url = $"/articles/{article.Slug}",
            Title = article.Title,
            Content = _markdownProcessor.ToHtml(article.Content),
            FrontMatter = new ArticleFrontMatter
            {
                Title = article.Title,
                Description = article.Summary,
                Tags = article.Tags?.Split(',') ?? [],
                PublishedDate = article.PublishedDate
            }
        }).ToImmutableList();

        return pages;
    }

    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        // Example 1: Simple top-level entry
        var articlesIndex = new ContentTocItem(
            "Articles",
            "/articles", 
            100, // Order
            ["articles"] // Hierarchy parts - creates top-level "Articles" section
        );

        // Example 2: Nested hierarchy with category grouping
        var categories = await _dbContext.Categories.ToListAsync();
        var categoryEntries = categories.Select(cat => new ContentTocItem(
            cat.Name,
            $"/articles/category/{cat.Slug}",
            200 + cat.Order,
            ["articles", "categories", cat.Slug] // Creates Articles -> Categories -> [Category Name]
        ));

        return [articlesIndex, ..categoryEntries];
    }

    public async Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
    {
        // Copy any uploaded images from the database to the output
        var images = await _dbContext.ArticleImages.ToListAsync();
        
        return images.Select(img => new ContentToCopy
        {
            SourcePath = img.FilePath,
            DestinationPath = $"images/{img.FileName}"
        }).ToImmutableList();
    }

    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var articles = await _dbContext.Articles.ToListAsync();
        
        return articles.Select(article => new CrossReference
        {
            Id = article.Id.ToString(),
            Title = article.Title,
            Url = $"/articles/{article.Slug}",
            Type = "article"
        }).ToImmutableList();
    }
}
```

## Service Registration

Register your custom content service in `Program.cs`, both when the concrete type is used and when it's registered as
an `IContentService`. You can use this pattern to ensure the same instance is used throughout the application:

```csharp
// For services that need file-watch cache invalidation, use AddFileWatched
var configuredServices = new ConfiguredContentEngineServiceCollection(builder.Services);
configuredServices.AddFileWatched<DatabaseContentService>();

// Register as IContentService (this allows multiple IContentService implementations)  
builder.Services.AddSingleton<IContentService>(provider => provider.GetRequiredService<DatabaseContentService>());
```

For multiple content services, the framework will combine results from all registered services for site generation and 
the table of contents.

## Understanding Hierarchy Parts

The `GetContentTocEntriesAsync()` method returns `ContentTocItem` objects that include hierarchy parts - an array of strings that defines where the entry appears in the navigation tree. This gives you complete control over the navigation structure independently of your URL structure.

### Hierarchy Examples

```csharp
// Creates a top-level "Blog" entry
new ContentTocItem("Blog", "/blog", 100, ["blog"])

// Creates Blog -> Posts -> "My First Post" 
new ContentTocItem("My First Post", "/blog/posts/first", 200, ["blog", "posts", "my-first-post"])

// Creates Documentation -> API -> Classes -> "ContentService"
new ContentTocItem("ContentService", "/api/contentservice", 300, ["documentation", "api", "classes", "contentservice"])
```

### Key Benefits

- **Custom Organization**: Group content logically regardless of URL structure
- **Multi-level Navigation**: Create deep hierarchies with unlimited nesting
- **Content Service Independence**: Each service controls its own navigation structure
- **Flexible Naming**: Hierarchy parts can differ from URL segments for better navigation labels

## Advanced Implementation Features

### Caching for Performance

For content services that make expensive operations (API calls, database queries),
consider implementing caching using [`AsyncLazy<T>` with `AddFileWatched<T>()`](../under-the-hood/hot-reload-architecture). The built-in
`IContentService` implementations use this pattern to cache results until a trigger occurs that requires a refresh.

### Content Transformation

Transform external content formats into HTML on demand rather than at loading time. For larger sites, the development
experience can be improved by only gathering the data needed to return the data for the `IContentService` methods. These
are needed for things such as site-wide navigation, cross-references, and static assets. If there's work that's only
presentation-related, wait until the user requests it to speed up the initial load time.

## Performance Considerations

- **Lazy Loading**: Only load content when needed
- **Parallel Processing**: Use `Task.WhenAll()` for independent operations
- **Memory Management**: Dispose of resources properly

