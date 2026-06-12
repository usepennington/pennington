---
title: "Navigation components"
description: "Parameters, slots, and NavigationInfo bindings for the four Pennington.UI navigation components — TableOfContentsNavigation, OutlineNavigation, Breadcrumb, and Pagination."
uid: reference.ui.navigation
order: 1
sectionLabel: UI Components
tags: [ui, navigation, razor, components]
---

The four navigation-oriented Razor components in `Pennington.UI`. `TableOfContentsNavigation` and `OutlineNavigation` render, respectively, the sidebar page tree and the floating in-page heading outline, and live in namespace `Pennington.UI.Components.Navigation`. `Breadcrumb` and `Pagination` render the article-header trail and prev/numbered/next paging controls, and live in the base namespace `Pennington.UI.Components`. All four are consumed by `Pennington.DocSite`'s `MainLayout`.

## `TableOfContentsNavigation`

### Declaration

```razor:symbol
src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor
```

Renders an ordered `<nav><ul>` of `NavigationTreeItem` entries, recursing one level into each entry's `Children` collection and sorting by `NavigationTreeItem.Order` at each level. Root entries with an empty `Route.CanonicalPath` render as plain section headers; entries with a path render as anchor links carrying `data-current="true"` when `IsSelected` is set.

### Parameters

Every `*Class` parameter defaults to `null` and resolves through the style-registry slot listed in its Default column; an explicitly passed value is Tailwind-merged over the slot for that instance. Run `dotnet run -- diag styles` for effective slot values, and see <xref:how-to.theming.component-styles> for overriding slots app-wide.

| Name | Type | Default | Description |
|---|---|---|---|
| `TableOfContents` | `ImmutableList<NavigationTreeItem>?` | `null` | Navigation tree to render; when `null` the component renders nothing. |
| `SectionLabel` | `string?` | `null` | Optional label forwarded from the caller's `NavigationInfo.SectionName`; not rendered by the default template. |
| `ListClass` | `string?` | `toc.list` slot | The outer `<ul>` that holds the top-level navigation entries. |
| `SectionClass` | `string?` | `toc.section` slot | Each top-level `<li>`. |
| `SectionTitleClass` | `string?` | `toc.section-title` slot | A section's label — the plain `<div>` for empty-route entries, or the `<a>` when a top-level entry has children. |
| `SectionListClass` | `string?` | `toc.section-list` slot | The nested `<ul>` that holds a section's child entries. |
| `LinkClass` | `string?` | `toc.link` slot | Each child-level `<a>`, including its `data-current=true` state styling. |
| `TopLinkClass` | `string?` | `toc.top-link` slot | A top-level leaf `<a>` (an entry with no children), including its `data-current=true` state styling. |

### Binding

`TableOfContents` accepts an `ImmutableList<NavigationTreeItem>` produced by `await NavigationBuilder.BuildTreeAsync(items, currentPath, locale)`. It does not accept a `NavigationInfo`. `SectionLabel` is typically passed from `NavigationInfo.SectionName`. No `RenderFragment` slots.

### Example

```razor
@{
    var tree = await NavigationBuilder.BuildTreeAsync(items, currentPath, locale);
}

<TableOfContentsNavigation TableOfContents="tree" SectionLabel="@navigation.SectionName" />
```

## `OutlineNavigation`

### Declaration

```razor:symbol
src/Pennington.UI/Components/Navigation/OutlineNavigation.razor
```

Emits a `data-role="page-outline"` container and an empty `<ul>` whose items are populated client-side by scraping headings from the element matched by `ContentSelector`. The component performs no server-side heading extraction; the companion script in `Pennington.UI/wwwroot/` reads `data-content-selector`, `data-outline-link-class`, and `data-outline-nested-link-class` to build and highlight the outline in the browser.

### Parameters

