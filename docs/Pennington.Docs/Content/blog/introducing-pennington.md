---
title: Introducing Pennington
description: A content engine for .NET — markdown, Roslyn-powered code samples, and ready-made documentation and blog templates.
author: Phil Scott
date: 2026-04-04
isDraft: false
tags:
  - announcements
  - release
---

Pennington is a content engine for .NET 11 and C# 15. Point it at a folder of
markdown and it builds a site — with navigation, search, feeds, and a static
build included.

If you've put together a documentation site in .NET before, you've probably
either reached for a JavaScript static-site generator, which means a second
toolchain, or hand-rolled Razor pages, which means a lot of layout plumbing.
Pennington is a .NET-native option that handles the plumbing for you.

## A folder of markdown, a working site

Pennington ships two templates. DocSite is for documentation: sidebar
navigation, an on-page table of contents, and cross-references between pages.
BlogSite is for posts: a home page, an archive, browse-by-tag pages, and an RSS
feed. Wiring either into an ASP.NET app takes a few lines:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDocSite();

var app = builder.Build();
app.UseDocSite();

await app.RunDocSiteAsync(args);
```

`RunDocSiteAsync` does two jobs. Run the project normally and it's a live dev
server with hot reload. Pass build arguments and the same project emits static
HTML you can host anywhere — see [how dev mode and build mode share a code
path](xref:explanation.core.dev-vs-build).

If you'd rather not write those lines, `dotnet new pennington-docs` scaffolds
the project for you.

## Code samples that stay in sync

Code samples in docs are usually copy-pasted snippets: correct the day they're
written, slowly wrong after that. Pennington can pull samples from a Roslyn
workspace instead. You reference a real symbol, and the current source renders
at build time:

````markdown
```csharp:xmldocid
T:Pennington.Pipeline.ContentPipeline
```
````

When that type changes, the sample changes with it on the next build — there's
no copy to keep up to date. Pennington can also embed whole files, diff two
versions of a symbol, and highlight or focus lines; the [code-block argument
reference](xref:reference.markdown.code-block-args) has the full set.

It's the feature several other posts on this blog come back to. To build a site
of your own, start with [creating your first Pennington
site](xref:tutorials.getting-started.first-site).
