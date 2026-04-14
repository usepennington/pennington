---
title: "Routing types"
description: "The four types in Pennington.Routing that own URL math, filesystem paths, canonical routes, and route construction across every content source."
sectionLabel: "Extension Points"
order: 20
tags: [routing, url-path, content-route, extension-points]
uid: reference.extension-points.routing
---

> **In this page.** `UrlPath`, `FilePath`, `ContentRoute`, and `ContentRouteFactory` — constructors, operators, and helper methods (`EnsureLeadingSlash`, `WithBaseUrl`, `AbsoluteUrl`, `IsDefaultLocale`).
>
> **Not in this page.** The broader URL design philosophy — why paths are value types, what invariants `ContentRoute` preserves, how the pipeline treats canonical vs. output paths — lives in [URL paths and content routes](/explanation/routing/url-paths).

## Summary

_**One sentence: what it is.** The four types that together form Pennington's canonical route coordinate system — two value-type path wrappers (`UrlPath`, `FilePath`), the `ContentRoute` record that every pipeline stage carries, and the `ContentRouteFactory` static that constructs routes from each supported content origin._
_**One sentence: where it lives.** Namespace `Pennington.Routing` (`src/Pennington/Routing/`); every downstream namespace depends on these types, so every `ContentItem`, every `IContentService`, and every rewriter ultimately speaks in `ContentRoute`._

## Overview

_Four-row table keyed by type. Columns: **Type**, **Kind**, **Purpose**. One-sentence purposes only — this is the landing index for the four types bundled on this page. Ordered by dependency: the two path value types first, then the route record, then the factory._

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

_The URL value type used throughout Pennington for everything that represents a URL path: canonical page paths, base URLs, locale prefixes, xref targets, link attributes. `Normalize` (internal) treats a trailing `/index.html` or `/index.htm` as equivalent to the directory form, which is why `Matches` considers `/docs/` and `/docs/index.html` equal._

### Constructor

```csharp:xmldocid
M:Pennington.Routing.UrlPath.#ctor(System.String)
```

_Primary constructor on the record struct. Takes a single `string Value`; no validation is performed on the input, so callers that need a leading slash must route the result through `EnsureLeadingSlash`._

### `implicit operator UrlPath(string)`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.op_Implicit(System.String)~Pennington.Routing.UrlPath
```

_Implicit conversion from `string` — every `string` literal or variable in a context expecting `UrlPath` is wrapped without an explicit `new`. Match the implicit conversion when writing overloads that accept `UrlPath`: do not add a separate `string` overload._

### `operator /(UrlPath, UrlPath)`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.op_Division(Pennington.Routing.UrlPath,Pennington.Routing.UrlPath)
```

_Path-composition operator. Trims trailing slashes from the left, leading slashes from the right, and joins with exactly one `/`. Empty operands collapse: `new UrlPath("") / "/x"` returns `/x`, and `new UrlPath("/a") / ""` returns `/a`. The result always has a leading slash._

### `EnsureLeadingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.EnsureLeadingSlash
```

_Returns `this` if `Value` already starts with `/`, otherwise a new `UrlPath` with one prepended. Idempotent; pairs with `EnsureTrailingSlash` to produce the directory-form canonical path that `ContentRoute.CanonicalPath` is documented to carry._

### `EnsureTrailingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.EnsureTrailingSlash
```

_Returns `this` if `Value` already ends with `/`, otherwise a new `UrlPath` with one appended. Idempotent; every path produced by `ContentRouteFactory` passes through this method before being stored on `ContentRoute.CanonicalPath`._

### `RemoveTrailingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.RemoveTrailingSlash
```

_Returns `this` if `Value.Length <= 1` or does not end with `/`; otherwise a new `UrlPath` with the trailing slash stripped. The length guard preserves the root `/` as a special case so `new UrlPath("/").RemoveTrailingSlash()` still yields `/`._

### `RemoveLeadingSlash`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.RemoveLeadingSlash
```

_Returns `this` if `Value` does not start with `/`; otherwise a new `UrlPath` with the leading slash stripped. Used by `ContentRouteFactory.BuildOutputFile` when converting a canonical URL path into an on-disk `OutputFile` path._

### `Matches`

