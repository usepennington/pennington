---
title: "Navigation-tree construction"
description: "How Pennington folds a flat list of ContentTocItems into the sidebar tree using HierarchyParts, folder-derived sections, and min-of-children ordering."
uid: explanation.routing.navigation-tree
order: 20
sectionLabel: "Routing and Navigation"
tags: [navigation, routing, hierarchy]
---

> **In this page.** _One sentence, paraphrased from the Covers line: how `NavigationBuilder` folds a flat list of `ContentTocItem`s into a tree via `HierarchyParts`, what happens when a folder has no `index.md`, and how locale prefixes are stripped before ordering. Keep the sentence descriptive — not instructional._
>
> **Not in this page.** _One sentence, paraphrased from the Does-not-cover line: pointer readers to the Reference page that catalogs the rendering components (`TableOfContentsNavigation` / `OutlineNavigation`) and to the UI reference for `NavigationTreeItem` members._

## The question

_One sentence, phrased as a real reader's question — something like: "Why does the Pennington sidebar match my folder layout even for folders I never named in front matter, and where does the ordering come from?" Resist answering here; the whole page is the answer._

## Context

_Three to five sentences. Set up the tension: every content service emits a flat list of `ContentTocItem`s, each carrying a `Route`, a `Title`, an `Order`, a `SectionLabel`, a `Locale`, and a pre-split `HierarchyParts` array (the canonical path segmented on `/`). The sidebar, however, is a tree with folders and selected-branch state. Some authoring workflows solve that by requiring a hand-written `nav.yml` that names each section, each ordering, and each label; Pennington's design target was "the filesystem is already the outline — honour it." Introduce the two-signal model: **folder structure** supplies the tree shape, **front matter** supplies leaf ordering and the breadcrumb label, and there is no third config surface in between. End by previewing that this page walks through the fold from flat list to tree, the rules for sections that lack a direct content file, and what `locale` stripping does to the algorithm._

> [!NOTE]
> _Optional callout. One or two sentences clarifying the most common confusion exposed during Wave 2/3 authoring: the **folder name** drives the sidebar grouping and the displayed section header, while the `sectionLabel:` front-matter key only controls the label shown in breadcrumbs and the prev/next strip. Authors who want a different sidebar header rename the folder (or add a `FormatSectionTitle`-friendly kebab-case segment); they do not reach for `sectionLabel:`._

## How it works

_Four subsections. Narrate the algorithm — do not list APIs. Budget the mechanism subsections roughly equally so "Sections without a direct content file" gets real prose rather than a footnote._

### HierarchyParts folds the flat TOC into a tree

_Two or three paragraphs. Each `ContentTocItem` carries a `HierarchyParts` array, which is the canonical path split on `/` (for example `["how-to", "configuration", "search"]` for `/how-to/configuration/search`). `NavigationBuilder.BuildTree` recurses one depth at a time: at each level it selects items whose `HierarchyParts.Length == depth + 1` and whose prefix matches `parentParts`, orders them by `Order` then case-insensitive `Title`, and dedupes by canonical path to defend against two content sources registering overlapping subtrees. The recursion is level-by-level, not item-by-item, which is what makes sibling ordering work across sources that were never aware of each other. Mention the "overview item" special case at depth 0 — a TOC entry whose `HierarchyParts.Length == 0` is treated as the area's landing page (its hierarchy was already stripped by `GetTocItemsForAreaAsync`) and is injected at the top of the tree with `Order = int.MinValue` so it anchors above the other roots regardless of its authored `order:`._

```csharp:xmldocid
T:Pennington.Content.ContentTocItem
```

_One sentence after the fence reinforcing which field each input provides: `HierarchyParts` shapes the tree, `Order` and `Title` sort siblings, `SectionLabel` surfaces only in prev/next and breadcrumbs, `Locale` feeds the filter below._

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

_After the fence: point out that `currentRoute` is what marks items `IsSelected` and propagates `IsExpanded` up the ancestor chain, so the same tree powers both "where am I" highlighting and the collapsed/expanded state of the surrounding folders. The method returns an `ImmutableList<NavigationTreeItem>` — the whole tree is a value, not a mutable model the UI binds to._

### Sections without a direct content file

_Two or three paragraphs — this is the subsection Wave 2/3 reviewers kept tripping on, so give it room. When `BuildLevel` finds deeper descendants under a hierarchy segment that has no direct item at the current depth (a folder like `/how-to/configuration/` with children but no `configuration/index.md`), it synthesizes a non-navigable "section node" on the fly: title from `FormatSectionTitle(folderName)` which kebab-to-title-cases the segment ("getting-started" → "Getting Started"), an empty `ContentRoute` so the UI renders it as a header rather than a link, and `IsExpanded` set from whether any descendant is selected. This is the mechanism that lets an author drop markdown files into `/how-to/deployment/` without creating a `deployment/index.md` first and still see "Deployment" appear as a sidebar heading._

