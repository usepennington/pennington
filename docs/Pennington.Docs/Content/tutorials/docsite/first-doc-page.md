---
title: "Author a documentation page with DocFrontMatter"
description: "Write a DocSite page with front matter, add alerts and a tabbed code group, and see the outline navigation populate."
section: "docsite"
order: 20
tags: []
uid: tutorials.docsite.first-doc-page
isDraft: true
search: false
llms: false
---

> **In this page.** Writing a page with `DocSiteFrontMatter` (`title`, `description`, `tags`, `section`, `order`), adding alerts and a tabbed code group, and seeing the outline navigation populate.
>
> **Not in this page.** Cross-references, snippets, or diagram blocks — those are per-feature how-tos.

## What you'll do

- **Artifact:** a new page at `/guides/first-doc` with front matter, alert callouts, a tabbed code sample, and body headings that appear in the outline automatically.
- **Skill:** you'll know how to create a normal DocSite page and use a few built-in markdown features without touching the site shell.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](/tutorials/docsite/scaffold/)
- A running DocSite project with a `Content/` folder and at least one page already rendering

The finished code for this tutorial lives in [`examples/BeaconDocsExample`](https://github.com/Phil-Scott-Msft/Pennington/tree/main/examples/BeaconDocsExample).

---

## 1. Create the page file

_Start with one new markdown file and make sure it shows up where you expect._

### Step 1.1 — Add `Content/guides/first-doc.md`

- Create a `guides` folder under `Content/` if you do not already have one.
- Add `first-doc.md` with front matter for `title`, `description`, `section`, `order`, and `tags`.
- Add one short paragraph below the front matter so the page is not empty.

```markdown file="examples/BeaconDocsExample/Content/getting-started/install.md"
```

_Use this example page as the model for your front matter and body shape._

### Step 1.2 — Open the new page in the browser

- Run the site if it is not already running.
- Visit `http://localhost:5000/guides/first-doc`.
- Confirm the title renders and the page appears in the sidebar under the section name you chose.

### Checkpoint — the page exists and is discoverable

- `/guides/first-doc` loads successfully
- The sidebar shows the new page
- You changed one markdown file and nothing else

---

## 2. Add alerts and a tabbed code group

_Now make the page feel like real documentation instead of a blank article._

### Step 2.1 — Add two alerts

- Under your intro paragraph, add one `> [!NOTE]` block and one `> [!WARNING]` block.
- Keep each alert short so the visual difference is obvious.
- Refresh the page and confirm they render as styled callouts instead of plain blockquotes.

```markdown file="examples/BeaconDocsExample/Content/guides/configuration.md"
```

_This example shows the exact alert syntax you can copy._

### Step 2.2 — Add a tabbed code sample

- Below the alerts, add a `:::tabs` block.
- Inside it, add two fenced code blocks with titles such as `C#` and `JSON`.
- Refresh the page and click between the tabs to confirm the content swaps in place.

### Checkpoint — the page has real doc widgets

- The note and warning alerts render correctly
- The tabbed code sample shows two tabs
- Clicking a tab changes the visible code without leaving the page

---

## 3. Add headings and watch the outline populate

_Finish by giving the page a few sections and checking that the right-hand outline updates on its own._

### Step 3.1 — Add three `##` headings

- Add three section headings such as `## Overview`, `## Usage`, and `## Next steps`.
- Put one short paragraph under each heading so the page has enough content to scroll.
- Save and refresh the page.

```markdown file="examples/BeaconDocsExample/Content/getting-started/index.md"
```

_This example shows a page shape with multiple headings that the outline can pick up._

### Step 3.2 — Use the outline navigation

- Look at the outline on the right side of the page.
- Confirm your three headings appear in source order.
- Click one of the outline links and confirm the page scrolls to that section.

### Checkpoint — the outline follows the page structure

- The outline lists the three headings you added
- Clicking an outline entry jumps to the matching section
- Adding or removing a heading changes the outline automatically

---

## Summary

- You created a new DocSite page with front matter and body content
- You used built-in alerts and a tabbed code sample in that page
- You added headings and saw the outline navigation update automatically
- You now have a repeatable pattern for creating more documentation pages

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
