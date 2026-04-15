---
title: "Routing types"
description: "The four types in Pennington.Routing that own URL math, filesystem paths, canonical routes, and route construction across every content source."
sectionLabel: "Extension Points"
order: 405020
tags: [routing, url-path, content-route, extension-points]
uid: reference.extension-points.routing
---

The four types in `Pennington.Routing` that form Pennington's canonical route coordinate system: two value-type path wrappers (`UrlPath`, `FilePath`), the `ContentRoute` record carried through every pipeline stage, and the `ContentRouteFactory` static that constructs routes from each supported content origin. Every `ContentItem`, `IContentService`, and rewriter operates in terms of these types.

## Overview

| Type | Kind | Purpose |
|---|---|---|
| `UrlPath` | `readonly record struct` | URL math value type — composition via `/` operator, leading/trailing-slash normalization, index-file-aware comparison. |
| `FilePath` | `readonly record struct` | Filesystem path value type — composition via `/` operator, extension/filename accessors, implicit conversion from `string`. |
| `ContentRoute` | `sealed record` | Universal route coordinate carried through the entire content pipeline; pairs `CanonicalPath` with `OutputFile`, optional `SourceFile`, `Locale`, and `IsFallback`. |
| `ContentRouteFactory` | `static class` | Canonical constructors that produce a `ContentRoute` from each supported origin (markdown file, Razor page, arbitrary URL, custom content service, redirect). |

## `UrlPath`

```csharp:xmldocid
T:Pennington.Routing.UrlPath
```

The URL value type used throughout Pennington for canonical page paths, base URLs, locale prefixes, xref targets, and link attributes. The internal `Normalize` method treats a trailing `/index.html` or `/index.htm` as equivalent to the directory form, so `Matches` considers `/docs/` and `/docs/index.html` equal.

### Constructor

```csharp:xmldocid
M:Pennington.Routing.UrlPath.#ctor(System.String)
```

Primary constructor on the record struct; accepts a single `string Value` with no validation, so callers that require a leading slash must pass the result through `EnsureLeadingSlash`.

### `implicit operator UrlPath(string)`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.op_Implicit(System.String)~Pennington.Routing.UrlPath
```

Implicit conversion from `string`; any `string` literal or variable in a `UrlPath` context is wrapped without an explicit `new`, so a separate `string` overload is unnecessary.

### `operator /(UrlPath, UrlPath)`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.op_Division(Pennington.Routing.UrlPath,Pennington.Routing.UrlPath)
```

Path-composition operator that trims trailing slashes from the left operand and leading slashes from the right, joins them with exactly one `/`, collapses empty operands to the non-empty side, and always produces a result with a leading slash.

### `EnsureLeadingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.EnsureLeadingSlash
```

Returns `this` if `Value` already starts with `/`; otherwise returns a new `UrlPath` with one prepended. Idempotent.

### `EnsureTrailingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.EnsureTrailingSlash
```

Returns `this` if `Value` already ends with `/`; otherwise returns a new `UrlPath` with one appended. Idempotent; every path produced by `ContentRouteFactory` passes through this method before being stored on `ContentRoute.CanonicalPath`.

### `RemoveTrailingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.RemoveTrailingSlash
```

Returns `this` if `Value.Length <= 1` or does not end with `/`; otherwise returns a new `UrlPath` with the trailing slash stripped. The length guard preserves the root `/` so `new UrlPath("/").RemoveTrailingSlash()` still yields `/`.

### `RemoveLeadingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.RemoveLeadingSlash
```

Returns `this` if `Value` does not start with `/`; otherwise returns a new `UrlPath` with the leading slash stripped. Used by `ContentRouteFactory.BuildOutputFile` when converting a canonical URL path into an on-disk `OutputFile` path.

### `Matches`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.Matches(Pennington.Routing.UrlPath)
```

Case-insensitive equality after normalization: strips a trailing `/`, then strips a trailing `/index.html` or `/index.htm`, then lowercases; the empty result is re-normalized to `/`. This is the comparison every request-time route lookup should use.

### Properties

| Name | Type | Description |
|---|---|---|
| `Value` | `string` | The underlying path string as supplied to the constructor; `ToString()` returns this value verbatim. |

## `FilePath`

```csharp:xmldocid
T:Pennington.Routing.FilePath
```

