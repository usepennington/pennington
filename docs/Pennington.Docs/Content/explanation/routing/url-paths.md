---
title: "URL paths and content routes"
description: "The UrlPath/FilePath value-type design, how ContentRoute carries canonical vs. output paths, and why path handling avoids string-typing."
section: "routing"
order: 10
tags: []
uid: explanation.routing.url-paths
isDraft: true
search: false
llms: false
---

> **In this page.** The `UrlPath`/`FilePath` value-type design, how `ContentRoute` carries canonical vs. output paths, and why path handling avoids string-typing.
>
> **Not in this page.** Member-level API details (see Reference).

## The question

Why does Pennington model URLs and filesystem paths as distinct value types instead of passing `string` everywhere?

## Context

- Static site generators are path-shaped: every stage (discovery, parsing, rendering, navigation, xref, sitemap, llms.txt, RSS, search, locale rewriting, base-url transport) has to reason about "where does this page live?"
- A string-typed pipeline conflates several unrelated coordinates: the URL the browser sees, the file on disk that produced it, and the file on disk that will be written. Each has different separator conventions, different normalization rules (trailing slash, `/index.html`), and different composition semantics.
- The loss of type distinction shows up as a family of bugs: double slashes from naive concatenation, mixed forward/backslashes on Windows, `/index.html` vs. directory-form mismatches when comparing, locale prefixes applied twice, absolute URLs treated as paths.
- Pennington's answer is three types in `Pennington.Routing`: two `readonly record struct` value types that own path math, and one `sealed record` that bundles the coordinates of a single page.

## How it works

### Two path kinds, two value types

`UrlPath` and `FilePath` are both `readonly record struct` wrappers over a single `Value` string, but they model different domains and expose different operations. `UrlPath` knows about leading/trailing slashes, `/index.html` normalization, and case-insensitive matching. `FilePath` knows about extensions, filename parts, and mixed path separators.

```csharp:xmldocid
T:Pennington.Routing.UrlPath
```

```csharp:xmldocid
T:Pennington.Routing.FilePath
```

Both types define an implicit conversion from `string` and a `/` operator for composition. The implicit conversion keeps call sites readable; the operator replaces `Path.Combine`-style concatenation with something that handles the empty-side and leading-slash cases consistently. `UrlPath.Matches` normalizes `/index.html` to directory form before comparing, which is the canonical source of truth for "are these two URLs the same page?"

### Canonical vs. output: one route, two coordinates

A single page has two paths that the compiler should never let you mix up: the URL users navigate to, and the file on disk the build writes. `ContentRoute` names both.

```csharp:xmldocid
T:Pennington.Routing.ContentRoute
```

- `CanonicalPath : UrlPath` — the directory-form URL the browser sees (`/docs/getting-started/`). Always leading-slashed, always trailing-slashed.
- `OutputFile : FilePath` — the file the static build writes (`docs/getting-started/index.html`). Always relative to the output root.
- `SourceFile : FilePath?` — optional provenance: the markdown or Razor file that produced the route, when one exists.
- `Locale` + `IsFallback` — locale coordinate and a flag marking default-locale content served in place of a missing translation.

The shape is deliberate. Canonical is what the user sees, xref resolution targets, sitemap entries enumerate, and RSS items link to. Output is where bytes land on disk. `WithBaseUrl` composes the canonical path with a site base; `AbsoluteUrl` handles the scheme-bearing case where `UrlPath`'s path-only operator would produce `/https://site.com`.

### Construction goes through a factory, not ad-hoc

`ContentRouteFactory` is the only sanctioned way to build a `ContentRoute`. Each entry point normalizes a different source of truth into the same two coordinates.

- `FromMarkdownFile` — resolves a source file against a content root, strips the extension, collapses `index` segments so `getting-started/index.md` becomes `/getting-started/` (not `/getting-started/index/`), applies the locale prefix, and derives `OutputFile` from the canonical path.
- `FromRazorPage` — the `@page` directive string is already a URL; the factory only locale-prefixes and normalizes slashes.
- `FromUrl` / `FromCustom` — programmatic registrations from content services.
- `ForRedirect` — source URL to route, where the output is a redirect HTML stub.

Every factory converges on the same invariant: `CanonicalPath` is leading- and trailing-slashed, `OutputFile` ends in `/index.html`, and the two are derived from each other rather than held independently. Downstream code never has to ask "is this path normalized yet?"

### Why this keeps the pipeline honest

`ContentRoute` is the universal coordinate threaded through the four-stage `union ContentItem` pipeline (`DiscoveredItem`, `ParsedItem`, `RenderedItem`, `FailedItem` each expose a `Route`). Navigation, xref, search, sitemap, RSS, llms.txt, locale-link rewriting, and base-URL rewriting all consume `UrlPath` arguments; `OutputGenerationService` and the llms.txt sidecar writer consume `FilePath`. Because the two types are distinct, a renderer that tries to write a URL to disk — or a navigation builder that tries to link to an output file — fails at the type level rather than producing a broken link at build time.

## Trade-offs

- **Cost:** Two more type names in every signature that used to take `string`. Readers new to the codebase have to learn that `UrlPath` and `FilePath` are value types, not validated wrappers, and that the implicit `string` conversion keeps call sites readable.
- **Alternative considered:** A single `Path` type parametrized by a kind tag, or an IDE-level convention of naming strings `url` vs. `file`. Both were rejected because neither stops a stray concatenation from compiling, and the single-type design doesn't let `UrlPath` own URL-specific normalization (`/index.html`) while `FilePath` owns filesystem-specific accessors (`Extension`, `FileName`).
- **Alternative considered:** Validating constructors that reject malformed input. Rejected because the value types are meant to be cheap — an implicit `string` conversion shouldn't allocate or throw. Normalization happens at factory boundaries (`ContentRouteFactory`) where the source of truth is known, not on every struct construction.
- **Consequence:** Every new path-shaped API should declare `UrlPath` or `FilePath`, not `string`. Reaching for `string` in a route signature is a signal that the author hasn't decided which coordinate they mean — and if they haven't, neither will the caller.

## Further reading

- Reference: [Routing extension points](/reference/extension-points/routing)
- Reference: [Host extensions](/reference/host/extensions)
- External: [Parse, don't validate (Alexis King)](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/)
