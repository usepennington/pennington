---
title: "Working with Multiple Content Sources"
description: "Configure multiple content sources with different front matter types and URL structures for complex sites"
uid: "docs.guides.multiple-content-sources"
order: 1500
---

This guide shows you how to configure multiple content sources in MyLittleContentEngine to create complex sites with different types of content, each with their own front matter schemas, URL structures, and behaviors.

## Understanding Multiple Content Sources

Multiple content sources allow you to organize different types of content with distinct characteristics:

- **Different Front Matter Types**: Blog posts with dates and RSS feeds vs. documentation with ordering
- **Separate URL Structures**: `/blog/` for articles, `/docs/` for documentation, `/` for static pages
- **Content-Specific Behaviors**: RSS generation for blogs but not docs, different ordering schemes
- **Organizational Flexibility**: Keep content types in separate directories with their own rules

## Basic Multiple Content Source Setup

Here's how to configure a site with three different content types:

```csharp
using MyLittleContentEngine;
using MyLittleContentEngine.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

// Global site configuration
builder.Services.AddContentEngineService(_ => new ContentEngineOptions
    {
        SiteTitle = "Daily Life Hub",
        SiteDescription = "Your everyday life, simplified",
        ContentRootPath = "Content",
    })
    .WithMarkdownContentService(_ => new MarkdownContentOptions<ContentFrontMatter>()
    {
        // Static pages at root level (about, contact, etc.)
        ContentPath = "Content",
        BasePageUrl = "",
        ExcludeSubfolders = true, // Don't include blog/ and docs/ subfolders
    })
    .WithMarkdownContentService(_ => new MarkdownContentOptions<BlogFrontMatter>()
    {
        // Blog posts with RSS feeds
        ContentPath = "Content/blog",
        BasePageUrl = "/blog"
    })
    .WithMarkdownContentService(_ => new MarkdownContentOptions<DocsFrontMatter>()
    {
        // Documentation with ordering
        ContentPath = "Content/docs",
        BasePageUrl = "/docs"
    });

builder.Services.AddMonorailCss();

var app = builder.Build();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<App>();
app.UseMonorailCss();

await app.RunOrBuildContent(args);
```

## Content Directory Structure

Organize your content directories to match your content services:

```
Content/
├── index.md              # Root content (ContentFrontMatter)
├── about.md              # Static pages (ContentFrontMatter)
├── blog/                 # Blog content (BlogFrontMatter)
│   ├── best-pizza-toppings.md
│   ├── mystery-of-missing-socks.md
│   └── office-plant-survival-guide.md
└── docs/                 # Documentation (DocsFrontMatter)
    ├── coffee-brewing-guide.md
    ├── indoor-herb-garden.md
    └── home-organization-systems.md
```

## Front Matter Types

Each content source needs its own front matter class implementing `IFrontMatter`. Here are three common patterns:

### Static Content Front Matter

For general pages without special ordering or RSS requirements:

```csharp
using MyLittleContentEngine.Models;

public class ContentFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "Untitled";
    public int Order { get; init; }
    public string[] Tags { get; init; } = [];
    public bool IsDraft { get; init; }
    public string? Uid { get; init; }
    public string? RedirectUrl { get; init; }
    
    public Metadata AsMetadata()
    {
        return new Metadata()
        {
            Title = Title,
            Description = string.Empty,
            LastMod = DateTime.MaxValue,
            Order = Order,
            RssItem = false // Static pages don't appear in RSS
        };
    }
}
```

### Blog Content Front Matter

For time-based content with RSS feeds:

```csharp
using MyLittleContentEngine.Models;

public class BlogFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "Untitled Post";
    public string Description { get; init; } = string.Empty;
    public string? Uid { get; init; }
    public DateTime Date { get; init; } = DateTime.Now;
    public bool IsDraft { get; init; } = false;
    public string[] Tags { get; init; } = [];
    public string? RedirectUrl { get; init; }
    
    public Metadata AsMetadata()
    {
        return new Metadata()
        {
            Title = Title,
            Description = Description,
            LastMod = Date,
            RssItem = true // Blog posts appear in RSS feeds
        };
    }
}
```

### Documentation Front Matter

For ordered documentation with no RSS:

