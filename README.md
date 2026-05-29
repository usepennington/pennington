# Pennington

A content engine for .NET that turns Markdown into static sites, documentation portals, and blogs.

## Features

- **Markdown processing** with front matter, syntax highlighting, tabbed content, and GitHub-style alerts
- **Static site generation** with automatic navigation and sitemaps (RSS feeds ship with the DocSite blog folder and the BlogSite template)
- **Razor component library** for navigation trees, code blocks, badges, and cards
- **Utility-first CSS** via MonorailCSS integration
- **Documentation site template** with built-in layouts, search, and content areas
- **Blog site template** for content-driven blogs

## Installation

```shell
dotnet add package Pennington
```

Additional packages for specific features:

```shell
dotnet add package Pennington.UI            # Razor components
dotnet add package Pennington.MonorailCss   # Utility-first CSS
dotnet add package Pennington.DocSite       # Documentation site template
dotnet add package Pennington.BlogSite      # Blog site template
```

## Quick Start

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    Description = "Project documentation",
});

var app = builder.Build();
app.UseDocSite();
await app.RunDocSiteAsync(args);
```

Add Markdown files to a `Content/` directory with YAML front matter:

```markdown
---
title: Getting Started
order: 1
---

# Welcome

Your documentation content goes here.
```

Build a static site with:

```shell
dotnet run -- build
```

## License

MIT
