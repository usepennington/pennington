---
title: "Working with Multiple Content Sources"
description: "Register multiple markdown content sources with different front matter types and URL structures"
uid: "penn.guides.multiple-content-sources"
order: 1500
---

A Penn site can serve more than one kind of content. A documentation section has ordering and sections. A blog has dates and authors. A set of landing pages needs neither. Each content type gets its own front matter schema, directory, and URL prefix -- but they all share the same navigation tree and cross-reference system.

This guide shows you how to register multiple markdown content sources within a single `AddPenn` call.

## Why Multiple Content Sources

Consider a site with three kinds of content:

- **Static pages** like "About" and "Contact" that need a title and nothing else.
- **Blog posts** with publication dates, authors, tags, and draft status.
- **Documentation** with explicit ordering, sections, and cross-reference UIDs.

Without multiple content sources, you'd need a single front matter type that accommodates every field. Blog posts would carry unused `Order` properties. Documentation pages would have empty `Date` fields.

Penn avoids this by letting you register multiple `MarkdownContentService<T>` instances, each parameterized with its own front matter type. Each source watches its own directory and maps content to its own URL prefix.

## The AddMarkdownContent API

Inside the `AddPenn` configuration lambda, call `AddMarkdownContent<T>()` for each content source. The generic parameter `T` must implement `IFrontMatter`. The configuration action sets the directory and URL prefix.

```csharp
penn.AddMarkdownContent<BlogFrontMatter>(opts =>
{
    opts.ContentPath = "Content/blog";
    opts.BasePageUrl = "/blog";
    opts.Section = "blog";
});
```

