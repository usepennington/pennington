---
title: "Navigation types"
description: "The navigation construction surface — NavigationBuilder with its BuildTree and BuildNavigationInfo static methods, and the NavigationInfo, NavigationTreeItem, and BreadcrumbItem data records produced by them."
sectionLabel: "Extension Points"
order: 405060
tags: [navigation, sidebar, breadcrumbs, extension-points]
uid: reference.extension-points.navigation
---

> **In this page.** `NavigationBuilder`, `NavigationInfo`, `NavigationTreeItem`, and `BreadcrumbItem`.
>
> **Not in this page.** Replacing the default sidebar — see the UI-component reference [Navigation components](xref:reference.ui.navigation).

## Summary

_**One sentence: what it is.** The builder plus three record types that fold a flat `IReadOnlyList<ContentTocItem>` into the hierarchical navigation shapes (tree, prev/next, breadcrumbs) the DocSite layout and `TableOfContentsNavigation` / `OutlineNavigation` components consume._
_**One sentence: where it lives.** Namespace `Pennington.Navigation` (`src/Pennington/Navigation/`); `NavigationBuilder` is registered by `AddPennington` and injected wherever a request needs to render navigation._

_No "why" sentences — the rationale for auto-created section nodes and locale-prefix stripping lives in the explanation [Navigation-tree construction](xref:explanation.routing.navigation-tree). TODO: confirm injection scope (singleton vs. scoped) when filling this in._

## Overview

_Four-row table keyed by type. Columns: **Type**, **Kind**, **Purpose**. One-sentence purposes only — this is the landing index for the four types bundled on this page. Order: the builder first (entry point), then the records it produces, alphabetical within the record grouping._

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

_The entry point to the navigation subsystem: two public static-shaped methods that consume `IContentService.GetContentTocEntriesAsync` output. Both methods accept an optional `locale`; when supplied, items from other locales are filtered out and the locale prefix is stripped from each item's `HierarchyParts` before tree construction._

_Implementation detail visible at the API surface: auto-created section nodes are synthesized for folders that contain descendants but no direct page, their `Title` is the kebab-to-title-case folder name, and their `Route` is an empty `ContentRoute` to signal "not navigable"._

### `BuildTree`

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

_Returns an `ImmutableList<NavigationTreeItem>` representing the full sidebar tree. Items are grouped by `HierarchyParts`, sorted by `Order` then `Title` (case-insensitive), deduplicated by canonical path. `currentRoute` controls the `IsSelected` / `IsExpanded` flags on returned nodes; passing `null` returns a tree where every node is unselected and collapsed._

### `BuildNavigationInfo`

```csharp:xmldocid
M:Pennington.Navigation.NavigationBuilder.BuildNavigationInfo(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)
```

_Returns a single `NavigationInfo` for `currentRoute`. Internally builds the tree, flattens it depth-first, locates `currentRoute` in the flat list to derive `PreviousPage` / `NextPage`, and walks the tree again to produce the `Breadcrumbs` trail from root to current. Returns a `NavigationInfo` with `PageTitle = ""` and null neighbours when `currentRoute` is not found in the tree._

## `NavigationInfo`

```csharp:xmldocid
T:Pennington.Navigation.NavigationInfo
```

_Per-page navigation bundle produced by `NavigationBuilder.BuildNavigationInfo`. Consumed by `TableOfContentsNavigation`, `OutlineNavigation`, and DocSite layout components for prev/next footers and breadcrumb rendering._

### Members

_One row per record parameter, in declaration order._

| Name | Type | Description |
|---|---|---|
| `SectionName` | `string?` | The `SectionLabel` of the matched current item, or `null` when no current item was located in the tree. |
| `SectionRoute` | `ContentRoute?` | Reserved for a section-landing route; currently always `null` in the shipped builder. TODO: confirm whether this is wired or awaiting future use. |
| `Breadcrumbs` | `ImmutableList<BreadcrumbItem>` | Root-to-current trail built by walking expanded branches of the tree; empty when the current route is not found. |
| `PageTitle` | `string` | The `Title` of the matched current item; empty string when no current item was located. |
| `PreviousPage` | `NavigationTreeItem?` | Item immediately before the current one in the depth-first flattening of the tree, or `null` at the start / when not found. |
| `NextPage` | `NavigationTreeItem?` | Item immediately after the current one in the depth-first flattening of the tree, or `null` at the end / when not found. |

## `NavigationTreeItem`

```csharp:xmldocid
T:Pennington.Navigation.NavigationTreeItem
```

_One node in the sidebar tree returned by `BuildTree`. Records can be auto-created section headers (non-navigable, empty `Route`, `SectionLabel = null`) or direct content entries (populated `Route`, `SectionLabel` from the source `ContentTocItem`)._

### Members

_One row per record parameter, in declaration order._

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Display title; for auto-created section nodes, the kebab-case folder name converted to title case (e.g. `getting-started` → `Getting Started`). |
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

_One step in the root-to-current breadcrumb trail produced by `BuildNavigationInfo`. Carries a display title and an optional route — auto-created section headers contribute a `BreadcrumbItem` whose `Route` points at the empty placeholder `ContentRoute` from the tree node._

### Members

_One row per record parameter, in declaration order._

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Display title copied from the corresponding `NavigationTreeItem.Title`. |
| `Route` | `ContentRoute?` | Target route; `null` is permitted by the type but the shipped builder always populates it from the tree node. |

## Example

_One minimal example pulled from `examples/GettingStartedFirstPageExample/Program.cs` — the canonical minimal-host wiring that injects `NavigationBuilder`, calls `BuildTree` over the aggregated TOC items, and renders a nav strip. Embedded via `:path` because `Program.cs` uses top-level statements and has no xmldocid-addressable symbol covering the registration + endpoint block._

```csharp:path
examples/GettingStartedFirstPageExample/Program.cs
```

_Reference shape for consuming `NavigationBuilder` from a minimal ASP.NET host; the DocSite template performs the equivalent wiring implicitly in `MainLayout`._

## See also

- How-to: [Customize the sidebar](xref:how-to.content-authoring.customize-sidebar)
- Related reference: [Navigation components](xref:reference.ui.navigation)
- Related reference: [Content pipeline interfaces](xref:reference.extension-points.content-pipeline)
- Background: [Navigation-tree construction](xref:explanation.routing.navigation-tree)
