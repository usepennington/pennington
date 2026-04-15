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

The URL value type used throughout Pennington for canonical page paths, base URLs, locale prefixes, xref targets, and link attributes. The internal `Normalize` method treats a trailing `/index.html` or `/index.htm` as equivalent to the directory form, so `Matches` considers `/docs/` and `/docs/index.html` equal.

```csharp
public readonly record struct UrlPath(string Value)
{
    public static implicit operator UrlPath(string value);
    public static UrlPath operator /(UrlPath left, UrlPath right);

    public UrlPath EnsureLeadingSlash();
    public UrlPath EnsureTrailingSlash();
    public UrlPath RemoveTrailingSlash();
    public UrlPath RemoveLeadingSlash();
    public bool Matches(UrlPath other);
}
```

- **`operator /(UrlPath, UrlPath)`** — trims trailing slashes from the left operand and leading slashes from the right, joins with exactly one `/`, collapses empty operands to the non-empty side, and always produces a result with a leading slash.
- **`EnsureLeadingSlash` / `EnsureTrailingSlash`** — idempotently prepend/append `/`. Every path produced by `ContentRouteFactory` passes through `EnsureTrailingSlash` before being stored on `ContentRoute.CanonicalPath`.
- **`RemoveTrailingSlash`** — preserves the root `/`, so `new UrlPath("/").RemoveTrailingSlash()` still yields `/`.
- **`RemoveLeadingSlash`** — used by `ContentRouteFactory.BuildOutputFile` when converting a canonical URL path into an on-disk `OutputFile` path.
- **`Matches`** — case-insensitive equality after normalization (strips trailing `/`, `/index.html`, `/index.htm`, lowercases; empty re-normalizes to `/`). This is the comparison every request-time route lookup should use.

## `FilePath`

The filesystem path value type used wherever Pennington holds a path that may be read from disk or written to the output tree: `DiscoveredItem` sources, `ContentRoute.OutputFile`, `ContentRoute.SourceFile`, and `ContentToCopy` entries. Unlike `UrlPath`, `FilePath` retains whatever slash form the caller supplied and does not normalize to `/` or `\`.

```csharp
public readonly record struct FilePath(string Value)
{
    public static implicit operator FilePath(string value);
    public static FilePath operator /(FilePath left, FilePath right);

    public string Extension { get; }             // Path.GetExtension(Value)
    public string FileName { get; }              // Path.GetFileName(Value)
    public string FileNameWithoutExtension { get; }
}
```

- **`operator /(FilePath, FilePath)`** — trims trailing `/` and `\` from the left operand and leading `/` and `\` from the right, joins with a forward slash, and collapses empty operands to the non-empty side. Unlike `UrlPath.op_Division`, the result does not force a leading slash.
- Pennington stores forward slashes in-process and relies on `Path` APIs to read and write, so cross-platform callers should pass already-normalized strings.

## `ContentRoute`

The universal route coordinate carried by every `ContentItem` case (`DiscoveredItem`, `ParsedItem`, `RenderedItem`, `FailedItem`) and referenced by every content service, navigation entry, cross-reference, search document, RSS item, and sitemap entry. `CanonicalPath` is the in-app URL including any locale prefix and a trailing slash; `OutputFile` is the path relative to `OutputOptions.OutputDirectory` that `OutputGenerationService` writes to.

### Properties

<ApiMemberTable XmlDocId="T:Pennington.Routing.ContentRoute" />

### Methods

- **`WithBaseUrl(UrlPath baseUrl)`** — returns `baseUrl / CanonicalPath`. Produces an app-relative URL prefixed with a plain base path (such as `/preview/`); for absolute URLs with a scheme use `AbsoluteUrl` instead.
- **`AbsoluteUrl(UrlPath baseUrl)`** — composes `CanonicalPath` with the site's canonical base URL. Schemes are detected via `Uri.TryCreate(..., UriKind.Absolute, out _)`: absolute bases (such as `https://site.com`) are joined by trimmed string concatenation to preserve the scheme, while plain-path bases defer to `UrlPath.op_Division`. When `CanonicalPath` is `/`, the result is `<base>/` with the trailing slash preserved.
- **`IsDefaultLocale` (property)** — returns `string.IsNullOrEmpty(Locale)`. Consumers must use this property rather than comparing `Locale` against `""` directly.

## `ContentRouteFactory`

The static class that constructs every `ContentRoute` in the codebase. Each method normalizes its inputs (leading slash, trailing slash, locale prefix) and builds the matching `OutputFile` via the shared internal `BuildOutputFile` helper, guaranteeing that `CanonicalPath` always ends with `/` and `OutputFile` always ends with `index.html`.

- **`FromMarkdownFile(FilePath sourceFile, FilePath contentRoot, UrlPath basePageUrl, string locale)`** — route for a markdown file relative to a content root. Resolves both paths via `Path.GetFullPath` and throws `ArgumentException` if `sourceFile` escapes `contentRoot`. A bare `index.md` maps to the base URL; a nested `.../index.md` maps to its parent directory rather than `.../index/`. Segments are lowercased and backslashes replaced; a non-empty locale prefix is prepended to `CanonicalPath`.
- **`FromRazorPage(string pageTemplate, string locale)`** — route for a Razor `@page` directive. Ensures a leading slash, strips any trailing slash, prepends a non-empty locale prefix, and appends a trailing slash. `SourceFile` is `null`.
- **`FromUrl(UrlPath url, string locale)`** — route for an explicit, programmatic URL. Primary factory for `IContentService` implementations that synthesize pages (feeds, llms.txt sidecars, custom index pages). Same normalization as `FromRazorPage`; `SourceFile` is `null`.
- **`FromCustom(UrlPath url, FilePath? sourceFile, string locale)`** — route for a non-markdown content service where a backing source file still exists (JSON release notes, YAML data files). `sourceFile` is stored on the resulting route so file-watching and cross-reference resolution can track the origin.
- **`ForRedirect(UrlPath sourceUrl)`** — route for a redirect stub from the source URL; the destination lives on the corresponding `RedirectSource`. Locale is always empty because redirects are resolved before locale routing. `SourceFile` is `null`.

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