`MarkdownContentOptions` exposes three properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ContentPath` | `string` | `"Content"` | Directory containing markdown files, relative to the project root |
| `BasePageUrl` | `string` | `"/"` | URL prefix for all pages in this source |
| `Section` | `string?` | `null` | Navigation section grouping for table of contents |

Each call to `AddMarkdownContent<T>()` registers a separate `MarkdownContentService<T>` as an `IContentService` in the DI container. Penn resolves all `IContentService` implementations at runtime and aggregates their content.

## Practical Example

Here is a complete `Program.cs` for a site with three content sources: static pages, a blog, and documentation.

```csharp
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "Greenfield Gardening";
    penn.SiteDescription = "Grow things. Write about it. Document everything.";
    penn.ContentRootPath = "Content";

    // Static pages at the root
    penn.AddMarkdownContent<PageFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/pages";
        opts.BasePageUrl = "/";
    });

    // Blog posts under /blog
    penn.AddMarkdownContent<BlogFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/blog";
        opts.BasePageUrl = "/blog";
        opts.Section = "blog";
    });

    // Documentation under /docs
    penn.AddMarkdownContent<DocFrontMatter>(opts =>
    {
        opts.ContentPath = "Content/docs";
        opts.BasePageUrl = "/docs";
        opts.Section = "docs";
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

The corresponding directory structure:

```
Content/
├── pages/
│   ├── index.md              (PageFrontMatter)
│   ├── about.md              (PageFrontMatter)
│   └── contact.md            (PageFrontMatter)
├── blog/
│   ├── spring-planting.md    (BlogFrontMatter)
│   ├── composting-basics.md  (BlogFrontMatter)
│   └── drought-tolerant.md   (BlogFrontMatter)
└── docs/
    ├── soil-types.md         (DocFrontMatter)
    ├── watering-schedule.md  (DocFrontMatter)
    └── pest-control.md       (DocFrontMatter)
```

## Front Matter Types and Capabilities

Every front matter type must implement `IFrontMatter`, which requires only a `Title` property. Beyond that, Penn provides eight capability interfaces that you opt into as needed:

| Interface | Property | Type | Purpose |
|-----------|----------|------|---------|
| `IDraftable` | `IsDraft` | `bool` | Exclude content from published output |
| `ITaggable` | `Tags` | `string[]` | Categorize content with tags |
| `IOrderable` | `Order` | `int` | Control sort position in navigation |
| `ICrossReferenceable` | `Uid` | `string?` | Enable `xref:` cross-reference links |
| `IDescribable` | `Description` | `string?` | SEO descriptions and summaries |
| `IDateable` | `Date` | `DateTime?` | Publication dates for chronological content |
| `ISectionable` | `Section` | `string?` | Group content into navigation sections |
| `IRedirectable` | `RedirectUrl` | `string?` | Redirect to another URL |

Penn checks for each interface at runtime via pattern matching (`fm is IDraftable { IsDraft: true }`). If your front matter type doesn't implement a capability interface, that behavior doesn't apply. Unused capabilities cost nothing.

### Static Page Front Matter

For pages that need a title and cross-referencing:

```csharp
using Penn.FrontMatter;

public record PageFrontMatter : IFrontMatter, ICrossReferenceable
{
    public string Title { get; init; } = "";
    public string? Uid { get; init; }
}
```

### Blog Front Matter

Penn ships `BlogFrontMatter` out of the box. It implements `IDraftable`, `ITaggable`, `IDescribable`, `IDateable`, and `ICrossReferenceable`:

```csharp
public record BlogFrontMatter : IFrontMatter, IDraftable, ITaggable,
    IDescribable, IDateable, ICrossReferenceable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime? Date { get; init; }
    public string? Author { get; init; }
    public string? Series { get; init; }
    public string? Uid { get; init; }
}
```

### Documentation Front Matter

Penn also ships `DocFrontMatter`. It implements `IDraftable`, `ITaggable`, `ISectionable`, `ICrossReferenceable`, `IOrderable`, and `IDescribable`:

```csharp
public record DocFrontMatter : IFrontMatter, IDraftable, ITaggable,
    ISectionable, ICrossReferenceable, IOrderable, IDescribable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Section { get; init; }
    public string? Uid { get; init; }
    public int Order { get; init; } = int.MaxValue;
}
```

You can use these built-in types directly or define your own records that implement whatever combination of capability interfaces you need.

## Content Examples

Front matter properties use CamelCase in YAML. This matches the C# property names and the default YamlDotNet naming convention.

### Static Page

```markdown
---
title: "About Greenfield Gardening"
uid: "pages.about"
---

We're a community of gardeners sharing what works, what doesn't,
and what the slugs got to before we could.
```

### Blog Post

```markdown
---
title: "Spring Planting: What to Start Indoors"
description: "A week-by-week guide to starting seeds indoors before the last frost"
date: 2026-03-15
tags:
  - planting
  - seeds
  - spring
author: "Marta Nguyen"
isDraft: false
uid: "blog.spring-planting"
---

## Start Eight Weeks Before Last Frost

Tomatoes, peppers, and eggplant need the longest head start...
```

### Documentation Page

```markdown
---
title: "Soil Types and Amendments"
description: "Identify your soil type and choose the right amendments"
order: 100
section: "fundamentals"
uid: "docs.soil-types"
tags:
  - soil
  - fundamentals
---

## Clay, Sand, Silt, and Loam

The first step in any garden plan is understanding what you're working with...
```

## URL Structure and Routing

Each content source maps its files to URLs using `BasePageUrl` as the prefix. A file at `Content/blog/spring-planting.md` with `BasePageUrl = "/blog"` becomes `/blog/spring-planting`. Subdirectories within a content path create nested URL segments: `Content/docs/guides/pest-control.md` becomes `/docs/guides/pest-control`.

```csharp
// Three sources, three URL namespaces
penn.AddMarkdownContent<PageFrontMatter>(opts =>
{
    opts.ContentPath = "Content/pages";
    opts.BasePageUrl = "/";           // /about, /contact
});

penn.AddMarkdownContent<BlogFrontMatter>(opts =>
{
    opts.ContentPath = "Content/blog";
    opts.BasePageUrl = "/blog";       // /blog/spring-planting
});

penn.AddMarkdownContent<DocFrontMatter>(opts =>
{
    opts.ContentPath = "Content/docs";
    opts.BasePageUrl = "/docs";       // /docs/soil-types
});
```

The `Section` property on `MarkdownContentOptions` controls navigation grouping independently of URL structure. Two content sources with different `BasePageUrl` values can share the same `Section` and appear under the same navigation heading.

> [!WARNING]
> Avoid overlapping `ContentPath` values. If a root source uses `ContentPath = "Content"` and a blog source uses `ContentPath = "Content/blog"`, both services will discover the blog files. Penn does not deduplicate. Use distinct, non-overlapping directories.

## How Multiple Sources Merge

At runtime, Penn collects every registered `IContentService` from the DI container and iterates over all of them. There is no priority system and no override mechanism -- just aggregation. Each service contributes independently to four operations:

- **Discovery**: `DiscoverAsync()` from each service feeds into the content pipeline. Every discovered item is parsed, rendered, and written to output.
- **Table of Contents**: `GetContentTocEntriesAsync()` from each service contributes entries to the navigation tree. The `Section` and `Order` properties determine where entries appear.
- **Cross-References**: `GetCrossReferencesAsync()` from each service populates the global xref map. A blog post can link to a documentation page with `xref:docs.soil-types`, and Penn resolves it across source boundaries.
- **Static Output**: `GetContentToCopyAsync()` from each service lists static files (images, PDFs) to include in the build output.

Cross-references work across source boundaries. A documentation page can link to a blog post with `xref:blog.spring-planting`, and Penn resolves it from the unified map regardless of which service registered the target.

If two services produce the same URL, the behavior is undefined. Keep your `BasePageUrl` values distinct.

## Best Practices

**Use non-overlapping content paths.** Each `ContentPath` should point to a distinct directory. Overlapping paths cause duplicate discovery and broken builds.

**Use the built-in front matter types when they fit.** `DocFrontMatter` and `BlogFrontMatter` cover the two most common content patterns. Create custom types only when the built-in ones don't match your needs.

**Implement `ICrossReferenceable` on every front matter type.** UIDs enable cross-source linking with `xref:` syntax. Without them, you're limited to relative paths that break when content moves between directories.

**Keep capability interfaces minimal.** Only implement the interfaces your content type actually uses. Blog posts rarely need `IOrderable`. Documentation rarely needs `IDateable`. Each interface is a promise that Penn will act on -- don't make promises you don't intend to keep.

**Use `Section` to control navigation grouping.** The `Section` property on `MarkdownContentOptions` determines where a content source's entries appear in the navigation tree. Set it explicitly rather than relying on directory structure.

**Consider a custom content service for non-markdown sources.** If content lives in a database or API, see [Create Custom Content Service](xref:penn.guides.custom-content-service). Custom services participate in the same aggregation as markdown sources.

## Related Guides

- [Create Custom Content Service](xref:penn.guides.custom-content-service) -- implement `IContentService` for non-markdown content
- [Front Matter Properties Reference](xref:penn.reference.front-matter-properties) -- complete list of front matter fields and capability interfaces
- [Using the BlogSite Package](xref:penn.guides.using-blogsite) -- the opinionated blog package that wires up `BlogFrontMatter` for you
