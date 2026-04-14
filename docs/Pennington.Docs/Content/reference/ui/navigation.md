---
title: "Navigation components"
description: "Parameters, slots, and NavigationInfo bindings for TableOfContentsNavigation and OutlineNavigation."
uid: reference.ui.navigation
order: 10
sectionLabel: UI Components
tags: [ui, navigation, razor, components]
---

> **In this page.** _`TableOfContentsNavigation` and `OutlineNavigation` — parameters, slots, and how they bind to `NavigationInfo`._
>
> **Not in this page.** _Rendering custom navigation shapes — a Razor authoring topic covered in the customization how-tos._

## Summary

_**One sentence: what these are.** `TableOfContentsNavigation` and `OutlineNavigation` are the two Razor components in `Pennington.UI` that render, respectively, the sidebar tree of pages and the floating in-page heading outline._
_**One sentence: where they live.** Namespace `Pennington.UI.Components.Navigation`, files `src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor` and `src/Pennington.UI/Components/Navigation/OutlineNavigation.razor`, consumed by `Pennington.DocSite`'s `MainLayout` and available to any host that references `Pennington.UI`._

_The two components share this page because they are the built-in navigation primitives the DocSite chrome composes, but they do not compose with each other — `TableOfContentsNavigation` binds to an `ImmutableList<NavigationTreeItem>` produced by `NavigationBuilder`, while `OutlineNavigation` binds to a client-side selector at runtime. Neither takes a `NavigationInfo` directly; callers pass the tree or selector they derive from one._

## `TableOfContentsNavigation`

### Declaration

```csharp:xmldocid
T:Pennington.UI.Components.Navigation.TableOfContentsNavigation
```

_One sentence: a Razor component that renders an ordered `<nav><ul>` of `NavigationTreeItem` entries, recursing one level into each entry's `Children` collection._
_One sentence: root entries with an empty `Route.CanonicalPath` render as plain section headers (no anchor); entries with a path render as anchor links carrying `data-current="true"` when `IsSelected` is set, and children are rendered one nesting level deep._

### Parameters

_Alphabetical. All parameters are public `[Parameter]` properties. Every class parameter has a tuned-for-DocSite default; callers override them to restyle the sidebar for other hosts._

| Name | Type | Default | Description |
|---|---|---|---|
| `ChildListClass` | `string` | `"mt-4"` | _One sentence: CSS classes applied to the nested `<ul>` that holds a section's child entries._ |
| `LinkColorClass` | `string` | _theme-aware color utilities (see source)_ | _One sentence: CSS classes applied to each child-level `<a>` element for color and `data-current=true` state, composed after `LinkStructureClass`._ |
| `LinkStructureClass` | `string` | `"block text-sm w-full border-l pl-3.5 py-1.5"` | _One sentence: layout and typography classes applied to each child-level `<a>` element under a section._ |
| `ListGapClass` | `string` | `"gap-4"` | _One sentence: CSS classes applied to the outer `<ul>` that holds the top-level navigation entries._ |
| `RootLinkColorClass` | `string` | _theme-aware color utilities (see source)_ | _One sentence: CSS classes applied to a leaf root-level `<a>` (a top-level entry with no children), composed after `RootLinkStructureClass`._ |
| `RootLinkStructureClass` | `string` | `"block w-full py-1"` | _One sentence: layout classes applied to a leaf root-level `<a>` when a top-level entry has no children._ |
| `SectionHeaderColorClass` | `string` | _theme-aware color utilities (see source)_ | _One sentence: CSS classes applied to section-header text — both the plain `<div>` used for empty-route entries and the `<a>` used when a top-level entry has children._ |
| `SectionHeaderStructureClass` | `string` | `"font-display font-medium first:pt-0"` | _One sentence: layout and typography classes applied to the section-header element._ |
| `SectionLabel` | `string?` | `null` | _One sentence: optional label forwarded from the caller's `NavigationInfo.SectionName`; accepted for symmetry with call sites and is not rendered by the default template._ |
| `TableOfContents` | `ImmutableList<NavigationTreeItem>?` | `null` | _One sentence: the navigation tree to render; when `null` the component renders nothing, and entries are sorted by `NavigationTreeItem.Order` at each level._ |

### Slots

_This component has no `RenderFragment` slots; all customization is performed through the class-name parameters above._

