---
title: "Why the sidebar mirrors your folders"
description: "Why Pennington derives the sidebar tree from folder structure and front-matter order instead of a hand-written nav file, and what that costs when sections reorder."
uid: explanation.routing.navigation-tree
order: 2
sectionLabel: "Routing and Navigation"
tags: [navigation, routing, hierarchy]
---

Why does the Pennington sidebar reflect the folder layout even for folders that were never named in front matter, and where does the ordering come from?

## Context

Every content service produces a flat list of `ContentTocItem` records. Each item carries a route, a title, an authored order value, a section label, a locale, and a `HierarchyParts` array — the canonical path segmented on `/`. The sidebar, by contrast, is a tree: folders group pages, sections expand and collapse, and the currently-viewed page is highlighted with its ancestors open. The gap between "flat list" and "navigable tree" has to be bridged somewhere.

Some documentation tools require a hand-written `nav.yml` that explicitly names every section, sets every order, and assigns every label. The maintenance cost is real — the config file drifts whenever authors move pages, and it duplicates information that the filesystem already encodes. Pennington's design target was to treat the filesystem as the primary outline and derive everything else from it, with `order:` in front matter as the only setting authors need to reach for.

The result uses two signals: **folder structure** supplies the tree, and **front matter** supplies leaf ordering and breadcrumb labels. There is no third configuration file sitting between them. The sections below trace how `NavigationBuilder` performs that fold — from flat list to tree, including what happens when a folder has no index page and how locale prefixes are removed before the recursion starts.

## How it works

### HierarchyParts folds the flat TOC into a tree

Each `ContentTocItem` carries a `HierarchyParts` array — for example, the item at `/how-to/configuration/search` arrives with `["how-to", "configuration", "search"]`. `NavigationBuilder.BuildTreeAsync` recurses level by level rather than item by item. At each depth it selects items whose `HierarchyParts.Length` equals `depth + 1` and whose prefix matches the current parent path, orders them by `Order` then case-insensitive title, and deduplicates by canonical path. That last step guards against two content sources registering overlapping subtrees — a situation that would otherwise produce duplicate sidebar entries.

Recursing level-by-level rather than item-by-item is what makes sibling ordering work across content sources that have no knowledge of each other. The algorithm sees all siblings at once before it descends, so the relative ordering between a page from a Razor source and a page from a Markdown source is resolved in the same pass.

There is one special case at depth 0: a `ContentTocItem` whose `HierarchyParts.Length` is 0 is treated as the area's landing page. Its hierarchy was already stripped by the content service before the list was handed to the builder, so the builder injects it at the top of the tree — anchored above every other root entry regardless of what `order:` value was authored.

Each field on `ContentTocItem` plays a distinct role in the algorithm: `HierarchyParts` shapes the tree, `Order` and `Title` sort siblings, `SectionLabel` surfaces only in prev/next and breadcrumbs, and `Locale` feeds the filter described below (see <xref:reference.api.content-toc-item> for the type).

The `currentPath` parameter passed to `BuildTreeAsync` marks items `IsSelected` and propagates `IsExpanded` up the ancestor chain. The same tree therefore powers both the "where am I" highlight and the collapsed or expanded state of every surrounding folder. The method returns an `ImmutableList<NavigationTreeItem>`, so the entire tree is a value rather than a mutable model the rendering layer binds to directly.

### Sections without a direct content file

When `BuildLevel` finds deeper descendants under a hierarchy segment that has no direct item at the current depth — a folder like `/how-to/configuration/` with children but no `configuration/index.md` — it synthesizes a non-navigable section node on the fly. The title is the folder segment kebab-to-title-cased: `getting-started` becomes "Getting Started". The node is given an empty `ContentRoute` so the rendering component treats it as a section header rather than a link, and `IsExpanded` is set by whether any descendant is currently selected.

