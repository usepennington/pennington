---
title: "Drop a Razor component into a markdown page"
description: "Embed Pennington.UI components (and your own Razor components) inside a `.md` file through Mdazor-backed rendering."
uid: how-to.rich-content.ui-components-in-markdown
order: 203030
sectionLabel: "Rich Content"
tags: [authoring, components, mdazor, razor]
---

To place a Razor component tag — `<Badge>`, `<Card>`, or one of your own — directly inside a `.md` file instead of authoring raw HTML, write the tag where CommonMark allows an HTML block. Mdazor matches the tag against the registered component types, binds attribute values to `[Parameter]` properties by case-insensitive name, and renders inner content through the markdown pipeline. To author a brand-new component from scratch, see <xref:tutorials.beyond-basics.custom-razor-component>.

## Assumptions

- A working Pennington site that renders markdown (see <xref:tutorials.getting-started.first-site> if not).
- The host calls `AddDocSite`, `AddBlogSite`, or `AddPennington`. The first two register seven of the eight Pennington.UI components automatically (everything but `CodeBlock`, which is Razor-page-only); bare `AddPennington` requires the one-line registration shown under "Register components on a bare host" below.
- Component tag names start with an uppercase letter and match the Razor component type name — case-sensitive on the leading character (`<Card>`, not `<card>`).

## The seven built-in components

`AddDocSite` and `AddBlogSite` pre-register `<Badge>`, `<BigTable>`, `<Card>`, `<CardGrid>`, `<LinkCard>`, `<Step>`, and `<Steps>`. Each H3 below shows the source markdown above the rendered output for the most common authoring shapes.

### Inline a built-in tag

Place the tag anywhere CommonMark allows an HTML block. Attribute values bind to `[Parameter]` properties by case-insensitive name match.

```markdown
<Badge>Preview</Badge>
```

<Badge>Preview</Badge>

### Pass markdown as `ChildContent`

Whatever appears between the open and close tags becomes the component's `ChildContent` render fragment and is parsed as markdown — `**bold**`, links, and nested components all work inside the body.

````markdown
<Card Title="What's new">
The **v2 pipeline** ships with [unified dev and build](xref:explanation.core.dev-vs-build).
</Card>
````

<Card Title="What's new">
The **v2 pipeline** ships with [unified dev and build](xref:explanation.core.dev-vs-build).
</Card>

### Bind primitive attributes

Only primitive parameter types (strings, numbers, booleans) bind from markdown attributes — the value arrives as a raw string and Mdazor converts it via reflection. For complex data, pack it into a delimited string and parse inside the component, or use `ChildContent` for rich content.

````markdown
<Card Title="Fast" Href="xref:explanation.core.dev-vs-build" Variant="primary">
Pages render in a single SSR pass.
</Card>
````

<Card Title="Fast" Href="xref:explanation.core.dev-vs-build" Variant="primary">
Pages render in a single SSR pass.
</Card>

## Register components on a bare host

`AddPennington` wires the component registry via `AddMdazor()` but does not register any components — that falls to the `AddDocSite` and `AddBlogSite` templates. Chain one `AddMdazorComponent<T>()` call per component that should be available in markdown.

```csharp:path
examples/DocSiteKitchenSinkExample/Program.cs
```

For the shape DocSite uses internally, see the Mdazor chain under <xref:reference.ui.content>.

## What the renderer emits

Mdazor parses the component tag out of the HTML block, instantiates the matching Razor component, binds attribute values to `[Parameter]` properties, and renders the result inline — the original `<Badge>` (or other tag) literal disappears from the output. If the tag does not match a registered component on a bare `AddPennington` host, it falls through unchanged and renders as literal text, which is the fastest way to confirm whether the registration is what activates a tag.

## Related

- Reference: [Content components](xref:reference.ui.content) — parameters and render behaviour for `Card`, `CardGrid`, `LinkCard`, `Badge`, `Step`, `Steps`, `CodeBlock`, and `BigTable`.
- Tutorial: [Author a custom Razor component for markdown](xref:tutorials.beyond-basics.custom-razor-component) — write your own `<PricingCard>`-style component and wire it through `AddMdazorComponent<T>()`.
- How-to: [Customize DocSite layouts and components](xref:how-to.response-pipeline.override-docsite-components) — when you need to change the surrounding page, not only embed a tag inside markdown.
