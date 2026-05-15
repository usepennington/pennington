---
title: "Mark drafts, tag pages, and control sort order"
description: "Hide unfinished pages, attach grouping keywords, and choose where a page lands in its sidebar section using three front-matter keys."
uid: how-to.pages.drafts-tags-ordering
order: 201020
sectionLabel: "Pages"
tags: [front-matter, drafts, tags, ordering]
---

To keep an unfinished page out of navigation, attach grouping keywords to a page, or change where a page appears within its sidebar section, set one of three front-matter keys. For how front matter is parsed, see <xref:how-to.pages.front-matter>.

## Before you begin
- A working Pennington site has markdown under `Content/` (see <xref:how-to.pages.front-matter> if not).
- Pages use a front-matter record that implements the capability each key relies on. `isDraft:` is universally available; `tags:` requires `ITaggable`; `order:` requires `IOrderable`. The four shipped records — `DocFrontMatter`, `DocSiteFrontMatter`, `BlogFrontMatter`, `BlogSiteFrontMatter` — implement different subsets; see <xref:reference.front-matter.keys> for the per-record matrix.
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

### Tag a page for grouping

`tags:` accepts a free-form string array that flows through `ITaggable` into `RenderedContent.Tags`, making it available to client-side filtering widgets. Tags do not produce `/tags/<name>` index pages on their own; to generate browse-by-tag pages from a tagged front-matter record, register a taxonomy — see <xref:how-to.content-services.taxonomy>.

```yaml
---
title: Deep dive
tags: [advanced, performance, pipeline]
---
```

Backing symbol on the DocSite front-matter record:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Tags
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

- Run `dotnet run` — the drafted page's URL still responds 200 but is absent from the sidebar and from `/search-index.json`
- The tagged page's HTML carries the tag strings in its rendered output (inspect `RenderedContent.Tags` or the page body)
- Sidebar entries within the section appear in ascending `order:` — swap two values and the order flips on next reload

## Related

- Reference: <xref:reference.front-matter.keys>
- How-to: <xref:how-to.navigation.customize-sidebar>
- How-to: <xref:how-to.content-services.taxonomy>
- Background: <xref:explanation.core.front-matter-capabilities>