### Example

```razor:path
src/Pennington.DocSite/Components/Layout/MainLayout.razor
```

_A single sentence of context: the DocSite `MainLayout` instantiates `TableOfContentsNavigation` twice — once per area when `DocSiteOptions.Areas` is populated and once against the root tree otherwise — passing the tree produced by `NavigationBuilder.BuildTree`._

## `OutlineNavigation`

### Declaration

```csharp:xmldocid
T:Pennington.UI.Components.Navigation.OutlineNavigation
```

_One sentence: a Razor component that emits a `data-role="page-outline"` container and an empty `<ul>` whose items are populated client-side by scraping headings from the element matched by `ContentSelector`._
_One sentence: the component does no server-side heading extraction — it renders the container shell and the companion script in `Pennington.UI/wwwroot/` reads `data-content-selector`, `data-outline-link-structure-class`, and `data-outline-link-color-class` to build and highlight the outline in the browser._

### Parameters

_Alphabetical. `ContentSelector` is `[EditorRequired]`; every other parameter carries a default tuned for the DocSite main-content column._

| Name | Type | Default | Description |
|---|---|---|---|
| `ContainerColorClass` | `string` | `""` | _One sentence: CSS classes applied to the outer container for color treatment, composed after `ContainerStructureClass`._ |
| `ContainerStructureClass` | `string` | `"border-l border-base-200 dark:border-base-800"` | _One sentence: layout and border classes applied to the outer `data-role="page-outline"` container._ |
| `ContentSelector` | `string` | `""` (required) | _One sentence: CSS selector the client-side outline script queries to discover heading elements; this parameter is `[EditorRequired]` and must be non-empty for the outline to populate._ |
| `ListColorClass` | `string` | _theme-aware color utilities (see source)_ | _One sentence: CSS classes applied to the `<ul>` that holds outline links, composed after `ListStructureClass`._ |
| `ListStructureClass` | `string` | `"list-none pl-4"` | _One sentence: layout classes applied to the outline `<ul>`._ |
| `OutlineLinkColorClass` | `string` | _theme-aware color utilities (see source)_ | _One sentence: CSS classes emitted on the container as `data-outline-link-color-class` and applied by the client-side script to each generated outline `<li><a>` for color and `data-selected=true` state._ |
| `OutlineLinkStructureClass` | `string` | `"py-1 ml-[calc(-1*(4em-1px))] pl-[calc(4em+1px)] "` | _One sentence: layout classes emitted on the container as `data-outline-link-structure-class` and applied by the client-side script to each generated outline `<li><a>`._ |
| `Title` | `string` | `"On This Page"` | _One sentence: textual label accepted for parity with other outline skins; not rendered by the default template._ |

### Slots

_This component has no `RenderFragment` slots; the outline list is populated at runtime by the companion client script._

### Example

```razor:path
src/Pennington.DocSite/Components/Layout/MainLayout.razor
```

_A single sentence of context: the DocSite `MainLayout` drops a single `<OutlineNavigation ContentSelector="article main" />` into the right-hand rail so the script binds to headings inside the rendered article._

## Binding to `NavigationInfo`

_`NavigationInfo` is the per-request record exposed through `Pennington.Navigation.NavigationBuilder.BuildNavigationInfo` and carries `SectionName`, `SectionRoute`, `Breadcrumbs`, `PageTitle`, `PreviousPage`, and `NextPage` — not a tree._

```csharp:xmldocid
T:Pennington.Navigation.NavigationInfo
```

_One sentence: `TableOfContentsNavigation.TableOfContents` is populated from the tree returned by `NavigationBuilder.BuildTree(items, currentRoute, locale)`, not from a `NavigationInfo`; `NavigationInfo.SectionName` is the value callers typically pass to `SectionLabel`._
_One sentence: `OutlineNavigation` does not read `NavigationInfo` at all — it is a client-side component bound to a DOM selector — so the previous/next navigation and breadcrumbs on `NavigationInfo` flow through other components or layout slots._

## See also

- How-to: [Customize the sidebar](/how-to/content-authoring/customize-sidebar)
- Related reference: [Navigation types](/reference/extension-points/navigation)
- Related reference: [Content components](/reference/ui/content)
- Background: TODO — explanation page on the navigation model (not yet authored in TOC)
