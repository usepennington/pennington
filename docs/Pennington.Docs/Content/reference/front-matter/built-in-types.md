---
title: "Built-in front-matter types"
description: "The four front-matter records Pennington ships — DocFrontMatter, BlogFrontMatter, DocSiteFrontMatter, and BlogSiteFrontMatter — with keys, capabilities, and which template wires each."
sectionLabel: "Front Matter"
order: 402030
tags: [front-matter, records, templates]
uid: reference.front-matter.built-in-types
---

Pennington ships four ready-made `IFrontMatter` records covering the doc and blog use cases at both the core-library and site-template layers.

| Type | Namespace | Wired by | Capabilities |
|---|---|---|---|
| `DocFrontMatter` | `Pennington.FrontMatter` | `AddMarkdownContent<DocFrontMatter>` (author-wired) | `ITaggable`, `ISectionable`, `IOrderable` |
| `BlogFrontMatter` | `Pennington.FrontMatter` | `AddMarkdownContent<BlogFrontMatter>` (author-wired) | `ITaggable` |
| `DocSiteFrontMatter` | `Pennington.DocSite` | `AddDocSite` | `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable` |
| `BlogSiteFrontMatter` | `Pennington.BlogSite` | `AddBlogSite` | `ITaggable`, `ISectionable`, `IRedirectable` |

> **Template binding.** `AddBlogSite` registers `AddMarkdownContent<BlogSiteFrontMatter>` — **not** `BlogFrontMatter`.

## `DocFrontMatter`

<ApiSummary XmlDocId="T:Pennington.FrontMatter.DocFrontMatter" />

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.DocFrontMatter" />

## `BlogFrontMatter`

<ApiSummary XmlDocId="T:Pennington.FrontMatter.BlogFrontMatter" />

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.BlogFrontMatter" />

## `DocSiteFrontMatter`

<ApiSummary XmlDocId="T:Pennington.DocSite.DocSiteFrontMatter" />

<ApiMemberTable XmlDocId="T:Pennington.DocSite.DocSiteFrontMatter" />

## `BlogSiteFrontMatter`

<ApiSummary XmlDocId="T:Pennington.BlogSite.BlogSiteFrontMatter" />

<ApiMemberTable XmlDocId="T:Pennington.BlogSite.BlogSiteFrontMatter" />

## Example

```markdown:path
examples/BlogSiteFirstPostExample/snippets/stage2.md
```

## See also

- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Related reference: [Front matter key reference](xref:reference.front-matter.keys)
- Related reference: [`IFrontMatter` and capability defaults](xref:reference.front-matter.ifrontmatter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
