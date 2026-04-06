---
title: "Using the BlogSite Package"
description: "Penn.BlogSite -- the blog-shaped package that doesn't exist yet"
uid: "penn.guides.using-blogsite"
order: 2500
---

Penn.BlogSite is the planned companion to Penn.DocSite: a ready-made blog layout with posts, archives, tags, RSS feeds, and all the other things you'd expect from a blog that someone actually thought about for more than fifteen minutes.

It doesn't exist yet.

## What's Coming

The plan for Penn.BlogSite mirrors how Penn.DocSite works: a single `AddBlogSite()` call that wires up Penn core, MonorailCSS, SPA navigation, and a blog-specific layout with opinionated defaults. You'll get:

- **Post listing** with date, description, and tag display
- **Tag-based filtering** and a tag index page
- **RSS feed** generation from your content
- **Archive pages** organized by date
- **SPA navigation** between posts (using the same island renderer pattern as DocSite)
- **Dark/light mode** with the standard Penn theme toggle

The configuration shape will look something like:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlogSite(() => new BlogSiteOptions
{
    SiteTitle = "My Blog",
    Description = "Thoughts, mostly about code",
    AuthorName = "Your Name",
    CanonicalBaseUrl = "https://myblog.example.com",
});

var app = builder.Build();
app.UseBlogSite();
await app.RunBlogSiteAsync(args);
```

This is aspirational API design. The real thing may look different.

## Building a Blog with Penn Core Today

You don't need to wait for Penn.BlogSite. Penn core has everything you need to build a blog -- you'll just wire it up yourself instead of getting it for free.

### 1. Define Blog Front Matter

```csharp
public class BlogFrontMatter : IFrontMatter
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Uid { get; set; }
    public int Order { get; set; }
    public bool IsDraft { get; set; }
    
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string[] Tags { get; set; } = [];
    public string? Author { get; set; }
}
```

### 2. Register Content

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Blog";
    penn.SiteDescription = "A blog about things";
    penn.CanonicalBaseUrl = "https://myblog.example.com";

    penn.AddMarkdownContent<BlogFrontMatter>(md =>
    {
        md.ContentPath = "Content/posts";
        md.BasePageUrl = "/blog";
        md.Section = "blog";
    });
});
```

### 3. Create Your Layout

Build Razor components for the blog layout, post pages, tag listing, and whatever else you want. Use `@page` directives for custom routes (like `/tags`) and let the markdown content service handle blog post routes.

### 4. Add SPA Navigation (Optional)

Follow the [Adding SPA Navigation](xref:penn.guides.adding-spa-navigation) guide to add instant page transitions between posts. The pattern is the same regardless of content type.

### 5. Build

```bash
# Development
dotnet watch

# Static build
dotnet run -- build /
```

## Why Not Just Release BlogSite Now?

Because a blog package that's "good enough" but changes constantly is worse than no package at all. Penn.DocSite already exists because it drives these docs -- it has a concrete use case and a real user (this site). Penn.BlogSite will ship when there's a blog that needs it and the API has settled enough to be worth packaging.

In the meantime, Penn core is perfectly capable of powering a blog. You just have to build the layout yourself. Think of it as an opportunity for creative expression. Or a chore. Depending on your relationship with CSS.
