---
title: "Content components"
description: "Parameter and usage reference for the eight Pennington.UI content components — Card, CardGrid, LinkCard, Badge, Step, Steps, CodeBlock, and BigTable."
sectionLabel: "UI Components"
order: 404020
tags: [ui, components, mdazor, razor]
uid: reference.ui.content
---

The content-oriented subset of the `Pennington.UI.Components` Razor component library, covering callout cards, numbered steps, syntax-highlighted code, and wide-table overflow handling. Components live in namespace `Pennington.UI.Components` (`src/Pennington.UI/`) and are pre-registered with Mdazor by `DocSiteServiceExtensions.AddDocSite`, making them available as tags inside markdown without additional wiring.

## Overview

| Component | Purpose | Razor usage | Markdown (Mdazor) usage |
|---|---|---|---|
| `Badge` | Inline pill rendering a short label in one of five variants. | `<Badge Text="New" Variant="tip" />` | `<Badge Text="New" Variant="tip" />` |
| `BigTable` | Wraps a wide table in a horizontal-scroll container. | `<BigTable>@ChildContent</BigTable>` | `<BigTable>` ... markdown table ... `</BigTable>` |
| `Card` | Static callout card with optional icon and title. | `<Card Title="..." Color="primary">@ChildContent</Card>` | `<Card Title="..." Color="primary">` ... `</Card>` |
| `CardGrid` | Responsive grid container for Card / LinkCard children. | `<CardGrid Columns="3">@ChildContent</CardGrid>` | `<CardGrid Columns="3">` ... `</CardGrid>` |
| `CodeBlock` | Renders syntax-highlighted code through `ICodeHighlighter`. | `<CodeBlock Language="csharp" Code="var x = 1;" />` | `<CodeBlock Language="csharp">var x = 1;</CodeBlock>` |
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

## `CodeBlock`

Razor-level wrapper around `ICodeHighlighter`; accepts code via `Code` or `ChildContent`, normalizes leading whitespace from `ChildContent`, delegates to the injected highlighter, and emits output as a `MarkupString`. An empty or missing `Language` renders an inline error element.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Alternative code source; extracted as text and de-indented before highlighting. |
| `Code` | `string?` | `null` | Literal code string to highlight; takes precedence over `ChildContent` when both are set. |
| `IsInTabGroup` | `bool` | `false` | When `true`, the component omits standalone container classes so the block composes inside a tabbed code group. |
| `Language` | `string` | `""` | Required (`EditorRequired`) highlighter language id such as `"csharp"`, `"javascript"`, `"shell"`. |

### Example

```razor
<CodeBlock Language="csharp" Code="var x = 1;" />
```

`<CodeBlock>` is intended for Razor pages and Mdazor tag invocations; for fenced-code-block authoring inside markdown prose, use the standard triple-backtick fence with an info string instead.

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

One numbered item inside a `Steps` list; renders an `<li>` with an absolute-positioned number badge pinned to the left rail and must be nested directly inside `<Steps>` for the rail border to align.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Step body content, rendered to the right of the numbered badge. |
| `StepNumber` | `string` | `"1"` | Label shown inside the circular badge — a string so non-numeric markers (for example, `"A"`, `"i"`) are possible. |

### Example

```razor
<Steps>
    <Step StepNumber="1">Install the template.</Step>
    <Step StepNumber="2">Run <code>dotnet run</code>.</Step>
</Steps>
```

## `Steps`

Container for a vertical numbered-step list; emits a `<div>` wrapping an `<ol>` with a left border as the connecting rail, populated by `Step` children.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | One or more `<Step>` children. |
| `Type` | `string` | `"primary"` | Declared parameter reserved for future theming; not currently applied to rendered markup. |

### Example

```razor
<Steps>
    <Step StepNumber="1">First, install.</Step>
    <Step StepNumber="2">Then, run.</Step>
    <Step StepNumber="3">Finally, deploy.</Step>
</Steps>
```

## Mdazor registration

All eight components are pre-registered with Mdazor by `DocSiteServiceExtensions.AddDocSite`, which means any site built on `AddDocSite` can invoke these tags directly inside markdown without calling `AddMdazorComponent<T>()` manually:

```csharp
services.AddMdazorComponent<Badge>()
        .AddMdazorComponent<BigTable>()
        .AddMdazorComponent<Card>()
        .AddMdazorComponent<CardGrid>()
        .AddMdazorComponent<CodeBlock>()
        .AddMdazorComponent<LinkCard>()
        .AddMdazorComponent<Step>()
        .AddMdazorComponent<Steps>();
```

For sites that do not use `AddDocSite` (for example, `AddBlogSite` or a hand-rolled `AddPennington` host), call `AddMdazorComponent<T>()` for each of the eight types to match the doc-site surface.

## See also

- How-to: [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown)
- Related reference: [Navigation components](xref:reference.ui.navigation)
- Related reference: [Utility components](xref:reference.ui.utility)
