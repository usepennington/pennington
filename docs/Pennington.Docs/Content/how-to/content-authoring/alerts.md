---
title: "Add a coloured callout for a note, tip, warning, or caution"
description: "GitHub-style alerts: open a blockquote whose first line is `[!KIND]` in uppercase. Pennington recognises five kinds and paints each one differently."
uid: how-to.content-authoring.alerts
order: 201070
sectionLabel: Content Authoring
tags: [authoring, alerts, markdown, callouts]
---

To surface a note, tip, or warning inline without reaching for a Razor component, open a standard blockquote whose first line is `[!KIND]` in uppercase. The five built-in kinds — `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION` — fix the visual treatment; pick the one whose signal strength matches the message. The `CustomAlertInlineParser` fires only when the marker is the first inline of the first paragraph, so no leading text before it.

## Assumptions

- An existing Pennington site renders markdown (see <xref:tutorials.getting-started.first-site> if not).
- The pipeline was built through `AddPennington` / `AddDocSite` / `AddBlogSite`, so `UseCustomAlerts()` is already wired into the default `MarkdownPipelineFactory`.
- The default MonorailCSS integration or a stylesheet targets the `markdown-alert` / `markdown-alert-{kind}` classes.

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

The parser rewrites the blockquote into an `AlertBlock` and stamps it with two classes: `markdown-alert` (always present) and `markdown-alert-{kind}` where `{kind}` is the lower-cased token. Stylesheets target those two classes for the colour treatment. Any other token fails the parse and the block falls back to a plain `<blockquote>` with no alert styling — useful for confirming you typed `[!NOTE]` and not `[!NOET]`.

````markdown
> [!NOET]
> Unknown kind: this falls back to a plain blockquote.
````

> [!NOET]
> Unknown kind: this falls back to a plain blockquote.

## Verify

- Each alert renders as a coloured callout with no `[!KIND]` text in the body.
- View source — the outer element carries `class="markdown-alert markdown-alert-note"` (or the matching kind).
- A typo like `[!NOET]` falls back to a plain `<blockquote>`, signalling that the parser rejected an unknown kind.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full list of non-CommonMark features including alerts and their emitted CSS classes
- How-to: <xref:how-to.content-authoring.diagrams> — Mermaid fences for when a callout is not the right shape
- How-to: <xref:how-to.content-authoring.ui-components-in-markdown> — `<Card>` or a custom component covers cases beyond the five built-in kinds
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline) — where the Markdig pipeline (and the alert extension) sits in the render chain
