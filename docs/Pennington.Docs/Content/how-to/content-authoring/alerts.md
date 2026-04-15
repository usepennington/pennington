---
title: "Add alerts and callouts"
description: "Drop GitHub-style `[!NOTE]` / `[!TIP]` / `[!IMPORTANT]` / `[!WARNING]` / `[!CAUTION]` blocks into markdown and let Pennington paint them as coloured callouts."
uid: how-to.content-authoring.alerts
order: 201070
sectionLabel: Content Authoring
tags: [authoring, alerts, markdown, callouts]
---

When you need to surface a note, tip, or warning inside flowing prose without reaching for a Razor component or custom CSS, Pennington's alert extension is the right tool. The five kinds — `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, and `CAUTION` — are fixed by the parser, so pick the one whose signal strength matches what you are communicating rather than trying to invent new ones.

## Assumptions

- You have an existing Pennington site rendering markdown (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- Your pipeline was built through `AddPennington` / `AddDocSite` / `AddBlogSite`, so `UseCustomAlerts()` is already wired into the default `MarkdownPipelineFactory` — you do not need to register the extension yourself.
- You are using the default MonorailCSS integration or a stylesheet that targets the `markdown-alert` / `markdown-alert-{kind}` classes — the parser only emits classes, not colours.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/alerts.md` stages one blockquote per kind and is the fixture this page embeds.

---

## Steps

### 1. Open a blockquote with an alert marker

Start a standard `>` blockquote whose very first line is `[!KIND]` in uppercase — the `CustomAlertInlineParser` only fires when the marker is the first inline on the first paragraph of the quote block, so no leading text before it is allowed.

````markdown
> [!NOTE]
> Notes carry side information the reader should glance at before continuing.
````

### 2. Pick one of the five built-in kinds

Choose from `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, or `CAUTION` based on signal strength: note (neutral fact) → tip (smart default) → important (load-bearing) → warning (avoidable problem) → caution (destructive or irreversible). Any other token fails the parse and the block renders as a plain `<blockquote>` with no alert styling.

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

Every line after the marker is regular markdown — inline formatting, links, lists, and code spans all work because the rest of the blockquote passes through the standard Markdig pipeline unchanged.

````markdown
> [!NOTE]
> You can link to [another page](/reference/markdown/extensions), use `inline code`,
> or drop a short list:
>
> - first point
> - second point
````

### 4. Know the classes the renderer emits

The parser rewrites the quote block into an `AlertBlock` and stamps it with two classes: `markdown-alert` (always present) and `markdown-alert-{kind}` where `{kind}` is the lower-cased token. Refer to the production parser type below if you need to reason about edge cases or extend the behavior.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CustomAlertInlineParser
```

### 5. Embed the reference fixture to mirror the rendered output

Embed the kitchen-sink fixture below to compare your authored markdown against the canonical page that exercises one of each alert kind.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/alerts.md
```

---

## Verify

- Run `dotnet run` and visit the page — each alert renders as a coloured callout (emerald / blue / sky / rose / amber for `NOTE` / `TIP` / `IMPORTANT` / `WARNING` / `CAUTION`) with no `[!KIND]` text visible in the body.
- View source on the rendered HTML — the outer element carries `class="markdown-alert markdown-alert-note"` (or the matching kind) so your stylesheet can target it.
- Introduce a typo like `[!NOET]` and confirm the block falls back to a plain `<blockquote>` — that is the signal the parser rejected an unknown kind.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full list of non-CommonMark features including alerts and their emitted CSS classes
- How-to: [Embed diagrams](xref:how-to.content-authoring.diagrams) — Mermaid fences for when a callout isn't the right shape
- How-to: [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown) — reach for `<Card>` or a custom component when you need more than the five built-in kinds
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline) — where the Markdig pipeline (and the alert extension) sits in the render chain
