---
title: "IFrontMatter and capability defaults"
description: "The IFrontMatter contract — its required Title, six default members, and the four remaining capability interfaces that pattern-match separately."
sectionLabel: "Front Matter"
order: 402020
tags: [front-matter, capabilities, interfaces]
uid: reference.front-matter.ifrontmatter
---

`IFrontMatter` is the universal front-matter contract that every Pennington content page implements, declaring one required `Title` property and six default members covering drafts, indexing opt-outs, uid, description, and date. It is declared in `Pennington.FrontMatter`; the four remaining capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) are in the same namespace.

## Declaration

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

## Members

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` (required) | — | The only abstract member; every implementation must supply a human-readable page title surfaced in `<title>`, navigation, breadcrumbs, and feed entries. |
| `Date` | `DateTime?` | `null` | Publication timestamp consumed by RSS, sitemap, and blog post ordering; when `null` the page is treated as undated. |
| `Description` | `string?` | `null` | Short summary emitted into `<meta name="description">`, social cards, and feed entries. |
| `IsDraft` | `bool` | `false` | When `true`, `ContentPipeline.GenerateAsync` skips the page so it does not appear in the output tree, sitemap, search index, or llms.txt. |
| `Llms` | `bool` | `true` | When `false`, `LlmsTxtService` excludes the page from both the `llms.txt` index and the stripped-markdown sidecar output. |
| `Search` | `bool` | `true` | When `false`, `SearchIndexBuilder` excludes the page from the per-locale search index JSON emitted at `/search-index-{code}.json`. |
| `Uid` | `string?` | `null` | Stable cross-reference identifier; when set, the page is registered with `XrefResolver` and can be linked via `<xref:uid>` or `[text](xref:uid)`. |

## Capability interfaces

The four remaining capability interfaces stay separate from `IFrontMatter` because not every content type implements them; consumers pattern-match these interfaces independently, and each interface declares exactly one property.

### `ITaggable`

```csharp:xmldocid
T:Pennington.FrontMatter.ITaggable
```

| Name | Type | Description |
|---|---|---|
| `Tags` | `string[]` | Non-null array of tag slugs used to group content in tag-index pages and surface tags in RSS and structured data. |

Implemented by `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter`.

### `IOrderable`

```csharp:xmldocid
T:Pennington.FrontMatter.IOrderable
```

| Name | Type | Description |
|---|---|---|
| `Order` | `int` | Sort key consumed by `NavigationBuilder.BuildTree` to order siblings within a sidebar group; lower values render first, with ties falling back to alphabetic title order. |

Implemented by `DocFrontMatter` and `DocSiteFrontMatter`, both defaulting to `int.MaxValue` so unset pages sort last.

### `ISectionable`

```csharp:xmldocid
T:Pennington.FrontMatter.ISectionable
```

| Name | Type | Description |
|---|---|---|
| `SectionLabel` | `string?` | Human-readable section label surfaced on breadcrumbs and prev/next navigation; it does **not** drive sidebar grouping (the content subfolder does — see <xref:reference.extension-points.navigation>). |

Implemented by `DocFrontMatter` and `DocSiteFrontMatter`.

### `IRedirectable`

```csharp:xmldocid
T:Pennington.FrontMatter.IRedirectable
```

| Name | Type | Description |
|---|---|---|
| `RedirectUrl` | `string?` | When non-null, the page is emitted as a meta-refresh stub pointing at the target URL with `<meta name="robots" content="noindex">` applied. |

Implemented by `DocSiteFrontMatter` and `BlogSiteFrontMatter`; see <xref:how-to.content-authoring.redirects> for authoring practice.

## Example

```csharp:xmldocid,bodyonly
T:DocSiteKitchenSinkExample.ApiFrontMatter
```

This record demonstrates the reference shape for a custom front-matter type with full capability coverage: `IFrontMatter` plus all four capability interfaces in a single declaration.

## See also

- How-to: [Work with front matter](xref:how-to.content-authoring.front-matter)
- Related reference: [Front matter key reference](xref:reference.front-matter.keys)
- Related reference: [Built-in front-matter types](xref:reference.front-matter.built-in-types)
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities)
