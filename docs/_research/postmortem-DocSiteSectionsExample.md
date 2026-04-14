# Post-mortem — DocSiteSectionsExample

> **Resolution (2026-04-14):** All flagged items addressed. See plan at
> `~/.claude/plans/abstract-noodling-taco.md`.
>
> - **SE1 — `section:` is metadata, not a grouping key.** Hard-renamed
>   `ISectionable.Section` → `SectionLabel` across the capability interface,
>   `ContentTocItem`, `NavigationTreeItem`, `SearchIndexDocument`,
>   `DocSiteFrontMatter`, `BlogSiteFrontMatter`, `DocFrontMatter`,
>   `ApiFrontMatter`, `MarkdownContentServiceOptions`, `MarkdownContentOptions`,
>   `IContentService.DefaultSection` → `DefaultSectionLabel` (+ all 6
>   implementers), and the `TableOfContentsNavigation.Section` Razor param.
>   YAML front matter key `section:` → `sectionLabel:` across 40 example
>   and template files.
> - **SE2 — Section sort rule (min-of-children + alphabetic tie-break).**
>   Baked into `docs/_research/examples-inventory.md` DocSiteSectionsExample
>   prose and into the §1.2.30 entry in `docs/docs-toc.md` as authoring
>   guidance (stagger `order:` across sibling sections, e.g. 10/20 + 40/50).
> - **SE3 — Area index not in its own sidebar TOC.** Fixed in
>   `NavigationBuilder.BuildLevel`: at tree root, items with empty
>   `HierarchyParts` (area index after `GetTocItemsForAreaAsync` strips the
>   prefix) are inserted first as the "overview" entry with
>   `Order = int.MinValue`. Verified on `DocSiteSectionsExample` — "Guides"
>   now lands at the top of its sidebar linking to `/guides/`.

## What was built

`examples/DocSiteSectionsExample/` — the third DocSite app. Same host shape
as #4/#5 (`AddDocSite` + `UseDocSite` + `RunDocSiteAsync`), no new DI. Two
areas (`Guides`, `Reference`); each area holds two subfolder-backed sections
(Getting Started + Advanced under guides, Core Api + Extensions under
reference). **Counts: 2 areas, 4 sections, 11 markdown pages** (2 area
index pages + 9 section pages at 2–3 per section). Stage files:
`Stage1_FlatArea.cs` (front matter with no `section:`/`order:`) and
`Stage2_SectionAndOrder.cs` (same page moved under a subfolder, `section:`
+ `order: 10` added) — both are `public static string Source()` in the
#5 idiom.

## Does NavigationBuilder need an explicit call?

**No.** `MainLayout.OnInitializedAsync` already iterates `DocSiteOptions.Areas`,
calls `ContentResolver.GetTocItemsForAreaAsync(locale, area)` for each, and
feeds the result into `NavBuilder.BuildTree(...)` to produce the per-area
tree dictionary the sidebar renders. App-level code for a DocSite site never
touches `NavigationBuilder` directly — it's a core-library concern DocSite
orchestrates. **Decision for kitchen-sink app #13:** only call
`NavigationBuilder` explicitly if that app also hosts a bare `AddPennington`
surface; inside a DocSite host, leave it implicit.

## How `section:` and `order:` are consumed — gotchas

- **`section:` is metadata, not a grouping key.** The YAML key flows through
  `MarkdownContentService` into `ContentTocItem.Section` and surfaces on
  `NavigationInfo.SectionName` (used by prev/next breadcrumbs). The sidebar
  **does not** group by it. What groups the sidebar is the **subfolder**
  under an area: `NavigationBuilder.BuildLevel` auto-creates a section node
  for every subfolder that has descendants and no direct item at the same
  depth. `FormatSectionTitle` converts the folder name from kebab-case to
  title case (`getting-started` → `Getting Started`).
- **Section sort order = min(order) of its children.** Two sections whose
  minimum-order pages tie break alphabetically on the auto-formatted folder
  name. Initial draft had `advanced/` order 10/20 and `getting-started/`
  order 10/20 — both mins tied at 10 so "Advanced" sorted first. Fixed by
  bumping `advanced/` to 40/50; kept clean 10/20/30/40/50 numbering per the
  repo's `feedback_numeric_constants` rule.
- **Area index page doesn't appear in its own TOC.** `Content/guides/index.md`
  strips to empty `HierarchyParts` after area-prefix removal, so it falls
  out of `BuildLevel`'s `itemsAtLevel` filter. That's consistent with app
  #4; the index still serves as the area landing page you hit by clicking
  the area tab.
- **Missing `order:` defaults to `int.MaxValue`** — unordered pages pile at
  the bottom, alphabetically. Missing `section:` is fine: grouping is
  folder-driven.

No blockers.
