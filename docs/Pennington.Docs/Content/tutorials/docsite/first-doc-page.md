---
title: "Author a documentation page with DocFrontMatter"
description: "Write a DocSite page end-to-end: populate DocSiteFrontMatter, add a GitHub-style alert, and group code samples into a tabbed block with a live outline nav."
sectionLabel: "Getting Started with DocSite"
order: 20
tags:
  - docsite
  - front-matter
  - markdown
  - authoring
uid: tutorials.docsite.first-doc-page
---

> **In this page.** You'll write a page with `DocSiteFrontMatter` (title, description, tags, section, order), add a `[!NOTE]` alert, drop in a tabbed code group, and watch the outline navigation populate from your headings.
>
> **Not in this page.** Cross-references between pages, reusable snippets, and diagram blocks live in their own per-feature how-tos — this tutorial stays focused on a single page.

## What you'll do

**Artifact.** A running DocSite with a `guides/authoring.md` page that shows a fully-populated front-matter block, a rendered `[!NOTE]` alert, and a three-panel tabbed code group — with the outline nav on the right listing every `##` heading on the page.

**Skill.** You'll know how to populate `DocSiteFrontMatter`, reach for Pennington's GitHub-style alerts, and group adjacent fenced code blocks into a tabbed component without writing a single line of Razor or JS.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a DocSite](xref:tutorials.docsite.scaffold) (or have an equivalent single-area DocSite project ready with a `Guides` area pointed at `Content/guides/`)

The finished code for this tutorial lives in [`examples/DocSiteAuthorExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteAuthorExample).

---

## 1. Populate `DocSiteFrontMatter`

_Start with the metadata block that drives the sidebar entry, the `<title>`, the meta description, and the tag chips. Stage 1 is just front matter plus a single `<h1>` — the skeleton every later stage builds on._

### Step 1.1 — Create `Content/guides/authoring.md`

_In your `Guides` area folder (`Content/guides/`), create a new file named `authoring.md`. Leave it empty for now; the next step fills it in._

### Step 1.2 — Paste the stage-1 markdown

_Copy the block below verbatim into `authoring.md`. The five keys — `title`, `description`, `tags`, `sectionLabel`, `order` — are the ones `DocSiteFrontMatter` reads to build the sidebar, meta tags, and tag chips. Everything below the closing `---` is the page body._

```markdown:xmldocid,bodyonly
M:DocSiteAuthorExample.Stage1.Source
```

_`sectionLabel` controls the prev/next breadcrumb label shown beneath the page header; it's not what groups pages into the sidebar (subfolder layout does that). `order` decides where this page sits relative to its siblings._

### Checkpoint — Stage-1 page renders

- Run `dotnet run` from the project folder and visit `http://localhost:5000/guides/authoring`.
- You should see the h1 "Authoring a doc page", the description rendered as meta, and a sidebar entry labelled "Authoring a doc page" under the `Guides` area.
- The outline nav on the right is empty — there are no `##` headings yet. That changes in unit 2.

---

## 2. Add a GitHub-style alert

_Pennington recognises the GitHub alert syntax: a block quote whose first line is `[!KIND]`. Adding one gives you a coloured callout and — because the callout introduces a `##` heading above it — your first outline-nav entry._

### Step 2.1 — Replace the file body with the stage-2 markdown

_Swap the current contents of `authoring.md` for the block below. It keeps the same front matter and adds a `## Callouts` section containing a `[!NOTE]` alert._

```markdown:xmldocid,bodyonly
M:DocSiteAuthorExample.Stage2.Source
```

_The supported kinds are `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, and `CAUTION`. Pennington's `CustomAlertInlineParser` rewrites the surrounding quote block into a `markdown-alert` container so CSS can style it._

### Checkpoint — Alert and first outline entry

- Reload `http://localhost:5000/guides/authoring`.
- The `[!NOTE]` block now renders as a blue-bordered callout with an info icon — not a plain block quote.
- The outline nav on the right shows a single entry, "Callouts", linking to `#callouts`.

---

## 3. Add a tabbed code group

_Mark two or more adjacent fenced code blocks with `tabs=true` and a `title="…"` fence argument and Pennington's `TabbedCodeBlockRenderer` groups them into a single ARIA tablist. The tab labels come from each block's `title`._

### Step 3.1 — Replace the file body with the stage-3 markdown

_Paste the block below over the current contents of `authoring.md`. It keeps the front matter and alert from stage 2 and adds a `## Tabbed code groups` section with three adjacent fenced blocks._

```markdown:xmldocid,bodyonly
M:DocSiteAuthorExample.Stage3.Source
```

_Each fence still gets normal syntax highlighting based on its language (`bash`, `powershell`, `xml`). The `tabs=true` flag and `title="…"` label are what tell the tabbed renderer these three blocks belong together — drop either one and the blocks render independently._

### Checkpoint — Tabs render and outline nav populates

- Reload `http://localhost:5000/guides/authoring`.
- The three fenced blocks under "Tabbed code groups" now render as one component with three selectable tabs ("dotnet CLI", "PowerShell", "csproj"); clicking a tab swaps the visible code.
- The outline nav on the right now shows two entries — "Callouts" and "Tabbed code groups" — each linking to its heading anchor.

---

## Summary

- You populated every key `DocSiteFrontMatter` reads — `title`, `description`, `tags`, `sectionLabel`, `order` — and saw them flow through to the sidebar, meta description, and prev/next label.
- You added a GitHub-style `[!NOTE]` alert and saw Pennington's `CustomAlertInlineParser` turn it into a styled callout.
- You grouped three adjacent fenced code blocks into a single tabbed component with `tabs=true` and `title="…"`.
- You watched the outline nav populate automatically from the page's `##` headings — no manual nav wiring.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
