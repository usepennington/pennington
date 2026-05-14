---
title: "Author a documentation page with DocFrontMatter"
description: "Write a DocSite page end-to-end: populate DocSiteFrontMatter, add a GitHub-style alert, and group code samples into a tabbed block with a live outline nav."
sectionLabel: "Getting Started with DocSite"
order: 102020
tags:
  - docsite
  - front-matter
  - markdown
  - authoring
uid: tutorials.docsite.first-doc-page
---

By the end of this tutorial the DocSite has a `guides/authoring.md` page showing a fully-populated front-matter block, a rendered `[!NOTE]` alert, and a three-panel tabbed code group — with the outline nav on the right listing every `##` heading on the page.

This tutorial covers populating `DocSiteFrontMatter`, reaching for Pennington's GitHub-style alerts, and grouping adjacent fenced code blocks into a tabbed component without writing a single line of Razor or JavaScript.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a DocSite](xref:tutorials.docsite.scaffold) (or have an equivalent single-area DocSite project ready with a `Guides` area pointed at `Content/guides/`)

The finished code for this tutorial lives in [`examples/DocSiteAuthorExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteAuthorExample).

---

## 1. Populate `DocSiteFrontMatter`

Let's start with the metadata block that drives the sidebar entry, the `<title>`, the meta description, and the tag chips. This first step is front matter plus a single heading — the skeleton later sections build on.

<Steps>
<Step StepNumber="1">

**Create `Content/guides/authoring.md`**

In the `Guides` area folder (`Content/guides/`), create a new file named `authoring.md` and leave it empty for now. The next step fills it in.

</Step>
<Step StepNumber="2">

**Paste the markdown below**

Copy the block below verbatim into `authoring.md`. The five keys — `title`, `description`, `tags`, `sectionLabel`, `order` — are the ones `DocSiteFrontMatter` reads to build the sidebar, meta tags, and tag chips. Everything below the closing `---` is the page body.

```markdown:path
examples/DocSiteAuthorExample/snippets/stage1.md
```

`sectionLabel` controls the prev/next breadcrumb label shown beneath the page header; it's not what groups pages into the sidebar (subfolder layout does that). `order` decides where this page sits relative to its siblings.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` from the project folder and visit `http://localhost:5000/guides/authoring`.
- The h1 reads "Authoring a doc page", the description renders as meta, and a sidebar entry labelled "Authoring a doc page" appears under the `Guides` area.
- The outline nav on the right is empty — there are no `##` headings yet. That changes in unit 2.

</Checkpoint>

---

## 2. Add a GitHub-style alert

Pennington recognises the GitHub alert syntax: a block quote whose first line is `[!KIND]`. Adding one produces a coloured callout and — because the callout introduces a `##` heading above it — the first outline-nav entry.

<Steps>
<Step StepNumber="1">

**Replace the file body with the markdown below**

Swap the current contents of `authoring.md` for the block below. It keeps the same front matter and adds a `## Callouts` section containing a `[!NOTE]` alert.

```markdown:path
examples/DocSiteAuthorExample/snippets/stage2.md
```

The supported kinds are `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, and `CAUTION`. Pennington wraps the surrounding block quote in a `markdown-alert` container so CSS can style it.

</Step>
</Steps>

<Checkpoint>

- Reload `http://localhost:5000/guides/authoring`.
- The `[!NOTE]` block now renders as a blue-bordered callout with an info icon — not a plain block quote.
- The outline nav on the right shows a single entry, "Callouts", linking to `#callouts`.

</Checkpoint>

---

## 3. Add a tabbed code group

Marking two or more adjacent fenced code blocks with `tabs=true` and a `title="…"` fence argument tells Pennington to group them into a single ARIA tablist. The tab labels come from each block's `title`.

<Steps>
<Step StepNumber="1">

**Replace the file body with the markdown below**

Paste the block below over the current contents of `authoring.md`. It keeps the front matter and alert from the previous step and adds a `## Tabbed code groups` section with three adjacent fenced blocks.

```markdown:path
examples/DocSiteAuthorExample/snippets/stage3.md
```

Each fence still gets normal syntax highlighting based on its language (`bash`, `powershell`, `xml`). The `tabs=true` flag and `title="…"` label are what tell the tabbed renderer these three blocks belong together — drop either one and the blocks render independently.

</Step>
</Steps>

<Checkpoint>

- Reload `http://localhost:5000/guides/authoring`.
- The three fenced blocks under "Tabbed code groups" render as one component with three selectable tabs ("dotnet CLI", "PowerShell", "csproj"); clicking a tab swaps the visible code.
- The outline nav on the right shows two entries — "Callouts" and "Tabbed code groups" — each linking to its heading anchor.

</Checkpoint>

---

## Summary

- Every key `DocSiteFrontMatter` reads — `title`, `description`, `tags`, `sectionLabel`, `order` — flows through to the sidebar, meta description, and prev/next label.
- A GitHub-style `[!NOTE]` alert goes through Pennington's `CustomAlertInlineParser` and renders as a styled callout.
- Three adjacent fenced code blocks with `tabs=true` and `title="…"` become a single tabbed component.
- The outline nav populates automatically from the page's `##` headings — no manual nav wiring.

