---
title: "Implement a custom IContentService"
description: "Adapt a non-markdown source — JSON, a database, a remote API — into the pipeline by implementing IContentService and yielding DiscoveredItems, TOC entries, and cross-references."
uid: how-to.extensibility.custom-content-service
order: 10
sectionLabel: Extensibility
tags: [extensibility, content-service, pipeline, cross-references]
---

> **In this page.** Discover custom sources by implementing `IContentService`, yield one `DiscoveredItem` per page, publish side-car files through `ContentToCopy` / `ContentToCreate`, and surface the pages in navigation and xref with `ContentTocItem`s plus `CrossReference`s.
>
> **Not in this page.** Parsing the raw source into a `ParsedItem` or rendering it to HTML — those are `IContentParser` and `IContentRenderer` concerns. For markdown, both are already wired; for a brand-new format see the parser and renderer how-tos (TODO — not yet written). Code-block preprocessing, highlighters, response processors, HTML rewriters, island renderers, and DocSite overrides each have their own how-to under [`/how-to/extensibility/`](/how-to/extensibility/code-block-preprocessor).

## When to use this

_Two to three sentences. Readers arrive here when their content lives somewhere `MarkdownContentService<T>` can't reach — a folder of JSON release notes, a SQL table, a GitHub API, generated API reference — and they need those pages to appear in navigation, cross-references, search, and the static build the same way markdown pages do. The recipe below uses the [`ReleaseNotesContentService`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample/ReleaseNotesContentService.cs) example, which turns `Content/releases/*.json` into `/releases/{version}/` routes. If you just want a second markdown tree with a different front-matter type, use chained `AddMarkdownContent<T>` instead — see [_Use multiple content sources_](/how-to/configuration/multiple-sources)._

## Assumptions

- You have a working Pennington site on bare `AddPennington` (see [_Create your first Pennington site_](/tutorials/getting-started/first-site) if not) — `AddDocSite` pins its own markdown service, so adding a second content service on top works but the concepts below assume you understand the un-wrapped host
- You understand the four-stage pipeline at a conceptual level ([_The content pipeline and union types_](/explanation/core/content-pipeline))
- Your source data can be enumerated synchronously or asynchronously on startup — `DiscoverAsync` runs both at build time and on demand for live requests

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — the `ReleaseNotesContentService` file is self-contained. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Model your source records

_Define an immutable record that represents one page's worth of source data — whatever your format parses into. `ReleaseEntry` is the JSON-backed shape the rest of the service keys off; your own type will have whatever columns your source has._

```csharp:xmldocid
T:ExtensibilityLabExample.ReleaseEntry
```

### 2. Implement `IContentService` and load the records once

_Create a sealed class that implements [`IContentService`](/reference/extension-points/content-pipeline), inject whatever you need to read the source (here, `IWebHostEnvironment` for `ContentRootPath`), and cache the parsed records in a `Lazy<ImmutableList<T>>` so discovery and the TOC share one pass over the source._

```csharp:xmldocid
T:ExtensibilityLabExample.ReleaseNotesContentService
```

### 3. Yield one `DiscoveredItem` per page from `DiscoverAsync`

_Build each item's `ContentRoute` through [`ContentRouteFactory.FromUrl`](/reference/extension-points/routing) (synthetic URL, no backing file) or `ContentRouteFactory.FromCustom` (URL plus an on-disk `FilePath` so file-watching picks up edits), and pair it with a `ContentSource` case — a `RedirectSource` is the right placeholder when an endpoint elsewhere in `Program.cs` produces the actual HTML. Yield an index route plus one route per record._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.DiscoverAsync
```

### 4. Emit `ContentTocItem`s for navigation and search

_Each `ContentTocItem` is one row in the sidebar and one document in the search index. Set `Title`, `Route`, `Order` (use tidy 10/20/30 sequences — see CLAUDE.md), `HierarchyParts` (the path segments that drive sidebar nesting), and `SectionLabel` (group header). Return an index entry first, then one per record._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentTocEntriesAsync
```

### 5. Return `ContentToCopy` / `ContentToCreate` lists

_`GetContentToCopyAsync` is for static assets that should be copied verbatim into the output tree (images, downloads). `GetContentToCreateAsync` is for dynamically generated files that are not routes the crawler will hit — the `LlmsTxtContentService` uses it for stripped-markdown sidecars. For a service whose only output is HTML served by a `MapGet` endpoint, both return `ImmutableList.Empty`._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCopyAsync
```

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetContentToCreateAsync
```

### 6. Publish xref ids through `GetCrossReferencesAsync`

_Each `CrossReference(uid, title, route)` registers a uid other content can target with `<xref:uid>` or `href="xref:uid"`. Pick a stable prefix (`release-1.0.0` here) so authors can deep-link specific entries without pasting URLs that might change._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.GetCrossReferencesAsync
```

### 7. Register the service in DI

_`AddPennington` does not auto-discover `IContentService`s — register yours directly on `IServiceCollection`. When an endpoint elsewhere in `Program.cs` needs the concrete type (to render detail pages), register it once by concrete type and forward `IContentService` to the same instance so the container does not create a second copy._

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/releases/` — the index lists every entry and each `/releases/{version}/` renders
- The "Releases" section shows up in navigation with one child per discovered record
- Authoring `<xref:release-1.0.0>` inside a markdown page resolves to the right URL in the rendered output, and the static build (`dotnet run -- build`) writes one HTML file per route under `output/releases/`

## Related

- Reference: [_Content pipeline interfaces_](/reference/extension-points/content-pipeline)
- Reference: [_Routing types_](/reference/extension-points/routing)
- Background: [_The content pipeline and union types_](/explanation/core/content-pipeline)
- Background: [_When is DocSite the right starting point?_](/explanation/core/docsite-positioning)
