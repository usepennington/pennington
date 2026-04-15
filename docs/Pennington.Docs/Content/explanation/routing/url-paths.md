---
title: "URL paths and content routes"
description: "Why Pennington models URLs and filesystem paths as value-type records and why ContentRoute separates canonical identity from output location."
uid: explanation.routing.url-paths
order: 303010
sectionLabel: "Routing and Navigation"
tags: [routing, value-types, urls, canonical]
---

Why does Pennington wrap paths in `UrlPath` and `FilePath` records and track a separate canonical path and output file on every route, rather than passing strings around the way most static site generators do?

## Context

The hard bugs in a content engine are almost never in the Markdown parser. They cluster around seams: where a URL becomes a file path, where a file path becomes a URL, where one URL is rewritten into another — locale prefix applied, base URL prepended, canonical link emitted, sitemap entry assembled. Each of those seams is a place where the wrong kind of string silently passes through and the error surfaces somewhere completely unrelated.

In a string-typed world, a method receives `string path` and callers squint at the parameter name to guess whether it carries a leading slash, a trailing slash, backslashes on Windows, a `.html` extension, a locale prefix, or the deployment base URL already folded in. The answer is almost never written down in the signature. It lives in a comment, a convention document, or the hard-won knowledge of whoever wrote the method. None of those survive a refactor.

The alternatives Pennington considered were a single `string` convention with documented normalization rules (document the expected shape and hope everyone remembers), a `System.Uri`-based shape (too broad — absolute URLs carry scheme and host concerns that belong to the transport layer, not the content model), and typed records per concern. The typed-record approach won, and the rest of this page explains why the shape holds up in practice — paths as value types, composition as an operator, and canonical identity separated from output location at every level.

## How it works

### `UrlPath` and `FilePath` as value types

`UrlPath` is a `readonly record struct` wrapping a single string. The implicit conversion from `string` keeps call sites readable — you can pass a string literal where a `UrlPath` is expected — but every parameter that means "URL" is typed that way rather than typed as `string`. That distinction is what lets the compiler catch the class of bugs that unit tests used to catch: a file path going into a URL slot, or a URL going into a file slot.

The `/` operator handles path composition. It trims a trailing slash from the left operand and a leading slash from the right, then joins them. The reason that operator exists, rather than a helper method named `Combine` or `Join`, is that composition is the dominant operation on paths in this codebase, and infix notation makes the intent read naturally at call sites. The normalization methods — `EnsureLeadingSlash`, `EnsureTrailingSlash`, `RemoveTrailingSlash`, `RemoveLeadingSlash` — share vocabulary across every call site so that "does this path have a leading slash?" is never a judgment call at the use site; it is answered once by the type.

The `Matches` method is load-bearing for the resolver and link checker: it treats `/foo/`, `/foo/index.html`, and `/foo` as the same directory page. That behavior centralizes a subtle normalization rule that would otherwise have to be replicated — slightly differently each time — wherever route matching happens.

```csharp:xmldocid
T:Pennington.Routing.UrlPath
```

`FilePath` is the filesystem-shaped peer — same value-record shape, same `/` composition operator, with `Extension`, `FileName`, and `FileNameWithoutExtension` standing in for the URL normalization helpers. The two types are deliberately not interchangeable: a URL is a logical address, a file path is a disk location, and the boundary between them is crossed explicitly through `ContentRoute` rather than accidentally through an untyped string.

```csharp:xmldocid
T:Pennington.Routing.FilePath
```

The ceremony — implicit conversions from string literals, operators for composition — keeps call-site code short while the type system does the policing.

### `ContentRoute`: canonical versus output

Every page has two different identities that happen to look similar. The canonical identity is the URL the reader bookmarks, the URL the xref resolver writes into cross-links, the URL the sitemap lists. The output location is where the static build writes HTML on disk. For a page served at `/docs/getting-started/`, canonical identity is `UrlPath("/docs/getting-started/")` and output location is `FilePath("docs/getting-started/index.html")`. Those differ by a trailing slash and a filename, and the difference matters: conflating them is how sitemaps end up listing on-disk paths or xrefs emit `/index.html` into bookmarked URLs.

