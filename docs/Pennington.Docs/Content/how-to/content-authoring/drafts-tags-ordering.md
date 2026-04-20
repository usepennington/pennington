---
title: "Mark drafts, tag pages, and control sort order"
description: "Hide unfinished pages, attach grouping keywords, and choose where a page lands in its sidebar section using three front-matter keys."
uid: how-to.content-authoring.drafts-tags-ordering
order: 201020
sectionLabel: Content Authoring
tags: [front-matter, drafts, tags, ordering]
---

To keep an unfinished page out of navigation, attach grouping keywords to a page, or change where a page appears within its sidebar section, set one of three front-matter keys. For how front matter is parsed, see <xref:how-to.content-authoring.front-matter>.

## Assumptions

- A working Pennington site has markdown under `Content/` (see <xref:how-to.content-authoring.front-matter> if not)
- Pages use a front-matter record that implements the capability each key relies on (see the applicability note below)
- The sidebar currently renders in file-order; `TableOfContentsNavigation` has not been customized

### Which front-matter types support which keys

`isDraft:` is always available — `IFrontMatter` has a default `IsDraft` member that every record inherits. `tags:` and `order:` only apply to records that opt in through `ITaggable` and `IOrderable` respectively.

| Front-matter type | `isDraft:` | `tags:` | `order:` |
|---|---|---|---|
| `DocSiteFrontMatter` (default after `AddDocSite`) | yes | yes | yes |
| `BlogSiteFrontMatter` (default after `AddBlogSite`) | yes | yes | **no** — date-driven ordering instead |
| `DocFrontMatter` (bare-host default) | yes | yes | yes |
| `BlogFrontMatter` (bare-host default) | yes | yes | **no** |
| Custom record | depends on which capability interfaces it implements | | |

Setting `order:` on a `BlogSiteFrontMatter` page has no effect — blog posts sort newest-first by `date:`. To reorder posts, adjust the date; to hide a post, use `isDraft: true`.

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

`tags:` accepts a free-form string array that flows through `ITaggable` into `RenderedContent.Tags`, making it available to client-side filtering widgets and future tag-index pages. Pennington does not currently emit `/tags/<name>` pages for DocSite — tag routing requires a custom content service.

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

Lower `order:` values sort earlier within a section. A section inherits its own sort key from the minimum `order:` among its children, so changing one page can reshuffle the whole section. Spacing like 10/20/30 leaves room for later inserts between existing siblings.

```yaml
---
title: Install
order: 10
---
```

Backing symbol:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Order
```

## Verify

- Run `dotnet run` — the drafted page's URL still responds 200 but is absent from the sidebar and from `/search-index.json`
- The tagged page's HTML carries the tag strings in its rendered output (inspect `RenderedContent.Tags` or the page body)
- Sidebar entries within the section appear in ascending `order:` — swap two values and the order flips on next reload

## Related

- Reference: <xref:reference.front-matter.keys>
- How-to: <xref:how-to.content-authoring.customize-sidebar>
- Background: <xref:explanation.core.front-matter-capabilities>
