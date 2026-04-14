---
title: "Navigation-tree construction"
description: "How NavigationBuilder folds flat ContentTocItems into a tree via HierarchyParts, what happens when a section has no direct content, and how locale prefixes are stripped for ordering."
section: "routing"
order: 20
tags: []
uid: explanation.routing.navigation-tree
isDraft: true
search: false
llms: false
---

> **In this page.** How `NavigationBuilder` folds flat `ContentTocItem`s into a tree via `HierarchyParts`, what happens when a section has no direct content, and how locale prefixes are stripped for ordering.
>
> **Not in this page.** The UI that renders the tree — see the Reference entries for `TableOfContentsNavigation` and `OutlineNavigation`.

## The question

Why does Pennington build its sidebar from a flat list of TOC entries instead of modelling navigation as a first-class tree up front?

## Context

- Content comes from multiple independent sources (`MarkdownContentService<T>`, `RazorPageContentService`, programmatic services). Each yields flat `ContentTocItem`s, not nested nodes.
- A nested source-of-truth would force every source to coordinate on parent pointers. Flat entries let sources stay oblivious to one another.
- The URL itself already encodes the hierarchy — the slash-separated canonical path is the shape the sidebar wants to reflect.
- Localization adds a wrinkle: a page's `CanonicalPath` begins with a locale segment (`/de/guides/install`) that is positional, not semantic, and must not appear as a sidebar folder.
- So the tree exists only at read-time, rebuilt from whatever the pipeline currently knows. This page explains that fold.

## How it works

### HierarchyParts as the coordinate system

- Each `ContentTocItem` carries `HierarchyParts: string[]` — the canonical path split on `/` with empty segments removed (produced in `MarkdownContentService.GetContentTocEntriesAsync` and `RazorPageContentService`).
- A page at `/guides/routing/locales` has `HierarchyParts = ["guides", "routing", "locales"]`; the array length equals the page's depth in the tree.
- `HierarchyParts` is the only cross-source coordinate the builder uses. No parent pointers, no source identity — just the path.

```csharp:xmldocid
T:Pennington.Content.ContentTocItem
```

### The recursive fold

- `NavigationBuilder.BuildTree(items, currentRoute?, locale?)` seeds recursion at `parentParts = []`.
- `BuildLevel` finds items whose `HierarchyParts.Length == depth + 1` and whose leading parts equal `parentParts`. Those are *direct* children of the current level.
- For each direct child, the builder recurses with that item's `HierarchyParts` as the new `parentParts`, collecting its subtree.
- Sibling items are ordered by `Order` ascending, then by `Title` with `StringComparer.OrdinalIgnoreCase` — the same comparator used throughout so casing never introduces ordering drift.
- A `DistinctBy(CanonicalPath, OrdinalIgnoreCase)` pass protects against two sources registering the same route (the pipeline emits a diagnostic for that misconfiguration; the dedup keeps the sidebar from showing the page twice anyway).

```csharp:xmldocid
T:Pennington.Navigation.NavigationBuilder
```

### Synthesizing section nodes for empty folders

- A section can have descendants without having a page of its own. `Content/guides/routing/` may contain `locales.md` and `xref.md` but no `routing.md`.
- After gathering direct items, `BuildLevel` scans for parts at `HierarchyParts[depth]` that are mentioned by deeper descendants but have no matching direct item. Each such part becomes a synthesized node.
- Synthesized nodes carry an empty `ContentRoute` (`CanonicalPath = ""`, `OutputFile = ""`) — deliberately unroutable. The UI reads the empty path as "this header is not a link."
- The title is derived from the folder name by `FormatSectionTitle`, which splits on `-` and title-cases each word. `getting-started` becomes `Getting Started`.
- A synthesized node has no `Order` of its own, so it inherits the minimum `Order` of its children. A folder sorts next to its earliest page — not at the end of the list, not at zero.
- Synthesized nodes are appended *before* direct items in construction order, then the final `OrderBy(Order).ThenBy(Title)` pass reshuffles both kinds into a single list. Synthesized and real items interleave by order.

### Selection and expansion

- `IsSelected` is set on the single node whose route matches `currentRoute` via `UrlPath.Matches` (which normalizes `/index.html` to directory form).
- `IsExpanded` is set on any ancestor of the selected node, or on any node whose subtree contains an expanded descendant. Expansion flows upward from the leaf; the sidebar opens exactly the branches needed to reveal the active page.

### Locale-aware ordering

- When a request is localized, `CanonicalPath` for non-default locale pages begins with the locale segment (`/de/guides/install`). Its `HierarchyParts[0]` is `"de"`.
- Naively folded, that produces a `De` folder at the root of every non-English sidebar — positional garbage masquerading as a section.
- `BuildTree` accepts an optional `locale`. When present, `FilterByLocale` runs first: it drops items whose `Locale` is set and does not match, keeps locale-agnostic items (`Locale == null`), and for kept items whose `HierarchyParts[0]` equals the locale, it rewrites the record with that segment removed.
- The fold then sees `["guides", "install"]` for German and English alike. The structural shape of the sidebar is locale-independent; ordering rules apply to the stripped paths, so `Order` values compare like-with-like across translations.

### Prev/next via depth-first flatten

- `BuildNavigationInfo` builds the tree, then flattens it depth-first. A node precedes its children, which precede its next sibling.
- `PreviousPage` and `NextPage` are simply the neighbours in that flat list, indexed by the current route.
- Synthesized section nodes appear in the flat list but are not selectable; they are skipped by the UI's prev/next logic because their route path is empty.

## Trade-offs

- **Cost — the tree is rebuilt on every call.** There is no persistent navigation graph; each request that needs a sidebar walks the full TOC. This is cheap in practice (TOCs are small and fully in-memory) and it means file edits surface immediately through the file-watched content services without cache invalidation.
- **Alternative considered — sources declare nested structure.** Each `IContentService` could emit a pre-built subtree. Rejected: it couples sources to one another (who owns the root?), duplicates the URL hierarchy as a second source of truth, and makes cross-source folders (e.g. a `releases/` folder composed of a Markdown source *and* a programmatic source) impossible without a separate merge step.
- **Alternative considered — build the tree once at pipeline `Generate` time.** Rejected: the dev host serves navigation live, and the same builder runs in-process on every request so that live edits are reflected without a rebuild step. Keeping one code path for dev and build is a deliberate invariant elsewhere in the engine, and the navigation fold follows it.
- **Consequence — `HierarchyParts` must stay faithful to `CanonicalPath`.** Any `IContentService` that invents a different hierarchy (e.g. group pages under a virtual `api/` folder that is not in the URL) must populate `HierarchyParts` itself. The builder does not re-derive the hierarchy from titles, sections, or anything other than the array it was given.

## Further reading

- Reference: [Navigation types](/reference/extension-points/navigation)
- Reference: [Navigation components](/reference/ui/navigation)
- How-to: [Customize the sidebar](/how-to/content-authoring/customize-sidebar)
- Explanation: [Locale-aware URLs and content fallback](/explanation/localization/urls-and-fallback)
