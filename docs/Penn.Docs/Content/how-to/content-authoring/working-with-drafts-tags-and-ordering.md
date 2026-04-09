---
title: "Working with Drafts, Tags, and Ordering"
description: "Use front matter capabilities to control content visibility with IsDraft, categorize with Tags, sequence pages with Order, and group into sections with Section"
uid: "penn.how-to.working-with-drafts-tags-and-ordering"
order: 30
---

## Beat 1: The Capability Interface Pattern

Introduce Penn's capability interface pattern: each content organization feature is a single-property interface that front matter types opt into independently.

### What to show
- Reference `T:Penn.FrontMatter.IDraftable` -- `P:Penn.FrontMatter.IDraftable.IsDraft` (bool)
- Reference `T:Penn.FrontMatter.ITaggable` -- `P:Penn.FrontMatter.ITaggable.Tags` (string[])
- Reference `T:Penn.FrontMatter.IOrderable` -- `P:Penn.FrontMatter.IOrderable.Order` (int)
- Reference `T:Penn.FrontMatter.ISectionable` -- `P:Penn.FrontMatter.ISectionable.Section` (string?)
- All defined in `:path src/Penn/FrontMatter/Capabilities.cs`
- Show that `T:Penn.FrontMatter.DocFrontMatter` implements all four: `DocFrontMatter : IFrontMatter, IDraftable, ITaggable, ISectionable, ICrossReferenceable, IOrderable, IDescribable`
- Show that `T:Penn.FrontMatter.BlogFrontMatter` implements `IDraftable`, `ITaggable`, `IDateable`, `ICrossReferenceable` but NOT `IOrderable` or `ISectionable` -- blog posts typically sort by date, not manual order
- Show that `T:Penn.DocSite.DocSiteFrontMatter` implements all four plus `IRedirectable`: the full DocSite feature set
- Show that `T:Penn.BlogSite.BlogSiteFrontMatter` implements `IDraftable`, `ITaggable`, `IDescribable`, `IDateable`, `ICrossReferenceable`, `ISectionable`, `IRedirectable`

### Key points
- Capabilities are checked via pattern matching: `fm is IDraftable { IsDraft: true }` -- no casting needed
- The base `T:Penn.FrontMatter.IFrontMatter` requires only `P:Penn.FrontMatter.IFrontMatter.Title` -- everything else is opt-in
- Custom front matter types can mix any combination of capability interfaces

## Beat 2: Mark a Post as Draft

Set `isDraft: true` on a blog post. Show that drafts are visible in dev mode but excluded from static builds.

### What to show
- YAML front matter with `isDraft: true` on `upcoming-talk.md` -- the `FrontMatterParser` deserializes this to `P:Penn.FrontMatter.IDraftable.IsDraft`
- In `M:Penn.Content.MarkdownContentService`1.GetContentTocEntriesAsync`, drafts are filtered from the navigation: `if (fm is IDraftable { IsDraft: true }) continue;` -- the page is omitted from `T:Penn.Content.ContentTocItem` results
- In `T:Penn.Pipeline.ContentPipeline` during the `GenerateAsync` phase, drafts are detected: `if (rendered.Metadata is IDraftable { IsDraft: true })` the page is added to `P:Penn.Generation.BuildReport.SkippedPages` instead of `P:Penn.Generation.BuildReport.GeneratedPages`
- Reference `M:Penn.Generation.BuildReport.WriteTo(System.IO.TextWriter)` which outputs `"{SkippedPages.Count} pages skipped (draft)"` in the build summary

### Key points
- Drafts still render in dev mode (they respond to HTTP requests) -- the filtering happens at navigation and build generation time
- The `BuildReport` separates skipped pages from failed pages: `P:Penn.Generation.BuildReport.SkippedPages` vs `P:Penn.Generation.BuildReport.FailedPages`
- `P:Penn.Generation.BuildReport.TotalPages` includes skipped pages in the count

## Beat 3: Add Tags to Posts

Add `tags` arrays to posts and show the YAML syntax. Explain how tags flow through the pipeline.

### What to show
- YAML syntax: `tags: ["performance", "dotnet"]` on two posts, `tags: ["architecture"]` on a third -- parsed by `T:Penn.FrontMatter.FrontMatterParser` into `P:Penn.FrontMatter.ITaggable.Tags` (string array)
- Show both YAML list forms: inline `["a", "b"]` and block form with `- a` / `- b`
- Tags are available on the parsed front matter record and can be consumed by layout components
- Reference how search indexing uses tags: `T:Penn.Search.SearchIndexBuilder` accesses tags from front matter for search index entries
- Reference how RSS/sitemap builders skip drafts but include tagged content: `T:Penn.Feeds.SitemapBuilder` and `T:Penn.Feeds.RssFeedBuilder` both check `IDraftable`

### Key points
- Tags are plain strings with no taxonomy enforcement -- any string is valid
- The `ITaggable` interface provides read-only access; the front matter record owns the data
- Tags do not affect navigation ordering; they are metadata for filtering, display, and search

## Beat 4: Control Page Ordering

Add explicit `order` values to control sidebar position. Demonstrate the two-tier sort: explicit order first, then alphabetical by title.

### What to show
- Set `order: 10` on `allocation-traps.md`, `order: 20` on `span-patterns.md`, `order: 30` on `benchmarking-tips.md`, leave `config-pitfalls.md` without an `order` value
- Reference `P:Penn.FrontMatter.IOrderable.Order` (int) -- defaults to `int.MaxValue` in both `T:Penn.FrontMatter.DocFrontMatter` and `T:Penn.DocSite.DocSiteFrontMatter` when not specified in YAML
- In `M:Penn.Content.MarkdownContentService`1.GetContentTocEntriesAsync`, order is extracted: `var order = fm is IOrderable orderable ? orderable.Order : int.MaxValue;` and stored in `T:Penn.Content.ContentTocItem`
- Reference `T:Penn.Navigation.NavigationBuilder` and its `BuildLevel` method which sorts items: `.OrderBy(item => item.Order).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)`
- The same two-tier sort is applied in the final tree assembly: `builder.OrderBy(i => i.Order).ThenBy(i => i.Title, StringComparer.OrdinalIgnoreCase).ToImmutableList()`
- Show the before/after sidebar: without `order` values, pages sort alphabetically (A, B, C, S); with `order` values, they sort 10, 20, 30, then alphabetically for unordered pages