```csharp
using MyLittleContentEngine.Models;

public class DocsFrontMatter : IFrontMatter
{
    public string Title { get; init; } = "Untitled Document";
    public string Description { get; init; } = string.Empty;
    public bool IsDraft { get; init; } = false;
    public string[] Tags { get; init; } = [];
    public string? RedirectUrl { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public string? Uid { get; init; }
    
    public Metadata AsMetadata()
    {
        return new Metadata()
        {
            Title = Title,
            Description = Description,
            LastMod = DateTime.MinValue,
            RssItem = false, // Documentation doesn't appear in RSS
            Order = Order
        };
    }
}
```

## Content Examples

### Static Page Example

```markdown
---
title: "Welcome to Daily Life Hub"
order: 1
tags:
  - home
  - welcome
isDraft: false
---

## Your Everyday Life, Simplified

Welcome to Daily Life Hub, where we explore the amusing, practical, and occasionally profound aspects of modern living.
```

### Blog Post Example

```markdown
---
title: "The Great Pizza Topping Debate: A Scientific Analysis"
description: "Exploring the most controversial food debate of our time"
date: 2025-01-15
tags:
  - food
  - opinion
  - pizza
isDraft: false
---

## The Eternal Question

Pizza toppings have divided families, ended friendships, and sparked heated debates...
```

### Documentation Example

```markdown
---
title: "Ultimate Coffee Brewing Guide"
description: "Master the art of coffee brewing with these comprehensive techniques"
tags:
  - coffee
  - brewing
  - guide
order: 100
isDraft: false
---

## Essential Equipment

### For the Minimalist
- French press
- Coffee grinder (burr preferred)
...
```

## Advanced Configuration Options

### Content Path Filtering

Use `ExcludeSubfolders` to prevent content services from processing subdirectories:

```csharp
// Only process files directly in Content/, not Content/blog/ or Content/docs/
builder.Services.WithMarkdownContentService(_ => new MarkdownContentOptions<ContentFrontMatter>()
{
    ContentPath = "Content",
    BasePageUrl = "",
    ExcludeSubfolders = true,
});
```

### Custom URL Structures

Configure different URL patterns for each content type:

```csharp
// Blog posts at /blog/post-name
builder.Services.WithMarkdownContentService(_ => new MarkdownContentOptions<BlogFrontMatter>()
{
    ContentPath = "Content/blog",
    BasePageUrl = "/blog"
});

// Documentation at /docs/guide-name
builder.Services.WithMarkdownContentService(_ => new MarkdownContentOptions<DocsFrontMatter>()
{
    ContentPath = "Content/docs",
    BasePageUrl = "/docs"
});

// API docs at /api/reference-name
builder.Services.WithMarkdownContentService(_ => new MarkdownContentOptions<ApiDocsFrontMatter>()
{
    ContentPath = "Content/api",
    BasePageUrl = "/api"
});
```

### Environment-Based Configuration

Configure different content paths for different environments:

```csharp
var contentRoot = Environment.GetEnvironmentVariable("CONTENT_ROOT") ?? "Content";
var blogPath = Path.Combine(contentRoot, "blog");
var docsPath = Path.Combine(contentRoot, "docs");

builder.Services.WithMarkdownContentService(_ => new MarkdownContentOptions<BlogFrontMatter>()
{
    ContentPath = blogPath,
    BasePageUrl = "/blog"
});
```

## Navigation and Table of Contents

Multiple content sources automatically combine their content in the navigation tree. The framework merges all content from all services when generating:

- **Table of Contents**: All content sources contribute to the site navigation
- **Cross-References**: Links work across all content types
- **Search Indexes**: All content is searchable regardless of source

## Content Service Priority

When multiple content services are registered, they all contribute to the final site. The framework automatically:

1. **Combines Content**: All pages from all services are included
2. **Merges Navigation**: Table of contents includes all content sources
3. **Resolves Cross-References**: Links work between different content types
4. **Handles Conflicts**: Later registrations don't override earlier ones

## Best Practices

### Organization Strategy
- **Group by Purpose**: Keep different content types in separate directories. Make it easy for Razor pages to discover
  their content by folder.
- **Consistent Naming**: Use clear, descriptive names for content types
- **Logical Hierarchies**: Structure directories to match your site's information architecture


### Front Matter Design
- **Shared Properties**: Include common properties (Title, Description, Tags) in all front matter types
- **Type-Specific Fields**: Add specialized fields only where needed (Date for blogs, Order for docs)
- **Consistent Naming**: Use consistent property names across front matter types


Multiple content sources provide the flexibility to create sophisticated content sites while maintaining clear separation of concerns and content-specific behaviors.