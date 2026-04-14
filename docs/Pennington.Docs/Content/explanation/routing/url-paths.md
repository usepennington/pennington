---
title: "URL paths and content routes"
description: "Why Pennington models URLs and filesystem paths as value-type records and why ContentRoute separates canonical identity from output location."
uid: explanation.routing.url-paths
order: 10
sectionLabel: "Routing and Navigation"
tags: [routing, value-types, urls, canonical]
---

> **In this page.** _Paraphrase the Covers line: why `UrlPath` and `FilePath` are value-type records with composition operators, how `ContentRoute` carries a canonical URL plus an output file as distinct fields, and what that shape buys over string paths threaded through method signatures. One sentence._
>
> **Not in this page.** _Paraphrase the Does-not-cover line: pointer to the full member catalog at `/reference/extension-points/routing` for readers who want every operator and helper. One sentence._

## The question

_Ask the reader's question in one sentence, something like: "Why does Pennington wrap paths in `UrlPath` and `FilePath` structs and track a separate `CanonicalPath` and `OutputFile` on every route, instead of just passing strings around the way most static site generators do?" Do not answer yet — the rest of the page is the answer._

## Context

_Three to five sentences. Open by noting that the hard bugs in a content engine are almost never in the Markdown parser — they cluster around the seams where a URL becomes a file, a file becomes a URL, or one URL is rewritten into another (locale prefix, base URL, canonical link, sitemap entry). Sketch the string-typed version of this world: a method takes `string path` and you squint at the parameter name to guess whether it has a leading slash, a trailing slash, backslashes, a `.html`, a locale prefix, or the base URL already applied. Mention that the alternatives on the table were a single `string` convention (document the expected shape and hope), a `Uri`-based shape (too loose — absolute URLs aren't what we want on disk), or typed records per concern. End the section by previewing the shape the rest of the page unfolds — paths are records, composition is an operator, and every route carries canonical identity separate from its output location._

## How it works

_Three subsections, narrative continuity across them. The first two describe the two moves: model paths as value types, then separate canonical identity from output. The third ties the shape to the rewriter pipeline so the reader sees the payoff in situ. Anchor each subsection with one xmldocid fence — no more — and only when the type signature makes the prose land harder._

### `UrlPath` and `FilePath` as value types

_Two or three paragraphs. Start with `UrlPath`: a `readonly record struct` wrapping a single string, with an implicit conversion from `string` so call sites read cleanly but every parameter that means "URL" is typed that way. Describe the `/` operator — composition, not division — that trims a trailing slash from the left and a leading slash from the right and joins them. Point out `EnsureLeadingSlash`, `EnsureTrailingSlash`, `RemoveTrailingSlash`, `RemoveLeadingSlash` as the normalization vocabulary every call site shares, so that "does this have a leading slash?" is never a guess. Note that `Matches` does the right thing for `/foo/`, `/foo/index.html`, and `/foo` — the three representations of the same directory page — which is load-bearing for the link checker and the resolver._

```csharp:xmldocid
T:Pennington.Routing.UrlPath
```

_After the fence, observe that `FilePath` is the filesystem-shaped peer — same value-record shape, same `/` composition operator, with `Extension`, `FileName`, and `FileNameWithoutExtension` standing in for the URL helpers. The two types are deliberately not interchangeable: a URL is a logical address, a file path is a disk location, and you cross the boundary explicitly through `ContentRoute` rather than by accident through `string`._

```csharp:xmldocid
T:Pennington.Routing.FilePath
```

_One sentence reinforcing that the ceremony — implicit conversions from literal strings, operators for composition — keeps the call-site prose short while the type system polices direction and shape._

### `ContentRoute`: canonical versus output

_The center of the page, two or three paragraphs. Open by naming the distinction: every page has a canonical identity — the URL the reader bookmarks, the URL the xref resolver writes, the URL the sitemap lists — and a separate output location — where the static build writes the HTML on disk. For a page served at `/docs/getting-started/`, canonical identity is `UrlPath("/docs/getting-started/")` and output is `FilePath("docs/getting-started/index.html")`. These are close, but conflating them is how you end up with bugs where the sitemap lists the on-disk path or the xref emits `/index.html` URLs._

```csharp:xmldocid
T:Pennington.Routing.ContentRoute
```

_After the fence, walk the reader through the remaining fields: `SourceFile` points back at the markdown on disk (or is null for programmatic routes), `Locale` annotates which translation this route serves, and `IsFallback` flags routes that serve default-locale content in place of a missing translation. Then introduce the composition methods — `WithBaseUrl` and `AbsoluteUrl` — as deliberate one-line operations: one produces a base-URL-prefixed path for sub-path hosting, the other produces a fully qualified URL for feeds and structured data. The point is that locale prefixing, base-URL application, and absolute-URL composition are all separate methods with separate call sites — they compose the canonical path rather than mutate it._

### Why this matters for locale and base-URL rewriting

_Two paragraphs. This is where the separation pays rent. The `IHtmlResponseRewriter` chain runs three rewriters in order — `XrefHtmlRewriter`, then `LocaleLinkHtmlRewriter`, then `BaseUrlHtmlRewriter` — each transforming the rendered HTML before it reaches the wire. Xref resolution emits canonical paths from uids. Locale rewriting prefixes internal links with the active locale. Base-URL rewriting prepends the deployment prefix last so it is the outermost transport layer. None of these rewriters would compose cleanly if the canonical URL were conflated with the output URL; the canonical path stays stable while transforms layer on, and the output file is computed once, at route construction, and never touched by rewriters._

_Second paragraph. The same separation drives the build crawler: `OutputGenerationService` fetches `CanonicalPath` over HTTP and writes the response to `OutputOptions.OutputDirectory / OutputFile`. If the two were one string, every rewriter would have to know whether it was rewriting "for display" or "for disk" and keep those modes in sync. Because they are two fields on one record, rewriters only ever see canonical paths, and the crawler only ever writes output files — the two worlds never have to negotiate._

## Trade-offs

- **Cost — more types to learn before reading any routing code.** A reader opening `ContentRouteFactory.FromMarkdownFile` for the first time meets `UrlPath`, `FilePath`, `ContentRoute`, and the `/` operator before meeting a method body. Strings would read faster on first contact. In exchange, the signatures that actually matter — every parameter that means "URL" is `UrlPath`, every parameter that means "file" is `FilePath` — cannot be accidentally crossed, and the compiler rejects mistakes that string-typed code would have to catch in a unit test.
- **Alternative considered — a single `string` convention with documented shape.** Every URL is canonicalized to leading-slash, trailing-slash, forward-slash before it enters the pipeline, and every function either obeys the convention or breaks. This is the shape most static site generators ship with. It was rejected because the convention only holds if every author of every function remembers to call the right normalizer, and the project's value is in the rewriter pipeline — three passes of URL transformation — where "did this already get normalized?" cannot be a judgment call.
- **Alternative considered — `System.Uri` for URLs.** `Uri` already knows about schemes, hosts, and paths, and comes with composition operators. It was rejected because most routes in the engine are path-only and treating them as `Uri` invited absolute-URL concerns at every seam; the canonical path needs to be a logical address, not a transport-layer one. `AbsoluteUrl` exists precisely to construct a `Uri`-shaped string when one is needed (RSS, JSON-LD) without forcing that shape on the ordinary cases.
- **Consequence — `ContentRoute` is the only place URLs and files touch.** If you find yourself converting between `UrlPath` and `FilePath` outside the routing namespace, the conversion probably belongs inside a `ContentRouteFactory` method. That is where the policy lives — index-file handling, locale prefixing, extension mapping — and duplicating it elsewhere is how the two representations drift.

## Further reading

- Reference: [Routing types](xref:reference.extension-points.routing)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
- Explanation: [Response processing and rewriters](xref:explanation.core.response-processing)
- External: [_TODO: add prior-art link — e.g., F#'s typed URLs in Giraffe, or a "Parse, don't validate" essay that motivates value-typed paths._]
