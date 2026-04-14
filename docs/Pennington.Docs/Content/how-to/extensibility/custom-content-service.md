---
title: "Implement a custom IContentService"
description: "Discover custom sources, yield DiscoveredItems and ContentToCopy/ContentToCreate, and emit ContentTocItems and cross-references."
section: "extensibility"
order: 10
tags: []
uid: how-to.extensibility.custom-content-service
isDraft: true
search: false
llms: false
---

> **In this page.** Discovering custom sources, yielding `DiscoveredItem`s and `ContentToCopy`/`ContentToCreate`, and emitting `ContentTocItem`s and cross-references.
>
> **Not in this page.** Parsing/rendering — those are separate interfaces covered below.

## When to use this

- Outline bullet: you have content that is not markdown-on-disk (JSON files, a database, a remote API, a file format with its own parser like Cooklang) and need to plug it into the Pennington pipeline without writing a full parser/renderer stack.
- Outline bullet: you need programmatic or parameterized routes (per-tag pages, per-locale duplicates, per-record detail pages) that `MarkdownContentService` and `RazorPageContentService` do not produce on their own.

## Assumptions

- You have an existing Pennington site with a working `AddPennington(...)` registration.
- You are comfortable defining an `IFrontMatter` record (see [_Work with front matter_](/how-to/content-authoring/front-matter) if not).
- You know whether each route delegates to an existing handler (Razor @page, markdown file) or produces its own bytes — that determines which `ContentSource` case to yield.

