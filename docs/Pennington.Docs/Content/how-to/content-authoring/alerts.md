---
title: "Add alerts and callouts"
description: "GitHub-style alert syntax (`> [!NOTE]`, `[!TIP]`, `[!CAUTION]`, `[!WARNING]`, `[!IMPORTANT]`) and how they render."
section: content-authoring
order: 70
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

When a markdown page needs to flag a note, tip, caveat, warning, or imperative aside, write it as a GitHub-style alert blockquote. No component import or code-behind is required — alerts travel with the default markdown pipeline.

## Assumptions

- You have an existing Pennington site with markdown content (see the [Getting Started tutorial](/tutorials/getting-started/first-site) if not).
- `AddPennington` or `AddDocSite` is already registered.
- You are comfortable editing a `.md` file under the configured `ContentPath`.

To copy a working setup, see [`examples/BeaconDocsExample`](https://github.com/usepennington/pennington/tree/main/examples/BeaconDocsExample).

---

## Steps

### 1. Open a markdown page under your content root

Pick any `.md` page served by your site — for example `examples/BeaconDocsExample/Content/guides/migration-v3.md`. This page assumes valid front matter already exists; see [Work with front matter](/how-to/content-authoring/front-matter) if it doesn't.

### 2. Write an alert as a blockquote whose first line is `[!KIND]`

The alert marker must be the first non-blank child of a `>` blockquote. The five supported kinds are `NOTE`, `TIP`, `CAUTION`, `WARNING`, and `IMPORTANT` (case-insensitive; uppercase is the GitHub convention). Body text goes on the following blockquote lines, and standard markdown inlines work inside.

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

### 3. Confirm the rendered classes

The rendered output is a `<div>` with classes `markdown-alert` and `markdown-alert-{kind}` (lowercased). The five class variants ship with default colors: `.markdown-alert-note` (emerald), `.markdown-alert-tip` (blue), `.markdown-alert-caution` (amber), `.markdown-alert-warning` (rose), and `.markdown-alert-important` (sky).

---

## Verify

- Run `dotnet run --project examples/BeaconDocsExample` and visit `/guides/migration-v3`.
- Expect a distinct colored callout box for each alert, prefixed with its kind label.
- Inspect the page source — each alert is a `<div class="markdown-alert markdown-alert-{kind}">`.

## Related

- Reference: [Markdown extensions](/reference/markdown/extensions)
- Reference: [`MonorailCssOptions`](/reference/monorailcss/options) — the color mapping for the five alert classes.
- Background: [MonorailCSS integration](/explanation/rendering/monorail-css)
