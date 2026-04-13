---
title: "JSON-LD schema types"
description: "The record types in Pennington.StructuredData.JsonLdTypes — JsonLdArticle, JsonLdBreadcrumbList, JsonLdBreadcrumbItem, JsonLdWebSite — plus the JsonLdSerializer helpers that feed the StructuredData UI component."
section: "structured-data"
order: 10
tags: []
uid: reference.structured-data.types
isDraft: true
search: false
llms: false
---

> **In this page.** The record types in `Pennington.StructuredData.JsonLdTypes` — `JsonLdArticle`, `JsonLdBreadcrumbList`, its `JsonLdBreadcrumbItem`, and `JsonLdWebSite` — plus the `JsonLdSerializer` helpers (`SerializeArticle`, `SerializeBreadcrumbList`, `SerializeWebSite`) and how these feed the `<StructuredData>` UI component.
>
> **Not in this page.** The schema.org vocabulary itself (out of scope) or the `<StructuredData>` component parameters (see `reference/ui/utility`).

## Summary

- Four record types and one static serializer class that emit schema.org-compatible JSON-LD suitable for embedding in a page `<head>`.
- Namespace `Pennington.StructuredData`; source files `src/Pennington/StructuredData/JsonLdTypes.cs` and `JsonLdSerializer.cs`.

## Record types

### `JsonLdArticle`

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdArticle
```

Represents a schema.org `Article`.

| Member | Type | Description |
|---|---|---|
| `Headline` | `string` | Article headline. Emitted as `headline` in the JSON-LD payload. |
| `Description` | `string?` | Optional summary. Emitted as `description` only when non-null. |
| `Url` | `string` | Canonical article URL. Emitted as `url`. |
| `DatePublished` | `DateTime?` | Optional publish date. Emitted as `datePublished` in `"yyyy-MM-ddTHH:mm:ssZ"` format when set. |
| `AuthorName` | `string?` | Optional author name. When set, emitted as a nested `{ "@type": "Person", "name": "…" }` object under `author`. |

### `JsonLdBreadcrumbItem`

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdBreadcrumbItem
```

One entry in a breadcrumb list.

| Member | Type | Description |
|---|---|---|
| `Position` | `int` | 1-based position of the item in the breadcrumb chain. |
| `Name` | `string` | Display name for the breadcrumb entry. |
| `Url` | `string?` | Optional URL for the item. When set, emitted as `item`; when null, the element has no `item` field. |

### `JsonLdBreadcrumbList`

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdBreadcrumbList
```

A `BreadcrumbList` containing an ordered sequence of `JsonLdBreadcrumbItem` entries.

| Member | Type | Description |
|---|---|---|
| `Items` | `IReadOnlyList<JsonLdBreadcrumbItem>` | The breadcrumb entries. |

### `JsonLdWebSite`

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdWebSite
```

A schema.org `WebSite` (typically used on the homepage).

| Member | Type | Description |
|---|---|---|
| `Name` | `string` | Site name. Emitted as `name`. |
| `Url` | `string` | Canonical site URL. Emitted as `url`. |
| `Description` | `string?` | Optional description. Emitted as `description` only when non-null. |

## `JsonLdSerializer`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdSerializer
```

Static helper that produces script-tag-safe JSON strings from the record types above.

### Methods

| Method | Returns | Behavior |
|---|---|---|
| `SerializeArticle(JsonLdArticle article)` | `string` | Emits `{ "@context": "https://schema.org", "@type": "Article", … }`. Null members are omitted via `DefaultIgnoreCondition = WhenWritingNull`. |
| `SerializeBreadcrumbList(JsonLdBreadcrumbList breadcrumbs)` | `string?` | Emits `{ "@context", "@type": "BreadcrumbList", "itemListElement": […] }`. **Returns `null`** when `breadcrumbs.Items` is empty — the `<StructuredData>` component suppresses the `<script>` tag in that case. |
| `SerializeWebSite(JsonLdWebSite webSite)` | `string` | Emits `{ "@context": "https://schema.org", "@type": "WebSite", … }`. |

### Escaping contract

- All three serializers pipe their output through `EscapeForScriptTag`, which replaces `</` with `<\/`. This prevents a payload with a literal `</script>` sequence from prematurely closing the surrounding `<script>` tag when the JSON is embedded inline.
- Serialization uses `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` so non-ASCII characters are emitted as-is rather than `\uXXXX`-encoded.

## Consumption

- The `<StructuredData>` utility component (`src/Pennington.UI/Components/StructuredData.razor`) takes optional `Article`, `Breadcrumbs`, and `WebSite` parameters; for each non-null parameter it calls the matching serializer and injects a `<script type="application/ld+json">` tag into `<head>` via `HeadContent`.
- Reference: [`StructuredData` component](/reference/ui/utility#structureddata).

## See also

- Related reference: [Utility components](/reference/ui/utility) — parameters of `<StructuredData>`.
- External: [schema.org Article](https://schema.org/Article), [BreadcrumbList](https://schema.org/BreadcrumbList), [WebSite](https://schema.org/WebSite).
