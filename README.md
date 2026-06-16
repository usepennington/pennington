# Pennington

A content engine for .NET that turns Markdown into static sites, documentation portals, and blogs.

## Features

- **Markdown processing** with front matter, syntax highlighting, tabbed content, and GitHub-style alerts
- **Static site generation** with automatic navigation and sitemaps (RSS feeds ship with the DocSite blog folder and the BlogSite template)
- **Razor component library** for navigation trees, code blocks, badges, and cards
- **Utility-first CSS** via MonorailCSS integration
- **Documentation site template** with built-in layouts, search, and content areas
- **Blog site template** for content-driven blogs

## Mental model

Pennington is an ASP.NET content engine. In development it serves your site as a normal web app; in build mode it crawls that same app in process and writes the responses as static files.

There are three entry points:

| Package | Use it when |
|---|---|
| `Pennington.DocSite` | You want a documentation site with sidebar navigation, search, styling, and static output wired for you. |
| `Pennington.BlogSite` | You want a content-driven blog as the whole site. |
| `Pennington` | You want the lower-level engine and will bring your own layout, routing, and styling. |

Most documentation projects should start with `Pennington.DocSite`. Drop down to `Pennington` when the template shape no longer fits.

## Installation

Pennington targets .NET 10 and .NET 11, and uses preview C# language features.

For the DocSite quick start:

```shell
dotnet add package Pennington.DocSite
```

Additional packages for specific features:

```shell
dotnet add package Pennington               # Core content engine
dotnet add package Pennington.UI            # Razor components
dotnet add package Pennington.MonorailCss   # Utility-first CSS
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
