---
title: "Use UI components inside markdown"
description: "Drop Pennington.UI components (and your own Razor components) straight into markdown through Mdazor-backed rendering."
uid: how-to.content-authoring.ui-components-in-markdown
order: 201090
sectionLabel: Content Authoring
tags: [authoring, components, mdazor, razor]
---

To embed a Razor component tag — such as `<Badge>` or `<Card>` — directly inside a `.md` file instead of writing raw HTML, use the patterns on this page. It also covers the extra registration step needed when the host calls `AddPennington` directly rather than through the `AddDocSite` or `AddBlogSite` templates. To author a new component from scratch, see the <xref:tutorials.beyond-basics.custom-razor-component> tutorial.

## Assumptions

- A working Pennington site that renders markdown (see the <xref:tutorials.getting-started.first-site> tutorial if not).
- The host calls `AddDocSite`, `AddBlogSite`, or `AddPennington` — the first two register the eight Pennington.UI components automatically; the last requires the one-line registration shown in step 4.
- Component tag names start with an uppercase letter and match the Razor component type name — case-sensitive on the leading character (for example, `<Card>`, not `<card>`).

The `examples/DocSiteKitchenSinkExample` project exercises three `<FeatureCallout>` instances plus a `<Badge>` and shows the `AddMdazorComponent<FeatureCallout>()` registration in `Program.cs`.

---

## Steps

<Steps>
<Step StepNumber="1">

**Drop a built-in component into a markdown page**

The eight Pennington.UI components (`<Badge>`, `<BigTable>`, `<Card>`, `<CardGrid>`, `<CodeBlock>`, `<LinkCard>`, `<Step>`, `<Steps>`) are pre-registered by `AddDocSite` and `AddBlogSite`, so the tag goes straight into any `.md` file. Place it anywhere CommonMark allows an HTML block — attribute values bind to `[Parameter]` properties by case-insensitive name match.

```markdown
# Release notes

<Badge>Preview</Badge>

The v2 pipeline is shipping today.
```

</Step>
<Step StepNumber="2">

**Pass markdown as `ChildContent`**

Whatever appears between the open and close tags becomes the component's `ChildContent` render fragment and is parsed as markdown — so `**bold**`, links, and nested components all work inside a `<Card>` or `<FeatureCallout>` body.

```markdown
<Card Title="What's new">
The **v2 pipeline** ships with [unified dev and build](/explanation/core/dev-vs-build).
</Card>
```

</Step>
<Step StepNumber="3">

**Bind primitive attributes to `[Parameter]` properties**

Only primitive parameter types (strings, numbers, booleans) bind from markdown attributes — the attribute value arrives as a raw string and Mdazor converts it via reflection. For complex data, pack it into a delimited string and parse it inside the component, or use `ChildContent` for rich content.

```markdown
<Card Title="Fast" Href="/explanation/core/dev-vs-build" Variant="primary">
Pages render in a single SSR pass.
</Card>
```

</Step>
<Step StepNumber="4">

**On a bare `AddPennington` host, register each component once**

`AddPennington` wires the component registry via `AddMdazor()` but does not register any components — that falls to the `AddDocSite` and `AddBlogSite` templates. Chain one `AddMdazorComponent<T>()` call per component that should be available in markdown.

```csharp:path
examples/DocSiteKitchenSinkExample/Program.cs
```

For the shape DocSite uses internally, see the Mdazor chain under <xref:reference.ui.content>.

</Step>
<Step StepNumber="5">

**Review the end-to-end fixture**

The kitchen-sink example page stages three `<FeatureCallout Kind="tip|info|warn">` instances alongside a built-in `<Badge>`, covering attributes, `ChildContent`, and multiple visual variants in one file.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/ui-components-in-markdown.md
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run` and visit the authored page — each component tag renders as the component's output (not raw text or an empty element) and `ChildContent` appears inside it as rendered markdown.
- View source on the rendered HTML — component markup replaces the tag entirely; no `<Badge>` literal remains in the output.
- On a bare `AddPennington` host, omit the `AddMdazorComponent<T>()` line for one component and reload — the tag renders as literal text, confirming the registration is what activates it.

## Related

- Reference: [Content components](xref:reference.ui.content) — parameters and render behaviour for `Card`, `CardGrid`, `LinkCard`, `Badge`, `Step`, `Steps`, `CodeBlock`, and `BigTable`.
- Tutorial: [Author a custom Razor component for markdown](xref:tutorials.beyond-basics.custom-razor-component) — write your own `<PricingCard>`-style component and wire it through `AddMdazorComponent<T>()`.
- How-to: [Customize DocSite layouts and components](xref:how-to.extensibility.override-docsite-components) — when you need to change the surrounding page, not only embed a tag inside markdown.
