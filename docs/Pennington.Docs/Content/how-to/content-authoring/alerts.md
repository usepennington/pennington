---
title: "Add alerts and callouts"
description: "Drop GitHub-style `[!NOTE]` / `[!TIP]` / `[!IMPORTANT]` / `[!WARNING]` / `[!CAUTION]` blocks into markdown and let Pennington paint them as coloured callouts."
uid: how-to.content-authoring.alerts
order: 201070
sectionLabel: Content Authoring
tags: [authoring, alerts, markdown, callouts]
---

To surface a note, tip, or warning inside flowing prose without reaching for a Razor component or custom CSS, Pennington's alert extension is the right tool. The parser fixes the five kinds — `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, and `CAUTION` — so pick the one whose signal strength matches the message rather than inventing new ones.

## Assumptions

- An existing Pennington site renders markdown (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- The pipeline was built through `AddPennington` / `AddDocSite` / `AddBlogSite`, so `UseCustomAlerts()` is already wired into the default `MarkdownPipelineFactory` — the extension does not need separate registration.
- The default MonorailCSS integration or a stylesheet targets the `markdown-alert` / `markdown-alert-{kind}` classes — the parser emits classes, not colours.

For a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/alerts.md` stages one blockquote per kind and is the fixture this page embeds.

---

## Steps

### 1. Open a blockquote with an alert marker

Start a standard `>` blockquote whose first line is `[!KIND]` in uppercase. The `CustomAlertInlineParser` fires only when the marker is the first inline on the first paragraph of the quote block, so no leading text before it.

````markdown
> [!NOTE]
> Notes carry side information worth a glance before continuing.
````

### 2. Pick one of the five built-in kinds

Choose from `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, or `CAUTION` based on signal strength: note (neutral fact) → tip (smart default) → important (load-bearing) → warning (avoidable problem) → caution (destructive or irreversible). Any other token fails the parse and the block renders as a plain `<blockquote>` with no alert styling.

````markdown
> [!TIP]
> Tips point at a smart default or a pattern that keeps the common case simple.

> [!IMPORTANT]
> Important callouts flag content that is load-bearing for the rest of the page.

> [!WARNING]
> Warnings flag output that is likely incorrect if the advice is ignored.

> [!CAUTION]
> Cautions surface destructive operations — wire-format breaks, security footguns.
````

### 3. Write the body as normal markdown

Every line after the marker is regular markdown — inline formatting, links, lists, and code spans all work because the rest of the blockquote passes through the standard Markdig pipeline unchanged.

````markdown
> [!NOTE]
> Link to [another page](/reference/markdown/extensions), use `inline code`,
> or drop a short list:
>
> - first point
> - second point
````

### 4. Know the classes the renderer emits

The parser rewrites the quote block into an `AlertBlock` and stamps it with two classes: `markdown-alert` (always present) and `markdown-alert-{kind}` where `{kind}` is the lower-cased token. The production parser type below is the place to look for edge cases or extension points.

```csharp:xmldocid
T:Pennington.Markdown.Extensions.CustomAlertInlineParser
```

### 5. Embed the reference fixture to mirror the rendered output

The kitchen-sink fixture below is the canonical page that exercises one of each alert kind — compare authored markdown against it.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/alerts.md
```

---

## Verify

- Run `dotnet run` and visit the page — each alert renders as a coloured callout (emerald / blue / sky / rose / amber for `NOTE` / `TIP` / `IMPORTANT` / `WARNING` / `CAUTION`) with no `[!KIND]` text in the body.
- View source on the rendered HTML — the outer element carries `class="markdown-alert markdown-alert-note"` (or the matching kind), which the stylesheet can target.
- Introduce a typo like `[!NOET]` — the block falls back to a plain `<blockquote>`, signalling that the parser rejected an unknown kind.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full list of non-CommonMark features including alerts and their emitted CSS classes
- How-to: [Embed diagrams](xref:how-to.content-authoring.diagrams) — Mermaid fences for when a callout isn't the right shape
- How-to: [Use UI components inside markdown](xref:how-to.content-authoring.ui-components-in-markdown) — `<Card>` or a custom component covers cases beyond the five built-in kinds
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline) — where the Markdig pipeline (and the alert extension) sits in the render chain
