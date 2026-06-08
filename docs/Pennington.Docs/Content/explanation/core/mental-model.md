---
title: "The Pennington mental model"
description: "A short map of the host, content sources, rendering pipeline, response pipeline, and DocSite/BlogSite templates before you dive into the deeper architecture pages."
uid: explanation.core.mental-model
order: 0
sectionLabel: "Core Architecture"
tags: [architecture, getting-started, host, pipeline]
---

Pennington is easiest to understand as a normal ASP.NET app with a content engine inside it. You run the app while writing, and you ask the same app to build static files when publishing.

That single idea explains most of the project:

- The host is ASP.NET. `Program.cs` owns dependency injection, middleware order, Razor components, and any custom endpoints.
- Content sources discover pages. Markdown folders, Razor pages, redirects, generated API reference, taxonomy pages, and custom services all report routes into the same site model.
- The content pipeline turns discovered content into rendered content. A page moves from "I know where it is" to "I parsed its front matter" to "I rendered its HTML"; failures travel through the same pipeline so the build report can name them.
- The response pipeline finishes the HTML. Cross-references, locale prefixes, base URLs, live reload, diagnostics, and other response processors run against the actual HTTP response.
- Build mode crawls the host. `dotnet run` serves through Kestrel; `dotnet run -- build` starts the same app on an in-process test server, requests every discovered route, and writes the responses to disk.

## The layers

### `Pennington`

`Pennington` is the lower-level engine. It gives you content discovery, markdown parsing, rendering, route resolution, response processing, diagnostics, search artifacts, feeds, and static output. You bring the site shell: layout, navigation markup, styling, and any app-specific endpoints.

Start here when you are embedding content into an existing ASP.NET app, building a custom layout, or mixing several kinds of content that do not fit a stock documentation or blog template. The first getting-started arc walks this path: <xref:tutorials.getting-started.first-site>.

### `Pennington.DocSite`

`Pennington.DocSite` is a pre-assembled documentation site on top of the engine. One `AddDocSite` call wires the engine, markdown content, DocSite layout, sidebar navigation, search, MonorailCSS styling, SPA navigation, feeds, and static build behavior.

Start here for a conventional documentation site. You can still customize options and add extension points, but the template owns the article-shaped layout. The scaffold tutorial starts here: <xref:tutorials.docsite.scaffold>.

### `Pennington.BlogSite`

`Pennington.BlogSite` is the same idea for a blog-first site. It is a template, not a separate engine: the underlying runtime is still Pennington content discovery plus the shared ASP.NET request pipeline.

Use it when the blog is the site. If you want a blog alongside documentation, use DocSite's built-in blog folder instead of combining DocSite and BlogSite in the same app.

## Terms you will see

| Term | Meaning |
|---|---|
| Host | The ASP.NET app that registers Pennington and handles requests. |
| Content source | A service that discovers routes and records for pages or generated artifacts. |
| Front matter | Typed metadata parsed from the YAML block at the top of a markdown file. |
| Route | The URL and output-file mapping for a page or endpoint. |
| Xref | A symbolic link such as `<xref:tutorials.docsite.scaffold>` that resolves to the current route for a page or API member. |
| Response processor | A hook that rewrites the HTTP response before it reaches the browser or static output folder. |
| Build report | The diagnostics summary printed by `dotnet run -- build`. |

## What to read next

- Build a bare host: <xref:tutorials.getting-started.first-site>
- Scaffold a documentation site: <xref:tutorials.docsite.scaffold>
- Understand why dev and build share one app: <xref:explanation.core.dev-vs-build>
- Understand the content pipeline internals: <xref:explanation.core.content-pipeline>
- See what DocSite and BlogSite wire for you: <xref:explanation.positioning.docsite-positioning>
