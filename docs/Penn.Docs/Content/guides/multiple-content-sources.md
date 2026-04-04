---
title: "Working with Multiple Content Sources"
description: "Configure multiple content sources with different front matter types and URL structures"
uid: "penn.guides.multiple-content-sources"
order: 1500
---

This guide shows you how to register multiple markdown content sources within a single Penn site. Different content types, different front matter schemas, different URL structures, same `AddPenn` lambda. Penn is inflexible in many ways, but it is flexible about this.

## Understanding Multiple Content Sources

Multiple content sources let you organize distinct content types with their own characteristics:

- **Different Front Matter Types**: Blog posts with dates vs. documentation with ordering
- **Separate URL Structures**: `/blog/` for articles, `/docs/` for documentation, `/` for static pages
- **Content-Specific Behaviors**: Tags on blog posts, sections on docs, neither on landing pages
- **Organizational Flexibility**: Keep content types in separate directories with their own rules

## Basic Multiple Content Source Setup

Here's a site with three content types, registered via `penn.AddMarkdownContent<T>()` calls inside the `AddPenn` lambda:

```csharp
using Penn;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "Daily Life Hub";
    penn.SiteDescription = "Your everyday life, simplified";
    penn.ContentRootPath = "Content";

    // Static pages at root level
    penn.AddMarkdownContent<ContentFrontMatter>(opts =>
    {
        opts.ContentPath = "Content";
        opts.BasePageUrl = "/";
    });

    // Blog posts
    penn.AddMarkdownContent<BlogFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/blog";
        opts.BasePageUrl = "/blog";
    });

    // Documentation with ordering
    penn.AddMarkdownContent<DocFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/docs";
        opts.BasePageUrl = "/docs";
    });
});

builder.Services.AddMonorailCss();

var app = builder.Build();
app.UseStaticFiles();
app.UsePenn();
app.UseMonorailCss();
app.MapRazorComponents<App>();

await app.RunOrBuildAsync(args);
```

Each `AddMarkdownContent<T>()` call registers a separate `MarkdownContentService<T>` as an `IContentService`. Penn resolves content paths using `IWebHostEnvironment.ContentRootPath`, so relative paths in `ContentPath` work across development and production.

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
└── docs/                 # Documentation (DocFrontMatter)
    ├── coffee-brewing-guide.md
    ├── indoor-herb-garden.md
    └── home-organization-systems.md
```

> [!WARNING]
> If your root content source (`ContentPath = "Content"`) overlaps with subdirectories used by other sources, both services will discover files in those subdirectories. Penn will not resolve this for you. Use distinct, non-overlapping paths, or accept the consequences of your architectural choices.

## Front Matter Types

Penn v2 uses a composable capability system. <xref:T:Penn.FrontMatter.IFrontMatter> requires only `Title`. Everything else comes from opt-in interfaces: <xref:T:Penn.FrontMatter.IDraftable>, <xref:T:Penn.FrontMatter.ITaggable>, <xref:T:Penn.FrontMatter.IOrderable>, <xref:T:Penn.FrontMatter.ICrossReferenceable>, <xref:T:Penn.FrontMatter.IDescribable>, `IDateable`, `ISectionable`, and `IRedirectable`.

### Static Content Front Matter

For pages that need nothing special:

```csharp
using Penn.FrontMatter;

public record ContentFrontMatter : IFrontMatter, ICrossReferenceable
{
    public string Title { get; init; } = "Untitled";
    public string? Uid { get; init; }
}
```

### Blog Content Front Matter

Penn provides <xref:T:Penn.FrontMatter.BlogFrontMatter> out of the box, but you can roll your own:

```csharp
using Penn.FrontMatter;

public record BlogFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IDateable, ICrossReferenceable
{
    public string Title { get; init; } = "Untitled Post";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime? Date { get; init; }
    public string? Uid { get; init; }
}
```

### Documentation Front Matter

Penn also provides <xref:T:Penn.FrontMatter.DocFrontMatter>, which covers the documentation use case with ordering, sections, and cross-references. Use it directly or use it as a template:

```csharp
using Penn.FrontMatter;

public record DocsFrontMatter : IFrontMatter, IOrderable, IDraftable,
    IDescribable, ICrossReferenceable, ISectionable
{
    public string Title { get; init; } = "Untitled Document";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public int Order { get; init; } = int.MaxValue;
    public string? Uid { get; init; }
    public string? Section { get; init; }
}
```

> [!NOTE]
> You don't have to implement all capabilities. A front matter record that only implements `IFrontMatter` works fine -- it just won't participate in drafts, ordering, cross-references, or any other capability-specific behavior. Penn checks for each interface at runtime with pattern matching (`fm is IDraftable { IsDraft: true }`), so unused capabilities cost nothing.

## Content Examples

### Static Page Example

```markdown
---
title: "Welcome to Daily Life Hub"
uid: "home"
---

## Your Everyday Life, Simplified

Welcome to Daily Life Hub.
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
---

## The Eternal Question

Pizza toppings have divided families, ended friendships, and sparked heated debates...
```

### Documentation Example

```markdown
---
title: "Ultimate Coffee Brewing Guide"
description: "Master the art of coffee brewing"
order: 100
uid: "docs.coffee-brewing"
section: "beverages"
---

## Essential Equipment

### For the Minimalist
- French press
- Coffee grinder (burr preferred)
```

## Custom URL Structures

Each content source gets its own URL namespace via `BasePageUrl`:

```csharp
builder.Services.AddPenn(penn =>
{
    // Blog posts at /blog/post-name
    penn.AddMarkdownContent<BlogFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/blog";
        opts.BasePageUrl = "/blog";
    });

    // Documentation at /docs/guide-name
    penn.AddMarkdownContent<DocFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/docs";
        opts.BasePageUrl = "/docs";
    });

    // API docs at /api/reference-name
    penn.AddMarkdownContent<ApiDocsFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/api";
        opts.BasePageUrl = "/api";
    });
});
```

The `Section` property on `MarkdownContentOptions` lets you group content sources for navigation without changing their URL structure. This is useful when you want multiple directories to appear under the same navigation heading.

## Navigation and Content Merging

Multiple content sources automatically combine. The framework merges all content from all registered `IContentService` implementations when building:

- **Table of Contents**: Each service contributes entries via `GetContentTocEntriesAsync()`
- **Cross-References**: All services contribute to the xref map via `GetCrossReferencesAsync()`
- **Static Assets**: Each service reports files to copy via `GetContentToCopyAsync()`
- **Discovery**: The pipeline iterates `DiscoverAsync()` across all services

Penn does not deduplicate. If two services produce the same URL, you get undefined behavior, which is a polite way of saying "a bug you caused."

## Best Practices

- **Use non-overlapping paths**: Each `ContentPath` should be distinct to avoid double-discovery
- **Use built-in front matter types when they fit**: `DocFrontMatter` and `BlogFrontMatter` cover the common cases. Custom types are for when they don't.
- **Implement `ICrossReferenceable` on everything**: UIDs make cross-source linking possible. Without them, you're back to relative paths and prayer.
- **Keep capability interfaces minimal**: Only implement the interfaces your content type actually uses. A blog post doesn't need `IOrderable`. Documentation doesn't need `IDateable`. Restraint is a virtue.
