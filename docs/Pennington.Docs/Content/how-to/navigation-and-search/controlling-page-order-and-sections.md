---
title: "Controlling Page Order and Sections"
description: "Control how pages appear in the sidebar: ordering rules, section headers from directories, index.md overrides, section front matter, and content areas"
uid: "penn.how-to.controlling-page-order-and-sections"
order: 10
---

You want to control where pages appear in the sidebar navigation — their order, how they're grouped into sections, and how those sections are labeled.

## Beat 1: How Directory Structure Maps to Navigation Hierarchy

Explain the fundamental mapping: directories become navigation sections, markdown files become pages, and the file system hierarchy defines the tree structure.

### What to show
- Reference `T:Pennington.Navigation.NavigationBuilder` (:path `src/Pennington/Navigation/NavigationBuilder.cs`) — the service that transforms flat `ContentTocItem` lists into a tree of `NavigationTreeItem` nodes
- Reference `T:Pennington.Content.ContentTocItem` (:path `src/Pennington/Content/ContentTocItem.cs`) — the flat input record with `Title`, `Route`, `Order`, `HierarchyParts` (string array derived from the file path), `Section`, and `Locale`
- Show how `HierarchyParts` is derived from the content file path: `Content/tutorials/monitoring/creating-monitors.md` produces `["tutorials", "monitoring", "creating-monitors"]`
- Show a simple directory tree and the resulting sidebar navigation

### Key points
- The navigation tree is built from flat `ContentTocItem` records, not by scanning the file system directly
- Each content service produces its own `ContentTocItem` records; `NavigationBuilder` merges them into a unified tree
- `HierarchyParts` defines the nesting depth — deeper arrays produce deeper tree levels

## Beat 2: Ordering Rules — Explicit Order, Then Alphabetical

Show how the `order` front matter property controls page sequence and what happens when it's omitted.

### What to show
- Reference the `BuildLevel` method in `T:Pennington.Navigation.NavigationBuilder` (:path `src/Pennington/Navigation/NavigationBuilder.cs`) — the private method that sorts items at each level: `.OrderBy(item => item.Order).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)`
- Show 4 pages without `order` values: they sort alphabetically by title
- Add `order: 10`, `order: 20`, etc. to the pages: they now sort by the explicit order
- Pages without an explicit `order` value have `Order = int.MaxValue` (the default) and sort AFTER all explicitly ordered pages, then alphabetically by title among other unordered pages.
- The final sort at each tree level (line with `builder.OrderBy(i => i.Order).ThenBy(i => i.Title)`) applies to both auto-created section nodes and direct items together

### Key points
- `Order` is an `int` — lower values appear first, default is `int.MaxValue`
- Pages with the same `Order` value fall back to alphabetical sorting by `Title` (case-insensitive via `StringComparer.OrdinalIgnoreCase`)
- The ordering applies per level of the tree, not globally — sibling pages are ordered relative to each other

## Beat 3: Auto-Generated Section Headers from Directories

Explain what happens when a directory has no `index.md` — NavigationBuilder creates a section header node automatically.

### What to show
- Reference the `sectionsWithDescendants` logic in `BuildLevel` (:path `src/Pennington/Navigation/NavigationBuilder.cs`) — finds directory names (hierarchy parts) that have descendants but no direct item at that level
- Reference `M:Pennington.Navigation.NavigationBuilder.FormatSectionTitle(System.String)` — converts kebab-case directory names to title case: `"getting-started"` becomes `"Getting Started"`
- Show that auto-created section nodes get an empty `Route` (non-navigable), meaning they render as section headers in the sidebar, not as clickable links
- The section node's `Order` is set to the minimum `Order` among its children, so it sorts naturally with sibling pages

### Key points
- Directories without an `index.md` get auto-generated section headers using `FormatSectionTitle`
- These section headers are non-navigable (empty `CanonicalPath`) — they only serve as grouping labels
- Adding an `index.md` to the directory converts the auto-generated header into a navigable page with a custom title

## Beat 4: Override Section Headers with index.md

Show how adding an `index.md` to a directory replaces the auto-generated header with a navigable page whose title comes from front matter.

### What to show
- Create `Content/tutorials/monitoring/index.md` with `title: "Monitoring & Alerts"` — this becomes a navigable entry that replaces the auto-generated "Monitoring" header
- Reference `T:Pennington.Navigation.NavigationTreeItem` (:path `src/Pennington/Navigation/NavigationTreeItem.cs`) — record with `Title`, `Route`, `Order`, `Section`, `IsSelected`, `IsExpanded`, `Children`
- When an `index.md` exists, the `ContentTocItem` for it has `HierarchyParts` matching the directory level, so it appears as a direct item (not an auto-created section) with its children nested below
- The `Title` from front matter replaces the kebab-case-derived title

