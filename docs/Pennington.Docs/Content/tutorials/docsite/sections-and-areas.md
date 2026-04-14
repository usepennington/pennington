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

> **In this page.** _One sentence paraphrasing the Covers line: the reader will split `Content/` into two areas (`Guides` and `Reference`), break each area into two subfolder-backed sections, fill in `sectionLabel:` and `order:` front matter on each page, and watch `NavigationBuilder` assemble the grouped sidebar — landing the rule that subfolders group the sidebar while `sectionLabel:` is only metadata, and that `order:` values should be staggered across sibling sections._
>
> **Not in this page.** _One sentence paraphrasing the Does-not-cover line: point the reader at the locale-prefixed navigation tutorial at `/tutorials/beyond-basics/add-a-locale`, at the Razor-page how-tos under `/how-to/content-authoring/`, and at the custom-content-service explanation at `/explanation/content-services` — those are out of scope here._

## What you'll do

_**Artifact** (one sentence): a running DocSite at `http://localhost:5000` with an area selector over `Guides` and `Reference`, each rendering a grouped sidebar — *Getting Started* + *Advanced* under Guides, *Core Api* + *Extensions* under Reference — with pages sorted by `order:` inside each group._

_**Skill** (one sentence): the reader walks away knowing that a top-level folder under `Content/` becomes a `ContentArea`, that each subfolder under an area becomes a sidebar section node driven by folder name (not the `sectionLabel:` key), and that staggering `order:` numbers across sibling sections is how you control which section header appears first._

## Prerequisites

_Keep this list to tools and prior tutorials. The `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` host is identical to the scaffolding tutorial — link back to it rather than re-explaining it. No Razor or custom-service knowledge is needed._

- .NET 11 SDK installed
- Completed [Scaffold a documentation site with DocSite](xref:tutorials.docsite.scaffold) (provides the single-area host shape this tutorial extends)
- Completed [Author a documentation page with DocFrontMatter](xref:tutorials.docsite.first-doc-page) (so `DocSiteFrontMatter` keys like `sectionLabel:` and `order:` are already familiar)

