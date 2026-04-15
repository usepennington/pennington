---
title: "Organize content with sections and areas"
description: "Split a DocSite's Content/ folder into two top-level areas with subfolder-driven sections, and use staggered order: values so the sidebar groups in the order you expect."
sectionLabel: "Getting Started with DocSite"
order: 102030
tags:
  - docsite
  - navigation
  - content-structure
  - front-matter
uid: tutorials.docsite.sections-and-areas
---

By the end of this tutorial you'll have a running DocSite at `http://localhost:5000` with an area selector showing **Guides** and **Reference**. Each area renders its own grouped sidebar: *Getting Started* and *Advanced* under Guides, *Core API* and *Extensions* under Reference, with pages sorted by `order:` inside each group.

You'll walk away knowing that a top-level folder under `Content/` becomes a `ContentArea`, that each subfolder under an area becomes a sidebar section node driven by the folder name rather than by `sectionLabel:`, and that staggering `order:` numbers across sibling sections is how you control which section header appears first.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (provides the single-area host shape this tutorial extends)
- Completed [Author a documentation page with DocFrontMatter](xref:tutorials.docsite.first-doc-page) (so `DocSiteFrontMatter` keys like `sectionLabel:` and `order:` are already familiar)

The finished code for this tutorial lives in [`examples/DocSiteSectionsExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteSectionsExample).

---

## 1. Start from a flat area and see the limit

Let's begin with a single page parked directly under an area folder — no subfolder, no `sectionLabel:`, no `order:` — so the sidebar has nothing to group. This establishes the baseline you'll improve on through the rest of the tutorial.

### Step 1.1 — Confirm the two-area host from the scaffolding tutorial

The `Program.cs` file wires up two `ContentArea` entries: `Guides` bound to the `guides` folder and `Reference` bound to the `reference` folder.

```csharp:path
examples/DocSiteSectionsExample/Program.cs
```

This file stays untouched for the rest of the tutorial. Every change from here on is a filesystem change under `Content/`. The area selector that appears in the top-left of the sidebar is rendered automatically by `MainLayout` whenever `DocSiteOptions.Areas` contains more than one entry — no extra code required.

Notice the two `ContentArea` constructors: the first argument is the label shown in the area selector, the second is the folder name under `Content/`. `AddDocSite` discovers both folders through a single markdown pipeline.

### Step 1.2 — Drop a single page into `Content/guides/` with no section or order

Create `Content/guides/install.md` with minimal front matter — a `title:` and a `description:`, nothing else.

```csharp:xmldocid,bodyonly
M:DocSiteSectionsExample.Stage1.Source
```

Paste the YAML-plus-markdown content above into `Content/guides/install.md`. With no subfolder and no `order:`, the page sorts to the top of the Guides area as a flat entry. The `order` key defaults to `int.MaxValue` and there is no sibling subfolder for the navigation builder to fold the page under. The next unit fixes that.

### Checkpoint — A single ungrouped entry under Guides

The sidebar should show the page directly, with no section header above it.

- Run `dotnet run` from `examples/DocSiteSectionsExample`
- Visit `http://localhost:5000/guides/install`
- The Guides sidebar shows the **Install Pennington** link at the top of the area with no section header above it

---

## 2. Move the page into a subfolder to create a section

Now let's move the same page under a `getting-started/` subfolder and add `sectionLabel:` plus `order:` to the front matter. The sidebar gains its first grouped section header.

### Step 2.1 — Move `install.md` under `Content/guides/getting-started/`

Delete `Content/guides/install.md` and create `Content/guides/getting-started/installation.md` in its place.

The load-bearing rule: **the subfolder name is what creates the sidebar section**, not the `sectionLabel:` key. `NavigationBuilder` title-cases the folder name (`getting-started` becomes *Getting Started*) and renders it as a non-navigable header above the page links.

### Step 2.2 — Add `sectionLabel: Getting Started` and `order: 10` to the front matter

```csharp:xmldocid,bodyonly
M:DocSiteSectionsExample.Stage2.Source
```

The two keys serve different purposes. `order:` is an integer that sorts pages inside a section — smaller numbers appear first, with ties broken alphabetically on title. `sectionLabel:` is metadata carried on `NavigationInfo.SectionName` and shown in breadcrumbs and prev/next chrome. If the file lives outside a subfolder, `sectionLabel:` has no grouping effect — it is a label, not a grouper.

One section, one subfolder. `sectionLabel:` names it in breadcrumbs.

### Checkpoint — One grouped section under Guides

- Reload `http://localhost:5000/guides/installation`
- The Guides sidebar shows a non-navigable **Getting Started** header with the **Install Pennington** link indented under it
- The breadcrumb at the top of the article reads *Guides › Getting Started › Install Pennington*

---

## 3. Fill in the rest of the Guides area

Let's add the remaining pages to `getting-started/` and `advanced/` so Guides has two sibling sections with staggered `order:` values — the exact pattern that prevents the tie-break surprise.

### Step 3.1 — Add two more pages to `getting-started/` with `order: 20` and `order: 30`

Add the Guides landing page and two more pages to the `getting-started/` subfolder. Give `first-project.md` an `order:` of `20` and `configuration.md` an `order:` of `30`. Each page also carries `sectionLabel: Getting Started`.

```markdown:path
examples/DocSiteSectionsExample/Content/guides/index.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/guides/getting-started/first-project.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/guides/getting-started/configuration.md
```

The 10/20/30 sequence is deliberate — it leaves room to insert pages later without renumbering everything. The minimum `order:` value in this section is `10`, which matters in the next step.

### Step 3.2 — Add the `advanced/` section with `order: 40` and `order: 50`

Create `Content/guides/advanced/` and add two pages with `sectionLabel: Advanced` and `order:` values of `40` and `50`.

```markdown:path
examples/DocSiteSectionsExample/Content/guides/advanced/custom-layouts.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/guides/advanced/response-pipeline.md
```

Stagger `order:` values across sibling sections — 10/20/30 inside `getting-started/` and 40/50 inside `advanced/` — so the two section headers sort in the order you expect. If both sections start at `10`, the navigation builder falls back to alphabetical ordering of the folder names, and `advanced/` would appear above *Getting Started* before you had a chance to wonder why.

### Checkpoint — Two sections under Guides, in the intended order

- Revisit `http://localhost:5000/guides/installation`
- The Guides sidebar shows, top to bottom: **Getting Started** (with *Install Pennington*, *Create your first project*, *Configure Pennington*) then **Advanced** (with *Custom layouts*, *The response pipeline*)
- Click around — breadcrumbs and prev/next labels reflect the `sectionLabel:` on each page

---

## 4. Populate the Reference area to confirm it repeats the pattern

You'll now apply the same subfolder-plus-staggered-order pattern to the `Reference` area, then switch between both areas using the sidebar's area selector to confirm each gets its own independent sidebar tree.

### Step 4.1 — Fill in `Content/reference/core-api/` with `order: 10` and `order: 20`

Create the `core-api/` subfolder under `Content/reference/` and add two pages, each with `sectionLabel: Core API` and `order:` values of `10` and `20`. The folder creates the section, the key labels it, and the staggered numbers keep sibling sections predictable.

```markdown:path
examples/DocSiteSectionsExample/Content/reference/index.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/reference/core-api/pennington-options.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/reference/core-api/content-pipeline.md
```

### Step 4.2 — Add `Content/reference/extensions/` with `order: 30` and `order: 40`

Create `extensions/` and drop two pages in it with `sectionLabel: Extensions` and `order:` values of `30` and `40`. Using `30`/`40` rather than restarting at `10` applies the same staggering rule from unit 3 — the *Core API* minimum is `10` and the *Extensions* minimum is `30`, so the sections sort *Core API → Extensions* without relying on the alphabetical tie-break.

```markdown:path
examples/DocSiteSectionsExample/Content/reference/extensions/markdown-extensions.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/reference/extensions/content-services.md
```

### Step 4.3 — Switch areas with the sidebar's area selector

Click the area selector pill at the top of the sidebar — the control that toggles between *Guides* and *Reference*. Each area has its own independent sidebar tree. The `ContentArea` bindings from `Program.cs` plus the subfolder layout are what make this work, with no extra code.

### Checkpoint — Both areas render correctly, independently

- With the host running, visit `http://localhost:5000/reference/core-api/pennington-options`
- The sidebar shows **Core API** above **Extensions**, with two pages under each in `order:` sequence
- Click the area selector to **Guides** — the sidebar replaces itself with the *Getting Started* / *Advanced* groups from unit 3
- The area selector tracks whichever area you're currently inside as you navigate

---

## Summary

- You can split a DocSite's `Content/` folder into multiple `ContentArea` entries and have each one get its own sidebar tree.
- **The subfolder name creates the sidebar section** — `sectionLabel:` is metadata for breadcrumbs and prev/next labels, not a grouper.
- You can stagger `order:` values across sibling sections (10/20/30 for one, 40/50 for the next) so section headers sort in the order you intend, without relying on alphabetical tie-breaks between folder names.
- You can predict the shape of the generated sidebar from the shape of the `Content/` folder before running the site.
