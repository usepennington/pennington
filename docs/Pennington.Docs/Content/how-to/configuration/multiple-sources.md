---
title: "Use multiple content sources"
description: "Register more than one markdown root — either as DocSite areas or as chained AddMarkdownContent calls on a bare Pennington host — and keep them from overlapping."
uid: how-to.configuration.multiple-sources
order: 10
sectionLabel: Configuration
tags: [configuration, content-sources, areas, overlap-detection]
---

> **In this page.** Chain `AddMarkdownContent<T>` calls with distinct `ContentPath` / `BasePageUrl` / `SectionLabel` / `ExcludePaths`, and read the overlap-detection warning when two roots collide. `AddDocSite` owns a single `AddMarkdownContent<DocSiteFrontMatter>` registration, so splitting one DocSite into multiple markdown trees is done through `ContentArea` entries on `DocSiteOptions.Areas`; pairing a second front-matter type with DocSite requires dropping to bare `AddPennington` — see [_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning).
>
> **Not in this page.** Implementing a non-markdown content service (JSON, database, remote API) — see [_Implement a custom content service_](xref:how-to.extensibility.custom-content-service).

## When to use this

_Two to three sentences. Readers arrive here when one markdown tree has outgrown a single root: they want a `/docs/` section and a separate `/blog/` section, or a catch-all root plus a specialised subtree (e.g., auto-generated API pages). Point out that the right recipe depends on whether the host is `AddDocSite` (one front-matter type, multiple `ContentArea` slugs) or bare `AddPennington` (any number of `AddMarkdownContent<T>` calls with independent front-matter types). If the reader has not yet stood up a site, redirect them to [_Your first Pennington site_](xref:tutorials.getting-started.first-page)._

## Assumptions

- You have a working Pennington site (see [_Your first Pennington site_](xref:tutorials.getting-started.first-page) if not)
- You know which host extension you are using — `AddDocSite` vs bare `AddPennington` — and why ([_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning))
- You understand `IFrontMatter` basics ([_Use a custom front-matter record_](xref:how-to.content-authoring.front-matter))

To copy a working DocSite multi-area setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). For the bare `AddPennington` chained-sources recipe, see [`examples/MultipleSourcesExample`](https://github.com/usepennington/pennington/tree/main/examples/MultipleSourcesExample) — the helpers on `ServiceConfiguration` back each step below.

---

## Steps

### 1. Decide: DocSite areas, or chained `AddMarkdownContent` calls?

_One or two sentences. If you are on `AddDocSite`, you get exactly one markdown pipeline keyed on `DocSiteFrontMatter`; use `ContentArea[]` on `DocSiteOptions.Areas` to split it into folder-scoped sub-trees (go to step 2). If you need two different front-matter types, or you are already on bare `AddPennington`, chain `AddMarkdownContent<T>` calls instead (jump to step 4)._

### 2. (DocSite) Declare the areas

_One sentence. Each `ContentArea` slug becomes both the URL prefix and the top-level folder under `ContentRootPath`._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildAreas
```

### 3. (DocSite) Wire the areas onto `DocSiteOptions.Areas`

_One sentence. `BuildDocSiteOptions` is the full record literal — the relevant line is `Areas = BuildAreas()`._

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

Skip to **Verify**.

### 4. (Bare Pennington) Register the first markdown source

_One sentence. Call `AddMarkdownContent<TFrontMatter>` inside `AddPennington` with a `ContentPath` that roots the first tree, a distinct `BasePageUrl`, and an optional `SectionLabel` that groups the source's pages in navigation._

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterDocSource(Pennington.Infrastructure.MarkdownContentOptions)
```

### 5. (Bare Pennington) Register the second markdown source

_One sentence. Point the second `AddMarkdownContent<T>` at a different `ContentPath` and `BasePageUrl`; the front-matter type can differ._

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterBlogSource(Pennington.Infrastructure.MarkdownContentOptions)
```

### 6. (Optional) Carve out an overlapping subtree with `ExcludePaths`

_One or two sentences. When one source's `ContentPath` is a parent of another's, Pennington emits an overlap warning at startup because both pipelines would discover the inner tree and race each other's outputs. Add `ExcludePaths` on the broader source so the specialised source owns that subtree._

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterOverlappingDocSource(Pennington.Infrastructure.MarkdownContentOptions)
```

---

## Verify

- Run `dotnet run` and visit each source's `BasePageUrl` — confirm pages render under both prefixes
- Startup logs contain no `Markdown content source rooted at '…' overlaps…` warnings (or, if expected, the warning text names the subtree you intend to exclude)
- Each source's pages appear under the correct `SectionLabel` / `ContentArea.Title` in the generated navigation

## Related

- Reference: [_`PenningtonOptions.AddMarkdownContent<T>`_](xref:reference.options.pennington-options) _(confirm path)_
- Reference: [_`DocSiteOptions.Areas` / `ContentArea`_](xref:reference.options.docsite-options) _(confirm path)_
- Background: [_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning)
- Extensibility: [_Implement a custom content service_](xref:how-to.extensibility.custom-content-service)
