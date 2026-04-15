---
title: "Customize the sidebar"
description: "Reorder, promote, rename, and hide pages in the auto-built sidebar using folder layout and front-matter keys."
uid: how-to.content-authoring.customize-sidebar
order: 201030
sectionLabel: Content Authoring
tags: [navigation, sidebar, sections, ordering]
---

When a DocSite groups pages under subfolders, the auto-generated sidebar may need adjustment — reordering siblings, making one page the section landing, renaming the section header, or hiding a page. These adjustments use front-matter keys and folder naming. To replace the sidebar component itself, see the extensibility guide for overriding DocSite components.

## Assumptions

- A Pennington DocSite has markdown under `Content/<area>/` with at least one subfolder (the subfolder is what creates a sidebar group — see [Work with front matter](xref:how-to.content-authoring.front-matter) if not)
- Pages use `DocSiteFrontMatter` or another type that implements `IOrderable` + `ISectionable`
- The basics of `order:` and `isDraft:` are familiar — if not, start with [Manage drafts, tags, and ordering](xref:how-to.content-authoring.drafts-tags-ordering)

For a working reference, see `examples/DocSiteKitchenSinkExample` — `Content/main/customize-sidebar.md` exercises the same keys.

---

## Steps

<Steps>
<Step StepNumber="1">

**Reorder pages within a section**

Lower `order:` values sort earlier inside a section; ties break alphabetically on `Title`. Use 10/20/30 spacing so later inserts land between siblings without renumbering every file.

```yaml
---
title: Install
order: 10
---
```

Backing symbol on the DocSite front-matter record:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Order
```

</Step>
<Step StepNumber="2">

**Promote a page to be the section landing**

Name the file `index.md` inside the section subfolder (for example `Content/main/widgets/index.md`). Pennington routes it at the subfolder URL and `NavigationBuilder` surfaces it as the section's lead entry rather than a separate child. A low `order:` — typically `10` — sorts the entire section earlier, because the section's aggregate sort key is the minimum `order:` of its direct children.

```yaml
---
title: Widgets
order: 10
---
```

The area-root landing follows the same pattern at one level up:

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/index.md
```

<!-- TODO: xmldocid needed — replace above with a section-level index.md fixture once one exists in examples/ -->

</Step>
<Step StepNumber="3">

**Override the displayed section title**

The sidebar section header comes from the folder name, with kebab-case converted to title case by `NavigationBuilder` (for example `getting-started` becomes "Getting Started"). Renaming the folder changes what the sidebar prints. The front-matter `sectionLabel:` key is separate — it sets the page-context label surfaced on `NavigationInfo.SectionName` for breadcrumbs and current-page context, not the sidebar group header.

```yaml
---
title: Install
sectionLabel: Quick Start
---
```

Backing symbol for the front-matter key:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.SectionLabel
```

</Step>
<Step StepNumber="4">

**Hide a page from the sidebar**

Set `isDraft: true` to keep the page compiled — so `xref:` links still resolve — while dropping it from the sidebar, the search index, and `llms.txt`. A page with `redirectUrl:` is also omitted from the sidebar regardless of other keys; the engine treats redirects as transport hops rather than content.

```yaml
---
title: Work in progress
isDraft: true
---
```

Backing symbol on `IFrontMatter` (the draft key is not specific to DocSite):

```csharp:xmldocid
P:Pennington.FrontMatter.IFrontMatter.IsDraft
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run`; reordered pages appear in ascending `order:` inside their section, and the section itself moves when its minimum-child order changes
- The section subfolder's `index.md` lands at `/<area>/<section>/` and renders as the section's lead entry in the sidebar
- The drafted page's URL returns 404 and the entry is absent from the sidebar on reload

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- Reference: [Navigation UI components](xref:reference.ui.navigation)
- Background: [How the sidebar is built](xref:explanation.routing.navigation-tree)
