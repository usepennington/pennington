---
title: "Organize content with sections and areas"
description: "Split a DocSite's Content/ folder into two top-level areas with subfolder-driven sections, and use staggered order: values so the sidebar groups in the order you expect."
sectionLabel: "Getting Started with DocSite"
order: 3
tags:
  - docsite
  - navigation
  - content-structure
  - front-matter
uid: tutorials.docsite.sections-and-areas
---

By the end of this tutorial the DocSite runs at `http://localhost:5000` with an area selector showing **Guides** and **Reference**. Each area renders its own grouped sidebar: *Getting Started* and *Advanced* under Guides, *Core API* and *Extensions* under Reference, with pages sorted by `order:` inside each group. For the algorithm behind the sidebar, see <xref:explanation.routing.navigation-tree>.

## Prerequisites

- .NET 10 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (provides the area-free `Content/guides/` host this tutorial turns into areas)
- Completed [Add doc pages and link between them](xref:tutorials.docsite.first-doc-page) (so `DocSiteFrontMatter` keys like `sectionLabel:` and `order:` are already familiar)

The finished code for this tutorial lives in [`examples/DocSiteSectionsExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteSectionsExample).

---

## 1. Register two areas and start from a flat page

So far `Content/` is area-free — every page shares one sidebar tree. This tutorial splits that into two switchable tabs, **Guides** and **Reference**, then fills each with grouped sections. To watch the grouping build up from nothing, reset the Guides area to a single flat page first: delete the `configure.md` and hub `index.md` you added in [Add doc pages and link between them](xref:tutorials.docsite.first-doc-page), leaving just `install.md`. Then register the areas and strip `install.md` back to minimal front matter, so the sidebar starts as a single ungrouped entry before any sections appear.

<Steps>
<Step StepNumber="1">

**Register two content areas in `Program.cs`**

Add two `ContentArea` entries — `Guides` bound to `guides/` and `Reference` bound to `reference/`. Each binds a top-level folder under `Content/` to its own sidebar tab, and the selector appears once more than one area is configured. Every other change in this tutorial is a filesystem change under `Content/`.

```csharp:symbol
examples/DocSiteSectionsExample/Program.cs
```

</Step>
<Step StepNumber="2">

**Strip `Content/guides/install.md` back to minimal front matter**

Leave `Content/guides/install.md` with just a `title:` and a `description:` — drop the `order:`, `uid:`, and the Next footer from the earlier tutorial so the page starts as a bare entry.

```markdown:symbol
examples/DocSiteSectionsExample/snippets/stage1.md
```

With no subfolder, the page is a single ungrouped entry directly under the Guides area — there is no section header because there is no folder to title-case into one. A missing `order:` defaults to `int.MaxValue`, so an un-ordered page sorts *after* any page with an explicit `order:` — but here it is the only page, so it just appears on its own.

</Step>
</Steps>

<Checkpoint>

The sidebar shows the page directly, with no section header above it.

- Run `dotnet run` from your project and visit `http://localhost:5000/guides/install`
- The Guides sidebar shows the **Install Pennington** link directly under the area with no section header above it

</Checkpoint>

---

## 2. Move the page into a subfolder to create a section

Now let's move the same page under a `getting-started/` subfolder and add `sectionLabel:` plus `order:` to the front matter. The sidebar gains its first grouped section header.

<Steps>
<Step StepNumber="1">

**Move `install.md` under `Content/guides/getting-started/`**

Delete `Content/guides/install.md` and create `Content/guides/getting-started/installation.md` in its place. The subfolder name is what creates the sidebar section header — Pennington title-cases the folder (`getting-started` → *Getting Started*) and renders it as a non-navigable group label.

Moving the file changes its URL. Routes preserve subfolders, so the page no longer serves at `/guides/install` — it now serves at `/guides/getting-started/installation/`, mirroring its path under `Content/`. The folder you added for grouping also became a URL segment.

</Step>
<Step StepNumber="2">

**Add `sectionLabel: Getting Started` and `order: 10` to the front matter**

```markdown:symbol
examples/DocSiteSectionsExample/snippets/stage2.md
```

`order:` sorts pages within the section (smaller first). `sectionLabel:` surfaces in breadcrumbs and prev/next chrome.

</Step>
</Steps>

<Checkpoint>

- The old `http://localhost:5000/guides/install` URL now 404s — the page moved. Visit `http://localhost:5000/guides/getting-started/installation/` instead
- The Guides sidebar shows a non-navigable **Getting Started** header with the **Install Pennington** link indented under it
- The breadcrumb at the top of the article reads *Guides › Getting Started › Install Pennington*

</Checkpoint>

---

## 3. Fill in the rest of the Guides area

Let's add the remaining pages to `getting-started/` and `advanced/` so Guides has two sibling sections with staggered `order:` values.

<Steps>
<Step StepNumber="1">

**Add two more pages to `getting-started/` with `order: 20` and `order: 30`**

Add the Guides landing page and two more pages to the `getting-started/` subfolder. Give `first-project.md` an `order:` of `20` and `configuration.md` an `order:` of `30`. Each page also carries `sectionLabel: Getting Started`.

```markdown:symbol
examples/DocSiteSectionsExample/Content/guides/index.md
```

```markdown:symbol
examples/DocSiteSectionsExample/Content/guides/getting-started/first-project.md
```

```markdown:symbol
examples/DocSiteSectionsExample/Content/guides/getting-started/configuration.md
```

The 10/20/30 spacing leaves room to drop pages in later without renumbering. The minimum `order:` value in the section is `10` — that matters in the next step.

</Step>
<Step StepNumber="2">

**Add the `advanced/` section with `order: 40` and `order: 50`**

Create `Content/guides/advanced/` and add two pages with `sectionLabel: Advanced` and `order:` values of `40` and `50`.

```markdown:symbol
examples/DocSiteSectionsExample/Content/guides/advanced/custom-layouts.md
```

```markdown:symbol
examples/DocSiteSectionsExample/Content/guides/advanced/response-pipeline.md
```

Section headers inherit the minimum `order:` of their pages. Leaving gaps between section order ranges — `getting-started/` at 10/20/30, `advanced/` at 40/50 — keeps *Getting Started* above *Advanced* without relying on alphabetical tie-breaks.

</Step>
</Steps>

<Checkpoint>

- Revisit `http://localhost:5000/guides/getting-started/installation/`
- The Guides sidebar shows, top to bottom: **Getting Started** (with *Install Pennington*, *Create your first project*, *Configure your site*) then **Advanced** (with *Custom layouts*, *The response pipeline*)
- Click around — breadcrumbs and prev/next labels reflect the `sectionLabel:` on each page

</Checkpoint>

---

## 4. Populate the Reference area to confirm it repeats the pattern

The same subfolder-plus-staggered-order pattern applies to the `Reference` area. Switching between both areas through the sidebar's area selector confirms each gets its own independent sidebar tree.

<Steps>
<Step StepNumber="1">

**Fill in `Content/reference/core-api/` with `order: 10` and `order: 20`**

Create the `core-api/` subfolder under `Content/reference/` and add two pages, each with `sectionLabel: Core API` and `order:` values of `10` and `20`. The folder creates the section, the key labels it, and the staggered numbers keep sibling sections predictable.

```markdown:symbol
examples/DocSiteSectionsExample/Content/reference/index.md
```

```markdown:symbol
examples/DocSiteSectionsExample/Content/reference/core-api/pennington-options.md
```

```markdown:symbol
examples/DocSiteSectionsExample/Content/reference/core-api/content-pipeline.md
```

</Step>
<Step StepNumber="2">

**Add `Content/reference/extensions/` with `order: 30` and `order: 40`**

Create `extensions/` and drop two pages in it with `sectionLabel: Extensions` and `order:` values of `30` and `40`. Continuing the count rather than restarting at `10` keeps the section order ranges separated — *Core API* at 10/20, *Extensions* at 30/40 — so *Core API* sorts above *Extensions* by the same minimum-`order:` rule from unit 3.

```markdown:symbol
examples/DocSiteSectionsExample/Content/reference/extensions/markdown-extensions.md
```

```markdown:symbol
examples/DocSiteSectionsExample/Content/reference/extensions/content-services.md
```

</Step>
<Step StepNumber="3">

**Switch areas with the sidebar's area selector**

Click the area selector pill at the top of the sidebar — the control that toggles between *Guides* and *Reference*. Each area has its own independent sidebar tree. The `ContentArea` bindings from `Program.cs` plus the subfolder layout are what make this work, with no extra code.

</Step>
</Steps>

<Checkpoint>

- With the host running, visit `http://localhost:5000/reference/core-api/pennington-options`
- The sidebar shows **Core API** above **Extensions**, with two pages under each in `order:` sequence
- Click the area selector to **Guides** — the sidebar replaces itself with the *Getting Started* / *Advanced* groups from unit 3
- The area selector tracks the current area as navigation moves between pages

</Checkpoint>

---

## Summary

- A DocSite's `Content/` folder splits into multiple `ContentArea` entries, and each one gets its own sidebar tree.
- **The subfolder name creates the sidebar section** — `sectionLabel:` is metadata for breadcrumbs and prev/next labels, not a grouper.
- Staggered `order:` values across sibling sections (10/20/30 for one, 40/50 for the next) sort section headers in the intended order, without relying on alphabetical tie-breaks between folder names.
- The shape of the generated sidebar is predictable from the shape of the `Content/` folder before running the site.
