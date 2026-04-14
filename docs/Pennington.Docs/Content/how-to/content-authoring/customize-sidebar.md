---
title: "Customize the sidebar"
description: "Reorder pages, promote a page to a section landing, override a section title, and hide a page from navigation."
section: content-authoring
order: 30
tags: []
uid: how-to.content-authoring.customize-sidebar
isDraft: true
search: false
llms: false
---

> **In this page.** Reordering pages with `order:`, promoting a page to be its section's landing, overriding the auto-derived section title, and hiding a page from navigation.
>
> **Not in this page.** Replacing `TableOfContentsNavigation` with a custom component — that lives under the extensibility quadrant.

## When to use this

- You want the sidebar to reflect a specific reading order or group structure beyond what the default folder-to-tree fold produces.
- You have a section folder whose displayed name or landing behavior doesn't match the defaults.

## Assumptions

- A working Pennington site with at least one `AddMarkdownContent<T>(...)` registration.
- Your front-matter record implements `ISectionable` (for `section:`) and `IOrderable` (for `order:`). Both are already on `DocFrontMatter` and `DocSiteFrontMatter`.
- `BlogFrontMatter` and `BlogSiteFrontMatter` do **not** implement `IOrderable`, so `order:` is a no-op on blog posts — this page targets doc-style sites.

To copy a working setup, see [`examples/BeaconDocsExample`](https://github.com/usepennington/pennington/tree/main/examples/BeaconDocsExample) — it ships a sectioned `Content/` tree with `order:` values on each page. Do not walk the example — this page is a recipe.

---

## Steps

### 1. Reorder pages with `order:`

Set `order:` to tidy increments in front matter. Lower sorts first; an unset value falls to `int.MaxValue` (the end).

```yaml
---
title: "Installation"
order: 20
---
```

- Prefer `10 / 20 / 30 / 40` sequences so later inserts don't force renumbering.
- Avoid negative values and round-thousand gaps — see `drafts-tags-ordering` for the full rationale.
- `NavigationBuilder` sorts siblings by `Order` ascending, then by `Title` (`StringComparer.OrdinalIgnoreCase`) as a tiebreaker.

### 2. Promote a page to be its section's landing page

A section folder with no direct page becomes a non-clickable heading in the sidebar. To make the heading clickable, add a page inside the folder whose `section:` matches the folder and whose `order:` is lower than every sibling.

```yaml
---
title: "Getting Started"
section: "getting-started"
order: 0
---
```

- File name convention: `index.md` is common but not required — any markdown file in the folder works.
- `NavigationBuilder` picks the lowest-ordered page as the group anchor when there is no synthesized section node to displace.
- Synthesized section nodes (from folders without a landing page) are deliberately unroutable — empty `CanonicalPath`, no click target. Adding a landing page replaces that synthesized node.

### 3. Override the displayed section title

Auto-created section headings derive their title from the folder name. `NavigationBuilder.FormatSectionTitle` splits on `-` and title-cases each word, so `getting-started` becomes `Getting Started`.

To override, create a section-landing page (step 2) and set `title:` to the label you want shown:

```yaml
---
title: "First steps"
section: "getting-started"
order: 0
---
```

- The landing page's `Title` is what the sidebar renders — the folder name is only used when no landing page exists.
- Renaming the folder also changes the auto-derived title; the landing-page override is the non-destructive path.

### 4. Hide a page from the sidebar

Set `isDraft: true` on the page's front matter.

```yaml
---
title: "Upcoming feature"
isDraft: true
---
```

- Draft pages render under `dotnet run` so you can preview them, and are excluded from navigation, sitemap, search, RSS, and build output (they appear under `BuildReport.SkippedPages`).
- There is no "in build output, out of sidebar" switch. If you need that, write a custom `IContentService` that populates `ContentTocItem` selectively — the built-in front-matter flags are all-or-nothing.
- See [drafts, tags, and ordering](/how-to/content-authoring/drafts-tags-ordering) for the full draft semantics.

### 5. Verify

Run the site and compare the sidebar to the file tree.

```bash
dotnet run --project src/MySite
```

- `order:` values drive sibling ordering (lower first; unset at the end).
- A section folder with an `order: 0` landing page has a clickable group anchor; a folder without one renders a non-clickable heading.
- Draft pages are absent from the sidebar but still reachable via direct URL under `dotnet run`.
- Renaming the folder (or adding a matching landing page with a new `title:`) changes the displayed section title.

---

## Verify

- Sidebar ordering matches the `order:` values set on siblings.
- Section headings are clickable iff the folder contains a landing page, and display the landing page's `title:` when present.
- Draft pages are missing from the sidebar.

## Related

- How-to: [Manage drafts, tags, and ordering](/how-to/content-authoring/drafts-tags-ordering)
- Reference: [Front matter key reference](/reference/front-matter/keys)
- Reference: [Navigation UI components](/reference/ui/navigation)
- Background: [Navigation-tree construction](/explanation/routing/navigation-tree)