The filesystem path value type used wherever Pennington holds a path that may be read from disk or written to the output tree: `DiscoveredItem` sources, `ContentRoute.OutputFile`, `ContentRoute.SourceFile`, and `ContentToCopy` entries. Unlike `UrlPath`, `FilePath` retains whatever slash form the caller supplied and does not normalize to `/` or `\`.

### Constructor

```csharp:xmldocid
M:Pennington.Routing.FilePath.#ctor(System.String)
```

Primary constructor on the record struct; accepts a single `string Value` with no validation or normalization. Pennington stores forward slashes in-process and relies on `Path` APIs to read and write, so cross-platform callers should pass already-normalized strings.

### `implicit operator FilePath(string)`

```csharp:xmldocid
M:Pennington.Routing.FilePath.op_Implicit(System.String)~Pennington.Routing.FilePath
```

Implicit conversion from `string`; any `string` literal or variable in a `FilePath` context is wrapped without an explicit `new`.

### `operator /(FilePath, FilePath)`

```csharp:xmldocid
M:Pennington.Routing.FilePath.op_Division(Pennington.Routing.FilePath,Pennington.Routing.FilePath)
```

Composition operator that trims trailing `/` and `\` from the left operand and leading `/` and `\` from the right, joins them with a forward slash, and collapses empty operands to the non-empty side. Unlike `UrlPath.op_Division`, the result does not force a leading slash.

### Properties

| Name | Type | Description |
|---|---|---|
| `Extension` | `string` | Delegates to `Path.GetExtension(Value)`; returns the extension including the leading dot, or `""` if none. |
| `FileName` | `string` | Delegates to `Path.GetFileName(Value)`; returns the final path segment. |
| `FileNameWithoutExtension` | `string` | Delegates to `Path.GetFileNameWithoutExtension(Value)`; returns the final path segment with the extension stripped. |
| `Value` | `string` | The underlying path string as supplied to the constructor; `ToString()` returns this value verbatim. |

## `ContentRoute`

```csharp:xmldocid
T:Pennington.Routing.ContentRoute
```

The universal route coordinate carried by every `ContentItem` case (`DiscoveredItem`, `ParsedItem`, `RenderedItem`, `FailedItem`) and referenced by every content service, navigation entry, cross-reference, search document, RSS item, and sitemap entry. `CanonicalPath` is the in-app URL including any locale prefix and a trailing slash; `OutputFile` is the path relative to `OutputOptions.OutputDirectory` that `OutputGenerationService` writes to.

### Properties

<ApiMemberTable XmlDocId="T:Pennington.Routing.ContentRoute" />

### `WithBaseUrl`

```csharp:xmldocid
M:Pennington.Routing.ContentRoute.WithBaseUrl(Pennington.Routing.UrlPath)
```

Returns `baseUrl / CanonicalPath` using `UrlPath.op_Division`. Produces an app-relative URL prefixed with a plain base path (such as `/preview/`); for absolute URLs with a scheme use `AbsoluteUrl` instead.

### `AbsoluteUrl`

```csharp:xmldocid
M:Pennington.Routing.ContentRoute.AbsoluteUrl(Pennington.Routing.UrlPath)
```

Composes `CanonicalPath` with the site's canonical base URL. Schemes are detected via `Uri.TryCreate(..., UriKind.Absolute, out _)`: absolute bases (such as `https://site.com`) are joined by trimmed string concatenation to preserve the scheme, while plain-path bases defer to `UrlPath.op_Division`. When `CanonicalPath` is `/`, the result is `<base>/` with the trailing slash preserved.

### `IsDefaultLocale`

```csharp:xmldocid
P:Pennington.Routing.ContentRoute.IsDefaultLocale
```

Computed property returning `string.IsNullOrEmpty(Locale)`. Consumers must use this property to test for the default locale rather than comparing `Locale` against `""` directly.

## `ContentRouteFactory`

```csharp:xmldocid
T:Pennington.Routing.ContentRouteFactory
```

The static class that constructs every `ContentRoute` in the codebase. Each method normalizes its inputs (leading slash, trailing slash, locale prefix) and builds the matching `OutputFile` via the shared internal `BuildOutputFile` helper, guaranteeing that `CanonicalPath` always ends with `/` and `OutputFile` always ends with `index.html`.

### `FromMarkdownFile`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromMarkdownFile(Pennington.Routing.FilePath,Pennington.Routing.FilePath,Pennington.Routing.UrlPath,System.String)
```

Builds the route for a markdown file relative to a content root. Resolves both paths via `Path.GetFullPath` and throws `ArgumentException` if `sourceFile` escapes `contentRoot`. A bare `index.md` maps to the base URL; a nested `.../index.md` maps to its parent directory rather than `.../index/`. Segment normalization lowercases and replaces backslashes; a non-empty locale prefix is prepended to `CanonicalPath`.

### `FromRazorPage`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromRazorPage(System.String,System.String)
```

Builds the route for a Razor `@page` directive. Ensures a leading slash, strips any trailing slash, prepends a non-empty locale prefix, and appends a trailing slash to the final canonical path. `SourceFile` is `null` because Razor pages do not have a single backing file from the route's perspective.

### `FromUrl`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromUrl(Pennington.Routing.UrlPath,System.String)
```

Builds the route for an explicit, programmatic URL. Applies the same normalization as `FromRazorPage` (leading slash, stripped trailing slash, optional locale prefix, trailing slash on the final canonical path). `SourceFile` is `null`. This is the primary factory for `IContentService` implementations that synthesize pages such as feeds, llms.txt sidecars, and custom index pages.

### `FromCustom`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromCustom(Pennington.Routing.UrlPath,System.Nullable{Pennington.Routing.FilePath},System.String)
```

Builds the route for a non-markdown content service where a backing source file still exists (such as JSON release notes or YAML data files). Applies the same URL normalization as `FromUrl`; `sourceFile` is stored on the resulting route so file-watching and cross-reference resolution can track the origin.

### `ForRedirect`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.ForRedirect(Pennington.Routing.UrlPath)
```

Builds the route for a redirect stub from the source URL (the URL that redirects); the destination lives on the corresponding `RedirectSource`. Locale is always empty because redirects are resolved before locale routing. `SourceFile` is `null`.

## Example

The following shows `IContentService.DiscoverAsync` from a custom content service, demonstrating `ContentRouteFactory.FromUrl` for the index page and `ContentRouteFactory.FromCustom` for each individual item.

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.DiscoverAsync
```

## See also

- How-to: [Implement a custom `IContentService`](xref:how-to.extensibility.custom-content-service)
- Related reference: [Content pipeline interfaces](xref:reference.extension-points.content-pipeline)
- Related reference: [Navigation types](xref:reference.extension-points.navigation)
- Background: [URL paths and content routes](xref:explanation.routing.url-paths)