```csharp:xmldocid
T:Pennington.Routing.ContentRoute
```

`ContentRoute` holds several fields alongside the canonical and output paths. `SourceFile` points back at the Markdown file on disk, or is absent for programmatic routes. `Locale` annotates which translation this route serves. `IsFallback` flags routes that serve default-locale content where a translation is missing. The composition methods — `WithBaseUrl` and `AbsoluteUrl` — are deliberate one-line operations rather than automatic transforms: one produces a base-URL-prefixed path for sub-path hosting scenarios, the other produces a fully qualified URL for feeds and structured data. Locale prefixing, base-URL application, and absolute-URL composition are all separate call sites, each composing the canonical path rather than quietly mutating it.

### Why this matters for locale and base-URL rewriting

This is where the canonical-versus-output separation pays its rent. The `IHtmlResponseRewriter` chain runs three rewriters in order: `XrefHtmlRewriter`, then `LocaleLinkHtmlRewriter`, then `BaseUrlHtmlRewriter`. Each transforms rendered HTML before it reaches the wire. Xref resolution emits canonical paths from uids. Locale rewriting prefixes internal links with the active locale. Base-URL rewriting prepends the deployment prefix last, as the outermost transport concern. None of these rewriters compose cleanly if the canonical URL is conflated with the output URL — the canonical path needs to stay stable as transforms layer on, and the output file needs to be computed once at route construction and then left alone.

The same separation drives the build crawler. `OutputGenerationService` fetches `CanonicalPath` over HTTP and writes the response body to `OutputOptions.OutputDirectory / OutputFile`. If canonical and output were a single string, every rewriter in the chain would need to know whether it was rewriting "for display" or "for disk" — and those two modes would have to stay in sync across every contributor and every future extension. Because they are two distinct fields on one record, rewriters only ever touch canonical paths and the crawler only ever writes output files. The two worlds do not need to negotiate.

## Trade-offs

- **Cost — more types to learn before reading any routing code.** Opening `ContentRouteFactory.FromMarkdownFile` for the first time means meeting `UrlPath`, `FilePath`, `ContentRoute`, and the `/` operator before meeting a method body. Strings would read faster on first contact. In exchange, the signatures that matter most — every parameter that means "URL" is `UrlPath`, every parameter that means "file" is `FilePath` — cannot be accidentally crossed, and the compiler rejects mistakes that string-typed code would have to catch in a unit test.
- **Alternative considered — a single `string` convention with documented shape.** Every URL is canonicalized to leading-slash, trailing-slash, forward-slash before it enters the pipeline, and every function either obeys the convention or breaks. This is the shape most static site generators ship with. It was rejected because the convention only holds if every author of every function remembers to call the right normalizer, and the project's value is in the rewriter pipeline — three passes of URL transformation — where "did this already get normalized?" cannot be a judgment call.
- **Alternative considered — `System.Uri` for URLs.** `Uri` already knows about schemes, hosts, and paths, and comes with composition operators. It was rejected because most routes in the engine are path-only and treating them as `Uri` invited absolute-URL concerns at every seam; the canonical path needs to be a logical address, not a transport-layer one. `AbsoluteUrl` exists precisely to construct a `Uri`-shaped string when one is needed (RSS, JSON-LD) without forcing that shape on the ordinary cases.
- **Consequence — `ContentRoute` is the only place URLs and files touch.** If you find yourself converting between `UrlPath` and `FilePath` outside the routing namespace, the conversion probably belongs inside a `ContentRouteFactory` method. That is where the policy lives — index-file handling, locale prefixing, extension mapping — and duplicating it elsewhere is how the two representations drift.

## Further reading

- Reference: [Routing types](xref:reference.extension-points.routing)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
- Explanation: [Response processing and rewriters](xref:explanation.core.response-processing)
- External: [Parse, don't validate (Alexis King)](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/)