```csharp:xmldocid
M:Pennington.Routing.UrlPath.Matches(Pennington.Routing.UrlPath)
```

_Case-insensitive equality check after normalization. Normalization strips a trailing `/`, then strips a trailing `/index.html` or `/index.htm`, then lowercases; the empty result is re-normalized to `/`. This is the comparison every request-time route lookup should use._

### Properties

| Name | Type | Description |
|---|---|---|
| `Value` | `string` | The underlying path string as supplied to the constructor; `ToString()` returns this value verbatim. |

## `FilePath`

```csharp:xmldocid
T:Pennington.Routing.FilePath
```

_The filesystem path value type used wherever Pennington holds a path that may be read from disk or written to the output tree: `DiscoveredItem` sources, `ContentRoute.OutputFile`, `ContentRoute.SourceFile`, `ContentToCopy` entries. Unlike `UrlPath`, `FilePath` retains whatever slash form the caller supplied — it does not normalize to `/` or `\`._

### Constructor

```csharp:xmldocid
M:Pennington.Routing.FilePath.#ctor(System.String)
```

_Primary constructor on the record struct. Takes a single `string Value`; no validation or normalization is performed. Callers constructing paths cross-platform should pass already-normalized strings (Pennington stores forward slashes in-process and relies on `Path` APIs to read/write)._

### `implicit operator FilePath(string)`

```csharp:xmldocid
M:Pennington.Routing.FilePath.op_Implicit(System.String)~Pennington.Routing.FilePath
```

_Implicit conversion from `string`; any `string` literal or variable passed where a `FilePath` is expected is wrapped without ceremony. Used heavily in examples that construct `DiscoveredItem`s with paths rooted at the content directory._

### `operator /(FilePath, FilePath)`

```csharp:xmldocid
M:Pennington.Routing.FilePath.op_Division(Pennington.Routing.FilePath,Pennington.Routing.FilePath)
```

_Composition operator. Trims trailing `/` and `\` from the left, leading `/` and `\` from the right, and joins with a forward slash. Empty operands collapse to the non-empty side; unlike `UrlPath.op_Division` the result does not force a leading slash._

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

_The universal route coordinate. Every `ContentItem` case (`DiscoveredItem`, `ParsedItem`, `RenderedItem`, `FailedItem`) carries one, and every content service, navigation entry, cross-reference, search document, RSS item, and sitemap entry is keyed off one. `CanonicalPath` is the in-app URL (with locale prefix and trailing slash); `OutputFile` is the path relative to `OutputOptions.OutputDirectory` that `OutputGenerationService` writes to._

### Properties

_Alphabetical. All five are init-only; `CanonicalPath` and `OutputFile` are required, the remaining three have defaults._

| Name | Type | Default | Description |
|---|---|---|---|
| `CanonicalPath` | `UrlPath` | _required_ | The in-app URL for this route, including any locale prefix and always in directory form (trailing slash); this is the path `ContentResolver` compares against the incoming request. |
| `IsFallback` | `bool` | `false` | True when this route serves default-locale content as a stand-in for a missing translation; surfaced by the UI via `FallbackNotice`. |
| `Locale` | `string` | `""` | Empty for default-locale routes, otherwise the locale code (e.g., `"es"`); `IsDefaultLocale` reports `string.IsNullOrEmpty(Locale)`. |
| `OutputFile` | `FilePath` | _required_ | The on-disk path, relative to `OutputOptions.OutputDirectory`, where the static build writes this route; always ends in `index.html`. |
| `SourceFile` | `FilePath?` | `null` | The backing source file when one exists (markdown pages, custom `FromCustom` routes); `null` for Razor pages, programmatic routes, and redirects. |

### `WithBaseUrl`

```csharp:xmldocid
M:Pennington.Routing.ContentRoute.WithBaseUrl(Pennington.Routing.UrlPath)
```

_Returns `baseUrl / CanonicalPath` using `UrlPath.op_Division`. Produces an app-relative URL prefixed with a plain base path (e.g., `/preview/`); for absolute URLs with a scheme use `AbsoluteUrl` instead._

### `AbsoluteUrl`

```csharp:xmldocid
M:Pennington.Routing.ContentRoute.AbsoluteUrl(Pennington.Routing.UrlPath)
```

_Composes `CanonicalPath` with the site's canonical base URL. Detects schemes via `Uri.TryCreate(..., UriKind.Absolute, out _)`: absolute bases (e.g., `https://site.com`) are joined by trimmed string concatenation so the scheme is preserved, while plain-path bases defer to `UrlPath.op_Division`. Root-path edge case: an absolute base with `CanonicalPath == "/"` yields `<base>/` (trailing slash preserved)._

