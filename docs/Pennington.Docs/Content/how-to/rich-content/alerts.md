---
title: "Add a colored callout for a note, tip, warning, or caution"
description: "GitHub-style alerts: open a blockquote whose first line is `[!KIND]` in uppercase. Pennington recognizes five kinds and paints each one differently."
uid: how-to.rich-content.alerts
order: 1
sectionLabel: "Rich Content"
tags: [authoring, alerts, markdown, callouts]
---

To surface a note, tip, or warning inline without reaching for a Razor component, open a standard blockquote whose first line is `[!KIND]` in uppercase. The five built-in kinds — `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION` — fix the visual treatment; pick the one whose signal strength matches the message. The marker must be the first inline of the first paragraph — no leading text.

## Before you begin
- An existing Pennington site renders markdown (see <xref:tutorials.getting-started.first-site> if not).
- The pipeline was built through `AddPennington` / `AddDocSite` / `AddBlogSite`, so `UseCustomAlerts()` is already wired into the default `MarkdownPipelineFactory`.
- The default [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) integration or a stylesheet targets the `markdown-alert` / `markdown-alert-{kind}` classes.

## The five alert kinds

Each kind below shows the source markdown above the rendered output. Every line after the marker is regular markdown — inline formatting, links, lists, and code spans all work because the rest of the blockquote passes through the standard Markdig pipeline unchanged.

### Note

````markdown
> [!NOTE]
> Notes carry side information worth a glance before continuing.
````

> [!NOTE]
> Notes carry side information worth a glance before continuing.

### Tip

````markdown
> [!TIP]
> Tips point at a smart default or a pattern that keeps the common case simple.
````

> [!TIP]
> Tips point at a smart default or a pattern that keeps the common case simple.

### Important

````markdown
> [!IMPORTANT]
> Important callouts flag content that is load-bearing for the rest of the page.
````

> [!IMPORTANT]
> Important callouts flag content that is load-bearing for the rest of the page.

### Warning

````markdown
> [!WARNING]
> Warnings flag output that is likely incorrect if the advice is ignored.
````

> [!WARNING]
> Warnings flag output that is likely incorrect if the advice is ignored.

### Caution

````markdown
> [!CAUTION]
> Cautions surface destructive operations — wire-format breaks, security footguns.
````

> [!CAUTION]
> Cautions surface destructive operations — wire-format breaks, security footguns.

## What the renderer emits

Each alert wraps in three CSS classes: `markdown-alert` (always present), `markdown-alert-{kind}` where `{kind}` is the lower-cased token, and `not-prose` (which isolates the alert from the surrounding page-prose typography rules). Stylesheets target the first two classes for the color treatment. An unrecognized token falls back to a plain `<blockquote>` with no alert styling, so the marker stays visible instead of turning into a misleading callout.

````markdown
> [!INFO]
> Unknown kind: this falls back to a plain blockquote.
````

> [!INFO]
> Unknown kind: this falls back to a plain blockquote.

See <xref:reference.markdown.extensions> for the full kind-to-class table.

## Verify

- Each alert renders as a colored callout with no `[!KIND]` text in the body.
- View source — the outer element carries `class="markdown-alert markdown-alert-note"` (or the matching kind).
- An unrecognized kind like `[!INFO]` falls back to a plain `<blockquote>`, signaling that the parser rejected the marker.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full list of non-CommonMark features including alerts and their emitted CSS classes
- How-to: <xref:how-to.rich-content.diagrams> — Mermaid fences for when a callout is not the right shape
- How-to: <xref:how-to.rich-content.ui-components-in-markdown> — `<Card>` or a custom component covers cases beyond the five built-in kinds
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline) — where the Markdig pipeline (and the alert extension) sits in the render chain