To copy a working setup, see [`examples/RecipeExample`](https://github.com/usepennington/pennington/tree/main/examples/RecipeExample), [`examples/ForgePortalExample`](https://github.com/usepennington/pennington/tree/main/examples/ForgePortalExample), and [`examples/YogaStudioExample`](https://github.com/usepennington/pennington/tree/main/examples/YogaStudioExample). Each takes a different pattern — file-format-backed, JSON-backed programmatic HTML, and parameterized route fan-out — do not walk any example end-to-end; this is a recipe, not a tour.

---

## Steps

### 1. Implement `IContentService` on a sealed class

- Outline bullet: add a class that implements `Pennington.Content.IContentService`; the interface requires `DiscoverAsync`, `GetContentToCopyAsync`, `GetContentToCreateAsync`, `GetContentTocEntriesAsync`, `GetCrossReferencesAsync`, plus `DefaultSection` and `SearchPriority` properties.
- Outline bullet: set `DefaultSection` to the navigation grouping key (used as the section fallback for emitted entries) and `SearchPriority` to an integer used by the search index to rank results from this source.
- Outline bullet: `GetIndexableEntriesAsync` is a default member on the interface that returns `GetContentTocEntriesAsync` — override it only when "shown in navigation" differs from "searchable".
- Outline bullet: minimal service shape using a file-format source:

```csharp:xmldocid
T:RecipeExample.RecipeContentService
```

### 2. Yield a `DiscoveredItem` per route from `DiscoverAsync`

- Outline bullet: `DiscoverAsync` returns `IAsyncEnumerable<DiscoveredItem>`; each yield is `new DiscoveredItem(ContentRoute, ContentSource)`.
- Outline bullet: build the `ContentRoute` with `ContentRouteFactory.FromRazorPage(url[, locale])` when an existing Razor `@page` component will render it, `ContentRouteFactory.FromCustom(urlPath)` for arbitrary programmatic routes, or a hand-built `new ContentRoute { CanonicalPath, OutputFile }` when you need to set `OutputFile` explicitly.
- Outline bullet: pick the `ContentSource` case that matches the origin:
  - `RazorPageSource(name)` — the URL round-trips through an existing Blazor endpoint (parameterized detail pages, locale-prefixed copies).
  - `ProgrammaticSource(IProgrammaticContentGenerator)` — your service synthesizes the HTML or bytes itself.
  - `MarkdownFileSource` / `RedirectSource` — already produced by the built-in services; rarely used from a custom service.
- Outline bullet: programmatic variant that emits `ProgrammaticSource` wired to an `IProgrammaticContentGenerator`:

```csharp:xmldocid
M:ForgePortalExample.ReleaseNotesContentService.DiscoverAsync
```

- Outline bullet: parameterized and locale-prefixed Razor routes that reuse an existing Blazor component:

```csharp:xmldocid
M:YogaStudioExample.Services.YogaRouteContentService.DiscoverAsync
```

### 3. Return static/generated files from `GetContentToCopyAsync` / `GetContentToCreateAsync`

- Outline bullet: `GetContentToCopyAsync` returns `ImmutableList<ContentToCopy>` where each `ContentToCopy(FilePath SourcePath, FilePath OutputPath)` is copied verbatim into the output tree — use for images, downloads, or sidecar assets the source owns.
- Outline bullet: `GetContentToCreateAsync` returns `ImmutableList<ContentToCreate>` where each `ContentToCreate(FilePath OutputPath, Func<Task<byte[]>> ContentGenerator, string ContentType)` is evaluated lazily and written under the output directory — use for generated JSON, feeds, or binary artifacts that are not full pages.
- Outline bullet: return `ImmutableList<...>.Empty` when the service has no files to copy or create (every example service does this for at least one of the two).

### 4. Populate navigation from `GetContentTocEntriesAsync`

- Outline bullet: return one `ContentTocItem(Title, Route, Order, HierarchyParts, Section, Locale)` per page you want to appear in the TOC; omit routes that should be reachable but invisible to navigation (see `YogaRouteContentService` which returns `Empty`).
- Outline bullet: `HierarchyParts` is a `string[]` of URL segments that drives the tree shape in `NavigationBuilder.BuildTree`; split the canonical path on `/` to build it.
- Outline bullet: set `Section` to group the page in navigation (defaults you emit should match `DefaultSection` unless you are mixing sections within one service).
- Outline bullet: set `ExcludeFromSearch` / `ExcludeFromLlms` on the `ContentTocItem` init properties when the underlying front matter's `Search` / `Llms` flags are false — the default `GetIndexableEntriesAsync` delegates to this list, so silent opt-outs require it.
- Outline bullet: reference pattern building hierarchy parts from the URL:

```csharp:xmldocid
M:RecipeExample.RecipeContentService.GetContentTocEntriesAsync
```

### 5. Emit uids from `GetCrossReferencesAsync`

- Outline bullet: return `ImmutableList<CrossReference>` where each `CrossReference(string Uid, string Title, ContentRoute Route)` makes the page resolvable via `<xref:uid>` or `href="xref:uid"` through `XrefResolver`.
- Outline bullet: pick a stable uid convention scoped to the source (e.g. `forge.release.v{version}`) so writers can link without knowing the URL shape.
- Outline bullet: return `ImmutableList<CrossReference>.Empty` when the source does not want cross-references — uids are optional per page.
- Outline bullet: example uid emission keyed off the record's version field:

```csharp:xmldocid
M:ForgePortalExample.ReleaseNotesContentService.GetCrossReferencesAsync
```

### 6. Register the service in DI

- Outline bullet: register as `IContentService` so the pipeline discovers it; singleton lifetime matches the built-in services and plays with `AddFileWatched<T>` if the source reads disk.
- Outline bullet: if consumers also need the concrete type (to call service-specific methods like `RecipeContentService.GetRecipeByUrlOrDefault`), register the concrete type first and resolve it from the `IContentService` factory:

```csharp
builder.Services.AddSingleton<RecipeContentService>(sp =>
    new RecipeContentService(recipePath));
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<RecipeContentService>());
```

- Outline bullet: plain registration when only the `IContentService` contract is needed:

```csharp
builder.Services.AddSingleton<Pennington.Content.IContentService, YogaRouteContentService>();
```

---

## Verify

- Run `dotnet run` and confirm each URL your service yields from `DiscoverAsync` returns 200 (check a programmatic route, a parameterized route, and a locale-prefixed route if applicable).
- Run `dotnet run -- build` and confirm the build report lists the expected page count and no `FailedItem` errors for your routes.
- Fetch `/search-index-{locale}.json` and `/sitemap.xml` and confirm your pages appear (or are correctly excluded when `ExcludeFromSearch` is set).

## Related

- Reference: [Content pipeline interfaces](/reference/extension-points/content-pipeline)
- Reference: [Routing types](/reference/extension-points/routing)
- Background: [The content pipeline and union types](/explanation/core/content-pipeline)