`ContentSelector` is `[EditorRequired]`. Every `*Class` parameter defaults to `null` and resolves through the style-registry slot listed in its Default column; an explicitly passed value is Tailwind-merged over the slot for that instance. Run `dotnet run -- diag styles` for effective slot values, and see <xref:how-to.theming.component-styles> for overriding slots app-wide.

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string` | `""` (required) | CSS selector the client-side outline script queries to discover heading elements; must be non-empty for the outline to populate. |
| `Title` | `string` | `"On this page"` | Eyebrow rendered above the outline list as a `<div>`; pass an empty string to suppress. |
| `TitleClass` | `string?` | `outline.title` slot | The eyebrow above the outline list. |
| `ContainerClass` | `string?` | `outline.container` slot | The outer `data-role="page-outline"` container; `relative` stays hardcoded for marker positioning. |
| `MarkerClass` | `string?` | `outline.marker` slot | The moving highlight bar that tracks the active heading; `absolute` and `opacity-0` stay hardcoded — the script positions the bar and toggles its opacity. |
| `ListClass` | `string?` | `outline.list` slot | The outline `<ul>`. |
| `LinkClass` | `string?` | `outline.link` slot | Emitted as `data-outline-link-class` and applied by the client-side script to each generated `<a>`, including its `data-selected=true` state styling. |
| `NestedLinkClass` | `string?` | `outline.nested-link` slot | Emitted as `data-outline-nested-link-class` and appended by the client-side script to nested (H3-level) outline links. |

### Binding

The component performs no server-side heading extraction. The outline list is populated at runtime by the companion client script in `Pennington.UI/wwwroot/`, which queries the element matched by `ContentSelector` and reads `data-content-selector`, `data-outline-link-class`, and `data-outline-nested-link-class` from the container. `NavigationInfo` is not consulted. No `RenderFragment` slots.

### Example

```razor
<OutlineNavigation ContentSelector="#main-content" Title="On this page" />
```

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

### Binding

`Items` accepts the `ImmutableList<BreadcrumbItem>` exposed as `NavigationInfo.Breadcrumbs`. The last item renders as the current page; every prior item with a `Route` renders as a link. `TrailingContent` is a `RenderFragment` slot for right-aligned chrome on the same row.

### Example

```razor
<Breadcrumb Items="navigation.Breadcrumbs">
    <TrailingContent>
        <a href="@editUrl">Edit on GitHub</a>
    </TrailingContent>
</Breadcrumb>
```

## `Pagination`

### Declaration

```razor:symbol
src/Pennington.UI/Components/Pagination.razor
```

Prev / numbered / next pagination controls. URL-pattern agnostic — the caller supplies a `Func<int, string>` that maps a 1-based page index to a URL, so the same component drives `/archive/page/N/`, `/tags/{tag}/page/N/`, or any other pattern. Renders nothing when `TotalPages` is 1 or less.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `CurrentPage` | `int` | `1` | 1-based page index for the current view; highlighted in the numbered list. |
| `TotalPages` | `int` | `1` | Total number of pages. The component renders nothing when this is 1 or less. |
| `UrlFor` | `Func<int, string>` | `page => "?page={page}"` | Returns the URL for a given 1-based page index. Callers should map page 1 to the canonical (non-paginated) URL of the listing. |
| `SiblingCount` | `int` | `1` | Number of numeric page links flanking the current page in the truncated list. The first and last pages are always rendered; gaps collapse to `...`. Default of 1 yields windows like `1 ... 4 5 6 ... 12`. |

### Binding

`Pagination` does not consult `NavigationInfo`. The caller supplies `CurrentPage` and `TotalPages` as plain integers and maps each 1-based page index to a URL through the `UrlFor` delegate, so the same component drives any paging URL shape. No `RenderFragment` slots.

### Example

```razor
@{
    string PageUrl(int page) => page == 1 ? "/archive/" : $"/archive/page/{page}/";
}

<Pagination CurrentPage="currentPage" TotalPages="totalPages" UrlFor="PageUrl" />
```

## See also

- How-to: [Customize the sidebar](xref:how-to.navigation.customize-sidebar)
- Related reference: [Navigation types](xref:reference.api.navigation-builder)
- Related reference: [Content components](xref:reference.ui.content)