The finished code for this tutorial lives in [`examples/DocSiteSectionsExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteSectionsExample).

---

## 1. Start from a flat area and see the limit

_One sentence: the reader begins with a single page parked directly under an area folder — no subfolder, no `sectionLabel:`, no `order:` — so the sidebar has nothing to group. This establishes the baseline the rest of the tutorial improves on._

### Step 1.1 — Confirm the two-area host from the scaffolding tutorial

_Show the canonical DocSite host with two `ContentArea` entries (`Guides` bound to `guides`, `Reference` bound to `reference`). Emphasize that this file is never edited again for the rest of the tutorial — every change from here on is a filesystem change under `Content/`. Note that the area selector the reader will see in the top-left of the sidebar is rendered automatically by `MainLayout` whenever `DocSiteOptions.Areas` has more than one entry._

```csharp:path
examples/DocSiteSectionsExample/Program.cs
```

_Call out the two `ContentArea` constructors: the first argument is the label shown in the area selector, the second is the folder name under `Content/`. No other wiring is needed — `AddDocSite` discovers both folders through a single markdown pipeline._

### Step 1.2 — Drop a single page into `Content/guides/` with no section or order

_Have the reader create `Content/guides/install.md` with minimal front matter — just `title:` and `description:`. Stress that with no subfolder and no `order:`, the page sorts to the top of the Guides area as a flat entry because `order` defaults to `int.MaxValue` and there's no sibling subfolder for the navigation builder to fold it under. This is the problem the next unit fixes._

```csharp:xmldocid,bodyonly
M:DocSiteSectionsExample.Stage1.Source
```

_Explain that the embedded YAML-plus-markdown string is what the reader actually pastes into `Content/guides/install.md`. Do not dive into `IOrderable` defaults or `NavigationBuilder` internals — the Explanation page covers that._

### Checkpoint — A single ungrouped entry under Guides

_Concrete verification. The sidebar should show the page directly, with no section header above it._

- Run `dotnet run` from `examples/DocSiteSectionsExample`
- Visit `http://localhost:5000/guides/install`
- The Guides sidebar should show the **Install Pennington** link at the top of the area with no section header above it — a flat entry

---

## 2. Move the page into a subfolder to create a section

_One sentence: the reader moves the same page under a `getting-started/` subfolder and adds `sectionLabel:` + `order:` to the front matter, so the sidebar gains its first grouped section header._

### Step 2.1 — Move `install.md` under `Content/guides/getting-started/`

_Walk the reader through the filesystem move: delete `Content/guides/install.md`, create `Content/guides/getting-started/installation.md`. Hammer the load-bearing rule: **the subfolder name is what creates the sidebar section**, not the `sectionLabel:` key. The folder name is title-cased (`getting-started` → *Getting Started*) and appears as a non-navigable header above the page link._

### Step 2.2 — Add `sectionLabel: Getting Started` and `order: 10` to the front matter

_Show the Stage 2 source — same page as Stage 1, but now with two extra front-matter keys. Clarify the split: `sectionLabel:` is metadata carried on `NavigationInfo.SectionName` and shown in breadcrumbs / prev-next chrome, while `order:` is the integer that sorts pages inside a section (smaller first, ties broken on title). Call out that `sectionLabel:` on its own will not group anything if the file lives outside a subfolder — it is a label, not a grouper._

```csharp:xmldocid,bodyonly
M:DocSiteSectionsExample.Stage2.Source
```

_One sentence on the practical rule: "one section = one subfolder; `sectionLabel:` just names it in breadcrumbs."_

### Checkpoint — One grouped section under Guides

_Concrete verification. The sidebar now has a header above the page._

- Reload `http://localhost:5000/guides/installation`
- The Guides sidebar shows a non-navigable **Getting Started** header with the **Install Pennington** link indented under it
- Hover the breadcrumb at the top of the article — it reads *Guides › Getting Started › Install Pennington*

---

## 3. Fill in the rest of the Guides area

_One sentence: the reader adds the remaining pages to `getting-started/` and `advanced/` so Guides has two sibling sections with staggered `order:` values — the exact pattern that prevents the tie-break gotcha._

### Step 3.1 — Add two more pages to `getting-started/` with `order: 20` and `order: 30`

_Point the reader at the Guides-landing and the two additional getting-started pages in the example. Each page gets `sectionLabel: Getting Started` plus its own `order:` (`first-project.md` at `20`, `configuration.md` at `30`). Emphasize that the reader is choosing tidy 10/20/30 numbers on purpose — it leaves room to insert pages later and it keeps the minimum order of this section at `10`._

```markdown:path
examples/DocSiteSectionsExample/Content/guides/index.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/guides/getting-started/first-project.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/guides/getting-started/configuration.md
```

### Step 3.2 — Add the `advanced/` section with `order: 40` and `order: 50`

_Have the reader create `Content/guides/advanced/` and add the two pages with `sectionLabel: Advanced` and `order: 40` / `order: 50`. Land the key authoring rule here as an instruction, not an algorithm: "Stagger `order:` values across sibling sections — 10/20/30 inside `getting-started/` and 40/50 inside `advanced/` — so the two section headers sort in the order you expect. If both sections start at 10, you'll get alphabetical ordering of the folder names instead, and `advanced/` would surprise you by appearing first." One short sentence, no derivation._

```markdown:path
examples/DocSiteSectionsExample/Content/guides/advanced/custom-layouts.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/guides/advanced/response-pipeline.md
```

### Checkpoint — Two sections under Guides, in the intended order

_Concrete verification. The sidebar shows **Getting Started** above **Advanced**, each with its pages in `order:` sequence._

- Revisit `http://localhost:5000/guides/installation`
- The Guides sidebar shows, top to bottom: **Getting Started** (with *Install Pennington*, *Create your first project*, *Configure Pennington*) and then **Advanced** (with *Custom layouts*, *The response pipeline*)
- Click around — the breadcrumb and prev/next labels reflect the `sectionLabel:` on each page

---

## 4. Populate the Reference area to confirm it repeats the pattern

_One sentence: the reader applies the same subfolder-plus-staggered-order pattern to the `Reference` area and switches between the two via the sidebar's area selector, confirming that each area gets its own independent sidebar tree._

### Step 4.1 — Fill in `Content/reference/core-api/` with `order: 10` and `order: 20`

_Have the reader create the `core-api/` subfolder and its two pages, each with `sectionLabel: Core API` and `order: 10` / `order: 20`. Reinforce the pattern one more time: the folder creates the section, the key labels it, the staggered numbers keep sibling sections predictable._

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

_Same move again: create `extensions/`, drop two pages with `sectionLabel: Extensions` and `order: 30` / `order: 40`. Note that using `30`/`40` here (rather than restarting at `10`) is the same staggering rule from unit 3 — the Reference area's *Core API* minimum is `10` and *Extensions* minimum is `30`, so the sections sort *Core API → Extensions* without relying on the alphabetical tie-break._

```markdown:path
examples/DocSiteSectionsExample/Content/reference/extensions/markdown-extensions.md
```

```markdown:path
examples/DocSiteSectionsExample/Content/reference/extensions/content-services.md
```

### Step 4.3 — Switch areas with the sidebar's area selector

_Tell the reader to click the area selector pill at the top of the sidebar (the one that toggles between *Guides* and *Reference*). Each area has its own independent sidebar tree — the `ContentArea` bindings from `Program.cs` plus the subfolder layout are what make this work. No extra code._

### Checkpoint — Both areas render correctly, independently

_Concrete verification. Each area selector click swaps the sidebar to the matching tree._

- With the host running, visit `http://localhost:5000/reference/core-api/pennington-options`
- The sidebar now shows **Core API** above **Extensions**, with two pages under each in `order:` sequence
- Click the area selector back to **Guides** — the sidebar replaces itself with the *Getting Started* / *Advanced* groups from unit 3
- Visit `/` (or hover links in either sidebar) — the area selector remembers whichever area you're currently inside

---

## Summary

_Three to five bullets. Each one names a capability the reader now has, not a topic the page covered._

- You can split a DocSite's `Content/` folder into multiple `ContentArea` entries and have each one get its own sidebar tree.
- You know that **the subfolder name creates the sidebar section** — `sectionLabel:` is metadata for breadcrumbs and prev/next labels, not a grouper.
- You can stagger `order:` values across sibling sections (10/20/30 for one, 40/50 for the next) so section headers sort in the order you intend, without relying on alphabetical tie-breaks between folder names.
- You can predict the shape of the generated sidebar from the shape of the `Content/` folder before running the site.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
