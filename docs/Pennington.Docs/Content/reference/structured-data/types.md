---
title: "JSON-LD schema types"
description: "The record types in Pennington.StructuredData.JsonLdTypes and the JsonLdSerializer static methods that project them to JSON-LD strings."
uid: reference.structured-data.types
order: 409010
sectionLabel: Structured Data
tags: [structured-data, json-ld, schema-org, seo]
---

`Pennington.StructuredData` provides four immutable record types — `JsonLdArticle`, `JsonLdBreadcrumbList`, `JsonLdBreadcrumbItem`, and `JsonLdWebSite` — that model the schema.org schemas Pennington emits, together with the `JsonLdSerializer` static class that projects each record to a script-tag-safe JSON-LD string. The records are defined in `JsonLdTypes.cs` and the serializer in `JsonLdSerializer.cs`; both are consumed by `Pennington.UI.Components.StructuredData`, which invokes the matching `Serialize*` method in `OnParametersSet` and injects each non-null result as a `<script type="application/ld+json">` tag via `<HeadContent>`.

## `JsonLdArticle`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdArticle
```

Positional record modeling the schema.org `Article` schema emitted on content pages, with five constructor-positional fields ordered required-then-nullable.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Headline` | `string` | — | Required article headline, rendered as schema.org `headline`. |
| `Description` | `string?` | — | Optional short description, emitted as `description` only when non-null. |
| `Url` | `string` | — | Required absolute article URL, rendered as schema.org `url`. |
| `DatePublished` | `DateTime?` | — | Optional publication date; when non-null, serialized as ISO-8601 UTC via `ToString("yyyy-MM-ddTHH:mm:ssZ")` and written to `datePublished`. |
| `AuthorName` | `string?` | — | Optional author display name; when non-null, expands to a nested `Person`-typed object under the `author` key. |

## `JsonLdBreadcrumbList`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdBreadcrumbList
```

Positional record wrapping an ordered collection of `JsonLdBreadcrumbItem` entries, modeling the schema.org `BreadcrumbList` emitted on interior pages.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<JsonLdBreadcrumbItem>` | — | Required ordered list of breadcrumb rungs; each element becomes a `ListItem` under the serialized `itemListElement` array. |

> **Note:** When `Items.Count == 0`, `JsonLdSerializer.SerializeBreadcrumbList` returns `null` rather than emitting an empty schema; the component uses that sentinel to skip the script tag entirely.

## `JsonLdBreadcrumbItem`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdBreadcrumbItem
```

Positional record modeling a single rung of a `BreadcrumbList` (schema.org `ListItem`), carrying its 1-based position, visible name, and optional URL.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Position` | `int` | — | Required 1-based position within the breadcrumb trail, serialized to the `position` field. |
| `Name` | `string` | — | Required human-readable label for the rung, serialized to `name`. |
| `Url` | `string?` | — | Optional absolute URL for the rung; when non-null, serialized to the `item` key (not `url`); omitted entirely when null. |

## `JsonLdWebSite`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdWebSite
```

Positional record modeling the schema.org `WebSite` schema emitted once on the site homepage.

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | — | Required site name, rendered as schema.org `name`. |
| `Url` | `string` | — | Required absolute site URL, rendered as schema.org `url`. |
| `Description` | `string?` | — | Optional tagline or description, emitted as `description` only when non-null. |

## `JsonLdSerializer`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdSerializer
```

Static class that projects each record type to a JSON string embeddable inside a `<script type="application/ld+json">` tag. The internal `JsonSerializerOptions` set `DefaultIgnoreCondition = WhenWritingNull`, `WriteIndented = false`, and `Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping`; a post-serialize pass rewrites `</` as `<\/` to prevent a literal from prematurely closing the surrounding script tag.

### Methods

#### `SerializeArticle(JsonLdArticle)`

```csharp:xmldocid
M:Pennington.StructuredData.JsonLdSerializer.SerializeArticle(Pennington.StructuredData.JsonLdArticle)
```

Emits `@context`, `@type`, `headline`, and `url` unconditionally, then conditionally adds `description`, `datePublished` (ISO-8601 UTC), and a nested `Person`-typed `author` object for any non-null optional fields. Returns the escaped JSON string; never returns null.

#### `SerializeBreadcrumbList(JsonLdBreadcrumbList)`

```csharp:xmldocid
M:Pennington.StructuredData.JsonLdSerializer.SerializeBreadcrumbList(Pennington.StructuredData.JsonLdBreadcrumbList)
```

Returns `null` when `Items` is empty so callers can skip the script tag; otherwise projects each `JsonLdBreadcrumbItem` into a schema.org `ListItem` (setting `item` only when the rung's `Url` is non-null) and wraps the elements in a `BreadcrumbList` under `itemListElement`. Returns the escaped JSON string, or `null` for the empty-list sentinel.

#### `SerializeWebSite(JsonLdWebSite)`

```csharp:xmldocid
M:Pennington.StructuredData.JsonLdSerializer.SerializeWebSite(Pennington.StructuredData.JsonLdWebSite)
```

Emits `@context`, `@type`, `name`, and `url` unconditionally and adds `description` when non-null. Returns the escaped JSON string; never returns null.

## Feeds into `<StructuredData>`

`Pennington.UI.Components.StructuredData` accepts `Article`, `Breadcrumbs`, and `WebSite` parameters of these record types, invokes the matching `JsonLdSerializer.Serialize*` method in `OnParametersSet`, and injects each non-null result as a `<script type="application/ld+json">` tag via `<HeadContent>`; see <xref:reference.ui.utility> for the component-level parameter table.

## Example

The `examples/BlogKitchenSinkExample` project maps a `BlogSiteFrontMatter` to a `JsonLdArticle` via the helper below:

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.StructuredDataBuilder.BuildArticle(Pennington.BlogSite.BlogSiteFrontMatter,System.String)
```

And serializes it to the script-tag-safe string:

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.StructuredDataBuilder.BuildArticleJson(Pennington.BlogSite.BlogSiteFrontMatter,System.String)
```

## See also

- Related reference: [Utility components](xref:reference.ui.utility) — the `<StructuredData>` component that consumes these records and emits `<script type="application/ld+json">` tags.
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options) — `CanonicalBaseUrl` gates structured-data emission in DocSite.
