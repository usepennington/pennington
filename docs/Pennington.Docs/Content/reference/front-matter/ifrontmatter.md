---
title: "IFrontMatter and capability defaults"
description: "The IFrontMatter contract — its required Title, six default members, and the four remaining capability interfaces that pattern-match separately."
sectionLabel: "Front Matter"
order: 20
tags: [front-matter, capabilities, interfaces]
uid: reference.front-matter.ifrontmatter
---

> **In this page.** The `IFrontMatter` contract, which keys have default implementations, and how the consolidated capabilities (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) surface as default members.
>
> **Not in this page.** The rationale for consolidation — see the Explanation page [The front-matter capability system](/explanation/core/front-matter-capabilities).

## Summary

_**One sentence: what it is.** The universal front-matter contract every Pennington content page implements — one required `Title` property plus six default members covering drafts, indexing opt-outs, uid, description, and date._
_**One sentence: where it lives.** Declared in namespace `Pennington.FrontMatter` at `src/Pennington/FrontMatter/IFrontMatter.cs`, with the four remaining capability interfaces (`ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`) in `src/Pennington/FrontMatter/Capabilities.cs`._

## Declaration

```csharp:xmldocid
T:Pennington.FrontMatter.IFrontMatter
```

_Show the full interface declaration so the reader sees the single required `Title` member plus the six default-implemented members in one place._

## Members

_Alphabetical by member name after the required `Title`. One-sentence descriptions only; every row is a lookup entry, not a walkthrough._

| Name | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` (required) | — | _One-sentence: the only abstract member — every implementation must supply a human-readable page title surfaced in `<title>`, navigation, breadcrumbs, and feed entries._ |
| `Date` | `DateTime?` | `null` | _One-sentence: publication timestamp consumed by RSS, sitemap, and blog post ordering; when `null` the page is treated as undated._ |
| `Description` | `string?` | `null` | _One-sentence: short human-readable summary emitted into `<meta name="description">`, social cards, and feed entries._ |
| `IsDraft` | `bool` | `false` | _One-sentence: when `true`, `ContentPipeline.GenerateAsync` skips the page during the generate stage so it does not appear in the output tree, sitemap, search index, or llms.txt._ |
| `Llms` | `bool` | `true` | _One-sentence: when `false`, `LlmsTxtService` excludes the page from both the `llms.txt` index and the stripped-markdown sidecar output._ |
| `Search` | `bool` | `true` | _One-sentence: when `false`, `SearchIndexBuilder` excludes the page from the per-locale search index JSON emitted at `/search-index-{code}.json`._ |
| `Uid` | `string?` | `null` | _One-sentence: stable cross-reference identifier — when set, the page is registered with `XrefResolver` and can be linked via `<xref:uid>` or `[text](xref:uid)`._ |

## Capability interfaces

_The four remaining capability interfaces stay separate from `IFrontMatter` because not every content type implements them; consumers pattern-match these interfaces when they care (see `NavigationBuilder` for `IOrderable`, `BlogSiteContentService` for `ITaggable`, etc.). Each interface declares exactly one property._

### `ITaggable`

```csharp:xmldocid
T:Pennington.FrontMatter.ITaggable
```

| Name | Type | Description |
|---|---|---|
| `Tags` | `string[]` | _One-sentence: non-null array of tag slugs used to group content in tag-index pages and surface `<meta>` tags in RSS/structured data._ |

_Implemented by `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, and `BlogSiteFrontMatter`._

### `IOrderable`

```csharp:xmldocid
T:Pennington.FrontMatter.IOrderable
```

| Name | Type | Description |
|---|---|---|
| `Order` | `int` | _One-sentence: sort key consumed by `NavigationBuilder.BuildTree` to order siblings within a sidebar group; lower values render first and ties fall back to alphabetic title order._ |

_Implemented by `DocFrontMatter` and `DocSiteFrontMatter` (both default to `int.MaxValue` so unset pages sort last)._

### `ISectionable`

```csharp:xmldocid
T:Pennington.FrontMatter.ISectionable
```

| Name | Type | Description |
|---|---|---|
| `SectionLabel` | `string?` | _One-sentence: human-readable section label surfaced on breadcrumbs and prev/next navigation — it does **not** drive sidebar grouping (the content subfolder does; see [Navigation types](/reference/extension-points/navigation))._ |

_Implemented by `DocFrontMatter` and `DocSiteFrontMatter`._

### `IRedirectable`

```csharp:xmldocid
T:Pennington.FrontMatter.IRedirectable
```

| Name | Type | Description |
|---|---|---|
| `RedirectUrl` | `string?` | _One-sentence: when non-null, the page is emitted as a meta-refresh stub pointing at the target URL with `<meta name="robots" content="noindex">` applied._ |

_Implemented by `DocSiteFrontMatter` and `BlogSiteFrontMatter`; the how-to [Configure redirects](/how-to/content-authoring/redirects) covers authoring practice._

## Example

_One minimal example pulled from `examples/DocSiteKitchenSinkExample/ApiFrontMatter.cs` — a custom front-matter record that implements `IFrontMatter` plus all four capability interfaces, so a reader sees the canonical "write your own front matter" shape in one record._

```csharp:xmldocid,bodyonly
T:DocSiteKitchenSinkExample.ApiFrontMatter
```

_A single sentence of context: this is the reference shape for declaring a custom front-matter record with full capability coverage — every surface on `IFrontMatter` plus each of the four capability interfaces in one declaration._

## See also

- How-to: [Work with front matter](/how-to/content-authoring/front-matter)
- Related reference: [Front matter key reference](/reference/front-matter/keys)
- Related reference: [Built-in front-matter types](/reference/front-matter/built-in-types)
- Background: [The front-matter capability system](/explanation/core/front-matter-capabilities)