This is the mechanism that lets an author drop markdown files into `/how-to/deployment/` without creating a `deployment/index.md` and still see "Deployment" appear as a collapsible sidebar heading. The folder itself is sufficient.

The important distinction is between folder-derived grouping and the per-page `sectionLabel:` front-matter key. Grouping comes entirely from subfolder; `sectionLabel:` controls only the label shown in breadcrumbs and prev/next. Two files carrying identical `sectionLabel: "Advanced"` values in different folders render under two different sidebar headers — each named after its own folder — rather than merging. Merging by label would let two unrelated folders collide under a single heading, reintroducing the configuration conflict the filesystem-driven approach was designed to eliminate.

The synthesized section node and a real leaf page share the same `NavigationTreeItem` record shape (see <xref:reference.api.navigation-tree-item>); a section node carries an empty route, which is how the rendering component tells the two apart.

### Ordering: front matter for leaves, `_meta.yml` for folders

Leaf pages at any given level sort first by their authored `Order` value, then by title using a case-insensitive ordinal comparison as a stable fallback. Folders, having no front matter of their own, take their order from a different source: by default a synthesized section node gets `Order = children.Min(c => c.Order)`, so it sorts as if it were whichever of its children would sort first. A folder with an `index.md` but no sidecar takes its order and title from that page instead.

That default — min-of-children — has an awkward consequence: sibling sections interleave by the smallest `order:` value found anywhere inside each. If "Getting Started" contains a page with `order: 10` and "Deployment" contains a page with `order: 20`, the sidebar places Getting Started above Deployment. If someone later adds a page with `order: 5` to Deployment — perhaps because they want it first within that section — the whole Deployment group jumps above Getting Started. Sites that grow past a handful of sections end up choosing globally-unique numeric prefixes (`301010, 301020, 302010, …`) to keep folder ordering stable while still allowing in-folder inserts.

The escape hatch is a `_meta.yml` sidecar: a folder declaring its own `order: 1` decouples its position from its children, so each folder's pages can restart at `1` without disturbing where the folder lands in its parent. The sidecar can also override the folder's display title and opt the subtree into a dedicated `llms.txt` split. The full schema and precedence rules live in the <xref:reference.front-matter.folder-sidecar> reference.

### Locale prefix stripping

Non-default locales are stored on disk under a locale folder (`Content/fr/...`), so a French page at `/fr/how-to/configuration/search` arrives in the flat list with `HierarchyParts` reading `["fr", "how-to", "configuration", "search"]`. If `BuildTreeAsync` recursed over those items without any preprocessing, every French page would nest under a `/fr/` root while English pages sat at the top level — two unrelated sibling trees rather than one coherent per-locale outline. The min-of-children ordering would also produce incorrect results, because "the first page in my folder" would mean something different in each language subtree.

`FilterByLocale` runs before the level-by-level recursion begins. It keeps items whose `Locale` matches the requested locale or is `null` (for locale-agnostic content), and — for non-default locales only — strips `HierarchyParts[0]` when it equals the locale code. The recursion then sees a shape identical to what the default locale sees, with the language prefix removed. The min-of-children ordering and the section-node synthesis therefore work the same way regardless of which locale is being rendered. Items carrying `Locale == null` pass through every filter unchanged, which is why redirects and feeds appear in every locale's sidebar without requiring duplicate files on disk.

## Further reading

- Reference: [Folder sidecar (`_meta.yml`)](xref:reference.front-matter.folder-sidecar) — the full schema and precedence rules for the folder-level overrides this page motivates.
- Reference: [Navigation components (`TableOfContentsNavigation`, `OutlineNavigation`)](xref:reference.ui.navigation) — the UI that consumes the tree `NavigationBuilder` returns.
- How-to: [Customize the sidebar](xref:how-to.navigation.customize-sidebar) — the recipe that leans on the ordering rules this page explains.
- Tutorial: [Organize content with sections and areas](xref:tutorials.docsite.sections-and-areas) — the tutorial that introduces folder-driven grouping for new authors.
