---
title: "Use UI components inside markdown"
description: "Drop Pennington.UI components (and your own Razor components) straight into markdown through Mdazor-backed rendering."
uid: how-to.content-authoring.ui-components-in-markdown
order: 90
sectionLabel: Content Authoring
tags: [authoring, components, mdazor, razor]
---

> **In this page.** _Paraphrase TOC "Covers": dropping Pennington.UI components into markdown through Mdazor-backed rendering, plus the one-line `AddMdazorComponent<T>()` registration shape needed when you are on a bare `AddPennington` host instead of `AddDocSite`/`AddBlogSite` (which register the eight built-ins for you)._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": the full Mdazor parser internals and broader DocSite layout customization. Point to [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components) and to the [Author a custom Razor component for markdown](xref:tutorials.beyond-basics.custom-razor-component) tutorial when the reader wants to write a new component._

## When to use this

_Two sentences. Frame the realistic arrival state: the reader already has a DocSite or BlogSite with markdown rendering, and they want to embed a component tag like `<Badge>` or `<Card>` inline in a `.md` file instead of hand-rolling HTML. Also cover the secondary case — a bare `AddPennington` host that has not picked up the Pennington.UI components automatically — and note that the custom-component tutorial handles authoring a new component._

## Assumptions

_Three bullets. Each is realistic prior state, not a tutorial step. Keep to the minimum the recipe depends on._

- You have a working Pennington site that renders markdown (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- Your host calls `AddDocSite`, `AddBlogSite`, or `AddPennington` — the first two register the eight Pennington.UI components for you; the last requires the one-line registration shown in step 4.
- Component tag names start with an uppercase letter and match the Razor component type name — case-sensitive on the leading character (e.g. `<Card>`, not `<card>`).

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/ui-components-in-markdown.md` exercises three `<FeatureCallout>` instances plus a `<Badge>`, and `Program.cs` shows the single `AddMdazorComponent<FeatureCallout>()` line. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four steps. Each opens with an imperative verb and keeps prose under two sentences. Markdown snippets use plain fences (the authored source is a `.md` file, not a C# symbol); step 4 uses the one `xmldocid` fence into the example's `Program.cs` registration line. Step 5 fences the whole fixture markdown file via `:path` so the reader sees every shape end-to-end._

### 1. Drop a built-in component into a markdown page

_One sentence: the eight Pennington.UI components (`<Badge>`, `<BigTable>`, `<Card>`, `<CardGrid>`, `<CodeBlock>`, `<LinkCard>`, `<Step>`, `<Steps>`) are pre-registered by `AddDocSite` and `AddBlogSite`, so you just type the tag inline. Use the tag anywhere CommonMark allows an HTML block — attribute values become `[Parameter]` bindings by case-insensitive name match._

```markdown
# Release notes

<Badge>Preview</Badge>

The v2 pipeline is shipping today.
```

### 2. Pass markdown as `ChildContent`

_One sentence: whatever appears between the open and close tags becomes the component's `ChildContent` render fragment and is parsed as markdown — so `**bold**`, links, and nested components all work inside a `<Card>` or `<FeatureCallout>` body._

```markdown
<Card Title="What's new">
The **v2 pipeline** ships with [unified dev and build](/explanation/core/dev-vs-build).
</Card>
```

### 3. Bind primitive attributes to `[Parameter]` properties

_Two sentences. Only primitive parameter types (strings, numbers, booleans) bind from markdown attributes because the attribute value arrives as a raw string and Mdazor converts it via reflection. Pack complex data into a delimited string and parse it inside the component, or use `ChildContent` for rich content._

```markdown
<Card Title="Fast" Href="/explanation/core/dev-vs-build" Variant="primary">
Pages render in a single SSR pass.
</Card>
```

### 4. On a bare `AddPennington` host, register each component once

_Two sentences. `AddPennington` calls `AddMdazor()` to wire the component registry but does not register any components — that is the DocSite/BlogSite templates' job. Chain one `AddMdazorComponent<T>()` call per component you want to use from markdown. The example embeds `Program.cs` via `:path` because top-level-statements files do not have a stable xmldocid to point at._

```csharp:path
examples/DocSiteKitchenSinkExample/Program.cs
```

_For the shape DocSite uses internally (all eight built-ins registered on one call chain), fence the extension method body:_

```csharp:xmldocid,bodyonly
M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})
```

### 5. Review the end-to-end fixture

_One sentence: the kitchen-sink example page stages three `<FeatureCallout Kind="tip|info|warn">` instances (the custom component defined under `Components/`) alongside a built-in `<Badge>`, covering attributes, `ChildContent`, and the three visual variants in one file._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/ui-components-in-markdown.md
```

---

## Verify

_Three bullets. Each is one observable check the reader can run without reading anything else._

- Run `dotnet run` and visit the authored page — each component tag renders as the component's output (not as raw text, not as an empty element) and `ChildContent` appears inside it as rendered markdown.
- View source on the rendered HTML — component markup replaces the tag entirely; there is no `<Badge>` literal in the output.
- On a bare `AddPennington` host, omit the `AddMdazorComponent<T>()` line for one component and reload — the tag renders as literal text or an empty element, confirming the registration is what activates it.

## Related

_Three cross-quadrant links. Reference for the component parameter catalog, the custom-component tutorial for authoring a new component, and the override how-to for layout-level changes. Do not link to the next how-to in this section — generated automatically._

- Reference: [Content components](xref:reference.ui.content) — parameters and render behaviour for `Card`, `CardGrid`, `LinkCard`, `Badge`, `Step`, `Steps`, `CodeBlock`, and `BigTable`.
- Tutorial: [Author a custom Razor component for markdown](xref:tutorials.beyond-basics.custom-razor-component) — write your own `<PricingCard>`-style component and wire it through `AddMdazorComponent<T>()`.
- How-to: [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components) — when you need to change the surrounding page, not just embed a tag inside markdown.
