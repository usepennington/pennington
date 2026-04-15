---
title: "Navigation types"
description: "The navigation construction surface — NavigationBuilder with its BuildTree and BuildNavigationInfo static methods, and the NavigationInfo, NavigationTreeItem, and BreadcrumbItem data records produced by them."
sectionLabel: "Extension Points"
order: 405060
tags: [navigation, sidebar, breadcrumbs, extension-points]
uid: reference.extension-points.navigation
---

| Type | Kind | Purpose |
|---|---|---|
| `NavigationBuilder` | sealed class | Folds flat TOC items into a tree and derives prev/next/breadcrumb metadata for a current route. |
| `BreadcrumbItem` | record | Title plus optional `ContentRoute` for one step in a root-to-current trail. |
| `NavigationInfo` | record | Per-page navigation bundle — section label, breadcrumbs, page title, previous page, next page. |
| `NavigationTreeItem` | record | One node in the sidebar tree — title, route, order, section label, selection/expansion flags, children. |

## `NavigationBuilder`

```csharp:xmldocid
T:Pennington.Navigation.NavigationBuilder
```

`NavigationBuilder` exposes two static methods that consume `IContentService.GetContentTocEntriesAsync` output; both accept an optional `locale` that filters out off-locale items and strips the locale prefix from each item's `HierarchyParts` before tree construction. Folders that contain descendants but no direct page produce auto-created section nodes whose `Title` is the kebab-to-title-case folder name and whose `Route` is an empty `ContentRoute` to signal "not navigable".

### `BuildTree`

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

Returns an `ImmutableList<NavigationTreeItem>` representing the full sidebar tree, with items grouped by `HierarchyParts`, sorted by `Order` then `Title` (case-insensitive), and deduplicated by canonical path. The `currentRoute` parameter controls the `IsSelected` and `IsExpanded` flags on returned nodes; passing `null` yields a tree where every node is unselected and collapsed.

### `BuildNavigationInfo`

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildNavigationInfo(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

Returns a single `NavigationInfo` for `currentRoute` by building the tree, flattening it depth-first to derive `PreviousPage` and `NextPage`, and walking the tree to produce the `Breadcrumbs` trail from root to current. Returns a `NavigationInfo` with `PageTitle = ""` and null neighbours when `currentRoute` is not found in the tree.

## `NavigationInfo`

```csharp:xmldocid
T:Pennington.Navigation.NavigationInfo
```

Per-page navigation bundle produced by `NavigationBuilder.BuildNavigationInfo`, consumed by `TableOfContentsNavigation`, `OutlineNavigation`, and DocSite layout components for prev/next footers and breadcrumb rendering.

### Members

| Name | Type | Description |
|---|---|---|
| `SectionName` | `string?` | The `SectionLabel` of the matched current item, or `null` when no current item was located in the tree. |
| `SectionRoute` | `ContentRoute?` | Reserved for a section-landing route; always `null` in the shipped builder. |
| `Breadcrumbs` | `ImmutableList<BreadcrumbItem>` | Root-to-current trail built by walking expanded branches of the tree; empty when the current route is not found. |
| `PageTitle` | `string` | The `Title` of the matched current item; empty string when no current item was located. |
| `PreviousPage` | `NavigationTreeItem?` | Item immediately before the current one in the depth-first flattening of the tree, or `null` at the start / when not found. |
| `NextPage` | `NavigationTreeItem?` | Item immediately after the current one in the depth-first flattening of the tree, or `null` at the end / when not found. |

## `NavigationTreeItem`

```csharp:xmldocid
T:Pennington.Navigation.NavigationTreeItem
```

One node in the sidebar tree returned by `BuildTree`; records are either auto-created section headers (non-navigable, empty `Route`, `SectionLabel = null`) or direct content entries (populated `Route`, `SectionLabel` from the source `ContentTocItem`).

### Members

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Display title; for auto-created section nodes, the kebab-case folder name converted to title case (for example, `getting-started` → `Getting Started`). |
| `Route` | `ContentRoute` | Target route for the node; auto-created section headers carry a `ContentRoute` with an empty `CanonicalPath` and `OutputFile` to signal "not navigable". |
| `Order` | `int` | Sort order used within a level; section nodes inherit the minimum `Order` of their children, and overview (area-index) items are assigned `int.MinValue` so they land first. |
| `SectionLabel` | `string?` | Section label from the source `ContentTocItem`; `null` for auto-created section headers. |
| `IsSelected` | `bool` | `true` when this node's `CanonicalPath` matches the `currentRoute` passed to `BuildTree`; always `false` on auto-created section headers. |
| `IsExpanded` | `bool` | `true` when the node is on the selected path — either selected itself or an ancestor of a selected / expanded descendant. |
| `Children` | `ImmutableList<NavigationTreeItem>` | Child nodes at the next depth, recursively built and sorted by `Order` then `Title`. |

## `BreadcrumbItem`

```csharp:xmldocid
T:Pennington.Navigation.BreadcrumbItem
```

One step in the root-to-current breadcrumb trail produced by `BuildNavigationInfo`; auto-created section headers contribute a `BreadcrumbItem` whose `Route` points at the empty placeholder `ContentRoute` from the tree node.

### Members

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Display title copied from the corresponding `NavigationTreeItem.Title`. |
| `Route` | `ContentRoute?` | Target route; `null` is permitted by the type but the shipped builder always populates it from the tree node. |

## Example

```csharp:path
examples/GettingStartedFirstPageExample/Program.cs
```

## See also

- How-to: [Customize the sidebar](xref:how-to.content-authoring.customize-sidebar)
- Related reference: [Navigation components](xref:reference.ui.navigation)
- Related reference: [Content pipeline interfaces](xref:reference.extension-points.content-pipeline)
- Background: [Navigation-tree construction](xref:explanation.routing.navigation-tree)
