---
title: "Navigation components"
description: "`TableOfContentsNavigation` and `OutlineNavigation` — parameters, slots, and how they bind to `NavigationInfo`."
section: "ui"
order: 10
tags: []
uid: reference.ui.navigation
isDraft: true
search: false
llms: false
---

> **In this page.** `TableOfContentsNavigation` and `OutlineNavigation` — parameters, slots, and how they bind to `NavigationInfo`.
>
> **Not in this page.** Rendering custom navigation shapes (a Razor authoring topic).

## Summary

- Two Razor components in `Pennington.UI` for rendering site navigation and in-page outline.
- Namespace `Pennington.UI.Components.Navigation`; source files `src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor` and `OutlineNavigation.razor`.
- Consume data produced by `NavigationBuilder` (`Pennington.Navigation`) — `ImmutableList<NavigationTreeItem>` for the TOC and `NavigationInfo.PageTitle` / heading scan for the outline.

## `TableOfContentsNavigation`

### Declaration

```razor
<TableOfContentsNavigation TableOfContents="..." />
```

- Component file: `src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor`.
- Renders a `<nav><ul>` of top-level `NavigationTreeItem` entries ordered by `Order`, with one level of children ordered by `Order`.
- Entries whose `Route.CanonicalPath.Value` is `""` render as a section header `<div>`; other entries render as `<a>` with `data-current="true|false"` bound to `NavigationTreeItem.IsSelected`.
- Child entries whose `Route.CanonicalPath.Value` is `""` are filtered out of the nested list.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `TableOfContents` | `ImmutableList<NavigationTreeItem>?` | `null` | Top-level navigation tree; when `null` the component renders nothing. |
| `Section` | `string?` | `null` | Declared but not referenced in the component markup. |
| `ListGapClass` | `string` | `"gap-4"` | Utility class applied to the root `<ul>` alongside `flex flex-col`. |
| `ChildListClass` | `string` | `"mt-4"` | Class applied to the nested child `<ul>`. |
| `SectionHeaderStructureClass` | `string` | `"font-display font-medium first:pt-0"` | Structural classes for section header rows and parent anchors (entries with children). |
| `SectionHeaderColorClass` | `string` | `"text-base-900 dark:text-base-50"` | Color classes for section header rows and parent anchors. |
| `LinkStructureClass` | `string` | `"block text-sm w-full border-l pl-3.5 py-1.5"` | Structural classes for child (leaf) anchors. |
| `LinkColorClass` | `string` | (see source) | Color / state classes for child anchors; includes `data-[current=true]` variants. |
| `RootLinkStructureClass` | `string` | `"block w-full py-1"` | Structural classes for root-level anchors that have no children. |
| `RootLinkColorClass` | `string` | (see source) | Color / state classes for root-level anchors that have no children; includes `data-[current=true]` variants. |

### Slots / RenderFragments

- No public `RenderFragment` parameters.
- Internal private `RenderFragment TocEntry(NavigationTreeItem)` emits each `<li>`; not overridable from outside.

### Binding to `NavigationInfo`

- Not bound to `NavigationInfo` directly. The component takes an `ImmutableList<NavigationTreeItem>` — typically the output of `NavigationBuilder.BuildTree(items, currentRoute?, locale?)`.
- `NavigationInfo` (record: `SectionName`, `SectionRoute`, `Breadcrumbs`, `PageTitle`, `PreviousPage`, `NextPage`) is produced by `NavigationBuilder.BuildNavigationInfo(...)` from the same tree; pass the tree to `TableOfContentsNavigation` and pass the `NavigationInfo` separately to whichever component renders previous/next / breadcrumbs.
- Each `NavigationTreeItem` contributes `Title` (anchor text), `Route.CanonicalPath.Value` (href), `Order` (sort key), `IsSelected` (drives `data-current`), and `Children` (one nested level).

## `OutlineNavigation`

### Declaration

```razor
<OutlineNavigation ContentSelector="article" />
```

- Component file: `src/Pennington.UI/Components/Navigation/OutlineNavigation.razor`.
- Renders an empty `<ul>` inside a container `<div data-role="page-outline">`; the list entries are populated client-side by JavaScript that scans headings in the element matched by `ContentSelector`.
- Emits a sibling `<div data-role="page-outline-highlighter">` used by the client script for scroll-position highlighting.
- Class name tokens for generated links are forwarded to the `<ul>` via `data-outline-link-structure-class` and `data-outline-link-color-class` attributes; the JavaScript reads these when building link markup.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string` | `""` | **`[EditorRequired]`.** CSS selector the client script scans for headings (e.g. `"article"`). Placed on the root container as `data-content-selector`. |
| `Title` | `string` | `"On This Page"` | Declared but not referenced in the component markup. |
| `ContainerStructureClass` | `string` | `"border-l border-base-200 dark:border-base-800"` | Structural classes merged onto the root container `<div>`. |
| `ContainerColorClass` | `string` | `""` | Color classes merged onto the root container `<div>`. |
| `ListStructureClass` | `string` | `"list-none pl-4"` | Structural classes applied to the `<ul>`. |
| `ListColorClass` | `string` | `"text-neutral-500 dark:text-neutral-400"` | Color classes applied to the `<ul>`. |
| `OutlineLinkStructureClass` | `string` | `"py-1 ml-[calc(-1*(4em-1px))] pl-[calc(4em+1px)] "` | Forwarded via `data-outline-link-structure-class`; applied to client-generated links. |
| `OutlineLinkColorClass` | `string` | (see source) | Forwarded via `data-outline-link-color-class`; includes `data-[selected=true]` variants. |

### Slots / RenderFragments

- No `RenderFragment` parameters.
- No server-rendered list items — all `<li>` / `<a>` markup is produced client-side by JavaScript scanning `ContentSelector`.

### Binding to `NavigationInfo`

- Not bound to `NavigationInfo`. The component operates on rendered DOM, not on the `OutlineEntry[]` produced by `IContentRenderer`.
- Use `NavigationInfo.PageTitle` in surrounding layout chrome; the in-page outline itself derives from whatever HTML `ContentSelector` resolves to.

## Example

```csharp:xmldocid,bodyonly
M:UserInterfaceExample.ContentHelper.GetNavigationTocAsync(System.String)
```

- Pass the returned tree to `<TableOfContentsNavigation TableOfContents="@_tableOfContents" />`; pair it with `<OutlineNavigation ContentSelector="article" />` alongside the rendered article.

## See also

- Related reference: [Navigation types](/reference/extension-points/navigation)
- How-to: [Structure a site with sections and order](/how-to/configuration/sections-and-order)
- Background: [Navigation-tree construction](/explanation/routing/navigation-tree)
