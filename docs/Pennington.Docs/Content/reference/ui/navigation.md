---
title: "Navigation components"
description: "Parameters, slots, and NavigationInfo bindings for TableOfContentsNavigation and OutlineNavigation."
uid: reference.ui.navigation
order: 404010
sectionLabel: UI Components
tags: [ui, navigation, razor, components]
---

`TableOfContentsNavigation` and `OutlineNavigation` are the two Razor components in `Pennington.UI` that render, respectively, the sidebar page tree and the floating in-page heading outline. Both live in namespace `Pennington.UI.Components.Navigation` and are consumed by `Pennington.DocSite`'s `MainLayout`, but are available to any host referencing `Pennington.UI`. `TableOfContentsNavigation` binds to an `ImmutableList<NavigationTreeItem>` produced by `NavigationBuilder`; `OutlineNavigation` binds to a client-side DOM selector at runtime. Neither accepts a `NavigationInfo` directly.

## `TableOfContentsNavigation`

### Declaration

```razor:path
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

### Slots

This component has no `RenderFragment` slots; all customization is performed through the class-name parameters above.

### Example

The DocSite `MainLayout` (`src/Pennington.DocSite/Components/Layout/MainLayout.razor`) instantiates `TableOfContentsNavigation` twice — once per area when `DocSiteOptions.Areas` is populated and once against the root tree otherwise — passing the tree produced by `NavigationBuilder.BuildTree`.

## `OutlineNavigation`

### Declaration

```razor:path
src/Pennington.UI/Components/Navigation/OutlineNavigation.razor
```

Emits a `data-role="page-outline"` container and an empty `<ul>` whose items are populated client-side by scraping headings from the element matched by `ContentSelector`. The component performs no server-side heading extraction; the companion script in `Pennington.UI/wwwroot/` reads `data-content-selector`, `data-outline-link-structure-class`, and `data-outline-link-color-class` to build and highlight the outline in the browser.

### Parameters

`ContentSelector` is `[EditorRequired]`; all other parameters carry defaults tuned for the DocSite main-content column.

| Name | Type | Default | Description |
|---|---|---|---|
| `ContentSelector` | `string` | `""` (required) | CSS selector the client-side outline script queries to discover heading elements; must be non-empty for the outline to populate. |
| `Title` | `string` | `"On This Page"` | Textual label accepted for parity with other outline skins; not rendered by the default template. |
| `ContainerStructureClass` | `string` | `"border-l border-base-200 dark:border-base-800"` | Layout and border classes applied to the outer `data-role="page-outline"` container. |
| `ContainerColorClass` | `string` | `""` | CSS classes applied to the outer container for color treatment, composed after `ContainerStructureClass`. |
| `ListStructureClass` | `string` | `"list-none pl-4"` | Layout classes applied to the outline `<ul>`. |
| `ListColorClass` | `string` | `"text-neutral-500 dark:text-neutral-400"` | CSS classes applied to the `<ul>` that holds outline links, composed after `ListStructureClass`. |
| `OutlineLinkColorClass` | `string` | see source | CSS classes emitted on the container as `data-outline-link-color-class` and applied by the client-side script to each generated `<li><a>` for color and `data-selected=true` state. |
| `OutlineLinkStructureClass` | `string` | see source | Layout classes emitted on the container as `data-outline-link-structure-class` and applied by the client-side script to each generated `<li><a>`. |

### Slots

This component has no `RenderFragment` slots; the outline list is populated at runtime by the companion client script.

### Example

The DocSite `MainLayout` drops a single `<OutlineNavigation ContentSelector="article main" />` into the right-hand rail so the script binds to headings inside the rendered article.

## Binding to `NavigationInfo`

`NavigationInfo` is the per-request record exposed by `NavigationBuilder.BuildNavigationInfo`; it carries `SectionName`, `SectionRoute`, `Breadcrumbs`, `PageTitle`, `PreviousPage`, and `NextPage`, not a navigation tree.

```csharp:xmldocid
T:Pennington.Navigation.NavigationInfo
```

`TableOfContentsNavigation.TableOfContents` is populated from the tree returned by `NavigationBuilder.BuildTree(items, currentRoute, locale)`, not from a `NavigationInfo`; `NavigationInfo.SectionName` is the value callers typically pass to `SectionLabel`. `OutlineNavigation` does not read `NavigationInfo` at all — it is a client-side component bound to a DOM selector — so previous/next navigation and breadcrumbs flow through other components or layout slots.

## See also

- How-to: [Customize the sidebar](xref:how-to.content-authoring.customize-sidebar)
- Related reference: [Navigation types](xref:reference.api.navigation-builder)
- Related reference: [Content components](xref:reference.ui.content)
