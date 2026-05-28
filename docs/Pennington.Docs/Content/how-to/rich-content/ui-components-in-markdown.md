---
title: "Drop a Razor component into a markdown page"
description: "Embed Pennington.UI components (and your own Razor components) inside a `.md` file through Mdazor-backed rendering."
uid: how-to.rich-content.ui-components-in-markdown
order: 3
sectionLabel: "Rich Content"
tags: [authoring, components, mdazor, razor]
---

To place a Razor component tag — `<Badge>`, `<Card>`, or one of your own — directly inside a `.md` file instead of authoring raw HTML, write the tag where CommonMark allows an HTML block. [Mdazor](xref:reference.ui.content) matches the tag against registered component types and binds attribute values to `[Parameter]` properties. To author a brand-new component from scratch, see <xref:tutorials.beyond-basics.custom-razor-component>.

## Before you begin
- A working Pennington site that renders markdown (see <xref:tutorials.getting-started.first-site> if not).
- The host calls `AddDocSite`, `AddBlogSite`, or `AddPennington`. The first two pre-register the Pennington.UI components meant for markdown use; bare `AddPennington` requires the registration shown under "Register components on a bare host" below.
- Component tag names start with an uppercase letter and match the Razor component type name — case-sensitive on the leading character (`<Card>`, not `<card>`).

## Authoring shapes

`AddBlogSite` pre-registers eight components — `<Badge>`, `<BigTable>`, `<Card>`, `<CardGrid>`, `<Checkpoint>`, `<LinkCard>`, `<Step>`, and `<Steps>`. `AddDocSite` adds one more, `<RenderedFixture>`, for embedding rendered example output into a page. The H3s below cover the three most common authoring shapes; for the full parameter surface of each component, see <xref:reference.ui.content>.

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

Only primitive parameter types (strings, numbers, booleans) bind from markdown attributes — the value arrives as a raw string and Mdazor converts it via reflection.

````markdown
<Card Title="Fast" Href="xref:explanation.core.dev-vs-build" Variant="primary">
Pages render in a single SSR pass.
</Card>
````

<Card Title="Fast" Href="xref:explanation.core.dev-vs-build" Variant="primary">
Pages render in a single SSR pass.
</Card>

For complex data, pack it into a delimited string and parse inside the component, or use `ChildContent` for rich content. <xref:reference.ui.content> shows the same pattern in the parameter tables.

## Register components on a bare host

`AddPennington` wires the component registry via `AddMdazor()` but does not register any components. Chain one `AddMdazorComponent<T>()` call per component that should be available in markdown — see <xref:reference.ui.content> for the registration block DocSite and BlogSite use.

```csharp
builder.Services.AddMdazorComponent<Badge>()
                .AddMdazorComponent<Card>()
                // ... one line per component
                ;
```

## What the renderer emits

Mdazor parses the component tag out of the HTML block, instantiates the matching Razor component, binds attribute values to `[Parameter]` properties, and renders the result inline. The original tag literal disappears from the output. An unregistered tag on a bare `AddPennington` host falls through unchanged and renders as literal text — the fastest way to confirm whether the registration is what activates a tag.

See <xref:reference.ui.content> for the parameters of each built-in component.

## Related

- Reference: [Content components](xref:reference.ui.content) — parameters and render behaviour for `Badge`, `BigTable`, `Card`, `CardGrid`, `Checkpoint`, `LinkCard`, `RenderedFixture` (DocSite only), `Step`, and `Steps`.
- Tutorial: [Author a custom Razor component for markdown](xref:tutorials.beyond-basics.custom-razor-component) — write your own `<PricingCard>`-style component and wire it through `AddMdazorComponent<T>()`.
- How-to: [Customize DocSite layouts and components](xref:how-to.response-pipeline.override-docsite-components) — when you need to change the surrounding page, not only embed a tag inside markdown.