### Key points
- The `index.md` title takes over as the section header text
- The section header becomes a clickable link (the route is populated, not empty)
- The `Order` from the `index.md` front matter controls where the section appears relative to sibling sections and pages

## Beat 5: The Section Front Matter Override

Show how the `section` front matter property can group a page under a specific section header, regardless of its directory location.

### What to show
- Reference the `Section` property on `T:Pennington.Content.ContentTocItem` (:path `src/Pennington/Content/ContentTocItem.cs`)
- Reference the `Section` property on `T:Pennington.Navigation.NavigationTreeItem` (:path `src/Pennington/Navigation/NavigationTreeItem.cs`)
- Show a page at `Content/guides/advanced-routing.md` with `section: "Monitoring & Alerts"` front matter — it groups under the "Monitoring & Alerts" section despite being in the `guides/` directory
- Explain that `Section` is a display property passed through to the tree item; the actual tree hierarchy is still determined by `HierarchyParts` (the file path)

### Key points
- The `section` front matter property provides a label but does not move the page in the tree hierarchy
- The section value is available to UI components like `TableOfContentsNavigation` for grouping display
- This is most useful when a page conceptually belongs to a section but lives in a different directory for organizational reasons

## Beat 6: Configure Content Areas for Tabbed Navigation

Show how `DocSiteOptions.Areas` segments the sidebar into tabbed content areas, each mapping to a top-level directory.

### What to show
- Reference `T:Pennington.DocSite.ContentArea` (:path `src/Pennington.DocSite/ContentArea.cs`) — record with `Title` (display name), `Slug` (URL prefix / directory name), optional `Icon` (SVG markup)
- Reference `P:Pennington.DocSite.DocSiteOptions.Areas` (:path `src/Pennington.DocSite/DocSiteOptions.cs`) — `IReadOnlyList<ContentArea>`, doc comment: "When empty or containing a single area, no area selector is shown. Each area's slug must match a top-level directory name under ContentRootPath."
- Configure three areas in `DocSiteOptions`:
  ```csharp
  Areas = [
      new ContentArea("Tutorials", "tutorials"),
      new ContentArea("Guides", "guides"),
      new ContentArea("API Reference", "api", Icon: "<svg>...</svg>")
  ]
  ```
- Each area's `Slug` corresponds to a top-level directory: `Content/tutorials/`, `Content/guides/`, `Content/api/`
- With multiple areas, the sidebar shows a tab selector at the top

### Key points
- A single area (or no areas) means no tab selector — the entire sidebar is one flat navigation
- The `Slug` must exactly match the top-level directory name under the content root
- Each area gets its own independent navigation tree, built from the `NavigationBuilder` filtered to that area's content

## Beat 7: Troubleshooting Navigation Issues

Address the most common navigation complaints and their fixes.

### What to show
- **"My page appears in the wrong position"**: check the `order` front matter value. Pages with `Order = int.MaxValue` (default) sort after explicitly ordered pages. If two pages have the same order, they sort alphabetically by title. Fix: set explicit `order` values with gaps (10, 20, 30) to allow future insertions
- **"My section header shows the wrong name"**: without an `index.md`, the section name comes from `FormatSectionTitle` (kebab-case to title case). Fix: add an `index.md` with the desired `title` in front matter
- **"Pages with the same order value are in unexpected order"**: they sort alphabetically by title (case-insensitive). Fix: use distinct order values or adjust titles
- **"Prev/next links skip a page"**: the page may be in a different content area. Prev/next only traverses within the same tree. Fix: ensure the page is in the expected content area directory
- The sidebar is rendered by the `TableOfContentsNavigation` component (`T:Pennington.UI.Components.Navigation.TableOfContentsNavigation`, :path `src/Pennington.UI/Components/Navigation/TableOfContentsNavigation.razor`) — see its API for CSS customization parameters
- In-page heading tracking is handled by the `OutlineNavigation` component (`T:Pennington.UI.Components.Navigation.OutlineNavigation`, :path `src/Pennington.UI/Components/Navigation/OutlineNavigation.razor`) — it generates an "On This Page" sidebar from rendered HTML headings

### Key points
- Navigation ordering is deterministic: `Order` ascending, then `Title` alphabetical (case-insensitive)
- The `FormatSectionTitle` method splits on hyphens and capitalizes each word — `"getting-started"` becomes `"Getting Started"`, but `"api"` becomes `"Api"` (add an `index.md` if you want "API")
- Content areas are independent trees — navigation does not cross area boundaries
