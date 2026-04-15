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

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.DocFrontMatter" />

## `BlogFrontMatter`

```csharp:xmldocid
T:Pennington.FrontMatter.BlogFrontMatter
```

The core-library blog-post record for author-wired blog hosts. Carries `Date`, `Author`, and `Series` alongside the `IFrontMatter` defaults. Implements `IFrontMatter` and `ITaggable`. Not the record bound by `AddBlogSite`.

### Properties

<ApiMemberTable XmlDocId="T:Pennington.FrontMatter.BlogFrontMatter" />

## `DocSiteFrontMatter`

```csharp:xmldocid
T:Pennington.DocSite.DocSiteFrontMatter
```

The record bound by `AddDocSite`. Extends the `DocFrontMatter` shape with `RedirectUrl` via `IRedirectable`. Implements `IFrontMatter`, `ITaggable`, `ISectionable`, `IOrderable`, and `IRedirectable`.

### Properties

<ApiMemberTable XmlDocId="T:Pennington.DocSite.DocSiteFrontMatter" />

## `BlogSiteFrontMatter`

```csharp:xmldocid
T:Pennington.BlogSite.BlogSiteFrontMatter
```

The record bound by `AddBlogSite`. Consolidates all post-authoring fields (`Author`, `Repository`, `Series`, `Date`, `RedirectUrl`) in one contract. Implements `IFrontMatter`, `ITaggable`, `ISectionable`, and `IRedirectable`.

### Properties

<ApiMemberTable XmlDocId="T:Pennington.BlogSite.BlogSiteFrontMatter" />

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
