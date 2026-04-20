---
title: "The response-processing pipeline"
description: "Why Pennington splits response rewriting into generic body processors and HTML-DOM rewriters that share one AngleSharp pass."
uid: explanation.core.response-processing
order: 301040
sectionLabel: "Core Architecture"
tags: [response-processing, rewriters, anglesharp, middleware]
---

Why are there two extension points for rewriting the response body — a general `IResponseProcessor` and an HTML-specific `IHtmlResponseRewriter` — instead of a single chain of string-to-string processors?

## Context

Two kinds of work want to touch the response body. Some concerns care about HTML structure — rewriting `href` attributes, inserting elements at specific selectors, normalizing document shape. Others treat the body as an opaque string — appending a script before `</body>`, scraping class names out of whatever HTML arrived, injecting a dev overlay into HTML responses only.

A single chain of string-to-string processors forces the structure-aware concerns to either reparse the body themselves or rewrite HTML with regex, neither of which composes. A single HTML-shaped rewriter forces the opaque-string concerns to take an AngleSharp dependency they do not need. Pennington splits the work along that fault line: a generic string-in/string-out contract for body-level concerns that have no need for a DOM, and one shared parse that all DOM-shaped concerns participate in together.

## How it works

### Tier A: `IResponseProcessor` (generic body capture)

`ResponseProcessingMiddleware` wraps the response body stream, captures it into memory, runs every registered `IResponseProcessor` whose `ShouldProcess` returns `true` in ascending `Order`, then flushes the final string back to the socket. The contract is intentionally narrow — an ordering integer, a predicate over `HttpContext`, and an async body transform — so anything that wants to touch the response body can participate without taking an AngleSharp dependency.

The built-in processors illustrate why the tier exists. `HtmlResponseRewritingProcessor` at `Order 10` hosts the entire Tier B pass. `LiveReloadScriptProcessor` at `Order 20` injects the reconnect script immediately before `</body>` during development. `DiagnosticOverlayProcessor` at `Order 30` renders the collected `DiagnosticContext` into a corner panel, also in dev mode. `CssClassCollectorProcessor`, contributed by the MonorailCSS integration, scrapes utility classes out of emitted HTML so the stylesheet endpoint can regenerate on the next request.

Three of those four are pure string operations — a targeted string insert, a before-`</body>` append, a regex scan. Routing them through AngleSharp would parse and serialize the document for no benefit. The tier boundary exists because "touches the body" is a broader category than "cares about HTML structure."

### Tier B: `IHtmlResponseRewriter` (shared AngleSharp pass)

Tier B lives entirely inside `HtmlResponseRewritingProcessor`. The orchestrator calls each rewriter's `ShouldApply(HttpContext)` first. If none return `true`, the body comes back untouched and AngleSharp never fires — the fast path for non-HTML content types, error pages, and opted-out endpoints. If at least one applies, the orchestrator runs all of them through a two-phase pipeline.

The first phase, `PreParseAsync`, operates on the raw HTML string. This handles constructs AngleSharp cannot represent cleanly — the `<xref:uid>` tag syntax is not valid HTML, so it needs to be rewritten into something the parser can consume before the document is built. The second phase, `ApplyAsync`, receives a single shared `IDocument` that every rewriter mutates in turn. The document is serialized once at the end.

The invariant worth internalizing: N rewriters, one parse, one serialize, one DOM. Adding a new DOM-shaped concern — a heading-anchor normalizer, an image lazy-loader, a table classifier — costs a method call, not another parse/serialize round trip. See <xref:reference.api.i-response-processor> for the `IResponseProcessor` and `IHtmlResponseRewriter` contracts.

### Why two tiers, not one

Collapsing everything into `IHtmlResponseRewriter` would be wrong in both directions. Pulling the string processors into Tier B forces an AngleSharp parse on operations that do not benefit from a DOM, and it also means the parser tries to fix partially-valid or framework-generated HTML that those processors are content to treat as an opaque string. Conversely, letting every DOM-shaped concern be its own `IResponseProcessor` means each one parses, mutates, and serializes independently — N parses and N DOM copies where one of each would do — and it hides the cross-concern ordering assumptions behind DI registration sequence.