_Call out explicitly that this is the **grouping** signal, distinct from the per-page `sectionLabel:` front-matter key. Grouping = which subfolder a file lives in. `sectionLabel:` = the label used in breadcrumbs and the prev/next footer for leaf pages. Two files with identical `sectionLabel: "Advanced"` values sitting in different folders will render under two different sidebar headers (named after their folders), not one merged "Advanced" group. This is deliberate: merging by label would reintroduce the "two config surfaces argue over the tree shape" failure mode the filesystem-driven design was built to avoid._

```csharp:xmldocid
T:Pennington.Navigation.NavigationTreeItem
```

_After the fence, note that the synthesized section node and a real leaf share the same record shape — the UI tells them apart by `Children.Count > 0 && Route.CanonicalPath.Value == ""`, not by a discriminator field. That keeps the rendering component simple and means section headers are first-class citizens of the tree, not a second structure glued on._

### Ordering: min-of-children with alphabetic tie-break

_Two paragraphs. Direct leaves at a level sort by their own `Order` then by `Title` (case-insensitive ordinal). Synthesized section nodes have no authored `order:` of their own, so `BuildLevel` assigns them `Order = children.Min(c => c.Order)` — the section sorts as if it were whichever of its children sorts first. That means authors control section ordering the same way they control leaf ordering: by setting `order:` on the **leaf pages inside the folder**, not on a section definition somewhere else._

_The practical consequence — and the one worth flagging to authors — is that sibling sections interleave by the smallest `order:` in each. If "Getting Started" has a leaf with `order: 10` and "Deployment" has a leaf with `order: 20`, the sidebar reads Getting Started, then Deployment. If a second author then adds a page with `order: 5` to Deployment (say, because they wanted it at the top of its own folder), the whole Deployment section jumps above Getting Started. The recommended defense is to stagger `order:` values across folders — 10/20/30 for one section, 40/50/60 for the next — so "first page in my folder" and "first section in the sidebar" are decoupled. When two items tie on `Order`, the case-insensitive title sort is the stable fallback._

### Locale prefix stripping for ordering

_Two paragraphs. Non-default locales land on disk under a locale folder (`Content/fr/...`), so a French page's canonical path is `/fr/how-to/configuration/search` and its `HierarchyParts` reads `["fr", "how-to", "configuration", "search"]`. If `BuildTree` recursed over that untouched, every French page would sit under a `/fr/` root with English pages as an unrelated sibling tree, and the ordering invariants above would silently break because "first page in my folder" means a different thing in each language tree._

_`FilterByLocale` runs before the level-by-level recursion. It keeps items whose `Locale` matches the requested locale (or is `null` for locale-agnostic content), and — only for non-default locales — strips `HierarchyParts[0]` when it matches the locale code. The recursion then sees exactly the shape an English author would see, minus the language prefix, so the min-of-children ordering and the section-node synthesis work identically regardless of which locale is being rendered. Items with `Locale == null` pass through every filter unchanged, which is why redirects and feeds show up in every locale's sidebar without duplication on disk._

## Trade-offs

_Required section. Two to four bullets. Name real costs — not "it's more complex" throwaways._

- **Cost — the folder layout is the source of truth.** _Expand in one sentence: renaming a folder renames the sidebar header, moving a file changes its URL, and there is no "alias" layer that lets the filesystem shape diverge from the public shape. For small sites this is a feature; for very large sites with historical URL obligations, it means redirects carry the weight that an alias system would._
- **Alternative considered — explicit nav config (`nav.yml`).** _One or two sentences: a hand-written ordered tree was rejected because it duplicates information already encoded in the filesystem and drifts whenever authors move files without updating the sidecar. Pennington instead expects `order:` on the leaf pages and lets min-of-children derive the rest._
- **Alternative considered — merge-by-`sectionLabel:`.** _One sentence: grouping the sidebar by a shared label was rejected because it lets two unrelated folders collide under one header and hides the folder layout from the URL shape, which contradicts `ContentRoute`'s canonical-path invariant._
- **Consequence — authors stagger `order:` across sibling folders.** _One sentence: because section ordering = `min(order:)` of children, an author who wants "group A before group B" must ensure A's lowest-ordered child has a lower `order:` than B's — hence the 10/20/30 vs 40/50/60 convention the tutorials recommend._

## Further reading

_Two to four cross-quadrant links, one per line. Do NOT link to the sibling explanation page on URL paths — that is auto-generated as prev/next. Suggested set:_

- Reference: [Navigation components (`TableOfContentsNavigation`, `OutlineNavigation`)](xref:reference.ui.navigation) — the UI that consumes the tree `NavigationBuilder` returns.
- How-to: [Customize the sidebar](xref:how-to.content-authoring.customize-sidebar) — the recipe that leans on the ordering rules this page explains.
- How-to: [Organize content with sections and areas](xref:tutorials.docsite.sections-and-areas) — the tutorial that introduces folder-driven grouping for new authors. _TODO: swap to a how-to link if a Wave-2 page on sidebar organization exists._
- External: _TODO — cite the MyLittleContentEngine or VitePress sidebar-derivation docs if an equivalent write-up exists; otherwise drop this bullet._
