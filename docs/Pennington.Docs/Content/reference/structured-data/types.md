---
title: "JSON-LD schema types"
description: "The record types in Pennington.StructuredData.JsonLdTypes and the JsonLdSerializer static methods that project them to JSON-LD strings."
uid: reference.structured-data.types
order: 409010
sectionLabel: Structured Data
tags: [structured-data, json-ld, schema-org, seo]
---

> **In this page.** _One sentence pulled from `docs-toc.md`: the record types in `Pennington.StructuredData.JsonLdTypes` — `JsonLdArticle`, `JsonLdBreadcrumbList` and its `JsonLdBreadcrumbItem`, `JsonLdWebSite` — plus `JsonLdSerializer` and how they feed the `<StructuredData>` UI component._
>
> **Not in this page.** _One sentence pulled from `docs-toc.md`: the schema.org vocabulary itself is out of scope; the `<StructuredData>` component parameters live on [`reference/ui/utility`](xref:reference.ui.utility)._

## Summary

_**One sentence: what it is.** The four immutable record types that model the schema.org schemas Pennington emits (`Article`, `BreadcrumbList` with its `ListItem`, `WebSite`) together with the `JsonLdSerializer` static class that projects each record to a script-tag-safe JSON-LD string._
_**One sentence: where it lives.** Namespace `Pennington.StructuredData`, source files `src/Pennington/StructuredData/JsonLdTypes.cs` (the four records) and `src/Pennington/StructuredData/JsonLdSerializer.cs` (the static serializer); consumed by `Pennington.UI.Components.StructuredData` which invokes the serializer from `OnParametersSet` and injects each non-null result as a `<script type="application/ld+json">` tag via `<HeadContent>`._

## `JsonLdArticle`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdArticle
```

_One sentence: positional record modeling the schema.org `Article` schema emitted on content pages (posts and documentation articles), with five constructor-positional fields ordered required-then-nullable._

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Headline` | `string` | — | _One sentence: required article headline, rendered as schema.org `headline` (typically the page title)._ |
| `Description` | `string?` | — | _One sentence: optional short description, emitted as `description` only when non-null (the serializer uses `JsonIgnoreCondition.WhenWritingNull`)._ |
| `Url` | `string` | — | _One sentence: required absolute article URL, rendered as schema.org `url`._ |
| `DatePublished` | `DateTime?` | — | _One to two sentences: optional publication date; when non-null, serialized as ISO-8601 UTC via `ToString("yyyy-MM-ddTHH:mm:ssZ")` and written to `datePublished`._ |
| `AuthorName` | `string?` | — | _One to two sentences: optional author display name; when non-null, expands to a nested `{ "@type": "Person", "name": <AuthorName> }` object under the `author` key._ |

## `JsonLdBreadcrumbList`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdBreadcrumbList
```

_One sentence: positional record wrapping an ordered collection of `JsonLdBreadcrumbItem` entries, modeling the schema.org `BreadcrumbList` emitted on interior pages._

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<JsonLdBreadcrumbItem>` | — | _One sentence: required ordered list of breadcrumb rungs; each element becomes a `ListItem` under the serialized `itemListElement` array._ |

### Note

_One sentence: when `Items.Count == 0`, `JsonLdSerializer.SerializeBreadcrumbList` returns `null` rather than emitting an empty schema — the component uses that sentinel to skip the script tag entirely._

## `JsonLdBreadcrumbItem`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdBreadcrumbItem
```

_One sentence: positional record modeling a single rung of a `BreadcrumbList` (schema.org `ListItem`), carrying its 1-based position, visible name, and optional URL._

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Position` | `int` | — | _One sentence: required 1-based position within the breadcrumb trail, serialized directly to the `position` field._ |
| `Name` | `string` | — | _One sentence: required human-readable label for the rung, serialized to `name`._ |
| `Url` | `string?` | — | _One to two sentences: optional absolute URL for the rung; when non-null it is serialized to the `item` key (note: `item`, not `url`), and omitted entirely when null (typical for the current page which is not itself clickable)._ |

