---
title: "Manage drafts, tags, and ordering"
description: "Hide unfinished pages, group related pages with tags, and choose their sidebar position with three front-matter keys."
uid: how-to.content-authoring.drafts-tags-ordering
order: 20
sectionLabel: Content Authoring
tags: [front-matter, drafts, tags, ordering]
---

> **In this page.** _Paraphrase TOC "Covers": hiding pages with `isDraft: true`, using `tags` for grouping, and using `order` to control sidebar position within a section. Two sentences max._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": tag-index pages and custom taxonomy generation require a custom content service — link out to the extensibility how-to on authoring a content service when that page lands._

## When to use this

_One to two sentences. Frame the realistic arrival state: the reader already has a DocSite with a few pages in `Content/`, the sidebar auto-builds from folder layout, and now they need to (a) keep an unfinished page out of nav, (b) attach grouping keywords, or (c) nudge a page up or down in the sidebar. Do not re-teach how front matter is parsed — point back to the front-matter how-to for that._

## Assumptions

_Keep to 3 bullets. Prior state must already include a working DocSite with at least two pages in one section so "ordering" is meaningful._

- You have a working Pennington site with markdown under `Content/` (see [Work with front matter](xref:how-to.content-authoring.front-matter) if not)
- Your pages use `DocSiteFrontMatter` (the default when you called `AddDocSite`) or another type that implements `ITaggable` + `IOrderable`
- The sidebar currently renders in file-order and you have not customized `TableOfContentsNavigation`

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — the `Content/main/drafts-tags-ordering.md` fixture uses all three keys in one file. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Three steps, one per front-matter key. Each step opens with an imperative verb ("Hide…", "Tag…", "Order…"), shows the YAML fragment in a plain `yaml` fence (the authored source is a markdown file, not a C# symbol), and closes with one sentence on what actually happens to the output. Keep prose under two sentences per step._

### 1. Hide an unfinished page with `isDraft: true`

_One sentence: setting `isDraft: true` keeps the page compiled (so `xref:` links still resolve) but drops it from navigation, search, and `llms.txt`. Show the minimal front-matter fragment inline — this is markdown content, so a plain `yaml` fence is correct; do not use xmldocid here._

```yaml
---
title: Coming soon
isDraft: true
---
```

_Optional closer (one line): the default is `false` and comes from the `IFrontMatter` default member — link to the reference page for the full key catalog._

### 2. Tag a page for grouping

_One sentence: `tags:` accepts a free-form string array that flows through `ITaggable` into `RenderedContent.Tags`, available to client-side filtering widgets and future tag-index pages. Tags are intentionally uninterpreted — Pennington does not currently emit `/tags/<name>` pages for DocSite (only BlogSite does)._

```yaml
---
title: Deep dive
tags: [advanced, performance, pipeline]
---
```

_Cross-reference: for the `DocSiteFrontMatter` shape that backs the YAML key, the symbol is:_

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Tags
```

### 3. Order a page inside its section

_Two sentences. Lower `order:` values sort earlier within a section; the section itself inherits its sort key from the minimum `order:` among its children, so bumping one page can reshuffle the whole section. Recommend the 10/20/30 spacing convention so later inserts get room to land between siblings._

```yaml
---
title: Install
order: 10
---
```

_Show the backing symbol for completeness:_

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Order
```

_Optional: fence the whole example page so readers see the three keys combined in one file:_

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/drafts-tags-ordering.md
```

---

## Verify

_Terse. One bullet per key so the reader can confirm each independently without reading anything else._

- Run `dotnet run`; the drafted page's URL still responds 200 but is absent from the sidebar and from `/search-index.json`
- The tagged page's `/{url}` HTML carries the tag strings in its rendered output (inspect `RenderedContent.Tags` or the page body)
- Sidebar entries within the section appear in ascending `order:` — swap two values and the order flips on next reload

## Related

_Two to four cross-quadrant links. Point at Reference for the full key table, the sibling how-to for sidebar shaping, and the Explanation page for the capability-interface background. Do not link to the next how-to in this section — generated automatically._

- Reference: [Front matter key reference](xref:reference.front-matter.keys)
- How-to: [Customize the sidebar](xref:how-to.content-authoring.customize-sidebar)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
