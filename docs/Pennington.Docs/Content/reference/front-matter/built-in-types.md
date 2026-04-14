---
title: "Built-in front-matter types"
description: "The four front-matter records Pennington ships — DocFrontMatter, BlogFrontMatter, DocSiteFrontMatter, and BlogSiteFrontMatter — with keys, capabilities, and which template wires each."
sectionLabel: "Front Matter"
order: 30
tags: [front-matter, records, templates]
uid: reference.front-matter.built-in-types
---

> **In this page.** _One sentence: the four built-in front-matter records (`DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`), the keys each supports, the capability interfaces each implements, the template that wires it, and the one-line "pick X when" decision rows._
>
> **Not in this page.** _One sentence: defining your own front-matter type is a task recipe — see the How-To [Work with front matter](xref:how-to.content-authoring.front-matter)._

## Summary

_**One sentence: what it is.** The four record types Pennington ships as ready-made `IFrontMatter` implementations, covering the doc and blog use cases at both the core-library and site-template layers._
_**One sentence: where it lives.** Core records live in namespace `Pennington.FrontMatter` (`src/Pennington/FrontMatter/`); the template records live in `Pennington.DocSite` and `Pennington.BlogSite`._

## Overview

_Four-row summary table keyed by type. Columns: **Type**, **Namespace**, **Wired by**, **Capabilities**. One row each for `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`. "Wired by" names the extension method that binds the record as the `AddMarkdownContent<T>` type parameter (`PenningtonOptions.AddMarkdownContent<T>` when the author wires it themselves for the core records; `AddDocSite` for `DocSiteFrontMatter`; `AddBlogSite` for `BlogSiteFrontMatter`). Capabilities column lists the capability interfaces each record implements — `IFrontMatter` is universal and not repeated._

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

_One sentence placing the record: the core-library's doc-page record, meant for bare `AddPennington` hosts that want a doc-shaped metadata contract without adopting the DocSite template. Implements `IFrontMatter, ITaggable, ISectionable, IOrderable`._

### Properties

_Alphabetical by key. `IsDraft`, `Search`, `Llms`, `Uid`, and `Description` come from `IFrontMatter` defaults — listed here because they are declared on the record and parsed from YAML._

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

_One sentence placing the record: the core-library's blog-post record for author-wired blog hosts, carrying `Date`, `Author`, and `Series` alongside the `IFrontMatter` defaults. Implements `IFrontMatter, ITaggable`. Not the record bound by `AddBlogSite`._

### Properties

_Alphabetical by key._

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

_One sentence placing the record: the record bound by `AddDocSite`; extends the `DocFrontMatter` shape with `RedirectUrl` via `IRedirectable` so doc pages can emit meta-refresh stubs. Implements `IFrontMatter, ITaggable, ISectionable, IOrderable, IRedirectable`._

### Properties

_Alphabetical by key._

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

_One sentence placing the record: the record bound by `AddBlogSite` — the BlogSite template's post-metadata contract. Carries every post-authoring field in one place (`Author`, `Repository`, `Series`, `Date`, `RedirectUrl`) and implements `IFrontMatter, ITaggable, ISectionable, IRedirectable`._

### Properties

_Alphabetical by key. Note the non-null defaults on `Author`, `Repository`, and `Series` — these are empty strings rather than `null`, so posts that omit the key still bind successfully._

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

_Four one-line decision rows. Product-dictated — no rationale, no narrative._

- Pick `DocFrontMatter` when you run a bare `AddPennington` host for documentation and want doc-shaped metadata (`SectionLabel`, `Order`, `Tags`) without the DocSite template.
- Pick `BlogFrontMatter` when you run a bare `AddPennington` host for a blog and want `Date`/`Author`/`Series` without the BlogSite template — `AddBlogSite` does **not** bind this record.
- Pick `DocSiteFrontMatter` when you use `AddDocSite` — it is the record the template registers via `AddMarkdownContent<DocSiteFrontMatter>`, with `RedirectUrl` on top of `DocFrontMatter`.
- Pick `BlogSiteFrontMatter` when you use `AddBlogSite` — it is the record the template registers via `AddMarkdownContent<BlogSiteFrontMatter>`, and it adds `Author`, `Repository`, `Series`, `RedirectUrl` over `BlogFrontMatter`.

## Example

_One minimal example from the tutorial backing project: a fully-populated `BlogSiteFrontMatter` block pulled from `BlogSiteFirstPostExample` stage 2 (returns the post's YAML front matter + body as a string)._

```csharp:xmldocid,bodyonly
M:BlogSiteFirstPostExample.Stage2.Source
```

_Shape of a populated `BlogSiteFrontMatter` YAML block and the keys a post author touches._

## See also

- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Related reference: [Front matter key reference](xref:reference.front-matter.keys)
- Related reference: [`IFrontMatter` and capability defaults](xref:reference.front-matter.ifrontmatter)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
