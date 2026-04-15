---
title: "Built-in front-matter types"
description: "The four front-matter records Pennington ships — DocFrontMatter, BlogFrontMatter, DocSiteFrontMatter, and BlogSiteFrontMatter — with keys, capabilities, and which template wires each."
sectionLabel: "Front Matter"
order: 402030
tags: [front-matter, records, templates]
uid: reference.front-matter.built-in-types
---

Pennington ships four ready-made `IFrontMatter` records covering the doc and blog use cases at both the core-library and site-template layers. Core records live in `Pennington.FrontMatter`; the template records live in `Pennington.DocSite` and `Pennington.BlogSite`.

## Overview

| Type | Namespace | Wired by | Capabilities |
|---|---|---|---|
| `DocFrontMatter` | `Pennington.FrontMatter` | `PenningtonOptions.AddMarkdownContent<DocFrontMatter>` (author-wired) | `ITaggable`, `ISectionable`, `IOrderable` |
| `BlogFrontMatter` | `Pennington.FrontMatter` | `PenningtonOptions.AddMarkdownContent<BlogFrontMatter>` (author-wired) | `ITaggable` |
| `DocSiteFrontMatter` | `Pennington.DocSite` | `AddDocSite` | `ITaggable`, `ISectionable`, `IOrderable`, `IRedirectable` |
| `BlogSiteFrontMatter` | `Pennington.BlogSite` | `AddBlogSite` | `ITaggable`, `ISectionable`, `IRedirectable` |

> **Template binding.** `AddBlogSite` registers `AddMarkdownContent<BlogSiteFrontMatter>` — **not** `BlogFrontMatter`. The `BlogFrontMatter` record is the core-library's blog shape; `BlogSiteFrontMatter` is the site-template record with additional post-metadata fields (`Author`, `Repository`, `Series`, `RedirectUrl`).

## `DocFrontMatter`

```csharp:xmldocid
T:Pennington.FrontMatter.DocFrontMatter
```

The core-library doc-page record for bare `AddPennington` hosts. Implements `IFrontMatter`, `ITaggable`, `ISectionable`, and `IOrderable`.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Description` | `string?` | `null` | Page description used for `<meta name="description">` and list-card previews. |
| `IsDraft` | `bool` | `false` | When `true`, the page is excluded from static build output. |
| `Llms` | `bool` | `true` | When `false`, the page is excluded from `llms.txt` and its stripped-markdown sidecar. |
| `Order` | `int` | `int.MaxValue` | Sidebar sort key within the page's section (lower = earlier); capability `IOrderable`. |
| `Search` | `bool` | `true` | When `false`, the page is excluded from the per-locale search index. |
| `SectionLabel` | `string?` | `null` | Breadcrumb and prev/next label for the section this page belongs to; capability `ISectionable`. Does not drive sidebar grouping — the subfolder does. |
| `Tags` | `string[]` | `[]` | Tag array used by `ITaggable` consumers (tag-index pages, search facets). |
| `Title` | `string` | `""` | Required page title from `IFrontMatter.Title`. |
| `Uid` | `string?` | `null` | Optional cross-reference id resolved by `<xref:uid>`. |

## `BlogFrontMatter`

```csharp:xmldocid
T:Pennington.FrontMatter.BlogFrontMatter
```

The core-library blog-post record for author-wired blog hosts. Carries `Date`, `Author`, and `Series` alongside the `IFrontMatter` defaults. Implements `IFrontMatter` and `ITaggable`. Not the record bound by `AddBlogSite`.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Author` | `string?` | `null` | Post author display name. |
| `Date` | `DateTime?` | `null` | Publication date surfaced in listings, RSS `<pubDate>`, and sitemap `<lastmod>`. |
| `Description` | `string?` | `null` | Post description used for `<meta>` tags and feed summaries. |
| `IsDraft` | `bool` | `false` | When `true`, the post is excluded from static build output. |
| `Llms` | `bool` | `true` | When `false`, the post is excluded from `llms.txt` output. |
| `Search` | `bool` | `true` | When `false`, the post is excluded from the search index. |
| `Series` | `string?` | `null` | Series slug grouping posts under a multi-part title. |
| `Tags` | `string[]` | `[]` | Tag array used by `ITaggable` consumers. |
| `Title` | `string` | `""` | Required post title. |
| `Uid` | `string?` | `null` | Optional cross-reference id. |

