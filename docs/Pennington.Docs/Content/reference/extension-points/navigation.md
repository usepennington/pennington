---
title: "Navigation types"
description: "The NavigationBuilder service plus the NavigationInfo, NavigationTreeItem, and BreadcrumbItem records it produces."
section: "extension-points"
order: 60
tags: []
uid: reference.extension-points.navigation
isDraft: true
search: false
llms: false
---

> **In this page.** `NavigationBuilder`, `NavigationInfo`, `NavigationTreeItem`, and `BreadcrumbItem`.
>
> **Not in this page.** Replacing the default sidebar is a UI-component topic covered separately.

## Summary

The `Pennington.Navigation` types convert flat `ContentTocItem` lists into a hierarchical sidebar tree with prev/next and breadcrumb metadata.
They live in `src/Pennington/Navigation/` and are consumed by Razor components such as `TableOfContentsNavigation` and `OutlineNavigation`.

## `NavigationBuilder`

### Declaration

```csharp:xmldocid
T:Pennington.Navigation.NavigationBuilder
```

### Methods

| Name | Returns | Description |
|---|---|---|
| `BuildTree(IReadOnlyList<ContentTocItem> items, ContentRoute? currentRoute = null, string? locale = null)` | `ImmutableList<NavigationTreeItem>` | Builds a hierarchical tree from flat TOC items; filters by `locale` and strips the locale prefix from hierarchy parts when supplied. |
| `BuildNavigationInfo(IReadOnlyList<ContentTocItem> items, ContentRoute currentRoute, string? locale = null)` | `NavigationInfo` | Builds the tree, flattens it depth-first, and returns previous/next siblings, breadcrumbs, and current page metadata for `currentRoute`. |

### `BuildTree`

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

Returns nodes sorted by `Order`, then `Title` (ordinal case-insensitive). Auto-creates section folder nodes when descendants exist without a direct item at that depth. Deduplicates by `Route.CanonicalPath`.

### `BuildNavigationInfo`

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildNavigationInfo(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

`PreviousPage` and `NextPage` come from a depth-first flattening of the tree. `Breadcrumbs` trace the expanded path from root to the selected item.

## `NavigationInfo`

### Declaration

```csharp:xmldocid
T:Pennington.Navigation.NavigationInfo
```

### Properties

| Name | Type | Description |
|---|---|---|
| `Breadcrumbs` | `ImmutableList<BreadcrumbItem>` | Ordered trail from root to the current page. |
| `NextPage` | `NavigationTreeItem?` | Next sibling in depth-first order, or `null` at the end of the tree. |
| `PageTitle` | `string` | Title of the current node, or empty string when the route is not found. |
| `PreviousPage` | `NavigationTreeItem?` | Previous sibling in depth-first order, or `null` at the start of the tree. |
| `SectionName` | `string?` | `Section` value of the current node. |
| `SectionRoute` | `ContentRoute?` | Always `null` as of this release. |

## `NavigationTreeItem`

### Declaration

```csharp:xmldocid
T:Pennington.Navigation.NavigationTreeItem
```

### Properties

| Name | Type | Description |
|---|---|---|
| `Children` | `ImmutableList<NavigationTreeItem>` | Nested descendant nodes, already sorted by `Order` then `Title`. |
| `IsExpanded` | `bool` | `true` when this node is selected or any descendant is selected or expanded. |
| `IsSelected` | `bool` | `true` when this node's canonical path matches the `currentRoute` passed to `BuildTree`. |
| `Order` | `int` | Sort key; auto-created section nodes use the minimum `Order` of their children. |
| `Route` | `ContentRoute` | Canonical route; auto-created section nodes carry an empty `CanonicalPath` and `OutputFile`. |
| `Section` | `string?` | `Section` value from the source `ContentTocItem`; `null` on auto-created section nodes. |
| `Title` | `string` | Display title; auto-created section nodes format the folder name (kebab-case to title case). |

## `BreadcrumbItem`

### Declaration

```csharp:xmldocid
T:Pennington.Navigation.BreadcrumbItem
```

### Properties

| Name | Type | Description |
|---|---|---|
| `Route` | `ContentRoute?` | Destination route for the crumb; may be `null` for non-navigable section nodes. |
| `Title` | `string` | Display title of the crumb. |

## See also

- Related reference: [Content pipeline interfaces](/reference/extension-points/content-pipeline)
- Related reference: [Routing types](/reference/extension-points/routing)
- UI: [Navigation components](/reference/ui/navigation)
