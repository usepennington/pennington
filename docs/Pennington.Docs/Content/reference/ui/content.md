---
title: "Content components"
description: "Card, CardGrid, LinkCard, Badge, Step, Steps, CodeBlock, and BigTable — parameters, render behavior, and the Pennington-facing component surface used with Mdazor-backed markdown content."
section: "ui"
order: 20
tags: []
uid: reference.ui.content
isDraft: true
search: false
llms: false
---

> **In this page.** `Card`, `CardGrid`, `LinkCard`, `Badge`, `Step`, `Steps`, `CodeBlock`, and `BigTable` — parameters, render behavior, and the Pennington-facing component surface used with Mdazor-backed markdown content.
>
> **Not in this page.** The Mdazor parser model, registration mechanics, or step-by-step authoring workflow (see the authoring how-to and the Mdazor project).

## Summary

- Eight Razor components in `Pennington.UI` for structured content inside a Razor layout or page.
- Namespace `Pennington.UI.Components`; source files under `src/Pennington.UI/Components/`.
- In Pennington docs, these are the component-level building blocks you expose through the Mdazor-based markdown-component flow. This page catalogs the component surface; Mdazor documents the deeper tag syntax and parser behavior.

## `Card`

### Declaration

- Component file: `src/Pennington.UI/Components/Card.razor`.
- Renders a titled rounded card with an optional leading icon slot, colored against the site's MonorailCSS palette. Wrapped in `not-prose` so it escapes prose styling when embedded in article markup.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Card heading; rendered inside an `<h2 class="font-display font-bold">`. |
| `Color` | `string` | `"primary"` | MonorailCSS color-family token; drives `text-`, `bg-`, and `border-` variants. |
| `Icon` | `RenderFragment?` | `null` | Optional icon slot rendered to the left of the title. |
| `ChildContent` | `RenderFragment?` | `null` | Card body; rendered at `text-sm`. |

## `CardGrid`

### Declaration

- Component file: `src/Pennington.UI/Components/CardGrid.razor`.
- Renders a responsive grid (`grid-cols-1 sm:grid-cols-{Columns}`) wrapped in `not-prose`, with `gap-6` between children.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Columns` | `string` | `"2"` | Column count at the `sm` breakpoint and up. Interpolated into `sm:grid-cols-@Columns`. |
| `ChildContent` | `RenderFragment?` | `null` | Grid children; typically a set of `<Card>` or `<LinkCard>` elements. |

## `LinkCard`

### Declaration

- Component file: `src/Pennington.UI/Components/LinkCard.razor`.
- Renders an `<a>`-wrapped `Card`; the whole card is clickable, with a hover-color shift against the configured color family.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Card heading; rendered inside an `<h2 class="font-display font-bold">`. |
| `Href` | `string?` | `null` | `<a href>` target. `null` produces an anchor with no `href`. |
| `Color` | `string` | `"primary"` | MonorailCSS color-family token; drives default + hover variants. |
| `Icon` | `RenderFragment?` | `null` | Optional icon slot rendered to the left of the title. |
| `ChildContent` | `RenderFragment?` | `null` | Card body; rendered at `text-sm`. |

## `Badge`

### Declaration

- Component file: `src/Pennington.UI/Components/Badge.razor`.
- Renders a small inline pill (`<span>`) suitable for status chips adjacent to text.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | The literal text shown inside the pill. |
| `Variant` | `string` | `"note"` | Color variant. Accepted values: `success` (emerald), `tip` (sky), `caution` (amber), `danger` (rose), `note` (base). Any other value falls through to `note`. |
| `Size` | `string` | `"medium"` | Size token. Accepted values: `small` (`text-xs`), `medium` (`text-sm`), `large` (`text-base`). Any other value falls through to `medium`. |

## `Step`

### Declaration

- Component file: `src/Pennington.UI/Components/Step.razor`.
- Renders one `<li>` inside a parent `<Steps>` list; shows a circled step indicator as a pseudo-element plus the authored body.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `StepNumber` | `string` | `"1"` | Text rendered inside the circular step indicator. Typically `"1"`, `"2"`, … but any string is accepted. |
| `ChildContent` | `RenderFragment?` | `null` | Step body content. |

## `Steps`

### Declaration

- Component file: `src/Pennington.UI/Components/Steps.razor`.
- Renders an `<ol>` with a left guide rail (`border-l-2`) wrapping a sequence of `<Step>` children.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Type` | `string` | `"primary"` | Declared but not referenced in the component markup today — retained for future variant hooks. |
| `ChildContent` | `RenderFragment?` | `null` | One or more `<Step>` children. |

## `CodeBlock`

### Declaration

- Component file: `src/Pennington.UI/Components/CodeBlock.razor`.
- Injects `ICodeHighlighter` and emits highlighted `<pre><code>` HTML for the given language. Accepts code via either the `Code` parameter or `ChildContent`.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Language` | `string` | `""` | **`[EditorRequired]`.** Language identifier passed to `ICodeHighlighter.Highlight` (e.g., `"csharp"`, `"python"`). Missing / whitespace emits an inline error message. |
| `Code` | `string?` | `null` | Code to highlight. Provide either this or `ChildContent`; if both are `null`/empty an inline error is emitted. |
| `ChildContent` | `RenderFragment?` | `null` | Alternative to `Code`; text frames inside the fragment are concatenated and indentation is normalized before highlighting. |
| `IsInTabGroup` | `bool` | `false` | When `true`, omits the stand-alone container classes so the block can nest inside a tabbed code group. |

## `BigTable`

### Declaration

- Component file: `src/Pennington.UI/Components/BigTable.razor`.
- Wraps `ChildContent` in `<div class="overflow-x-scroll text-sm">`; intended for wide tables that would otherwise overflow the article column.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Table markup (typically a `<table>` element or a markdown table compiled by Markdig). |

## See also

- Related reference: [Navigation components](/reference/ui/navigation) — `TableOfContentsNavigation`, `OutlineNavigation`.
- Related reference: [Utility components](/reference/ui/utility) — `LanguageSwitcher`, `StructuredData`, `FallbackNotice`.
- How-to: [Use UI components inside markdown](/how-to/content-authoring/ui-components-in-markdown).
- External: [Mdazor](https://github.com/phil-scott-78/Mdazor) — underlying component-tag syntax, registration model, nested markdown behavior, and limitations.