The split follows a natural fault line: does this concern care about HTML structure? Body-level injection and scraping live on one side; link rewriting and attribute manipulation live on the other. Each contract stays narrow to its side of that line.

The body-capture middleware itself remains deliberately agnostic about HTML. The gate — checking `Content-Type`, confirming `StatusCode` is in the 2xx/3xx range, verifying at least one rewriter's `ShouldApply` returned `true` — lives in `HtmlResponseRewritingProcessor.ShouldProcess`, not in the middleware. That keeps the AngleSharp dependency from activating on JSON responses, 404 pages, or any endpoint that opted out. Delegating that gate downward is the sort of thing a generic middleware should do rather than special-case inline.

### Why order is load-bearing

The three built-in HTML rewriters run in a specific sequence because each one produces the link shape the next one expects to consume.

`XrefHtmlRewriter` at `Order 10` resolves `<xref:uid>` tag syntax (in `PreParseAsync`) and `href="xref:uid"` attributes (in `ApplyAsync`) into canonical root-relative paths. It runs first so everything downstream sees real URLs rather than symbolic cross-reference handles.

`LocaleLinkHtmlRewriter` at `Order 20` prefixes internal links with the active locale segment, turning `/some/path` into `/fr/some/path`. It runs after xref resolution because an unresolved `xref:uid` is not a path yet, and before base-URL rewriting so the locale segment ends up inside the base URL rather than outside it.

`BaseUrlHtmlRewriter` at `Order 30` prefixes root-relative URLs with the configured deployment base URL and stamps `data-base-url` on `<body>`. Running last means it acts as the outermost transport layer: the two earlier rewriters can work with clean `/`-rooted paths and never have to strip a base URL before operating.

Reversing any two of these breaks one of the others' invariants. Keeping the ordering explicit in the `Order` property — rather than implicit in DI registration sequence — is what makes the dependency between rewriters visible at the call site.

## Trade-offs

- **Cost:** A rewriter cannot assume it is operating on unmodified HTML — earlier rewriters may have already transformed the links it cares about. Authors need to reason about where in the ordering sequence their rewriter belongs, which is more cognitive load than a last-in-wins chain.
- **Cost:** The shared `IDocument` is mutable and traversed by every rewriter in sequence. A rewriter that over-selects — querying all anchors when it only cares about external links, for instance — can silently interfere with a neighbor that queries the same elements. Narrow selectors are what keep the pipeline composable.
- **Alternative considered:** A chain of pure string-to-string processors, each reparsing as needed. This was the original shape, and it either duplicates parsing work or pushes every HTML-shaped concern into ad-hoc regex. Regex is fragile against real-world HTML that AngleSharp's tolerance for malformed input handles gracefully.
- **Alternative considered:** A single monolithic `HtmlRewriter` with hardcoded stages. This resolves the parse cost but closes the extensibility surface — every new concern would have to land in core rather than in a library consumer's assembly. The `IHtmlResponseRewriter` boundary is what keeps that story open.
- **Consequence:** `ShouldApply` is the cheapest gate in the pipeline. When every rewriter returns `false`, the orchestrator skips the parse entirely. This is the fast path for non-HTML responses, search-index files, the SPA envelope endpoint, and any other endpoint where rewriting is a no-op.

## Further reading

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor) — the member-by-member catalog of `IResponseProcessor`, `IHtmlResponseRewriter`, and the three built-in rewriters.
- How-to: [Write a response processor](xref:how-to.extensibility.response-processor) — for touching the raw body.
- How-to: [Write an HTML rewriter](xref:how-to.extensibility.html-rewriter) — for working inside the shared DOM pass.
- Related explanation: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — why the same processor chain runs against both live requests and the static-build crawler.
