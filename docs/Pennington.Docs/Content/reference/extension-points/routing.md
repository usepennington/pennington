---
title: "Routing types"
description: "UrlPath, FilePath, ContentRoute, and ContentRouteFactory — constructors, operators, and helper methods (EnsureLeadingSlash, WithBaseUrl, AbsoluteUrl, IsDefaultLocale)."
section: "extension-points"
order: 20
tags: []
uid: reference.extension-points.routing
isDraft: true
search: false
llms: false
---

> **In this page.** `UrlPath`, `FilePath`, `ContentRoute`, and `ContentRouteFactory` — constructors, operators, and helper methods (`EnsureLeadingSlash`, `WithBaseUrl`, `AbsoluteUrl`, `IsDefaultLocale`).
>
> **Not in this page.** The broader URL design philosophy (see Explanation).

## Summary

The four value and factory types in `Pennington.Routing` that express URL paths, filesystem paths, and canonical content routes.
Namespace `Pennington.Routing`, declared under `src/Pennington/Routing/`; every downstream namespace depends on these types as the universal content coordinate.

## `UrlPath`

`readonly record struct` wrapping a URL path string with composition and normalization helpers.

### Declaration

```csharp:xmldocid
T:Pennington.Routing.UrlPath
```

### Constructor

| Signature | Description |
|---|---|
| `UrlPath(string Value)` | Positional record-struct constructor; `Value` is the raw path string. |

### Operators

| Operator | Signature | Description |
|---|---|---|
| Implicit | `implicit operator UrlPath(string)` | Converts a string literal to a `UrlPath`. |
| `/` | `static UrlPath operator /(UrlPath left, UrlPath right)` | Composes two paths with a single `/` separator; trims a trailing slash from `left` and a leading slash from `right`; ensures a leading slash on the result when either side is non-empty. |

### Methods

| Signature | Returns | Description |
|---|---|---|
| `EnsureLeadingSlash()` | `UrlPath` | Returns this value with a `/` prefix if it is not already present. |
| `EnsureTrailingSlash()` | `UrlPath` | Returns this value with a `/` suffix if it is not already present. |
| `RemoveTrailingSlash()` | `UrlPath` | Returns this value with any single trailing `/` removed, unless the value is exactly `/`. |
| `RemoveLeadingSlash()` | `UrlPath` | Returns this value with any single leading `/` removed. |
| `Matches(UrlPath other)` | `bool` | Case-insensitive equality after trimming trailing slashes and collapsing `/index.html` or `/index.htm` to directory form. |
| `ToString()` | `string` | Returns `Value`. |

## `FilePath`

`readonly record struct` wrapping a filesystem path string with composition and `System.IO.Path`-style accessors.

### Declaration

```csharp:xmldocid
T:Pennington.Routing.FilePath
```

### Constructor

| Signature | Description |
|---|---|
| `FilePath(string Value)` | Positional record-struct constructor; `Value` is the raw filesystem path. |

### Operators

| Operator | Signature | Description |
|---|---|---|
| Implicit | `implicit operator FilePath(string)` | Converts a string literal to a `FilePath`. |
| `/` | `static FilePath operator /(FilePath left, FilePath right)` | Combines two file paths with a single forward-slash separator; trims trailing `/` or `\` from `left` and leading `/` or `\` from `right`. |

### Properties

| Name | Type | Description |
|---|---|---|
| `Extension` | `string` | Returns `Path.GetExtension(Value)`. |
| `FileName` | `string` | Returns `Path.GetFileName(Value)`. |
| `FileNameWithoutExtension` | `string` | Returns `Path.GetFileNameWithoutExtension(Value)`. |

### Methods

| Signature | Returns | Description |
|---|---|---|
| `ToString()` | `string` | Returns `Value`. |

## `ContentRoute`

`sealed record` describing a canonical content route: URL, output file, optional source file, locale, and fallback flag.

### Declaration

```csharp:xmldocid
T:Pennington.Routing.ContentRoute
```

### Properties

| Name | Type | Default | Required |
|---|---|---|---|
| `CanonicalPath` | `UrlPath` | — | yes |
| `OutputFile` | `FilePath` | — | yes |
| `IsFallback` | `bool` | `false` | no |
| `Locale` | `string` | `""` | no |
| `SourceFile` | `FilePath?` | `null` | no |

### Derived property

| Name | Type | Description |
|---|---|---|
| `IsDefaultLocale` | `bool` | `true` when `Locale` is null or empty; identifies routes that serve the default locale. |

### Methods

| Signature | Returns | Description |
|---|---|---|
| `WithBaseUrl(UrlPath baseUrl)` | `UrlPath` | Composes `baseUrl` with `CanonicalPath` via the `UrlPath /` operator. |
| `AbsoluteUrl(UrlPath canonicalBase)` | `UrlPath` | Composes `CanonicalPath` with `canonicalBase`; when `canonicalBase` is an absolute URL with scheme, uses string concatenation to preserve the scheme, otherwise delegates to the `UrlPath /` operator. |

## `ContentRouteFactory`

`static class` with the five factory methods used to construct `ContentRoute` instances from the supported content origins.

### Declaration

```csharp:xmldocid
T:Pennington.Routing.ContentRouteFactory
```

### Methods

| Signature | Returns | Description |
|---|---|---|
| `FromMarkdownFile(FilePath sourceFile, FilePath contentRoot, UrlPath basePageUrl, string locale = "")` | `ContentRoute` | Converts a markdown file path relative to `contentRoot` into a canonical route; collapses `index` segments to the parent directory; prefixes `locale` when non-empty; throws `ArgumentException` when `sourceFile` resolves outside `contentRoot`. |
| `FromRazorPage(string pageRoute, string locale = "")` | `ContentRoute` | Builds a route from a Razor `@page` directive; normalizes leading and trailing slashes and prefixes `locale` when non-empty. |
| `FromUrl(UrlPath url, string locale = "")` | `ContentRoute` | Builds a route from an explicit URL for programmatic content; normalizes slashes and prefixes `locale` when non-empty. |
| `FromCustom(UrlPath url, FilePath? sourceFile = null, string locale = "")` | `ContentRoute` | Builds a route for non-markdown content services; optional `sourceFile` is preserved on the returned record. |
| `ForRedirect(UrlPath sourceUrl)` | `ContentRoute` | Builds a route whose output file holds redirect HTML; no locale prefix is applied. |

## Example

```csharp:xmldocid,bodyonly
T:MultipleContentSourceExample.ContentFrontMatter
```

A record type whose route is materialized by `ContentRouteFactory.FromMarkdownFile` as markdown files are discovered under a configured content root.

## See also

- Related reference: [Content pipeline interfaces](/reference/extension-points/content-pipeline)
- Related reference: [Navigation types](/reference/extension-points/navigation)
- Background: [URL paths and content routes](/explanation/routing/url-paths)
