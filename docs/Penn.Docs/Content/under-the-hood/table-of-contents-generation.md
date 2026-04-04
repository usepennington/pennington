---
title: "Table of Contents Generation"
description: "How Penn builds hierarchical navigation trees, breadcrumbs, and prev/next links from flat content"
uid: "penn.under-the-hood.table-of-contents-generation"
order: 3020
---

Documentation sites live and die by their navigation. Penn's table of contents system takes a flat list of content pages and produces a hierarchical navigation tree, breadcrumbs, and prev/next links. It handles section auto-creation, ordering, and active-state tracking. All from data your content services already provide.

No configuration files for navigation structure. No manually maintained sidebar definitions. The tree builds itself from your content, and you control it with front matter and folder structure.

## ContentTocItem: The Input

Every `IContentService` provides table of contents entries through `GetContentTocEntriesAsync()`. Each entry is a `ContentTocItem`:

```csharp:xmldocid
T:Penn.Content.ContentTocItem
```

Six fields:

- **`Title`**: Display name in navigation. Pages without titles are excluded from navigation entirely.
- **`Route`**: The `ContentRoute` with canonical URL and output file path.
- **`Order`**: Sorting weight. Lower numbers appear first.
- **`HierarchyParts`**: An array of strings that defines position in the tree. This is the key to everything.
- **`Section`**: Optional section grouping (for multi-section sites).
- **`Locale`**: Optional locale identifier.

### HierarchyParts: The Tree Coordinates

`HierarchyParts` is the mechanism that turns flat content into a tree. For a markdown file at `Content/guides/getting-started/installation.md` with base URL `/docs`, the content service might produce:

```
HierarchyParts = ["guides", "getting-started", "installation"]
```

The array length determines the depth, and the values at each position determine the parent-child relationships. Think of it as a path through the tree, where each element names a node.

Content services have full control over how they generate hierarchy parts. The markdown service uses folder structure. An API reference service might use namespaces. A blog service might use date-based groupings. The `NavigationBuilder` does not care where the parts come from -- it just builds a tree from them.

## NavigationBuilder: The Tree Constructor

`NavigationBuilder` is the engine that transforms a flat `IReadOnlyList<ContentTocItem>` into an `ImmutableList<NavigationTreeItem>`:

```csharp:xmldocid
T:Penn.Navigation.NavigationBuilder
```

### Building the Tree

The algorithm is recursive and works level by level:

1. **Find items at the current depth.** For the root level (depth 0), find all items with `HierarchyParts.Length == 1`. For depth 1, find items with length 2 whose first part matches the parent.

2. **Auto-create section nodes.** Find hierarchy parts at this depth that have descendants but no direct item. For example, if there are items at `["guides", "installation"]` and `["guides", "configuration"]` but nothing at just `["guides"]`, the builder creates a non-navigable section node for "guides."

3. **Sort.** Items are ordered by `Order`, then alphabetically by `Title`.

4. **Recurse.** For each item at this level, build its children by descending one level deeper.

### Auto-Created Section Nodes

When a folder exists in the hierarchy but has no corresponding page, the builder creates a section node automatically:

```
Content/
  guides/                   <-- no index.md here
    installation.md
    configuration.md
```

This produces a tree like:

```
Guides (non-navigable section)
  +-- Installation
  +-- Configuration
```

The section title is generated from the folder name: kebab-case is converted to title case (`getting-started` becomes "Getting Started"). The section's order is inherited from its lowest-ordered child.

These auto-created nodes have empty routes -- they are not clickable. They exist purely to organize the navigation visually.

### NavigationTreeItem

Each node in the tree is a `NavigationTreeItem`:

```csharp:xmldocid
T:Penn.Navigation.NavigationTreeItem
```

The record carries:

- **`Title`** and **`Route`**: What to display and where to link.
- **`Order`**: For sorting.
- **`Section`**: Optional section grouping.
- **`IsSelected`**: Whether this is the current page.
- **`IsExpanded`**: Whether this node's subtree should be visible (true if any descendant is selected).
- **`Children`**: An `ImmutableList<NavigationTreeItem>` of child nodes.

`IsExpanded` is the key to collapsible navigation. When you visit `/guides/installation/`, the "Guides" section node gets `IsExpanded = true` because one of its descendants is selected. The "API Reference" section stays collapsed.

## NavigationInfo: The Full Picture

For a given page, `BuildNavigationInfo` provides everything the layout needs:

```csharp:xmldocid
T:Penn.Navigation.NavigationInfo
```

This includes:

