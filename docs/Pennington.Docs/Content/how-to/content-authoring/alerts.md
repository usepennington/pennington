---
title: "Add alerts and callouts"
description: "Drop GitHub-style `[!NOTE]` / `[!TIP]` / `[!IMPORTANT]` / `[!WARNING]` / `[!CAUTION]` blocks into markdown and let Pennington paint them as coloured callouts."
uid: how-to.content-authoring.alerts
order: 70
sectionLabel: Content Authoring
tags: [authoring, alerts, markdown, callouts]
---

> **In this page.** _Paraphrase the TOC "Covers" line: authoring the five GitHub-style alert blocks (`> [!NOTE]`, `[!TIP]`, `[!CAUTION]`, `[!WARNING]`, `[!IMPORTANT]`) in a standard blockquote and understanding the CSS classes the renderer emits so your stylesheet can pick them up._
>
> **Not in this page.** _Paraphrase "Does not cover": restyling the alerts beyond the default palette, Mermaid diagrams (see [Embed diagrams](xref:how-to.content-authoring.diagrams)), or the Pennington.UI `<Card>` component surfaced inside markdown (see [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown))._

## When to use this

_Two sentences. Frame the reader's goal: they have a working Pennington page and want to flag a note, tip, or warning in the flow of prose without reaching for a Razor component or custom CSS. Point out that the five kinds are fixed by the parser — pick one that matches the signal the callout is sending rather than inventing new ones._

## Assumptions

_Three bullets. Keep each to a single realistic prior state._

- You have an existing Pennington site rendering markdown (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- Your pipeline was built through `AddPennington` / `AddDocSite` / `AddBlogSite`, so `UseCustomAlerts()` is already wired into the default `MarkdownPipelineFactory` — you do not need to register the extension yourself.
- You are using the default MonorailCSS integration or a stylesheet that targets the `markdown-alert` / `markdown-alert-{kind}` classes — the parser only emits classes, not colours.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/alerts.md` stages one blockquote per kind and is the fixture this page embeds.

---

## Steps

_Five steps. Each is one imperative action. Snippets are plain markdown fences because the feature is a pure markdown syntax — the xmldocid fence only appears when pointing at the production parser type._

### 1. Open a blockquote with an alert marker

_One sentence: start a standard `>` blockquote whose very first non-whitespace line is `[!KIND]` in uppercase. The `CustomAlertInlineParser` only fires when the marker is the first inline on the first paragraph of a quote block, so no leading text is allowed._

````markdown
> [!NOTE]
> Notes carry side information the reader should glance at before continuing.
````

### 2. Pick one of the five built-in kinds

_Two sentences: choose from `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, or `CAUTION` based on signal strength — note (neutral fact) → tip (smart default) → important (load-bearing) → warning (avoidable problem) → caution (destructive / irreversible). Any other token fails the parse and the block renders as a plain blockquote._

````markdown
> [!TIP]
> Tips point at a smart default or a pattern that keeps the common case simple.

> [!IMPORTANT]
> Important callouts flag content that is load-bearing for the rest of the page.

> [!WARNING]
> Warnings surface something that will produce an incorrect result if ignored.

> [!CAUTION]
> Cautions surface destructive operations — wire-format breaks, security footguns.
````

### 3. Write the body as normal markdown

_One sentence: every line after the marker is regular markdown — inline formatting, links, lists, and code spans all work because the rest of the blockquote is parsed through the standard Markdig pipeline._

````markdown
> [!NOTE]
> You can link to [another page](/reference/markdown/extensions), use `inline code`,
> or drop a short list:
>
> - first point
> - second point
````

### 4. Know the classes the renderer emits

_Two sentences: the parser rewrites the quote block into an `AlertBlock` and stamps it with `markdown-alert` plus `markdown-alert-{kind}` (lower-cased). The production parser below is the authoritative source if you need to reason about edge cases._

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CustomAlertInlineParser
```

### 5. Embed the reference fixture to mirror the rendered output

_One sentence: embed the kitchen-sink fixture so you can compare your authored markdown against the canonical "one of each kind" page the docs site renders._

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/alerts.md
```

---

## Verify

_Three bullets. One observable check each._

- Run `dotnet run` and visit the page — each alert renders as a coloured callout (emerald / blue / sky / rose / amber for `NOTE` / `TIP` / `IMPORTANT` / `WARNING` / `CAUTION`) with no `[!KIND]` text visible in the body.
- View source on the rendered HTML — the outer element carries `class="markdown-alert markdown-alert-note"` (or the matching kind) so your stylesheet can target it.
- Introduce a typo like `[!NOET]` and confirm the block falls back to a plain `<blockquote>` — that is the signal the parser rejected an unknown kind.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full list of non-CommonMark features including alerts and their emitted CSS classes
- How-to: [Embed diagrams](xref:how-to.content-authoring.diagrams) — Mermaid fences for when a callout isn't the right shape
- How-to: [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown) — reach for `<Card>` or a custom component when you need more than the five built-in kinds
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline) — where the Markdig pipeline (and the alert extension) sits in the render chain
