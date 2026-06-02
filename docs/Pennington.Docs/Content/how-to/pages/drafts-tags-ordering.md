---
title: "Mark drafts, schedule posts, tag pages, and control sort order"
description: "Hide unfinished pages, embargo posts until a release date, attach grouping keywords, and choose where a page lands in its sidebar section using front-matter keys."
uid: how-to.pages.drafts-tags-ordering
order: 2
sectionLabel: "Pages"
tags: [front-matter, drafts, tags, ordering, scheduling]
---

To keep an unfinished page out of navigation, embargo a post until a release date, attach grouping keywords to a page, or change where a page appears within its sidebar section, set one of four front-matter keys. For how front matter is parsed, see <xref:how-to.pages.front-matter>.

## Before you begin
- A working Pennington site has markdown under `Content/` (see <xref:how-to.pages.front-matter> if not).
- Pages use a front-matter record that implements the capability each key relies on. `isDraft:` and `date:` are universally available; `tags:` requires `ITaggable`; `order:` requires `IOrderable`. The five shipped records — `DocFrontMatter`, `BlogFrontMatter`, `BlogPostFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter` — implement different subsets; see <xref:reference.front-matter.keys> for the per-record matrix.
- The sidebar currently renders in file-order; `TableOfContentsNavigation` has not been customized.

Setting `order:` on a `BlogSiteFrontMatter` or `BlogFrontMatter` page has no effect — blog posts sort newest-first by `date:`. To reorder posts, adjust the date; to hide a post, use `isDraft: true`.

## Options

### Hide an unfinished page with `isDraft: true`

Setting `isDraft: true` keeps the page compiled — `xref:` links targeting it still resolve — but drops it from navigation, search, and `llms.txt`.

```yaml
---
title: Coming soon
isDraft: true
---
```

The default is `false`. For the full key catalog, see <xref:reference.front-matter.keys>.

### Schedule a post for the future with `date:`

A page whose `date:` is later than the build-machine wall clock is treated the same as a draft: visible in `dotnet run` so you can preview, excluded from `dotnet run -- build` output, feeds, and search. As soon as the clock crosses the date, the next build picks the page up — no flag flip required.

```yaml
---
title: New feature announcement
date: 2030-11-14T09:00:00
---
```

The comparison uses the build server's local wall clock, so a date without a time component (`date: 2030-11-14`) releases at local midnight. CI that runs hourly will pick the post up on the first build after that boundary.

To override the wall clock — for tests, for re-running yesterday's build, or for previewing tomorrow's release — replace the registered `TimeProvider` in DI with a fixed one (the `Microsoft.Extensions.TimeProvider.Testing` package ships `FakeTimeProvider` for exactly this).

### Tag a page for grouping

`tags:` accepts a free-form string array that flows through `ITaggable` into `RenderedContent.Tags`, making it available to client-side filtering widgets. Tags do not produce `/tags/<name>` index pages on their own; to generate browse-by-tag pages from a tagged front-matter record, register a taxonomy — see <xref:how-to.content-services.taxonomy>.

```yaml
---
title: Deep dive
tags: [advanced, performance, pipeline]
---
```

Backing symbol on the DocSite front-matter record:

```csharp:symbol
src/Pennington.DocSite/DocSiteFrontMatter.cs > DocSiteFrontMatter.Tags
```

### Order a page inside its section

Lower `order:` values sort earlier within a section. Spacing like 10/20/30 leaves room for later inserts between existing siblings.

```yaml
---
title: Install
order: 201020
---
```

For how sections inherit their own sort key from child `order:` values, see <xref:explanation.routing.navigation-tree>.

## Verify

- Run `dotnet run` — the drafted page's URL still responds 200 but is absent from the sidebar and from the per-locale search index under `/search/{locale}/` (for example `/search/en/index.json`)
- A page with a future `date:` behaves the same way: URL responds in dev, absent from sidebar/search/RSS; `dotnet run -- build` lists it under "Skipped"
- The tagged page's HTML carries the tag strings in its rendered output (inspect `RenderedContent.Tags` or the page body)
- Sidebar entries within the section appear in ascending `order:` — swap two values and the order flips on next reload

## Related

- Reference: <xref:reference.front-matter.keys>
- How-to: <xref:how-to.navigation.customize-sidebar>
- How-to: <xref:how-to.content-services.taxonomy>
- Background: <xref:explanation.core.front-matter-capabilities>