- **`SectionName`**: The section the current page belongs to.
- **`Breadcrumbs`**: A list of `BreadcrumbItem` records from root to current page.
- **`PageTitle`**: The current page's title.
- **`PreviousPage`** and **`NextPage`**: For sequential navigation.

### Prev/Next Computation

Previous and next pages are determined by flattening the tree depth-first:

```
Home                    [0]
Getting Started         [1]  (section)
  Installation          [2]
  First Steps           [3]
Guides                  [4]  (section)
  Basic Usage           [5]
  Advanced Features     [6]
```

If you are on "Installation" (index 2), previous is "Getting Started" (index 1) and next is "First Steps" (index 3). The flattening respects the tree order, so prev/next follows the same path a reader would take going through the docs linearly.

Section nodes *are* included in the flattened list. If a section node is navigable (has a route), it participates in prev/next. Auto-created sections with empty routes are also in the list -- whether your UI skips them is a presentation decision.

### Breadcrumbs

Breadcrumbs are computed by walking the tree from root to the selected node:

```
Home > Getting Started > Installation
```

Each breadcrumb is a `BreadcrumbItem` with a title and optional route. The algorithm follows the `IsExpanded` trail: starting from the root, it enters each expanded node until it finds the selected one.

## Ordering

The ordering system is straightforward:

1. **Explicit order**: Set `order` in your front matter. Lower numbers sort first.
2. **Auto-created sections**: Inherit the minimum `Order` value from their children.
3. **Alphabetical fallback**: Items with the same order are sorted by title (case-insensitive).

```yaml
---
title: "Installation Guide"
order: 100
---
```

A practical ordering scheme:

| Order Range | Purpose |
|---|---|
| 0-99 | Top-level landing pages |
| 100-199 | Getting Started section |
| 200-299 | Core concepts |
| 1000-1999 | Reference docs |
| 3000-3999 | Under the Hood (you are here) |
| 9000+ | Appendices, changelog |

Leave gaps between items so you can insert new pages without renumbering everything. If you have learned anything from BASIC line numbers, it is this.

## Multi-Section Sites

Penn supports multiple independent content sources, each with their own section. For example, a project might have:

- `/docs/` -- Documentation (section: "docs")
- `/blog/` -- Blog posts (section: "blog")
- `/api/` -- API reference (section: "api")

Each section's content service provides `ContentTocItem` records with appropriate `Section` values. The `NavigationBuilder` processes all items together, and the `Section` field lets the UI filter or group navigation by section.

## A Complete Example

Given this content structure:

```
Content/
+-- index.md              (order: 1, title: "Home")
+-- getting-started/
|   +-- installation.md   (order: 110, title: "Installation")
|   +-- first-steps.md    (order: 120, title: "First Steps")
+-- guides/
|   +-- index.md          (order: 200, title: "User Guides")
|   +-- basic-usage.md    (order: 210, title: "Basic Usage")
|   +-- advanced.md       (order: 220, title: "Advanced Features")
+-- api/
    +-- overview.md       (order: 1000, title: "API Overview")
    +-- content-item.md   (order: 1010, title: "ContentItem")
```

The `NavigationBuilder` produces:

```
Home                          (order: 1, navigable)
Getting Started               (order: 110, auto-created section, not navigable)
  +-- Installation            (order: 110, navigable)
  +-- First Steps             (order: 120, navigable)
User Guides                   (order: 200, navigable -- has index.md)
  +-- Basic Usage             (order: 210, navigable)
  +-- Advanced Features       (order: 220, navigable)
Api                           (order: 1000, auto-created section, not navigable)
  +-- API Overview            (order: 1000, navigable)
  +-- ContentItem             (order: 1010, navigable)
```

Key observations:

- "Getting Started" is auto-created because there is no `getting-started/index.md`. Its order (110) comes from its lowest child.
- "User Guides" is navigable because `guides/index.md` exists. It is both a page and a container.
- "Api" is auto-created with title derived from the folder name `api` (title-cased to "Api"). If you wanted "API Reference" instead, you would either add an `index.md` with that title or -- if Penn supports folder metadata -- configure it there.

When visiting `/guides/basic-usage/`:

```
Breadcrumbs: Home > User Guides > Basic Usage
Previous: User Guides
Next: Advanced Features
IsExpanded: User Guides = true, Getting Started = false, Api = false
IsSelected: Basic Usage = true
```

The whole thing is computed from flat data. No configuration file describes this tree structure. It emerges from the hierarchy parts, order values, and the recursive build algorithm. Change your folder structure, and the navigation updates automatically.
