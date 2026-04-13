---
title: "Built-in front-matter types"
description: "Keys supported by the four shipped front-matter records — DocFrontMatter, BlogFrontMatter, DocSiteFrontMatter, and BlogSiteFrontMatter."
section: "front-matter"
order: 30
tags: []
uid: reference.front-matter.built-in-types
isDraft: true
search: false
llms: false
---

> **In this page.** `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter` — which keys each supports, which template wires each one, and when to choose which.
>
> **Not in this page.** Defining custom front-matter types (see How-Tos).

## Summary

- _One sentence: what they are._ The four record types shipped by Pennington that implement `IFrontMatter` and can be bound directly by `MarkdownContentService<T>` without further customization.
- _One sentence: where they live._ `Pennington.FrontMatter.DocFrontMatter`, `Pennington.FrontMatter.BlogFrontMatter`, `Pennington.DocSite.DocSiteFrontMatter`, and `Pennington.BlogSite.BlogSiteFrontMatter`.

## Type matrix

| Type | Assembly | Content service / binding | Capability interfaces |
|---|---|---|---|
| `DocFrontMatter` | `Pennington` | Generic documentation pages bound via `AddMarkdownContent<DocFrontMatter>` | `IFrontMatter`, `ITaggable`, `ISectionable`, `IOrderable` |
| `BlogFrontMatter` | `Pennington` | Generic blog posts bound via `AddMarkdownContent<BlogFrontMatter>` (not used by `AddBlogSite`) | `IFrontMatter`, `ITaggable` |
| `DocSiteFrontMatter` | `Pennington.DocSite` | Pages served by the DocSite template (`AddDocSite` / `UseDocSite`) | `IFrontMatter`, `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable` |
| `BlogSiteFrontMatter` | `Pennington.BlogSite` | Posts served by the BlogSite template (`AddBlogSite` binds `AddMarkdownContent<BlogSiteFrontMatter>` internally) | `IFrontMatter`, `ITaggable`, `ISectionable`, `IRedirectable` |

> **Choosing between `BlogFrontMatter` and `BlogSiteFrontMatter`.** `AddBlogSite` wires `BlogSiteFrontMatter`, not `BlogFrontMatter`. Use `BlogFrontMatter` only when you call `AddMarkdownContent<BlogFrontMatter>` directly in a site that does **not** use `AddBlogSite`.

## `DocFrontMatter`

### Declaration

```csharp:xmldocid
T:Pennington.FrontMatter.DocFrontMatter
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Page title. Required by `IFrontMatter`. |
| `Description` | `string?` | `null` | Short page description surfaced in metadata and social cards. |
| `IsDraft` | `bool` | `false` | When `true`, the page is skipped during generation. |
| `Tags` | `string[]` | `[]` | Tag list. Implements `ITaggable`. |
| `Section` | `string?` | `null` | Section the page belongs to. Implements `ISectionable`. |
| `Uid` | `string?` | `null` | Cross-reference identifier for `<xref:uid>` / `href="xref:uid"`. |
| `Order` | `int` | `int.MaxValue` | Sort order within a section. Implements `IOrderable`. |
| `Search` | `bool` | `true` | When `false`, the page is excluded from the search index. |
| `Llms` | `bool` | `true` | When `false`, the page is excluded from `llms.txt` output. |

## `BlogFrontMatter`

### Declaration

```csharp:xmldocid
T:Pennington.FrontMatter.BlogFrontMatter
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Post title. Required by `IFrontMatter`. |
| `Description` | `string?` | `null` | Short post description surfaced in metadata and social cards. |
| `IsDraft` | `bool` | `false` | When `true`, the post is skipped during generation. |
| `Tags` | `string[]` | `[]` | Tag list. Implements `ITaggable`. |
| `Date` | `DateTime?` | `null` | Publication date used for ordering and feeds. |
| `Author` | `string?` | `null` | Post author name. |
| `Series` | `string?` | `null` | Series identifier for grouping related posts. |
| `Uid` | `string?` | `null` | Cross-reference identifier for `<xref:uid>` / `href="xref:uid"`. |
| `Search` | `bool` | `true` | When `false`, the post is excluded from the search index. |
| `Llms` | `bool` | `true` | When `false`, the post is excluded from `llms.txt` output. |

## `DocSiteFrontMatter`

### Declaration

```csharp:xmldocid
T:Pennington.DocSite.DocSiteFrontMatter
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Page title. Required by `IFrontMatter`. |
| `Description` | `string?` | `null` | Short page description surfaced in metadata and social cards. |
| `IsDraft` | `bool` | `false` | When `true`, the page is skipped during generation. |
| `Tags` | `string[]` | `[]` | Tag list. Implements `ITaggable`. |
| `Order` | `int` | `int.MaxValue` | Sort order within a section. Implements `IOrderable`. |
| `RedirectUrl` | `string?` | `null` | When set, the page emits a redirect to this URL. Implements `IRedirectable`. |
| `Section` | `string?` | `null` | Section the page belongs to. Implements `ISectionable`. |
| `Uid` | `string?` | `null` | Cross-reference identifier for `<xref:uid>` / `href="xref:uid"`. |
| `Search` | `bool` | `true` | When `false`, the page is excluded from the search index. |
| `Llms` | `bool` | `true` | When `false`, the page is excluded from `llms.txt` output. |

## `BlogSiteFrontMatter`

### Declaration

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteFrontMatter
```

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `"Empty title"` | Post title. Required by `IFrontMatter`; the record initializer provides the `"Empty title"` default so parsing never fails on a missing `title`. |
| `Author` | `string` | `""` | Post author name; overrides `BlogSiteOptions.AuthorName` when set. |
| `Description` | `string?` | `null` | Short post description surfaced in metadata, social cards, and RSS `<description>`. |
| `Repository` | `string` | `""` | Source-repository URL. When non-empty, `Pennington.BlogSite.Components.Layout.BlogPost` renders a "source repository" link card on the post. Template-only key — not present on any other built-in record. |
| `Date` | `DateTime?` | `null` | Publication date; drives ordering on the blog index and `/archive`, and populates RSS `<pubDate>`. |
| `IsDraft` | `bool` | `false` | When `true`, the post is skipped during generation. |
| `Tags` | `string[]` | `[]` | Tag list. Implements `ITaggable`; drives the `/tags/{TagEncodedName}` routes. |
| `Series` | `string` | `""` | Series identifier for grouping related posts. |
| `RedirectUrl` | `string?` | `null` | When set, the post emits a redirect to this URL. Implements `IRedirectable`. |
| `Section` | `string?` | `null` | Section the post belongs to. Implements `ISectionable`. |
| `Uid` | `string?` | `null` | Cross-reference identifier for `<xref:uid>` / `href="xref:uid"`. |
| `Search` | `bool` | `true` | When `false`, the post is excluded from the search index. |
| `Llms` | `bool` | `true` | When `false`, the post is excluded from `llms.txt` output. |

## See also

- Related reference: [`IFrontMatter`](/reference/front-matter/ifrontmatter)
- Related reference: [Front matter key reference](/reference/front-matter/keys)
- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
- How-to: [Define a custom front-matter type](/how-to/content-authoring/front-matter)
