---
title: "Content components"
description: "Parameter and usage reference for the nine Pennington.UI content components — Card, CardGrid, LinkCard, Badge, Step, Steps, Checkpoint, BigTable (Mdazor-registered), and CodeBlock (Razor-page only)."
sectionLabel: "UI Components"
order: 2
tags: [ui, components, mdazor, razor]
uid: reference.ui.content
---

The content-oriented subset of the `Pennington.UI.Components` Razor component library, covering callout cards, numbered steps, syntax-highlighted code, and wide-table overflow handling. Components live in namespace `Pennington.UI.Components` (`src/Pennington.UI/`). All but `CodeBlock` are pre-registered with Mdazor by `DocSiteServiceExtensions.AddDocSite`, making them available as tags inside markdown without additional wiring; `CodeBlock` is Razor-page-only — markdown authors use fenced code blocks instead.

## Stylesheet

The components ship as MonorailCSS utility classes; the package contributes no separate stylesheet. There is no `_content/Pennington.UI/styles.css` to load.

## Overview

| Component | Purpose | Razor usage | Markdown (Mdazor) usage |
|---|---|---|---|
| `Badge` | Inline pill rendering a short label in one of five variants. | `<Badge Text="New" Variant="tip" />` | `<Badge Text="New" Variant="tip" />` |
| `BigTable` | Wraps a wide table in a horizontal-scroll container. | `<BigTable>@ChildContent</BigTable>` | `<BigTable>` ... markdown table ... `</BigTable>` |
| `Card` | Static callout card with optional icon and title. | `<Card Title="..." Color="primary">@ChildContent</Card>` | `<Card Title="..." Color="primary">` ... `</Card>` |
| `CardGrid` | Responsive grid container for Card / LinkCard children. | `<CardGrid Columns="3">@ChildContent</CardGrid>` | `<CardGrid Columns="3">` ... `</CardGrid>` |
| `Checkpoint` | "Verify what you should see now" callout for tutorial pages. | `<Checkpoint>@ChildContent</Checkpoint>` | `<Checkpoint>` ... `</Checkpoint>` |
| `LinkCard` | Clickable card wrapping its content in an anchor. | `<LinkCard Title="..." Href="/foo">@ChildContent</LinkCard>` | `<LinkCard Title="..." Href="/foo">` ... `</LinkCard>` |
| `Step` | Single numbered list item inside a `Steps` container. | `<Step StepNumber="1">@ChildContent</Step>` | `<Step StepNumber="1">` ... `</Step>` |
| `Steps` | Vertical numbered-step list container for `Step` children. | `<Steps>@ChildContent</Steps>` | `<Steps>` ... `</Steps>` |

Each component is listed alphabetically below with its declaration fence, parameter table, and a minimal usage example.

## `Badge`

Inline pill with ring and tinted background, variant-mapped to a MonorailCSS color palette; renders a `<span>` and is safe inside flowing prose and table cells.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Size` | `string` | `"medium"` | One of `"small"`, `"medium"`, `"large"`; drives padding and text size. |
| `Text` | `string` | `""` | Label rendered inside the badge. |
| `Variant` | `string` | `"note"` | One of `"note"`, `"success"`, `"tip"`, `"caution"`, `"danger"`; selects the color palette (base / emerald / sky / amber / rose). |

### Example

```razor
<Badge Text="New" Variant="tip" Size="small" />
```

## `BigTable`

Overflow wrapper for tables wider than the main column; emits a `<div>` with horizontal scroll and reduced text size, with the table supplied as `ChildContent`.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | The table content (typically a `<table>` element or a GFM table when used from Mdazor). |

### Example

```razor
<BigTable>
    <table>...</table>
</BigTable>
```

## `Card`

Static non-clickable callout card; renders a rounded, tinted panel with an optional icon region and bold heading, with `not-prose` applied inside so surrounding prose styles do not affect card body content.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Body content rendered beneath the title. |
| `Color` | `string` | `"primary"` | MonorailCSS color-family name used to tint borders, background, text, and icon fill. |
| `Icon` | `RenderFragment?` | `null` | Optional leading icon fragment rendered to the left of the title + body stack. |
| `Title` | `string?` | `null` | Bold heading rendered above `ChildContent`. |

### Example

```razor
<Card Title="Fast" Color="emerald">
    Pages render in a single SSR pass through the content pipeline.
</Card>
```

## `CardGrid`

Responsive grid container for `Card` or `LinkCard` children; renders one column on small viewports and a `Columns`-wide grid from the `sm` breakpoint up, with `Columns` interpolated into a MonorailCSS class.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Grid items — typically `<Card>` or `<LinkCard>` children. |
| `Columns` | `string` | `"2"` | Number of columns at the `sm` breakpoint and above; passed through as a MonorailCSS class fragment. |

### Example

```razor
<CardGrid Columns="3">
    <LinkCard Title="Getting started" Href="/tutorials" />
    <LinkCard Title="How-to guides" Href="/how-to" />
    <LinkCard Title="Reference" Href="/reference" />
</CardGrid>
```

## `Checkpoint`

Standalone "verify what you should see now" callout for tutorial pages; emits `<div class="markdown-alert markdown-alert-checkpoint not-prose">` with a literal **Checkpoint** label paragraph and the body content beneath it, sharing chrome with GitHub-style alerts. Renders as a `<div>`, not a heading, so the right-side outline nav skips it.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Body of the checkpoint callout — usually one or two paragraphs and a verification list. |

### Example

```razor
<Checkpoint>

Run `dotnet run` and visit `http://localhost:5000/`.

- The page renders with the expected H1.
- The right-side outline lists only the real section headings.

