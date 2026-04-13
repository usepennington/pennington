---
title: "Alert blocks"
description: "The five alert kinds (NOTE, TIP, CAUTION, WARNING, IMPORTANT), their emitted CSS classes, and default icons."
section: markdown
order: 30
tags: []
uid: reference.markdown.alerts
isDraft: true
search: false
llms: false
---

> **In this page.** The five alert kinds (`NOTE`, `TIP`, `CAUTION`, `WARNING`, `IMPORTANT`), their emitted CSS classes, and default icons.
>
> **Not in this page.** Defining new alert kinds (would require a Markdig extension).

## Summary

- Sentence: an alert block is a `>`-blockquote whose first inline is `[!KIND]`, parsed by `CustomAlertInlineParser` and rendered by Markdig's default `AlertBlockRenderer`.
- Sentence: only the five kinds below carry built-in MonorailCSS styling; the parser accepts any alpha kind but unstyled kinds render as a bare `.markdown-alert` wrapper with no color theme.

## Declaration

- Wrapper element (verbatim shape): `<div class="markdown-alert markdown-alert-{kind}">`.
- Title element (verbatim shape): `<p class="markdown-alert-title"><svg …></svg>{Kind}</p>`.
- Kind token in the class and title text is lowercased (class) / title-cased (label) by Markdig; source kind in markdown is conventionally uppercase.
- Parser: `src/Pennington/Markdown/Extensions/CustomAlertInlineParser.cs`.
- Renderer: `Markdig.Extensions.Alerts.AlertBlockRenderer` with `RenderKind = AlertBlockRenderer.DefaultRenderKind`, registered by `MarkdownPipelineFactory.UseCustomAlerts`.
- Style table: `src/Pennington.MonorailCss/MonorailCssOptions.cs`, lines 477-485.

## Kinds

| Kind | Syntax | Emitted CSS class | Default icon | Description |
|---|---|---|---|---|
| `NOTE` | `> [!NOTE]` on the first line of a blockquote | `markdown-alert markdown-alert-note` | GitHub `info` octicon (filled circle with `i`) emitted by Markdig `AlertBlockRenderer.DefaultRenderKind` | Emerald color theme via MonorailCSS (`fill-emerald-700 … bg-emerald-100/75 …`). |
| `TIP` | `> [!TIP]` on the first line of a blockquote | `markdown-alert markdown-alert-tip` | GitHub `light-bulb` octicon emitted by Markdig `AlertBlockRenderer.DefaultRenderKind` | Blue color theme (`fill-blue-700 … bg-blue-100/75 …`). |
| `CAUTION` | `> [!CAUTION]` on the first line of a blockquote | `markdown-alert markdown-alert-caution` | GitHub `stop` octicon emitted by Markdig `AlertBlockRenderer.DefaultRenderKind` | Amber color theme (`fill-amber-700 … bg-amber-100/75 …`). |
| `WARNING` | `> [!WARNING]` on the first line of a blockquote | `markdown-alert markdown-alert-warning` | GitHub `alert` octicon (triangle with `!`) emitted by Markdig `AlertBlockRenderer.DefaultRenderKind` | Rose color theme (`fill-rose-700 … bg-rose-100/75 …`). |
| `IMPORTANT` | `> [!IMPORTANT]` on the first line of a blockquote | `markdown-alert markdown-alert-important` | GitHub `report` octicon emitted by Markdig `AlertBlockRenderer.DefaultRenderKind` | Sky color theme (`fill-sky-700 … bg-sky-100/75 …`). |

## Shared styles

- Entry: `.markdown-alert` -> `my-6 px-4 flex flex-row gap-2.5 rounded-2xl border text-sm items-center`.
- Entry: `.markdown-alert a` -> `underline`.
- Entry: `.markdown-alert-title` -> `text-[0px]` (hides the kind label text but preserves the icon).
- Entry: `.markdown-alert svg` -> `h-4 w-4 mt-0.5`.

## Parser behavior

- Entry: marker must be the first inline of the first paragraph inside a `>` blockquote; anything before it causes the parser to bail and the blockquote renders as a normal quote.
- Entry: kind token is matched by `StringSlice.IsAlpha` — letters only, no digits, no hyphens; any alpha string is accepted by the parser, but only the five kinds above have styling.
- Entry: the blockquote must not already be an `AlertBlock` (re-entry guard in `CustomAlertInlineParser.Match`).

## Example

- Entry: one minimal raw example (authoring snippet, not xmldocid — there is no example project that reifies the renderer output as a type member).

```markdown
> [!NOTE]
> Useful information the reader should not miss.
```

## See also

- How-to: [Add alerts and callouts](/how-to/content-authoring/alerts)
- Related reference: [Markdown extensions catalog](/reference/markdown/extensions)
- Related reference: [`MonorailCssOptions`](/reference/options/monorailcssoptions)
