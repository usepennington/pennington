---
title: "Manage drafts, tags, and ordering"
description: "Hide unfinished pages, group related pages with tags, and choose their sidebar position with three front-matter keys."
uid: how-to.content-authoring.drafts-tags-ordering
order: 201020
sectionLabel: Content Authoring
tags: [front-matter, drafts, tags, ordering]
---

When a working DocSite needs to keep an unfinished page out of navigation, attach grouping keywords to a page, or control where a page appears within its sidebar section, three front-matter keys cover it. For how front matter is parsed in the first place, see <xref:how-to.content-authoring.front-matter>.

## Assumptions

- A working Pennington site has markdown under `Content/` (see <xref:how-to.content-authoring.front-matter> if not)
- Pages use `DocSiteFrontMatter` (the default after `AddDocSite`) or another type that implements `ITaggable` + `IOrderable`
- The sidebar currently renders in file-order; `TableOfContentsNavigation` has not been customized

The `DocSiteKitchenSinkExample` project includes a fixture that uses all three keys in one file — see the `markdown:path` embed in step 3 below.

---

## Steps

<Steps>
<Step StepNumber="1">

**Hide an unfinished page with `isDraft: true`**

Setting `isDraft: true` keeps the page compiled — `xref:` links targeting it still resolve — but drops it from navigation, search, and `llms.txt`.

```yaml
---
title: Coming soon
isDraft: true
---
```

The default is `false`. For the full key catalog, see <xref:reference.front-matter.keys>.

</Step>
<Step StepNumber="2">

**Tag a page for grouping**

`tags:` accepts a free-form string array that flows through `ITaggable` into `RenderedContent.Tags`, making it available to client-side filtering widgets and future tag-index pages. Pennington does not currently emit `/tags/<name>` pages for DocSite — tag routing requires a custom content service.

```yaml
---
title: Deep dive
tags: [advanced, performance, pipeline]
---
```

For the `DocSiteFrontMatter` shape that backs this key:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Tags
```

</Step>
<Step StepNumber="3">

**Order a page inside its section**

Lower `order:` values sort earlier within a section. A section inherits its own sort key from the minimum `order:` among its children, so changing one page can reshuffle the whole section. Spacing like 10/20/30 leaves room for later inserts between existing siblings.

```yaml
---
title: Install
order: 10
---
```

For the backing symbol:

```csharp:xmldocid
P:Pennington.DocSite.DocSiteFrontMatter.Order
```

To see all three keys combined in one file:

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/drafts-tags-ordering.md
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run` — the drafted page's URL still responds 200 but is absent from the sidebar and from `/search-index.json`
- The tagged page's HTML carries the tag strings in its rendered output (inspect `RenderedContent.Tags` or the page body)
- Sidebar entries within the section appear in ascending `order:` — swap two values and the order flips on next reload

## Related

- Reference: <xref:reference.front-matter.keys>
- How-to: <xref:how-to.content-authoring.customize-sidebar>
- Background: <xref:explanation.core.front-matter-capabilities>