### Key points
- Pages without an explicit `order` get `int.MaxValue`, so they sort after all explicitly ordered pages
- Among pages with the same `order` value (or both without), alphabetical title is the tiebreaker
- The sort is case-insensitive (`StringComparer.OrdinalIgnoreCase`)
- Auto-created section nodes (folder headers) inherit the minimum `order` of their children for positioning

## Beat 5: Group Pages into Sections

Add `section` values to group pages under navigation headers. Show how directory structure creates implicit sections.

### What to show
- Set `section: "Deep Dives"` on `allocation-traps.md` and `span-patterns.md` -- the `P:Penn.FrontMatter.ISectionable.Section` property groups them under a "Deep Dives" header
- In `MarkdownContentService.GetContentTocEntriesAsync`: `var section = fm is ISectionable sectionable ? sectionable.Section : _options.Section;` -- if no explicit section, the service-level default applies
- Reference `T:Penn.Navigation.NavigationTreeItem` record with its `P:Penn.Navigation.NavigationTreeItem.Section` property and `P:Penn.Navigation.NavigationTreeItem.Children` for nested structure
- In `NavigationBuilder.BuildLevel`, when items at a depth have deeper descendants but no direct item, auto-created section nodes are generated with `FormatSectionTitle` converting kebab-case folder names to title case: `"getting-started"` becomes `"Getting Started"`
- Reference `P:Penn.Content.ContentTocItem.HierarchyParts` (string[]) which represents the URL path segments used to build the tree hierarchy

### Key points
- Explicit `section` in front matter groups pages regardless of directory structure
- Directory-based implicit sections are created when pages exist in subdirectories but no explicit section node is present
- Section nodes have an empty route (`CanonicalPath = ""`) -- they are headers, not navigable pages
- Sections can be nested: a directory structure `guides/configuration/` creates a two-level tree

## Beat 6: Navigation Tree Assembly

Walk through how `NavigationBuilder` assembles all capabilities into the final sidebar tree, including prev/next navigation.

### What to show
- Reference `M:Penn.Navigation.NavigationBuilder.BuildTree(System.Collections.Generic.IReadOnlyList{Penn.Content.ContentTocItem},Penn.Routing.ContentRoute,System.String)` which takes flat TOC items and builds a tree
- Reference `M:Penn.Navigation.NavigationBuilder.BuildNavigationInfo(System.Collections.Generic.IReadOnlyList{Penn.Content.ContentTocItem},Penn.Routing.ContentRoute,System.String)` which additionally computes prev/next and breadcrumbs
- Reference `T:Penn.Navigation.NavigationInfo` record: `SectionName`, `Breadcrumbs`, `PageTitle`, `PreviousPage`, `NextPage`
- The `Flatten` method does depth-first traversal for prev/next computation -- the order in the sidebar IS the prev/next order
- Reference `T:Penn.Navigation.BreadcrumbItem` for the breadcrumb trail from root to selected item

### Key points
- Draft pages are already filtered out before the tree is built (in `GetContentTocEntriesAsync`)
- The tree is rebuilt per request from the latest content service data; `NavigationBuilder` is stateless
- For multi-locale sites, `FilterByLocale` strips locale prefixes from hierarchy parts before building the tree

## Beat 7: Combining All Capabilities

Create a single post that uses all four capabilities simultaneously. Show that they compose independently.

### What to show
- A page with `isDraft: true`, `tags: ["conferences"]`, `order: 5`, `section: "Events"` -- all four capabilities on one front matter block
- Walk through how each capability is consumed independently:
  - `IDraftable` check in `ContentPipeline.GenerateAsync`: excluded from build output
  - `ITaggable`: tags are still on the record, available for search indexing if the page were published
  - `IOrderable`: the `order: 5` would position it first IF it were not a draft
  - `ISectionable`: would appear under "Events" section IF it were not a draft
- The draft check short-circuits everything: a draft page appears in `SkippedPages` regardless of its order, tags, or section
- Reference `T:Penn.DocSite.DocSiteFrontMatter` as the concrete type that implements all capabilities including `IRedirectable`: `:path src/Penn.DocSite/DocSiteFrontMatter.cs`

### Key points
- Capabilities are orthogonal: each interface controls exactly one behavior
- The pattern-matching approach (`fm is IDraftable { IsDraft: true }`) means capabilities degrade gracefully -- a front matter type that does not implement `IOrderable` simply gets `int.MaxValue` as its default order
- Custom front matter types can implement any subset of interfaces; only the implemented ones affect behavior
