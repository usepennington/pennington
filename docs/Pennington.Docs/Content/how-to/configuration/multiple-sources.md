---
title: "Use multiple content sources"
description: "Register more than one markdown root — either as DocSite areas or as chained AddMarkdownContent calls on a bare Pennington host — and keep them from overlapping."
uid: how-to.configuration.multiple-sources
order: 202010
sectionLabel: Configuration
tags: [configuration, content-sources, areas, overlap-detection]
---

When one markdown tree has outgrown a single root — a `/docs/` section alongside a separate `/blog/` section, or a catch-all root paired with a specialised subtree — you need to register multiple content sources. The right recipe depends on your host: `AddDocSite` supports multiple folder-scoped sub-trees through `ContentArea` entries, while bare `AddPennington` lets you chain any number of `AddMarkdownContent<T>` calls with independent front-matter types. If you haven't stood up a site yet, start with <xref:tutorials.getting-started.first-page>.

## Assumptions

- You have a working Pennington site (see [_Your first Pennington site_](xref:tutorials.getting-started.first-page) if not)
- You know which host extension you are using — `AddDocSite` vs bare `AddPennington` — and why ([_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning))
- You understand `IFrontMatter` basics ([_Use a custom front-matter record_](xref:how-to.content-authoring.front-matter))

To copy a working DocSite multi-area setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). For the bare `AddPennington` chained-sources recipe, see [`examples/MultipleSourcesExample`](https://github.com/usepennington/pennington/tree/main/examples/MultipleSourcesExample) — the helpers on `ServiceConfiguration` back each step below.

---

## Steps

### 1. Decide: DocSite areas, or chained `AddMarkdownContent` calls?

`AddDocSite` owns exactly one markdown pipeline keyed on `DocSiteFrontMatter`; use `ContentArea[]` on `DocSiteOptions.Areas` to split it into folder-scoped sub-trees (continue to step 2). When you need two different front-matter types, or you are already on bare `AddPennington`, chain `AddMarkdownContent<T>` calls instead (jump to step 4).

### 2. (DocSite) Declare the areas

Each `ContentArea` slug becomes both the URL prefix and the top-level folder under `ContentRootPath`.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildAreas
```

### 3. (DocSite) Wire the areas onto `DocSiteOptions.Areas`

Assign the areas array when building `DocSiteOptions` — the relevant property is `Areas = BuildAreas()`.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

Skip to **Verify**.

### 4. (Bare Pennington) Register the first markdown source

Call `AddMarkdownContent<TFrontMatter>` inside `AddPennington` with a `ContentPath` that roots the first tree, a distinct `BasePageUrl`, and an optional `SectionLabel` that groups the source's pages in navigation.

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterDocSource(Pennington.Infrastructure.MarkdownContentOptions)
```

### 5. (Bare Pennington) Register the second markdown source

Point the second `AddMarkdownContent<T>` at a different `ContentPath` and `BasePageUrl`; the front-matter type can differ from the first source.

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterBlogSource(Pennington.Infrastructure.MarkdownContentOptions)
```

### 6. (Optional) Carve out an overlapping subtree with `ExcludePaths`

When one source's `ContentPath` is a parent of another's, Pennington emits an overlap warning at startup because both pipelines would discover the inner tree and produce conflicting outputs. Add `ExcludePaths` on the broader source so the specialised source owns that subtree exclusively.

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
