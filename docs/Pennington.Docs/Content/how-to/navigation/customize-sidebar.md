---
title: "Reorder, rename, or hide entries in the sidebar"
description: "Use front-matter keys, a folder _meta.yml sidecar, and folder layout to control the auto-built sidebar — reorder pages and sections, promote a section landing, override the section header, and hide drafts."
uid: how-to.navigation.customize-sidebar
order: 1
sectionLabel: "Navigation & Links"
tags: [navigation, sidebar, sections, ordering]
---

The sidebar is generated from folder layout, a per-folder `_meta.yml` sidecar, and per-page front matter. Each adjustment below is independent — pick the one that matches the change you need. To replace the sidebar component itself, see <xref:how-to.response-pipeline.override-docsite-components>.

## Before you begin
- A Pennington DocSite has markdown under `Content/<area>/` with at least one subfolder (the subfolder is what creates a sidebar group — see [Work with front matter](xref:how-to.pages.front-matter) if not)
- Pages use `DocSiteFrontMatter` or another type that implements `IOrderable` + `ISectionable`
- The basics of `order:` and `isDraft:` are familiar — if not, start with [Manage drafts, tags, and ordering](xref:how-to.pages.drafts-tags-ordering)

For a working reference, see `examples/DocSiteKitchenSinkExample` — `Content/main/customize-sidebar.md` exercises the same keys.

---

## Options

### Reorder pages within a folder

Lower `order:` values sort earlier inside a folder; ties break alphabetically on `Title`. Numbers are local to the folder — start at `1` and count up. Leave gaps (`10, 20, 30`) only if you anticipate frequent inserts between siblings.

```yaml
---
title: Install
order: 2
---
```

### Reorder sections (folders) within the sidebar

Drop a `_meta.yml` sidecar into the folder you want to reposition. Its `order:` sets where the folder lands among its siblings — independent of any `order:` on the pages inside.

```yaml
# Content/main/widgets/_meta.yml
order: 3
```

Without a sidecar, the folder's position falls back to the lowest `order:` of any descendant (the min-of-children rule). Mixing modes is fine — folders with a sidecar sort by the explicit value, folders without sort by the emergent value. See <xref:reference.front-matter.folder-sidecar> for the full sidecar schema.

### Promote a page to be the section landing

Name the file `index.md` inside the section subfolder (for example `Content/main/widgets/index.md`). Pennington routes it at the subfolder URL and surfaces it as the section's lead entry rather than a separate child.

```yaml
---
title: Widgets
order: 1
---
```

When the folder also has a `_meta.yml`, the sidecar's `order:` overrides the index.md's `order:` for the **section's** position in its parent. The index.md's own `order:` then has no effect — set it to `1` for clarity or omit it.

### Override the displayed section title

There are two ways to change the printed section header. The folder name converts from kebab-case to title case by default (`getting-started` → "Getting Started").

**Option A — rename the folder.** The header follows the new name.

**Option B — set `title:` in `_meta.yml`.** Wins over both the auto-formatted folder name and any `title:` on a sibling `index.md`.

```yaml
# Content/main/widgets/_meta.yml
title: "Widget Catalog"
order: 3
```

The front-matter `sectionLabel:` key is separate from both — it sets the page-context label surfaced for breadcrumbs and prev/next navigation, not the sidebar group header.

### Hide an unfinished page from the sidebar

For a page that isn't ready to publish, set `isDraft: true`. It drops out of the sidebar, the search index, and `llms.txt`, and `dotnet run` still serves it so you can preview your work.

```yaml
---
title: Work in progress
isDraft: true
---
```

Under `dotnet run -- build` a draft is excluded from the static output entirely — the page file is never written, so any `xref:` link to it fails to resolve. This is the canonical draft rule documented in [the front-matter key reference](xref:reference.front-matter.keys); `isDraft` is for pages that aren't meant to ship, not for hiding pages you still want published.

### Keep a page published but out of the sidebar

To ship a page at its URL while keeping it off the sidebar — a "published but unlisted" page — leave its `title:` empty instead of drafting it. A page with no title produces no sidebar entry (and no search or `llms.txt` entry), but the route still renders and builds, so `xref:` links to it resolve.

```yaml
---
title: ""
---
```

A page with `redirectUrl:` is also omitted from the sidebar.

---

## Verify

- Run `dotnet run`; reordered pages appear in ascending `order:` inside their folder
- A folder with a `_meta.yml` lands at the position its `order:` specifies, even when its descendants' values would have placed it elsewhere
- The section subfolder's `index.md` lands at `/<area>/<section>/` and renders as the section's lead entry in the sidebar
- The drafted page's URL still serves the page in `dotnet run`; the entry is absent from the sidebar on reload. Under `dotnet run -- build` the page is excluded from the static output.
- The empty-`title:` page has no sidebar entry but its URL serves in both `dotnet run` and `dotnet run -- build`

## Related

- Reference: [Folder sidecar (`_meta.yml`)](xref:reference.front-matter.folder-sidecar)
- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Reference: [Navigation UI components](xref:reference.ui.navigation)
- Background: [How the sidebar is built](xref:explanation.routing.navigation-tree)
