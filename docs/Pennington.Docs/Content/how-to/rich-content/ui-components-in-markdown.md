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

`AddDocSite` pre-registers nine components — `<Badge>`, `<BigTable>`, `<Card>`, `<CardGrid>`, `<Checkpoint>`, `<LinkCard>`, `<RenderedFixture>`, `<Step>`, and `<Steps>`. `AddBlogSite` pre-registers the same set minus `<RenderedFixture>` (eight). The H3s below cover the three most common authoring patterns; for the full parameters of each component, see <xref:reference.ui.content>.

### Inline a built-in tag

Place the tag anywhere CommonMark allows an HTML block. Attribute values bind to `[Parameter]` properties by case-insensitive name match.

```markdown
<Badge Text="Preview" />
```

<Badge Text="Preview" />

### Pass markdown as `ChildContent`

Whatever appears between the open and close tags becomes the component's `ChildContent` render fragment and is parsed as markdown — `**bold**`, links, and nested components all work inside the body.

````markdown
<Card Title="New in v2">
The **v2 pipeline** ships with [unified dev and build](xref:explanation.core.dev-vs-build).
</Card>
````

<Card Title="New in v2">
The **v2 pipeline** ships with [unified dev and build](xref:explanation.core.dev-vs-build).
</Card>

### Bind primitive attributes

Only primitive parameter types (strings, numbers, booleans) bind from markdown attributes — the value arrives as a raw string and Mdazor converts it via reflection.

````markdown
<Card Title="Fast" Color="accent">
Pages render in a single SSR pass.
</Card>
````

<Card Title="Fast" Color="accent">
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

## Read page context in a component

Attributes carry what the author types on the tag. For facts about the *page* — the source file, the canonical URL, the front matter — a component reads the ambient `MdazorContext` that Pennington supplies for every rendered page. Declare a `[CascadingParameter]` of type `MdazorContext`; nothing goes on the tag.

```razor
@using Mdazor
@using Pennington.FrontMatter

<p>Rendered from <code>@Context?["FileName"]</code> at <code>@Context?["Url"]</code>.</p>

@code {
    [CascadingParameter] public MdazorContext? Context { get; set; }
}
```

`MdazorContext` exposes the bag through `Values`, an indexer (`Context["FileName"]`), `TryGet`, and `Get<T>`. Pennington fills it with these keys, matched case-insensitively:

| Key | Value |
| --- | --- |
| `SourceFile` | Source path on disk for the page |
| `FileName` / `FileNameWithoutExtension` | The source file name, with and without extension |
| `Url` / `CanonicalPath` | Canonical URL path for the page |
| `OutputFile` | Static output path written during `build` |
| `Locale` | Locale code; empty for the default locale |
| `Metadata` | The page's front matter as an [`IFrontMatter`](xref:reference.front-matter.keys) (`Title`, `Description`, `Uid`, …) |
| `Derived` | Enricher-contributed values (reading time, git last-modified, …) keyed by enricher name |

The context is delivered as a cascading value, so it reaches the component and any components nested inside it. It does **not** cross into an interactive (WebAssembly/Server) island, so read it from the statically rendered components that make up the page body. The [`BeyondCustomRazorComponentExample`](https://github.com/usepennington/pennington/tree/main/examples/BeyondCustomRazorComponentExample) `PageFacts` component shows the full pattern.

## Verify

- The `<Badge Text="Preview" />` example renders as a styled pill — a rounded, ring-bordered chip — not the literal text `<Badge Text="Preview" />`. Seeing the raw tag means the component is not registered on the host.
- View source — the badge is a `<span class="not-prose inline-flex ...">`, and no `<Badge>` literal survives in the HTML.
- On a bare `AddPennington` host, an unregistered tag passes through unchanged and shows as literal text. Add the `AddMdazorComponent<Badge>()` call and the pill appears, confirming the registration is what activates the tag.

## Related

- Reference: [Content components](xref:reference.ui.content) — parameters and render behavior for `Badge`, `BigTable`, `Card`, `CardGrid`, `Checkpoint`, `LinkCard`, `RenderedFixture` (DocSite only), `Step`, and `Steps`.
- Tutorial: [Author a custom Razor component for markdown](xref:tutorials.beyond-basics.custom-razor-component) — write your own `<PricingCard>`-style component and wire it through `AddMdazorComponent<T>()`.
- How-to: [Customize DocSite layouts and components](xref:how-to.response-pipeline.override-docsite-components) — when you need to change the surrounding page, not only embed a tag inside markdown.
