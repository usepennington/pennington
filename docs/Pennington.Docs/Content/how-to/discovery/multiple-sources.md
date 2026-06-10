---
title: "Serve docs and a blog from separate content roots"
description: "Register more than one markdown root — either as DocSite areas or as chained AddMarkdownContent calls on a bare Pennington host — and keep them from overlapping."
uid: how-to.discovery.multiple-sources
order: 1
sectionLabel: "Content Discovery"
tags: [configuration, content-sources, areas, overlap-detection]
---

When one markdown tree needs more than one content root — a `/docs/` section alongside a separate `/blog/` section, or a catch-all root paired with a specialized subtree — registering multiple content sources is the answer. The right recipe depends on the host: `AddDocSite` supports multiple folder-scoped sub-trees through `ContentArea` entries on a single `DocSiteFrontMatter` pipeline; bare `AddPennington` allows any number of chained `AddMarkdownContent<T>` calls with independent front-matter types. For a first site, start with <xref:tutorials.getting-started.first-page>.

## Before you begin

- A working Pennington site (see [Your first Pennington site](xref:tutorials.getting-started.first-page) if not).
- The chosen host extension — `AddDocSite` versus bare `AddPennington` — and the reason for that choice (<xref:explanation.positioning.docsite-positioning>).
- Familiarity with `IFrontMatter` basics (<xref:how-to.pages.front-matter>).

For a working DocSite multi-area setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). For the bare `AddPennington` chained-sources recipe, see [`examples/MultipleSourcesExample`](https://github.com/usepennington/pennington/tree/main/examples/MultipleSourcesExample).

## Split a DocSite into areas

`AddDocSite` owns exactly one markdown pipeline keyed on `DocSiteFrontMatter`. To split that pipeline into folder-scoped sub-trees, populate `DocSiteOptions.Areas` with one `ContentArea` per slug — each slug becomes both the URL prefix and the top-level folder under `ContentRootPath`.

### Declare the areas

Build the `ContentArea` list — one entry per slug, where the slug is both the URL prefix and the top-level content folder.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildAreas
```

### Wire the areas onto `DocSiteOptions`

Assign that list to `DocSiteOptions.Areas` so the single `DocSiteFrontMatter` pipeline discovers each folder as its own sub-tree.

```csharp:symbol,bodyonly
examples/DocSiteKitchenSinkExample/ServiceConfiguration.cs > ServiceConfiguration.BuildDocSiteOptions
```

## Chain `AddMarkdownContent` on a bare host

On bare `AddPennington`, call `AddMarkdownContent<TFrontMatter>` once per source. Each call accepts its own `ContentPath`, `BasePageUrl`, and optional `SectionLabel`. Front-matter types can differ between sources.

### Register the first source

```csharp:symbol,bodyonly
examples/MultipleSourcesExample/ServiceConfiguration.cs > ServiceConfiguration.RegisterDocSource
```

### Register a second source with a different front-matter type

```csharp:symbol,bodyonly
examples/MultipleSourcesExample/ServiceConfiguration.cs > ServiceConfiguration.RegisterBlogSource
```

### Carve out an overlapping subtree with `ExcludePaths`

When one source's `ContentPath` is a parent of another's, Pennington emits an overlap warning at startup because both pipelines would discover the inner tree and produce conflicting outputs. Adding `ExcludePaths` on the broader source gives the specialized source exclusive ownership of that subtree.

```csharp:symbol,bodyonly
examples/MultipleSourcesExample/ServiceConfiguration.cs > ServiceConfiguration.RegisterOverlappingDocSource
```

## Verify

- Run `dotnet run` and visit each source's `BasePageUrl`. Pages render under both prefixes.
- Startup logs contain no `Markdown content source rooted at '…' overlaps…` warnings, or — when an overlap is intentional — the warning text names the subtree set aside for exclusion.
- Each source's pages appear under the correct `SectionLabel` / `ContentArea.Title` in the generated navigation.

## Related

- Reference: [`PenningtonOptions.AddMarkdownContent<T>`](xref:reference.api.pennington-options)
- Reference: [`DocSiteOptions.Areas` and `ContentArea`](xref:reference.api.doc-site-options)
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning)
- Extensibility: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
