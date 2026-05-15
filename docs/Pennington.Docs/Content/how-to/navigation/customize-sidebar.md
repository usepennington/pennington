---
title: "Reorder, rename, or hide entries in the sidebar"
description: "Use front-matter keys and folder layout to control the auto-built sidebar — reorder siblings, promote a section landing, override the section header, and hide drafts."
uid: how-to.navigation.customize-sidebar
order: 204010
sectionLabel: "Navigation & Links"
tags: [navigation, sidebar, sections, ordering]
---

The sidebar is generated from folder layout and front matter. Each adjustment below is independent — pick the one that matches the change you need. To replace the sidebar component itself, see <xref:how-to.response-pipeline.override-docsite-components>.

## Before you begin
- A Pennington DocSite has markdown under `Content/<area>/` with at least one subfolder (the subfolder is what creates a sidebar group — see [Work with front matter](xref:how-to.pages.front-matter) if not)
- Pages use `DocSiteFrontMatter` or another type that implements `IOrderable` + `ISectionable`
- The basics of `order:` and `isDraft:` are familiar — if not, start with [Manage drafts, tags, and ordering](xref:how-to.pages.drafts-tags-ordering)

For a working reference, see `examples/DocSiteKitchenSinkExample` — `Content/main/customize-sidebar.md` exercises the same keys.

---

## Options

### Reorder pages within a section

Lower `order:` values sort earlier inside a section; ties break alphabetically on `Title`. Use 10/20/30 spacing so later inserts land between siblings without renumbering every file.

```yaml
---
title: Install
order: 204010
---
```

### Promote a page to be the section landing

Name the file `index.md` inside the section subfolder (for example `Content/main/widgets/index.md`). Pennington routes it at the subfolder URL and surfaces it as the section's lead entry rather than a separate child. A low `order:` (typically `10`) sorts the whole section earlier — see <xref:explanation.routing.navigation-tree> for how section sort keys are derived.

```yaml
---
title: Widgets
order: 204010
---
```

### Override the displayed section title

The sidebar section header comes from the folder name (kebab-case converts to title case: `getting-started` → "Getting Started"). Rename the folder to change the printed header. The front-matter `sectionLabel:` key is separate — it sets the page-context label surfaced for breadcrumbs and current-page context, not the sidebar group header.

```yaml
---
title: Install
sectionLabel: "Navigation & Links"
---
```

### Hide a page from the sidebar

Set `isDraft: true` to keep the page compiled — so `xref:` links still resolve — while dropping it from the sidebar, the search index, and `llms.txt`. A page with `redirectUrl:` is also omitted from the sidebar.

```yaml
---
title: Work in progress
isDraft: true
---
```

---

## Verify

- Run `dotnet run`; reordered pages appear in ascending `order:` inside their section, and the section itself moves when its minimum-child order changes
- The section subfolder's `index.md` lands at `/<area>/<section>/` and renders as the section's lead entry in the sidebar
- The drafted page's URL returns 404 and the entry is absent from the sidebar on reload

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Reference: [Navigation UI components](xref:reference.ui.navigation)
- Background: [How the sidebar is built](xref:explanation.routing.navigation-tree)