## `DocSiteFrontMatter`

```csharp:xmldocid
T:Pennington.DocSite.DocSiteFrontMatter
```

The record bound by `AddDocSite`. Extends the `DocFrontMatter` shape with `RedirectUrl` via `IRedirectable`. Implements `IFrontMatter`, `ITaggable`, `ISectionable`, `IOrderable`, and `IRedirectable`.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Description` | `string?` | `null` | Page description used for `<meta>` tags and card previews. |
| `IsDraft` | `bool` | `false` | When `true`, the page is excluded from static build output. |
| `Llms` | `bool` | `true` | When `false`, the page is excluded from `llms.txt` output. |
| `Order` | `int` | `int.MaxValue` | Sidebar sort key within the page's section; capability `IOrderable`. |
| `RedirectUrl` | `string?` | `null` | Target URL for a meta-refresh redirect stub (with `noindex`); capability `IRedirectable`. |
| `Search` | `bool` | `true` | When `false`, the page is excluded from the search index. |
| `SectionLabel` | `string?` | `null` | Breadcrumb and prev/next label; capability `ISectionable`. |
| `Tags` | `string[]` | `[]` | Tag array used by `ITaggable` consumers. |
| `Title` | `string` | `""` | Required page title. |
| `Uid` | `string?` | `null` | Optional cross-reference id. |

## `BlogSiteFrontMatter`

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteFrontMatter
```

The record bound by `AddBlogSite`. Consolidates all post-authoring fields (`Author`, `Repository`, `Series`, `Date`, `RedirectUrl`) in one contract. Implements `IFrontMatter`, `ITaggable`, `ISectionable`, and `IRedirectable`.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Author` | `string` | `""` | Post author display name rendered on the post and in the RSS `<author>` element. |
| `Date` | `DateTime?` | `null` | Publication date used for listings, RSS `<pubDate>`, and sitemap `<lastmod>`. |
| `Description` | `string?` | `null` | Post description used for `<meta>` tags and feed summaries. |
| `IsDraft` | `bool` | `false` | When `true`, the post is excluded from static build output. |
| `Llms` | `bool` | `true` | When `false`, the post is excluded from `llms.txt` output. |
| `RedirectUrl` | `string?` | `null` | Target URL for a meta-refresh redirect stub; capability `IRedirectable`. |
| `Repository` | `string` | `""` | Source-repository URL rendered as a "source repository" link card on the post. |
| `Search` | `bool` | `true` | When `false`, the post is excluded from the search index. |
| `SectionLabel` | `string?` | `null` | Breadcrumb and prev/next label; capability `ISectionable`. |
| `Series` | `string` | `""` | Series slug grouping posts under a multi-part title. |
| `Tags` | `string[]` | `[]` | Tag array used by `ITaggable` consumers. |
| `Title` | `string` | `"Empty title"` | Required post title. Default is the literal string `"Empty title"`, not `""`. |
| `Uid` | `string?` | `null` | Optional cross-reference id. |

## Choosing a type

- `DocFrontMatter` — bare `AddPennington` host for documentation; provides `SectionLabel`, `Order`, and `Tags` without the DocSite template.
- `BlogFrontMatter` — bare `AddPennington` host for a blog; provides `Date`, `Author`, and `Series` without the BlogSite template. `AddBlogSite` does **not** bind this record.
- `DocSiteFrontMatter` — `AddDocSite` hosts; the template registers `AddMarkdownContent<DocSiteFrontMatter>` automatically.
- `BlogSiteFrontMatter` — `AddBlogSite` hosts; the template registers `AddMarkdownContent<BlogSiteFrontMatter>` automatically.

## Example

```csharp:xmldocid,bodyonly
M:BlogSiteFirstPostExample.Stage2.Source
```

## See also

- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Related reference: [Front matter key reference](xref:reference.front-matter.keys)
- Related reference: [`IFrontMatter` and capability defaults](xref:reference.front-matter.ifrontmatter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
