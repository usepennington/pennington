---
title: "The response-processing pipeline"
description: "Why Pennington splits response rewriting into generic body processors and HTML-DOM rewriters that share one AngleSharp pass."
uid: explanation.core.response-processing
order: 5
sectionLabel: "Core Architecture"
tags: [response-processing, rewriters, anglesharp, middleware]
---

Why are there two extension points for rewriting the response body — a general `IResponseProcessor` and an HTML-specific `IHtmlResponseRewriter` — instead of a single chain of string-to-string processors?

## Context

Two kinds of work want to touch the response body. Some concerns care about HTML structure — rewriting `href` attributes, inserting elements at specific selectors, normalizing document shape. Others treat the body as an opaque string — appending a script before `</body>`, prepending a cache buster, injecting a dev overlay into HTML responses only.

A single chain of string-to-string processors forces the structure-aware concerns to either reparse the body themselves or rewrite HTML with regex, neither of which composes. A single HTML-shaped rewriter forces the opaque-string concerns to take an AngleSharp dependency they do not need. Pennington splits the work along that line: a generic string-in/string-out contract for body-level concerns that have no need for a DOM, and one shared parse that all DOM concerns participate in together.

## How it works

Pennington uses two extension points. The first is the generic body pipeline (`IResponseProcessor`); every body-touching concern, HTML or not, registers here. The second is one specific `IResponseProcessor` — `HtmlResponseRewritingProcessor` — that hosts a shared AngleSharp pass for HTML-DOM concerns (`IHtmlResponseRewriter`).

Several body processors ship built in: the rewriter host above, a CSS URL rewriter, the dev-only live-reload and diagnostic-overlay injectors, and `NotFoundStatusProcessor`. They are all body-level concerns that share the same generic contract; the [response-processing interfaces reference](xref:reference.api.i-response-processor) catalogs each one.

`NotFoundStatusProcessor` is the one that needs a word, because it is doing something the others are not. A content page that resolves to a missing route does not set `StatusCode = 404` itself — it sets a marker on `HttpContext.Items` and renders the 404 body normally. Every other processor in the chain gates on a 2xx status, so flipping the status early would short-circuit them. `NotFoundStatusProcessor` runs last and flips the status to 404 only after the body is fully composed, which keeps the rendered 404 page — localized chrome, layout, structured data — intact while still surfacing a real 404 to crawlers and link checkers.

### Tier A: `IResponseProcessor` (generic body capture)

`ResponseProcessingMiddleware` wraps the response body stream, captures it into memory, runs every registered `IResponseProcessor` whose `ShouldProcess` returns `true` in ascending `Order`, then flushes the final string back to the socket. The contract is intentionally narrow — an ordering integer, a predicate over `HttpContext`, and an async body transform — so anything that wants to touch the response body can participate without taking an AngleSharp dependency.

`LiveReloadScriptProcessor` and `DiagnosticOverlayProcessor` illustrate why the tier exists. Both are pure string operations — a targeted insert and a before-`</body>` append. Routing them through AngleSharp would parse and serialize the document for no benefit. The tier boundary exists because "touches the body" is a broader category than "cares about HTML structure."

### Tier B: `IHtmlResponseRewriter` (shared AngleSharp pass)

The HTML rewriters live entirely inside `HtmlResponseRewritingProcessor`. The orchestrator calls each rewriter's `ShouldApply(HttpContext)` first. If none return `true`, the body comes back untouched and AngleSharp never fires — non-HTML content types, error pages, and opted-out endpoints skip the parse entirely. If at least one applies, the orchestrator runs all of them through a two-phase pipeline.

The first phase, `PreParseAsync`, operates on the raw HTML string. This handles constructs AngleSharp cannot represent cleanly — the `<xref:uid>` tag syntax is not valid HTML, so it needs to be rewritten into something the parser can consume before the document is built. The second phase, `ApplyAsync`, receives a single shared `IDocument` that every rewriter mutates in turn. The document is serialized once at the end.

The result is the invariant that matters here: N rewriters, one parse, one serialize, one DOM. Adding a new DOM-shaped concern — a heading-anchor normalizer, an image lazy-loader, a table classifier — costs a method call, not another parse/serialize round trip. See <xref:reference.api.i-response-processor> for the `IResponseProcessor` and `IHtmlResponseRewriter` contracts.

### Why two tiers, not one

Collapsing everything into `IHtmlResponseRewriter` would be wrong in both directions. Making the string processors HTML rewriters forces an AngleSharp parse on operations that do not benefit from a DOM, and it also means the parser tries to fix partially-valid or framework-generated HTML that those processors are content to treat as an opaque string. Conversely, letting every DOM concern be its own `IResponseProcessor` means each one parses, mutates, and serializes independently — N parses and N DOM copies instead of one of each — and it hides the cross-concern ordering assumptions behind DI registration sequence.

The split follows one question: does this concern care about HTML structure? Body-level injection and scraping live on one side; link rewriting and attribute manipulation live on the other. Each contract stays narrow to its side of that line.

### Why the order matters

This is the page that owns the rewriter ordering; the other explanation pages that depend on one slot of it link here rather than restate the whole chain. Six rewriters ship, and they run lowest `Order` first:

| `Order` | Rewriter | What it does |
|---|---|---|
| 10 | `XrefHtmlRewriter` | Resolves `<xref:uid>` tags and `href="xref:uid"` into canonical paths |
| 20 | `LocaleLinkHtmlRewriter` | Prefixes internal links with the active locale segment |
| 25 | `HeadCompositionHtmlRewriter` | Composes the `IHeadContributor` output into the head |
| 30 | `BaseUrlHtmlRewriter` | Prepends the deployment base URL and stamps `data-base-url` |
| 40 | `FallbackLangHtmlRewriter` | Marks links served from default-locale fallback |
| 60 | `WordBreakHtmlRewriter` | Inserts soft word breaks into long identifiers |

The first four form a dependency chain because each produces the link shape the next consumes. `XrefHtmlRewriter` runs first so everything downstream sees real URLs rather than symbolic cross-reference handles. `LocaleLinkHtmlRewriter` runs after it because an unresolved `xref:uid` is not yet a path, and before base-URL rewriting so the locale segment ends up inside the base URL rather than outside it. `HeadCompositionHtmlRewriter` slots in between locale and base-URL rewriting so that any root-relative `href` a head contributor emits gets sub-path prefixed exactly as literal head markup would — the head subsystem explains this dependency from its own side. `BaseUrlHtmlRewriter` runs last of the four as the outermost transport layer: everyone upstream works with clean `/`-rooted paths and never has to strip a base URL before operating.

`FallbackLangHtmlRewriter` and `WordBreakHtmlRewriter` slot in afterward without joining that chain — they read the finished URLs but no later rewriter depends on their output. Reversing any two of the first four breaks one of the others' invariants. Keeping the ordering explicit in the `Order` property — rather than implicit in DI registration sequence — is what makes the dependency between rewriters visible at the call site.

## Further reading

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor) — the member-by-member catalog of `IResponseProcessor`, `IHtmlResponseRewriter`, and the three built-in rewriters.
- How-to: [Write a response processor](xref:how-to.response-pipeline.response-processor) — for touching the raw body.
- How-to: [Write an HTML rewriter](xref:how-to.response-pipeline.html-rewriter) — for working inside the shared DOM pass.
- Related explanation: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — why the same processor chain runs against both live requests and the static-build crawler.
