---
title: "Navigation-tree construction"
description: "How Pennington folds a flat list of ContentTocItems into the sidebar tree using HierarchyParts, folder-derived sections, and min-of-children ordering."
uid: explanation.routing.navigation-tree
order: 303020
sectionLabel: "Routing and Navigation"
tags: [navigation, routing, hierarchy]
---

Why does the Pennington sidebar reflect your folder layout even for folders you never named in front matter, and where does the ordering come from?

## Context

Every content service produces a flat list of `ContentTocItem` records. Each item carries a route, a title, an authored order value, a section label, a locale, and a `HierarchyParts` array — the canonical path segmented on `/`. The sidebar, by contrast, is a tree: folders group pages, sections expand and collapse, and the currently-viewed page is highlighted with its ancestors open. The gap between "flat list" and "navigable tree" has to be bridged somewhere.

Some documentation tools require a hand-written `nav.yml` that explicitly names every section, sets every order, and assigns every label. The ergonomic cost is real — the config file drifts whenever authors move pages, and it duplicates information that the filesystem already encodes. Pennington's design target was to treat the filesystem as the primary outline and derive everything else from it, with `order:` in front matter as the only tuning knob authors need to reach for.

The result is a two-signal model: **folder structure** supplies the tree shape, and **front matter** supplies leaf ordering and breadcrumb labels. There is no third configuration surface sitting between them. The sections below trace how `NavigationBuilder` performs that fold — from flat list to tree, including what happens when a folder has no index page and how locale prefixes are removed before the recursion starts.

> [!NOTE]
> A common point of confusion: the **folder name** drives sidebar grouping and the section header text, while the `sectionLabel:` front-matter key controls only the label shown in breadcrumbs and the prev/next footer. The two serve different surfaces. Renaming the folder changes the sidebar header; changing `sectionLabel:` does not.

## How it works

### HierarchyParts folds the flat TOC into a tree

Each `ContentTocItem` carries a `HierarchyParts` array — for example, the item at `/how-to/configuration/search` arrives with `["how-to", "configuration", "search"]`. `NavigationBuilder.BuildTree` recurses level by level rather than item by item. At each depth it selects items whose `HierarchyParts.Length` equals `depth + 1` and whose prefix matches the current parent path, orders them by `Order` then case-insensitive title, and deduplicates by canonical path. That last step guards against two content sources registering overlapping subtrees — a situation that would otherwise produce duplicate sidebar entries.

Recursing level-by-level rather than item-by-item is what lets sibling ordering work correctly across content sources that have no knowledge of each other. The algorithm sees all siblings at once before it descends, so the relative ordering between a page from a Razor source and a page from a Markdown source is resolved at the same pass.

There is one special case at depth 0: a `ContentTocItem` whose `HierarchyParts.Length` is 0 is treated as the area's landing page. Its hierarchy was already stripped by the content service before the list was handed to the builder, so the builder injects it at the top of the tree with `Order = int.MinValue`. That anchors it above every other root entry regardless of what `order:` value was authored.

```csharp:xmldocid
T:Pennington.Content.ContentTocItem
```

Each field on `ContentTocItem` plays a distinct role in that algorithm: `HierarchyParts` shapes the tree, `Order` and `Title` sort siblings, `SectionLabel` surfaces only in prev/next and breadcrumbs, and `Locale` feeds the filter described below.

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

The `currentRoute` parameter passed to `BuildTree` is what marks items `IsSelected` and propagates `IsExpanded` up the ancestor chain. The same tree therefore powers both the "where am I" highlight and the collapsed or expanded state of every surrounding folder — the UI does not need to maintain that state separately. The method returns an `ImmutableList<NavigationTreeItem>`, so the entire tree is a value rather than a mutable model the rendering layer binds to directly.

### Sections without a direct content file

When `BuildLevel` finds deeper descendants under a hierarchy segment that has no direct item at the current depth — a folder like `/how-to/configuration/` with children but no `configuration/index.md` — it synthesizes a non-navigable section node on the fly. The title comes from `FormatSectionTitle`, which kebab-to-title-cases the folder segment: `getting-started` becomes "Getting Started". The node is given an empty `ContentRoute` so the rendering component treats it as a section header rather than a link, and `IsExpanded` is set by whether any descendant is currently selected.

This is the mechanism that lets an author drop markdown files into `/how-to/deployment/` without creating a `deployment/index.md` and still see "Deployment" appear as a collapsible sidebar heading. The folder itself is sufficient.

The important distinction here is between this folder-derived grouping signal and the per-page `sectionLabel:` front-matter key. Grouping is determined entirely by which subfolder a file lives in. `sectionLabel:` controls only the label shown in breadcrumbs and the prev/next footer for leaf pages. Two files carrying identical `sectionLabel: "Advanced"` values in different folders render under two different sidebar headers — each named after its own folder — rather than merging into one shared "Advanced" group. That behavior is deliberate. Merging by label would let two unrelated folders collide under a single heading, reintroducing a configuration-surface conflict that the filesystem-driven approach was designed to eliminate.

