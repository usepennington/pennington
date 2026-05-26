---
title: "Add doc pages and link between them"
description: "Build out a Guides area with two content pages, wire sibling navigation, hub-style absolute links, and rename-safe uid cross-references."
sectionLabel: "Getting Started with DocSite"
order: 2
tags:
  - docsite
  - authoring
  - linking
  - cross-references
uid: tutorials.docsite.first-doc-page
---

By the end of this tutorial the Guides area has two new pages — `install.md` and `configure.md` — wired into the sidebar in `order:` sequence, cross-linked with relative paths, and reachable from a hub `index.md` that uses both absolute paths and a `uid:`-based `xref:` link.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a DocSite](xref:tutorials.docsite.scaffold) (or have a single-area DocSite project ready with a `Guides` area pointed at `Content/guides/`)

The finished code for this tutorial lives in [`examples/DocSitePagesAndLinksExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSitePagesAndLinksExample).

---

## 1. Add two content pages

Let's drop two markdown files into the Guides area and watch them slot into the sidebar. The pages stand alone for now — linking arrives in the next unit.

<Steps>
<Step StepNumber="1">

**Create `Content/guides/install.md`**

Add a new file at `Content/guides/install.md` with the markdown below. The four front-matter keys are the ones [`DocSiteFrontMatter`](xref:reference.api.doc-site-front-matter) reads for sidebar wiring: `title` is the link label, `description` becomes the meta tag, `sectionLabel` carries through to breadcrumbs and prev/next chrome, and `order` decides where the page sorts among siblings.

```markdown:symbol
examples/DocSitePagesAndLinksExample/snippets/install-step1.md
```

</Step>
<Step StepNumber="2">

**Create `Content/guides/configure.md`**

Add a second file next to the first with the markdown below. Note `order: 30` — a larger number than install's `order: 20`, so configure sorts after install in the sidebar.

```markdown:symbol
examples/DocSitePagesAndLinksExample/snippets/configure-step1.md
```

The 20 / 30 spacing leaves room to drop a page between them later without renumbering. For deeper coverage of section grouping and `order:` strategy, see <xref:tutorials.docsite.sections-and-areas>.

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/guides/install` — the Install Pennington page renders.
- Visit `http://localhost:5000/guides/configure` — the Configure the site page renders.
- The Guides sidebar lists both new pages, with **Install Pennington** above **Configure the site** (sorted by `order:`).

</Checkpoint>

---

## 2. Link siblings with relative paths

Both pages exist but neither knows about the other. Adding a relative-path link at the bottom of each one creates a natural "previous / next" flow without hardcoding the area slug.

<Steps>
<Step StepNumber="1">

**Add a "Next" footer to `install.md`**

Append a `## Next` heading and a relative-path link to the bottom of `install.md`. `./configure` resolves against the current page's URL (`/guides/install`), so it points at `/guides/configure` no matter where the area sits.

```markdown:symbol
examples/DocSitePagesAndLinksExample/snippets/install-with-next.md
```

</Step>
<Step StepNumber="2">

**Add a "Previously" footer to `configure.md`**

Mirror the same shape on `configure.md` with a `./install` link back to the first page. Both pages now link to their sibling with a path that survives any move of the `Content/guides/` folder.

```markdown:symbol
examples/DocSitePagesAndLinksExample/Content/guides/configure.md
```

</Step>
</Steps>

<Checkpoint>

- Reload `http://localhost:5000/guides/install` — a **Configure the site** link sits at the bottom of the page. Click it.
- The browser lands on `/guides/configure`. A **Install Pennington** link at the bottom of that page returns home.
- Both transitions are instant — the SPA navigation that ships with DocSite swaps the article without a full page reload.

</Checkpoint>

---

## 3. Turn the index into a hub with absolute paths

The `Content/guides/index.md` page from the scaffold still says "Authoring walkthroughs live in this area" or similar — a placeholder that no longer matches the content under it. Let's rewrite it as a hub that links to both pages with absolute paths.

<Steps>
<Step StepNumber="1">

**Replace `index.md` with the hub markdown below**

Absolute paths (`/guides/install`) survive folder moves of the source page. Use them when the target sits in a different folder than the source, or when the link is structural rather than narrative. For the full link-form rundown, see <xref:how-to.navigation.linking>.

```markdown:symbol
examples/DocSitePagesAndLinksExample/snippets/index-as-hub.md
```

</Step>
</Steps>

<Checkpoint>

- Visit `http://localhost:5000/guides/` — the Guides landing page now lists the two walkthroughs.
- Click **Install Pennington** — the browser navigates to `/guides/install`.
- Click **Configure the site** — the browser navigates to `/guides/configure`.

</Checkpoint>

---

## 4. Make one link rename-safe with `uid:` + `xref:`

Absolute paths break the moment the target file moves or gets renamed. A `uid:` declared in the page's front matter gives the page a stable identifier; `xref:` links resolve through it, so the link survives the file moving or even the URL changing.

<Steps>
<Step StepNumber="1">

**Add `uid: guides.install` to `install.md`'s front matter**

Open `install.md` and add one front-matter key — the page is now reachable by `xref:guides.install` no matter where the file lives.

```markdown:symbol
examples/DocSitePagesAndLinksExample/Content/guides/install.md
```

</Step>
<Step StepNumber="2">

**Swap the install link in `index.md` to use `xref:`**

Open `index.md` and replace `/guides/install` with `xref:guides.install`. The configure link stays as an absolute path — handy for seeing both forms side by side in the rendered output.

```markdown:symbol
examples/DocSitePagesAndLinksExample/Content/guides/index.md
```

</Step>
</Steps>

<Checkpoint>

- Reload `http://localhost:5000/guides/` — both links in the hub still work. The rendered `<a>` for **Install Pennington** points at `/guides/install` just as the absolute-path version did.
- View source: the `xref:guides.install` href has been rewritten to the canonical URL. The xref form is the same shape an editor would have produced — but the source markdown now survives any rename of `install.md`.

</Checkpoint>

---

## Summary

- Two markdown files under `Content/guides/` showed up in the sidebar without any extra wiring, sorted by `order:` from front matter.
- Relative paths (`./configure`) link tightly coupled sibling pages — the form that survives area-folder renames.
- Absolute paths (`/guides/configure`) link from a hub where the source page may move but the target's location is stable.
- `uid:` plus `xref:` — the rename-safe form — turns the page identifier itself into the link target.
- For the full link-form reference (anchors, assets, sub-path deployments), see <xref:how-to.navigation.linking>. For deeper `uid:` semantics, see <xref:how-to.navigation.cross-references>.
