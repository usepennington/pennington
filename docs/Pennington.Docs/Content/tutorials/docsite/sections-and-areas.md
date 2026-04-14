---
title: "Organize content with sections and areas"
description: "Structure Content/ into areas and sections, use section and order in front matter, and see how Pennington turns a flat file tree into a sidebar."
section: "docsite"
order: 30
tags: []
uid: tutorials.docsite.sections-and-areas
isDraft: true
search: false
llms: false
---

> **In this page.** Structuring `Content/` into areas and sections, using `section` / `order` in front matter, and how Pennington turns a flat file tree into a sidebar.
>
> **Not in this page.** Locale-prefixed navigation, Razor-page integration, or custom content source implementations.

## What you'll do

- **Artifact:** a DocSite split into two top-level areas (e.g., "Docs" and "API") with multiple sections each, and a sidebar that reflects the area/section hierarchy in the right order.
- **Skill:** you'll know the difference between an area (a top-level content silo) and a section (a mid-level grouping inside one area), how front matter drives both, and where to look when navigation does not appear the way you expect.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](/tutorials/docsite/scaffold) and [Author a documentation page with DocFrontMatter](/tutorials/docsite/first-doc-page)
- A running DocSite with at least one rendered page

The finished sections pattern can be studied in [`examples/BeaconDocsExample`](https://github.com/usepennington/pennington/tree/main/examples/BeaconDocsExample), [`examples/NorthwindHandbookExample`](https://github.com/usepennington/pennington/tree/main/examples/NorthwindHandbookExample), and [`examples/SpectreConsoleExample`](https://github.com/usepennington/pennington/tree/main/examples/SpectreConsoleExample).

---

## 1. Use `section` to group pages inside one content tree

- Bullets to cover under this unit:
- The `section:` key groups pages so the sidebar shows them together.
- Two common patterns:
  1. Folder-only: create `Content/guides/` and set `section: "guides"` in every page inside; the folder name becomes the navigation group.
  2. Explicit: set `section: "Getting Started"` on loose pages in `Content/` so they group together even if they live in different folders.
- `order:` is an integer within a section (default `int.MaxValue`). Use tidy 10/20/30 increments so inserts don't force renumbering.

### Step 1.1 — Pick a section name and set it on three pages

- Create three markdown files under `Content/getting-started/`.
- Give each an `order` of `10`, `20`, `30`.
- Give each `section: "getting-started"` (or leave the folder-name default).

```markdown file="examples/BeaconDocsExample/Content/index.md"
```

- _Use this as a shape reference — ignore the body, copy the front-matter layout._

### Step 1.2 — Reload the site and read the sidebar

- Run `dotnet run` and visit `/`.
- Confirm the three pages appear grouped under "Getting Started" in the sidebar in the order you set.
- Swap two `order` values, reload, and watch the order flip.

### Checkpoint — sections are working

- Three pages appear under one sidebar group.
- The `order` values drive the sort.
- Changing `section` on one page moves it to a different group.

---

## 2. Use a `section-index.md` for group landing pages

- Bullets to cover under this unit:
- A section group is usually clickable. That landing page is an ordinary page whose `section:` matches the group name and whose `order:` is `0` (or the smallest in that group).
- Pennington promotes that page to the group-level anchor in the sidebar.

### Step 2.1 — Add a section-level index page

- Create `Content/getting-started/index.md` with `title:`, `section: "getting-started"`, and `order: 0`.
- Write a one-paragraph "what's in this section" intro in the body.

### Step 2.2 — Verify the group now has a landing page

- Reload the site.
- Click the group name in the sidebar; it should now route to the section-level index rather than to the first child.
- Confirm the children still show underneath.

### Checkpoint — section landing pages work

- The group name in the sidebar is a link.
- Visiting that link serves the index you wrote.
- The first real page in the group is still reachable underneath.

---

## 3. Split the site into two areas

- Bullets to cover under this unit:
- `DocSiteOptions.Areas` is a top-level silo above sections. Each area maps to a top-level folder under `Content/`.
- An area gives the site multiple independent content trees (e.g., "Docs" and "API Reference") each with its own sidebar.
- If you do not set `Areas`, the whole site is implicitly one area rooted at `Content/`.

### Step 3.1 — Create two top-level folders under `Content/`

- Rename / move existing files so you have `Content/docs/…` and `Content/api/…`.
- Each folder becomes the content tree for one area.

### Step 3.2 — Configure `DocSiteOptions.Areas`

- In `Program.cs`, set `Areas = [ new ContentArea("Docs", "docs"), new ContentArea("API", "api") ]` on the options object you pass to `AddDocSite`.
- The first positional argument is the display label; the second is the folder name under `Content/`.

```csharp file="docs/Pennington.Docs/Program.cs"
```

- _Use this as the reference for the options shape._

### Step 3.3 — Verify the navigation shows one area at a time

- Reload the site.
- The header or chrome should show an area switcher (or the URL should prefix the area slug, depending on template).
- Pages under `Content/docs/` appear in the Docs sidebar; pages under `Content/api/` in the API sidebar.
- Cross-area links still work, but each sidebar only shows its own area's content.

### Checkpoint — two areas are live

- `Content/docs/` and `Content/api/` are both served.
- Switching areas shows a different sidebar.
- Pages within each area still respect `section` and `order`.

---

## Summary

- You grouped pages into sections using `section:` and `order:` front-matter keys.
- You added a section-level landing page by setting `order: 0` on an index page that shares the section.
- You split the site into two areas via `DocSiteOptions.Areas`, each rooted at a top-level folder under `Content/`.
- You know how Pennington folds a flat file tree plus front-matter hints into the sidebar structure.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