```csharp:xmldocid
T:Pennington.Navigation.NavigationTreeItem
```

The synthesized section node and a real leaf page share the same `NavigationTreeItem` record shape. The rendering component distinguishes them by checking `Children.Count > 0 && Route.CanonicalPath.Value == ""` rather than consulting a discriminator field. Section headers are therefore first-class members of the tree rather than a parallel structure layered on top.

### Ordering: min-of-children with alphabetic tie-break

Leaf pages at any given level sort first by their authored `Order` value, then by title using a case-insensitive ordinal comparison as a stable fallback. Synthesized section nodes have no authored order of their own, so `BuildLevel` assigns them `Order = children.Min(c => c.Order)` — the section sorts as if it were whichever of its children would sort first. This means authors control section ordering the same way they control leaf ordering: by setting `order:` on the pages inside the folder, not on any separate section definition.

The practical consequence is that sibling sections interleave by the smallest `order:` value found anywhere inside each. If "Getting Started" contains a page with `order: 10` and "Deployment" contains a page with `order: 20`, the sidebar places Getting Started above Deployment. If someone later adds a page with `order: 5` to Deployment — perhaps because they want it first within that section — the whole Deployment group jumps above Getting Started. The tradeoff here is real: the design avoids a separate configuration file at the cost of making section position an emergent property of leaf ordering. The practical defense is to stagger `order:` values across sibling folders rather than within each independently — 10, 20, 30 for section A's pages; 40, 50, 60 for section B's — so that "first in my folder" and "first in the sidebar" are decoupled.

### Locale prefix stripping

Non-default locales are stored on disk under a locale folder (`Content/fr/...`), so a French page at `/fr/how-to/configuration/search` arrives in the flat list with `HierarchyParts` reading `["fr", "how-to", "configuration", "search"]`. If `BuildTree` recursed over those items without any preprocessing, every French page would nest under a `/fr/` root while English pages sat at the top level — two unrelated sibling trees rather than one coherent per-locale outline. The min-of-children ordering would also produce incorrect results, because "the first page in my folder" would mean something different in each language subtree.

`FilterByLocale` runs before the level-by-level recursion begins. It keeps items whose `Locale` matches the requested locale or is `null` (for locale-agnostic content), and — for non-default locales only — strips `HierarchyParts[0]` when it equals the locale code. The recursion then sees a shape identical to what the default locale sees, with the language prefix removed. The min-of-children ordering and the section-node synthesis therefore work the same way regardless of which locale is being rendered. Items carrying `Locale == null` pass through every filter unchanged, which is why redirects and feeds appear in every locale's sidebar without requiring duplicate files on disk.

## Trade-offs

**The folder layout is the source of truth.** Renaming a folder renames the sidebar header, and moving a file changes its URL — there is no alias layer that lets the filesystem shape diverge from the public URL shape. For smaller sites or new documentation projects this is a feature: the filesystem is auditable and self-documenting. For sites with long URL histories and accumulated inbound links, it shifts the burden onto redirects. The absence of an alias layer is a considered choice rather than an omission, but it is a cost that grows with site age.

**Explicit nav config was rejected as a primary mechanism.** A hand-written ordered tree duplicates information that the filesystem already encodes, and it drifts whenever authors move files without remembering to update the sidecar. The tradeoff Pennington accepts is that the `order:` staggering convention across folders is required knowledge for anyone managing section sequencing — it is implicit rather than spelled out in a config file, which makes it easier to get right at authoring time but harder to audit at a glance.

**Merge-by-`sectionLabel:` was also considered and rejected.** Grouping the sidebar by a shared front-matter label would let two unrelated folders appear to merge under one header. Beyond the confusing UX, it would contradict `ContentRoute`'s canonical-path invariant: the URL structure and the navigation tree would diverge, and that divergence is the failure mode the design most wanted to avoid.

**Section ordering is emergent, not explicit.** Because a section's position is determined by the minimum `order:` among its children, authors who want to control section ordering must think in terms of the lowest-ordered page in each group, not the group itself. That mental model shift is the real ergonomic cost, and it is why the tutorials recommend staggering `order:` values across sibling folders rather than treating each folder's range independently.

## Further reading

- Reference: [Navigation components (`TableOfContentsNavigation`, `OutlineNavigation`)](xref:reference.ui.navigation) — the UI that consumes the tree `NavigationBuilder` returns.
- How-to: [Customize the sidebar](xref:how-to.content-authoring.customize-sidebar) — the recipe that leans on the ordering rules this page explains.
- Tutorial: [Organize content with sections and areas](xref:tutorials.docsite.sections-and-areas) — the tutorial that introduces folder-driven grouping for new authors.
