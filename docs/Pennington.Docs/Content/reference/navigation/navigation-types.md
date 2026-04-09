---
title: "Navigation Types"
description: "Reference for NavigationBuilder API, NavigationTreeItem record, NavigationInfo record, BreadcrumbItem record, and ContentTocItem record — including locale filtering behavior"
uid: "penn.reference.navigation-types"
order: 10
---

Document the types used to build and represent navigation. `NavigationBuilder`: `BuildTree(tocItems, locale?)` returns `ImmutableList<NavigationTreeItem>`, `BuildNavigationInfo(tree, route)` returns `NavigationInfo`. `NavigationTreeItem` record: Title, Route (ContentRoute?), Order, Section, IsSelected (bool), IsExpanded (bool), Children (ImmutableList). `NavigationInfo` record: SectionName, SectionRoute, Breadcrumbs (ImmutableList<BreadcrumbItem>), PageTitle, PreviousPage (NavigationTreeItem?), NextPage (NavigationTreeItem?). `BreadcrumbItem` record: Title, Route. `ContentTocItem` record: Title, Route (ContentRoute), Order, HierarchyParts (string[]), Section, Locale. Document the locale filtering parameter on `BuildTree`.
