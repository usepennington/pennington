---
title: "Navigation components"
description: "Parameters, slots, and NavigationInfo bindings for TableOfContentsNavigation and OutlineNavigation."
uid: reference.ui.navigation
order: 1
sectionLabel: UI Components
tags: [ui, navigation, razor, components]
---

`TableOfContentsNavigation` and `OutlineNavigation` are the two Razor components in `Pennington.UI` that render, respectively, the sidebar page tree and the floating in-page heading outline. Both live in namespace `Pennington.UI.Components.Navigation` and are consumed by `Pennington.DocSite`'s `MainLayout`.

## `TableOfContentsNavigation`

### Declaration

```razor:symbol
src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor
```

Renders an ordered `<nav><ul>` of `NavigationTreeItem` entries, recursing one level into each entry's `Children` collection. Root entries with an empty `Route.CanonicalPath` render as plain section headers; entries with a path render as anchor links carrying `data-current="true"` when `IsSelected` is set.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `TableOfContents` | `ImmutableList<NavigationTreeItem>?` | `null` | Navigation tree to render; when `null` the component renders nothing, and entries are sorted by `NavigationTreeItem.Order` at each level. |
| `SectionLabel` | `string?` | `null` | Optional label forwarded from the caller's `NavigationInfo.SectionName`; not rendered by the default template. |
| `ListGapClass` | `string` | `"gap-4"` | CSS classes applied to the outer `<ul>` that holds the top-level navigation entries. |
| `ChildListClass` | `string` | `"mt-4"` | CSS classes applied to the nested `<ul>` that holds a section's child entries. |
| `SectionHeaderStructureClass` | `string` | `"font-display font-medium first:pt-0"` | Layout and typography classes applied to the section-header element. |
| `SectionHeaderColorClass` | `string` | `"text-base-900 dark:text-base-50"` | CSS classes applied to section-header text — both the plain `<div>` for empty-route entries and the `<a>` when a top-level entry has children. |
| `LinkStructureClass` | `string` | `"block text-sm w-full border-l pl-3.5 py-1.5"` | Layout and typography classes applied to each child-level `<a>` element under a section. |
| `LinkColorClass` | `string` | see source | CSS classes applied to each child-level `<a>` for color and `data-current=true` state, composed after `LinkStructureClass`. |
| `RootLinkStructureClass` | `string` | `"block w-full py-1"` | Layout classes applied to a leaf root-level `<a>` when a top-level entry has no children. |
| `RootLinkColorClass` | `string` | see source | CSS classes applied to a leaf root-level `<a>` (a top-level entry with no children), composed after `RootLinkStructureClass`. |

### Binding

`TableOfContents` accepts an `ImmutableList<NavigationTreeItem>` produced by `await NavigationBuilder.BuildTreeAsync(items, currentRoute, locale)`. It does not accept a `NavigationInfo`. `SectionLabel` is typically passed from `NavigationInfo.SectionName`. No `RenderFragment` slots.

## `OutlineNavigation`

### Declaration

```razor:symbol
src/Pennington.UI/Components/Navigation/OutlineNavigation.razor
```

Emits a `data-role="page-outline"` container and an empty `<ul>` whose items are populated client-side by scraping headings from the element matched by `ContentSelector`. The component performs no server-side heading extraction; the companion script in `Pennington.UI/wwwroot/` reads `data-content-selector`, `data-outline-link-structure-class`, and `data-outline-link-color-class` to build and highlight the outline in the browser.

### Parameters

`ContentSelector` is `[EditorRequired]`; all other parameters carry defaults tuned for the DocSite main-content column.

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string` | `""` (required) | CSS selector the client-side outline script queries to discover heading elements; must be non-empty for the outline to populate. |
| `Title` | `string` | `"On this page"` | Eyebrow rendered above the outline list as a `<div>`; pass an empty string to suppress. |
| `TitleStructureClass` | `string` | `"font-display text-[13px] font-semibold mb-3"` | Layout and typography classes applied to the eyebrow above the outline list. |
| `TitleColorClass` | `string` | `"text-base-600 dark:text-base-300"` | CSS classes applied to the eyebrow text. |
| `ContainerStructureClass` | `string` | `"border-l border-base-200 dark:border-base-800"` | Layout and border classes applied to the outer `data-role="page-outline"` container. |
| `ContainerColorClass` | `string` | `""` | CSS classes applied to the outer container for color treatment, composed after `ContainerStructureClass`. |
| `ListStructureClass` | `string` | `"list-none pl-4"` | Layout classes applied to the outline `<ul>`. |
| `ListColorClass` | `string` | `"text-base-500 dark:text-base-400"` | CSS classes applied to the `<ul>` that holds outline links, composed after `ListStructureClass`. |
| `OutlineLinkColorClass` | `string` | see source | CSS classes emitted on the container as `data-outline-link-color-class` and applied by the client-side script to each generated `<li><a>` for color and `data-selected=true` state. |
| `OutlineLinkStructureClass` | `string` | see source | Layout classes emitted on the container as `data-outline-link-structure-class` and applied by the client-side script to each generated `<li><a>`. |

### Binding

The component performs no server-side heading extraction. The outline list is populated at runtime by the companion client script in `Pennington.UI/wwwroot/`, which queries the element matched by `ContentSelector` and reads `data-content-selector`, `data-outline-link-structure-class`, and `data-outline-link-color-class` from the container. `NavigationInfo` is not consulted. No `RenderFragment` slots.

## `Breadcrumb`

### Declaration

```razor:symbol
src/Pennington.UI/Components/Breadcrumb.razor
```

Renders a visible breadcrumb trail inside an article header from the `ImmutableList<BreadcrumbItem>` `NavigationBuilder` exposes via `NavigationInfo`. Each item links to its route except the last (which renders as a current-page `<span aria-current="page">`); the trail renders nothing when the list is empty. `TrailingContent` supplies optional right-aligned chrome on the same row (an "Edit on GitHub" link, repository metadata).

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Items` | `ImmutableList<BreadcrumbItem>` | `[]` | The breadcrumb trail to render. Empty list renders nothing. |
| `TrailingContent` | `RenderFragment?` | `null` | Optional content rendered on the trailing edge of the breadcrumb row; pushed right via `ml-auto`. |

## `Pagination`

### Declaration

```razor:symbol
src/Pennington.UI/Components/Pagination.razor
```

Prev / numbered / next pagination controls. URL-shape agnostic — the caller supplies a `Func<int, string>` that maps a 1-based page index to a URL, so the same component drives `/archive/page/N/`, `/tags/{tag}/page/N/`, or any other shape. Renders nothing when `TotalPages` is 1 or less.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `CurrentPage` | `int` | `1` | 1-based page index for the current view; highlighted in the numbered list. |
| `TotalPages` | `int` | `1` | Total number of pages. The component renders nothing when this is 1 or less. |
| `UrlFor` | `Func<int, string>` | `page => "?page={page}"` | Returns the URL for a given 1-based page index. Callers should map page 1 to the canonical (non-paginated) URL of the listing. |
| `SiblingCount` | `int` | `1` | Number of numeric page links flanking the current page in the truncated list. The first and last pages are always rendered; gaps collapse to `...`. Default of 1 yields windows like `1 ... 4 5 6 ... 12`. |

## See also

- How-to: [Customize the sidebar](xref:how-to.navigation.customize-sidebar)
- Related reference: [Navigation types](xref:reference.api.navigation-builder)
- Related reference: [Content components](xref:reference.ui.content)
