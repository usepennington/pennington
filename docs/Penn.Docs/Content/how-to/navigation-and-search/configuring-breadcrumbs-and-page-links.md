---
title: "Configuring Breadcrumbs and Page Links"
description: "Understand how breadcrumbs are generated from the navigation tree and how prev/next links follow depth-first ordering"
uid: "penn.how-to.configuring-breadcrumbs-and-page-links"
order: 15
---

You want to understand how breadcrumbs and prev/next page links are generated so you can control or troubleshoot them.

## Beat 1: Breadcrumb Generation

Show how breadcrumbs are computed from the navigation tree and exposed through `NavigationInfo`.

### What to show
- Reference `M:Pennington.Navigation.NavigationBuilder.BuildNavigationInfo(System.Collections.Generic.IReadOnlyList{Pennington.Content.ContentTocItem},Pennington.Routing.ContentRoute,System.String)` (:path `src/Pennington/Navigation/NavigationBuilder.cs`) — builds the full `NavigationInfo` for a specific route
- Reference `T:Pennington.Navigation.NavigationInfo` (:path `src/Pennington/Navigation/NavigationInfo.cs`) — record with `SectionName`, `SectionRoute`, `Breadcrumbs` (`ImmutableList<BreadcrumbItem>`), `PageTitle`, `PreviousPage`, `NextPage`
- Reference `T:Pennington.Navigation.BreadcrumbItem` (:path `src/Pennington/Navigation/BreadcrumbItem.cs`) — record with `Title` and `Route` (nullable `ContentRoute`)
- The `BuildBreadcrumbs` private method walks the tree depth-first using `FindPath`, accumulating `BreadcrumbItem` entries from root to the selected item
- Example: navigating to `Content/tutorials/monitoring/setting-up-alerts.md` produces breadcrumbs: `[Tutorials, Monitoring & Alerts, Setting Up Alerts]`

### Key points
- Breadcrumbs follow the tree path, not the URL path — if a section header has a custom title from an `index.md`, the breadcrumb uses that title
- The breadcrumb trail always includes the current page as the last entry
- Non-navigable section headers (auto-generated from directories without `index.md`) appear in the breadcrumb trail with a null `Route`

## Beat 2: Previous/Next Page Links

Show how prev/next navigation follows depth-first order across the entire tree.

### What to show
- Reference `P:Pennington.Navigation.NavigationInfo.PreviousPage` and `P:Pennington.Navigation.NavigationInfo.NextPage` (both `NavigationTreeItem?`) on `T:Pennington.Navigation.NavigationInfo` (:path `src/Pennington/Navigation/NavigationInfo.cs`)
- The `Flatten` private method in `NavigationBuilder` (:path `src/Pennington/Navigation/NavigationBuilder.cs`) performs depth-first traversal, producing a linear list of all pages in tree order
- `PreviousPage` is the item at `currentIndex - 1` in the flattened list; `NextPage` is at `currentIndex + 1`
- Prev/next links cross section boundaries — the last page in "Tutorials > Monitoring" links to the first page in the next sibling section

### Key points
- The depth-first order means prev/next follows the same visual order as the sidebar
- Auto-created section headers (non-navigable) are included in the flat list but have empty routes, so they may appear as prev/next targets — UI components should handle this
- When a page is the first in the tree, `PreviousPage` is null; when it's the last, `NextPage` is null

## Beat 3: Troubleshooting Breadcrumb and Link Issues

Address the most common breadcrumb and prev/next link complaints and their fixes.

### What to show
- **"Breadcrumbs show a wrong title"**: the breadcrumb title comes from the navigation tree, not the URL. If a directory has an `index.md`, the breadcrumb uses its `title` front matter. Fix: check the `title` in the relevant `index.md` and update it to the desired label
- **"Prev/next links skip a page"**: prev/next traversal only operates within a single content area tree. If a page is in a different content area, it will not appear in the prev/next sequence. Fix: verify the page is in the expected content area directory and check that its `order` value places it in the expected position
- **"Breadcrumb has a non-clickable entry"**: a breadcrumb entry with no link means the corresponding directory has no `index.md`, so NavigationBuilder created a non-navigable section header. Fix: add an `index.md` to that directory with the desired `title` in front matter to make it a clickable breadcrumb entry

### Key points
- Breadcrumb titles are derived from the navigation tree, which uses `index.md` titles or `FormatSectionTitle` for directories without one
- Prev/next links respect content area boundaries — they do not cross from one area to another
- Non-navigable entries in the breadcrumb trail indicate directories that lack an `index.md`
