---
title: "Table of Contents Generation"
description: "How the navigation tree is assembled from flat ContentTocItem lists — hierarchy inference from URL segments, auto-created folder nodes, sort order algorithm, locale filtering and prefix stripping, the flatten-for-prev/next algorithm, and breadcrumb path finding via depth-first traversal"
uid: "penn.explanation.table-of-contents-generation"
order: 10
---

Explain how `NavigationBuilder` assembles a tree from flat `ContentTocItem` lists. The input is a list of items each with `HierarchyParts` (string array acting as tree coordinates — derived from URL path segments). The `BuildLevel` algorithm recursively groups items by their first hierarchy part, creating a tree level for each group. When a directory has child pages but no index page, the builder auto-creates a section node using the folder name (kebab-case converted to title-case). Walk through the sort algorithm: items with explicit `Order` values sort numerically first, then items without order sort alphabetically by title. Explain locale filtering — when `BuildTree` receives a locale parameter, it strips locale prefixes from routes and filters out items from other locales. Discuss prev/next link generation via depth-first flattening of the tree (skipping section-only nodes). Explain breadcrumb generation: walk the tree from root to the current page following the expanded path. Include a worked example with a concrete directory tree, the resulting `ContentTocItem` list, and the final navigation tree.