## `JsonLdWebSite`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdWebSite
```

_One sentence: positional record modeling the schema.org `WebSite` schema emitted once on the site homepage._

### Properties

| Name | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | — | _One sentence: required site name, rendered as schema.org `name`._ |
| `Url` | `string` | — | _One sentence: required absolute site URL, rendered as schema.org `url`._ |
| `Description` | `string?` | — | _One sentence: optional tagline/description, emitted as `description` only when non-null._ |

## `JsonLdSerializer`

### Declaration

```csharp:xmldocid
T:Pennington.StructuredData.JsonLdSerializer
```

_One to two sentences: static class that projects each record type to a JSON string embeddable inside a `<script type="application/ld+json">` tag. The internal `JsonSerializerOptions` pin three behaviors: `DefaultIgnoreCondition = WhenWritingNull`, `WriteIndented = false`, and `Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping`; a post-serialize pass rewrites `</` as `<\/` so an embedded literal cannot prematurely close the surrounding script tag._

### Methods

#### `SerializeArticle(JsonLdArticle)`

```csharp:xmldocid
M:Pennington.StructuredData.JsonLdSerializer.SerializeArticle(Pennington.StructuredData.JsonLdArticle)
```

_Two to three sentences: emits the `@context`/`@type`/`headline`/`url` keys unconditionally, then conditionally adds `description`, `datePublished` (ISO-8601 UTC), and a nested `Person`-typed `author` object based on which optional fields are set. Returns the escaped JSON string; never returns null._

#### `SerializeBreadcrumbList(JsonLdBreadcrumbList)`

```csharp:xmldocid
M:Pennington.StructuredData.JsonLdSerializer.SerializeBreadcrumbList(Pennington.StructuredData.JsonLdBreadcrumbList)
```

_Two to three sentences: returns `null` when `Items` is empty so callers can skip the script tag; otherwise projects each `JsonLdBreadcrumbItem` into a schema.org `ListItem` (with `item` set only when the rung's `Url` is non-null) and wraps the elements in a `BreadcrumbList` under `itemListElement`. Returns the escaped JSON string, or `null` for the empty-list sentinel._

#### `SerializeWebSite(JsonLdWebSite)`

```csharp:xmldocid
M:Pennington.StructuredData.JsonLdSerializer.SerializeWebSite(Pennington.StructuredData.JsonLdWebSite)
```

_Two sentences: emits the `@context`/`@type`/`name`/`url` keys unconditionally and adds `description` when non-null. Returns the escaped JSON string; never returns null._

## Feeds into `<StructuredData>`

_One sentence: `Pennington.UI.Components.StructuredData` takes `Article`, `Breadcrumbs`, and `WebSite` parameters of these record types, invokes the matching `JsonLdSerializer.Serialize*` method in `OnParametersSet`, and injects each non-null result into a `<script type="application/ld+json">` tag via `<HeadContent>` — see [`reference/ui/utility`](xref:reference.ui.utility) for the component-level parameter table._

## Example

_Construct the record directly and hand it to `JsonLdSerializer`, or pass it as a parameter to the `<StructuredData>` component — the serializer emits a `<script type="application/ld+json">`-safe string and skips any optional field left `null`. The `examples/BlogKitchenSinkExample` project projects a `BlogSiteFrontMatter` into a `JsonLdArticle` via the helper below:_

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.StructuredDataBuilder.BuildArticle(Pennington.BlogSite.BlogSiteFrontMatter,System.String)
```

_And serializes it to the script-tag-safe string:_

```csharp:xmldocid,bodyonly
M:BlogKitchenSinkExample.StructuredDataBuilder.BuildArticleJson(Pennington.BlogSite.BlogSiteFrontMatter,System.String)
```

## See also

- Related reference: [Utility components](xref:reference.ui.utility) — the `<StructuredData>` component that consumes these records and emits `<script type="application/ld+json">` tags.
- Related reference: [`DocSiteOptions`](xref:reference.options.docsite-options) — `CanonicalBaseUrl` gates structured-data emission in DocSite.
