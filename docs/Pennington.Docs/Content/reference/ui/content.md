---
title: "Content components"
description: "Parameter and usage reference for the eight Pennington.UI content components — Card, CardGrid, LinkCard, Badge, Step, Steps, CodeBlock, and BigTable."
sectionLabel: "UI Components"
order: 20
tags: [ui, components, mdazor, razor]
uid: reference.ui.content
---

> **In this page.** `Card`, `CardGrid`, `LinkCard`, `Badge`, `Step`, `Steps`, `CodeBlock`, and `BigTable` — parameters, render behavior, and the component surface used from Mdazor-backed markdown content and Razor pages.
>
> **Not in this page.** Mdazor parser internals or step-by-step authoring workflow — see [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown).

## Summary

_**One sentence: what it is.** The content-oriented subset of the `Pennington.UI.Components` Razor component library — eight components covering callout cards, numbered steps, syntax-highlighted code, and wide-table overflow handling._
_**One sentence: where it lives.** Namespace `Pennington.UI.Components` (project `src/Pennington.UI/`); pre-registered with Mdazor by `DocSiteServiceExtensions.AddDocSite` so markdown pages can invoke the same tags used from Razor._

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

_Each component is listed alphabetically below with its declaration fence, parameter table, and a minimal usage example. All eight are pre-registered with Mdazor by `AddDocSite` and therefore usable as tags inside markdown content without further wiring._

## `Badge`

```csharp:xmldocid
T:Pennington.UI.Components.Badge
```

_Inline pill with ring + tinted background, variant-mapped to a MonorailCSS color palette. Renders a `<span>` (inline), so it is safe inside flowing prose and table cells._

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

_TODO: confirm whether `Text` and `ChildContent` are both supported at runtime — the current component reads only `Text`; markdown usage of `<Badge>Kitchen sink</Badge>` in `examples/DocSiteKitchenSinkExample/Content/main/ui-components-in-markdown.md` may rely on Mdazor mapping the inner text to `Text`._

## `BigTable`

```csharp:xmldocid
T:Pennington.UI.Components.BigTable
```

_Overflow wrapper for tables wider than the main column. Emits a single `<div>` with `overflow-x-scroll` and `text-sm`; the caller is expected to place a `<table>` (or markdown table) as `ChildContent`._

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

```csharp:xmldocid
T:Pennington.UI.Components.Card
```

_Static non-clickable callout card. Renders a rounded, tinted panel with an optional icon region and a bold heading; body content flows through `ChildContent` with `not-prose` applied so surrounding prose styles are suppressed inside the card._

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

```csharp:xmldocid
T:Pennington.UI.Components.CardGrid
```

_Responsive grid container for `Card` or `LinkCard` children. Renders a one-column grid on small viewports and a `Columns`-wide grid from the `sm` breakpoint up; the `Columns` value is interpolated into a MonorailCSS class (e.g. `sm:grid-cols-3`)._

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

```csharp:xmldocid
T:Pennington.UI.Components.CodeBlock
```

_Razor-level wrapper around `ICodeHighlighter`. Accepts code via either the `Code` parameter or `ChildContent`, normalizes common leading whitespace from `ChildContent`, then delegates to the injected highlighter; output is emitted as a `MarkupString`. An empty or missing `Language` renders an inline error div._

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

_For fenced-code-block authoring inside markdown prose, prefer the standard triple-backtick fence with an info string — `<CodeBlock>` is intended for Razor pages and Mdazor tag invocations where a fence is awkward._

## `LinkCard`

```csharp:xmldocid
T:Pennington.UI.Components.LinkCard
```

_Clickable variant of `Card`. The entire card body is wrapped in an `<a>` whose `href` is bound to `Href`; hover states tint the background using the `Color` palette._

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

```csharp:xmldocid
T:Pennington.UI.Components.Step
```

_One numbered item inside a `Steps` list. Renders an `<li>` with an absolute-positioned number badge pinned to the list's left rail; expects to be nested directly inside `<Steps>` so the parent's `<ol>` and rail border align._

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Step body content, rendered to the right of the numbered badge. |
| `StepNumber` | `string` | `"1"` | Label shown inside the circular badge — a string so non-numeric markers (e.g. `"A"`, `"i"`) are possible. |

### Example

```razor
<Steps>
    <Step StepNumber="1">Install the template.</Step>
    <Step StepNumber="2">Run <code>dotnet run</code>.</Step>
</Steps>
```

## `Steps`

```csharp:xmldocid
T:Pennington.UI.Components.Steps
```

_Container for a vertical numbered-step list. Emits a `<div>` wrapping an `<ol>` with a left border acting as the connecting rail; `Step` children provide the numbered items._

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | One or more `<Step>` children. |
| `Type` | `string` | `"primary"` | _TODO: parameter is declared but not currently read by the rendered markup; confirm whether it is reserved for future theming or should be removed._ |

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

```csharp:xmldocid,bodyonly
M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})
```

_TODO: confirm the exact xmldocid signature for `AddDocSite` — the overload above matches the public signature in `src/Pennington.DocSite/DocSiteServiceExtensions.cs`; adjust parameter types if the source file diverges._

For sites that do not use `AddDocSite` (for example, `AddBlogSite` or a hand-rolled `AddPennington` host), call `AddMdazorComponent<T>()` for each of the eight types to match the doc-site surface.

## See also

- How-to: [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown)
- Related reference: [Navigation components](xref:reference.ui.navigation)
- Related reference: [Utility components](xref:reference.ui.utility)
