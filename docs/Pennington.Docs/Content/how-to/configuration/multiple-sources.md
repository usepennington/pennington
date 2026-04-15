---
title: "Use multiple content sources"
description: "Register more than one markdown root — either as DocSite areas or as chained AddMarkdownContent calls on a bare Pennington host — and keep them from overlapping."
uid: how-to.configuration.multiple-sources
order: 202010
sectionLabel: Configuration
tags: [configuration, content-sources, areas, overlap-detection]
---

When one markdown tree outgrows a single root — a `/docs/` section alongside a separate `/blog/` section, or a catch-all root paired with a specialised subtree — registering multiple content sources is the answer. The right recipe depends on the host: `AddDocSite` supports multiple folder-scoped sub-trees through `ContentArea` entries, while bare `AddPennington` allows any number of chained `AddMarkdownContent<T>` calls with independent front-matter types. For a first site, start with <xref:tutorials.getting-started.first-page>.

## Assumptions

- A working Pennington site (see [_Your first Pennington site_](xref:tutorials.getting-started.first-page) if not)
- The chosen host extension — `AddDocSite` vs bare `AddPennington` — and the reason for that choice ([_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning))
- Familiarity with `IFrontMatter` basics ([_Use a custom front-matter record_](xref:how-to.content-authoring.front-matter))

For a working DocSite multi-area setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample). For the bare `AddPennington` chained-sources recipe, see [`examples/MultipleSourcesExample`](https://github.com/usepennington/pennington/tree/main/examples/MultipleSourcesExample); the helpers on `ServiceConfiguration` back each step below.

---

## Steps

<Steps>
<Step StepNumber="1">

**Decide: DocSite areas, or chained `AddMarkdownContent` calls?**

`AddDocSite` owns exactly one markdown pipeline keyed on `DocSiteFrontMatter`; use `ContentArea[]` on `DocSiteOptions.Areas` to split it into folder-scoped sub-trees (continue to step 2). For two different front-matter types, or for a site already on bare `AddPennington`, chain `AddMarkdownContent<T>` calls instead (jump to step 4).

</Step>
<Step StepNumber="2">

**(DocSite) Declare the areas**

Each `ContentArea` slug becomes both the URL prefix and the top-level folder under `ContentRootPath`.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildAreas
```

</Step>
<Step StepNumber="3">

**(DocSite) Wire the areas onto `DocSiteOptions.Areas`**

Assign the areas array when building `DocSiteOptions` — the relevant property is `Areas = BuildAreas()`.

```csharp:xmldocid,bodyonly
M:DocSiteKitchenSinkExample.ServiceConfiguration.BuildDocSiteOptions
```

Skip to **Verify**.

</Step>
<Step StepNumber="4">

**(Bare Pennington) Register the first markdown source**

Call `AddMarkdownContent<TFrontMatter>` inside `AddPennington` with a `ContentPath` that roots the first tree, a distinct `BasePageUrl`, and an optional `SectionLabel` to group the source's pages in navigation.

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterDocSource(Pennington.Infrastructure.MarkdownContentOptions)
```

</Step>
<Step StepNumber="5">

**(Bare Pennington) Register the second markdown source**

Point the second `AddMarkdownContent<T>` at a different `ContentPath` and `BasePageUrl`; the front-matter type can differ from the first source.

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterBlogSource(Pennington.Infrastructure.MarkdownContentOptions)
```

</Step>
<Step StepNumber="6">

**(Optional) Carve out an overlapping subtree with `ExcludePaths`**

When one source's `ContentPath` is a parent of another's, Pennington emits an overlap warning at startup because both pipelines would discover the inner tree and produce conflicting outputs. Adding `ExcludePaths` on the broader source gives the specialised source exclusive ownership of that subtree.

```csharp:xmldocid,bodyonly
M:MultipleSourcesExample.ServiceConfiguration.RegisterOverlappingDocSource(Pennington.Infrastructure.MarkdownContentOptions)
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run` and visit each source's `BasePageUrl`. Confirm pages render under both prefixes
- Startup logs contain no `Markdown content source rooted at '…' overlaps…` warnings (or, if expected, the warning text names the subtree intended for exclusion)
- Each source's pages appear under the correct `SectionLabel` / `ContentArea.Title` in the generated navigation

## Related

- Reference: [_`PenningtonOptions.AddMarkdownContent<T>`_](xref:reference.options.pennington-options) _(confirm path)_
- Reference: [_`DocSiteOptions.Areas` / `ContentArea`_](xref:reference.options.docsite-options) _(confirm path)_
- Background: [_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning)
- Extensibility: [_Implement a custom content service_](xref:how-to.extensibility.custom-content-service)