### `IsDefaultLocale`

```csharp:xmldocid
P:Pennington.Routing.ContentRoute.IsDefaultLocale
```

_Computed property returning `string.IsNullOrEmpty(Locale)`. The canonical way to test whether a route belongs to the default locale; consumers must not compare `Locale` against `""` directly._

## `ContentRouteFactory`

```csharp:xmldocid
T:Pennington.Routing.ContentRouteFactory
```

_The static class that constructs every `ContentRoute` in the codebase. Each method normalizes its inputs (leading slash, trailing slash, locale prefix) and builds the matching `OutputFile` via the shared internal `BuildOutputFile` helper, so every constructed route satisfies the invariants `CanonicalPath` ends with `/` and `OutputFile` ends with `index.html`._

### `FromMarkdownFile`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromMarkdownFile(Pennington.Routing.FilePath,Pennington.Routing.FilePath,Pennington.Routing.UrlPath,System.String)
```

_Builds the route for a markdown file relative to a content root. Resolves both paths via `Path.GetFullPath` and throws `ArgumentException` if `sourceFile` escapes `contentRoot`. Index-file collapse: a bare `index.md` maps to the base URL, and a nested `.../index.md` maps to its parent directory (not `.../index/`). Segment normalization lowercases and replaces backslashes; locale prefix is applied to `CanonicalPath` when non-empty._

### `FromRazorPage`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromRazorPage(System.String,System.String)
```

_Builds the route for a Razor `@page` directive. Ensures a leading slash, strips any trailing slash, prepends the locale prefix when non-empty, and ends the canonical path with a trailing slash. `SourceFile` is left `null` — Razor pages do not have a single backing file from the route's perspective._

### `FromUrl`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromUrl(Pennington.Routing.UrlPath,System.String)
```

_Builds the route for an explicit, programmatic URL. Same normalization as `FromRazorPage`: leading slash, strip trailing slash, optional locale prefix, trailing slash on the final canonical path. `SourceFile` is `null`. This is the primary factory used by `IContentService` implementations that synthesize pages (feeds, llms.txt sidecars, custom index pages)._

### `FromCustom`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.FromCustom(Pennington.Routing.UrlPath,System.Nullable{Pennington.Routing.FilePath},System.String)
```

_Builds the route for a non-markdown content service where a backing source file still exists (e.g., JSON release notes, YAML data files). Same URL normalization as `FromUrl`; `sourceFile` is stored on the resulting route so file-watching and cross-reference resolution can track the origin._

### `ForRedirect`

```csharp:xmldocid
M:Pennington.Routing.ContentRouteFactory.ForRedirect(Pennington.Routing.UrlPath)
```

_Builds the route for a redirect stub. Takes only the source URL (the URL that will redirect); the destination lives on the corresponding `RedirectSource`. Locale is always empty — redirects are resolved before locale routing. `SourceFile` is `null`._

## Example

_One minimal example pulled from `examples/ExtensibilityLabExample/ReleaseNotesContentService.cs` — the `IContentService.DiscoverAsync` method that constructs a route via `ContentRouteFactory.FromUrl` for the index page and a second via `ContentRouteFactory.FromCustom` for each release. Shown as the method body so the two factory calls, the `UrlPath` constructors, and the `FilePath` constructor are all visible in one place._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.ReleaseNotesContentService.DiscoverAsync
```

_Reference shape for constructing `ContentRoute`s from a custom `IContentService`: wrap the URL strings in `UrlPath`, wrap the source path in `FilePath`, call the matching `ContentRouteFactory` method._

## See also

- How-to: [Implement a custom `IContentService`](/how-to/extensibility/custom-content-service)
- Related reference: [Content pipeline interfaces](/reference/extension-points/content-pipeline)
- Related reference: [Navigation types](/reference/extension-points/navigation)
- Background: [URL paths and content routes](/explanation/routing/url-paths)