</Checkpoint>
```

## `CodeBlock`

Razor-page entry to the shared code-block rendering pipeline — registered `ICodeBlockPreprocessor` implementations (including tree-sitter `:symbol` fences when `AddPenningtonTreeSitter` is wired), highlighter dispatch via `HighlightingService`, `[!code …]` line transformations, and the standard `code-highlight-wrapper` container. Not registered with Mdazor — markdown authors should use a fenced code block (same pipeline, same output shape) instead.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Code content as the component's child text; de-indented before rendering. |
| `Code` | `string?` | `null` | Code content as a string attribute; takes precedence over `ChildContent` when both are set. |
| `IsInTabGroup` | `bool` | `false` | When `true`, omits standalone container classes so the block composes inside a tabbed code group. |
| `Language` | `string` | `""` | Required (`EditorRequired`) [fence info-string](xref:reference.markdown.code-block-args) — a bare language like `"csharp"` or a modifier-bearing form like `"csharp:symbol,bodyonly"` (the `:symbol` family ships with `Pennington.TreeSitter`). |

### Example

```razor
<CodeBlock Language="csharp">
var x = 1;
</CodeBlock>

<CodeBlock Language="csharp:symbol,bodyonly">examples/Foo/Program.cs &gt; Program.Main</CodeBlock>
```

## `LinkCard`

Clickable variant of `Card`; wraps the entire card body in an `<a>` bound to `Href`, with hover states tinting the background using the `Color` palette.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Body content rendered beneath the title. |
| `Color` | `string` | `"primary"` | MonorailCSS color-family name used for borders, background, hover state, text, and icon fill. |
| `Href` | `string?` | `null` | Destination URL; passed through directly to the wrapping anchor. |
| `Icon` | `RenderFragment?` | `null` | Optional leading icon fragment. |
| `Title` | `string?` | `null` | Bold heading rendered above `ChildContent`. |

### Example

```razor
<LinkCard Title="Getting started" Href="/tutorials/getting-started" Color="primary">
    A zero-to-running-site walkthrough.
</LinkCard>
```

## `Step`

One step inside a `Steps` list; renders a `<section class="step">` with a numbered medallion on the left rail, an optional title, the body content, and an optional `Checkpoint` slot for a "verify the result" callout. Must be nested directly inside `<Steps>` for the rail border to align.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Step body content, rendered to the right of the numbered medallion. |
| `Checkpoint` | `RenderFragment?` | `null` | Optional inline checkpoint rendered after the body using `markdown-alert-checkpoint` chrome — handy for tutorials that verify a result at the end of a step. |
| `StepNumber` | `string` | `"1"` | Label shown inside the circular medallion — a string so non-numeric markers (for example, `"A"`, `"i"`) are possible. |
| `Title` | `string?` | `null` | Optional title rendered above the body content. |

### Example

```razor
<Steps>
    <Step StepNumber="1" Title="Install the template">Run the dotnet new install command.</Step>
    <Step StepNumber="2">Run <code>dotnet run</code>.</Step>
</Steps>
```

## `Steps`

Container for a vertical step list; emits `<div class="steps-thread not-prose">` rendering a vertical thread on the left, populated by `Step` children which punch a numbered medallion onto the thread.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | One or more `<Step>` children. |

### Example

```razor
<Steps>
    <Step StepNumber="1">First, install.</Step>
    <Step StepNumber="2">Then, run.</Step>
    <Step StepNumber="3">Finally, deploy.</Step>
</Steps>
```

## `RenderedFixture` (DocSite only)

Embeds a fixture file (markdown or HTML) from anywhere in the solution as a captioned `<figure>`, rendering markdown through the standard `MarkdownPipeline`. Useful when a how-to page wants to show the actual rendered output of a complete example file (a full alert syntax, a composed configuration) rather than authoring the same content twice. Registered with Mdazor by `AddDocSite` but not by `AddBlogSite`.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Path` | `string` | `""` (required) | Solution-relative path to the fixture file (for example, `examples/Foo/Content/bar.md`). Rejected when it contains `..` or is rooted. |
| `Caption` | `string?` | `null` | Caption shown in the `<figcaption>` above the rendered output. Defaults to `"Rendered output"`. |

### Example

```razor
<RenderedFixture Path="examples/DocSitePagesAndLinksExample/snippets/markdown-alert-example.md"
                 Caption="Built-in alert syntax" />
```

## Mdazor registration

`AddDocSite` pre-registers nine components with Mdazor (eight content components plus `RenderedFixture`); `AddBlogSite` registers eight (everything but `RenderedFixture`). Sites built on either template can invoke these tags directly inside markdown without calling `AddMdazorComponent<T>()` manually.

```csharp
// AddDocSite
services.AddMdazorComponent<Badge>()
        .AddMdazorComponent<BigTable>()
        .AddMdazorComponent<Card>()
        .AddMdazorComponent<CardGrid>()
        .AddMdazorComponent<Checkpoint>()
        .AddMdazorComponent<LinkCard>()
        .AddMdazorComponent<RenderedFixture>() // AddDocSite only
        .AddMdazorComponent<Step>()
        .AddMdazorComponent<Steps>();
```

Hosts without `AddDocSite` register the same surface through one `AddMdazorComponent<T>()` call per component. See <xref:how-to.rich-content.ui-components-in-markdown> for the host-by-host recipe.

## See also

- How-to: [Use UI components inside markdown](xref:how-to.rich-content.ui-components-in-markdown)
- Related reference: [Navigation components](xref:reference.ui.navigation)
- Related reference: [Utility components](xref:reference.ui.utility)
