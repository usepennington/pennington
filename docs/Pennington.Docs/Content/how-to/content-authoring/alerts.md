---
title: "Add alerts and callouts"
description: "GitHub-style alert syntax (`> [!NOTE]`, `[!TIP]`, `[!CAUTION]`, `[!WARNING]`, `[!IMPORTANT]`) and how they render."
section: content-authoring
order: 50
uid: how-to.content-authoring.alerts
isDraft: true
search: false
llms: false
tags: []
---

> **In this page.** GitHub-style alert syntax (`> [!NOTE]`, `[!TIP]`, `[!CAUTION]`, `[!WARNING]`, `[!IMPORTANT]`) and how they render.
>
> **Not in this page.** Custom alert styles, Mermaid diagrams, or the `<Card>` component — those live on separate pages.

## When to use this

- Outline: one sentence — reader has a markdown page and wants to flag a note, tip, caveat, warning, or imperative aside.
- Outline: one sentence — do not retread front matter or the Markdig pipeline; link out if reader is too early.

## Assumptions

- Bullet: existing Pennington site with markdown content (link to Getting Started tutorial).
- Bullet: `AddPennington` / `AddDocSite` already registered so `MarkdownPipelineFactory.CreateWithExtensions` is in effect (this is what wires `UseCustomAlerts`).
- Bullet: reader is comfortable editing a `.md` file under the configured `ContentPath`.
- Bullet: pointer to `examples/BeaconDocsExample` as a working copy-from site.

---

## Steps

### 1. Open (or create) a markdown page under your content root

- Bullet: pick any `.md` page served by a `MarkdownContentService` (e.g. `examples/BeaconDocsExample/Content/guides/migration-v3.md`).
- Bullet: link to the front-matter how-to — this page assumes valid front matter already exists.
- Bullet: fence slot — reference the raw file that already uses alerts.

```markdown file="examples/BeaconDocsExample/Content/guides/migration-v3.md"
```

### 2. Write an alert as a blockquote whose first line is `[!KIND]`

- Bullet: syntax rule — the alert marker must be the first non-blank child of a `>` blockquote (parser in `src/Pennington/Markdown/Extensions/CustomAlertInlineParser.cs` rejects anything else).
- Bullet: the five supported kinds are `NOTE`, `TIP`, `CAUTION`, `WARNING`, `IMPORTANT` (case-insensitive in the parser; uppercase is the GitHub convention).
- Bullet: body text goes on the following blockquote lines — standard markdown inlines work inside.
- Bullet: show a minimal raw example for each kind in one plain fence (not xmldocid, not raw file — these are authoring snippets).

```markdown
> [!NOTE]
> Useful information the reader should not miss.

> [!TIP]
> An optional shortcut or helpful extra.

> [!CAUTION]
> Action with potential negative consequences.

> [!WARNING]
> Urgent information demanding immediate reader attention.

> [!IMPORTANT]
> Key content the reader needs in order to succeed.
```

### 3. Confirm the rendered HTML carries the expected classes

- Bullet: rendered output is a `<div>` with classes `markdown-alert` and `markdown-alert-{kind}` (lowercased), emitted by the `AlertBlockRenderer` registered in `MarkdownPipelineFactory.CreateWithExtensions` via `UseCustomAlerts`.
- Bullet: the five class variants styled out of the box live in `src/Pennington.MonorailCss/MonorailCssOptions.cs`: `.markdown-alert-note` (emerald), `.markdown-alert-tip` (blue), `.markdown-alert-caution` (amber), `.markdown-alert-warning` (rose), `.markdown-alert-important` (sky).
- Bullet: no component import, no code-behind — alerts travel with your default markdown pipeline.

---

## Verify

- Bullet: run `dotnet run --project examples/BeaconDocsExample` and visit `/guides/migration-v3`.
- Bullet: expect five distinct colored callout boxes (or however many you authored), each prefixed with its kind label.
- Bullet: inspect the page source — each alert is a `<div class="markdown-alert markdown-alert-{kind}">`.

## Related

- Reference: Markdown extensions reference (covers `UseCustomAlerts` and surrounding pipeline hooks).
- Reference: `MonorailCssOptions` color mapping for the five alert classes.
- Background: Explanation of the Markdig extension pipeline and why Pennington replaces the built-in alert inline parser.
