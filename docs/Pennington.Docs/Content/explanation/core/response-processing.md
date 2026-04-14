---
title: "The response-processing pipeline"
description: "Why Pennington splits response rewriting into generic body processors and HTML-DOM rewriters that share one AngleSharp pass."
uid: explanation.core.response-processing
order: 301040
sectionLabel: "Core Architecture"
tags: [response-processing, rewriters, anglesharp, middleware]
---

> **In this page.** Why HTML rewriting is consolidated into a single AngleSharp pass, how `IResponseProcessor` differs from `IHtmlResponseRewriter`, and the order the built-in rewriters run.
>
> **Not in this page.** The recipe for authoring your own rewriter — see the how-to on writing an HTML rewriter.

## The question

_One sentence, in the reader's voice. Draft: "Why are there two extension points for rewriting the response body — a general `IResponseProcessor` and an HTML-specific `IHtmlResponseRewriter` — instead of a single chain of string-to-string processors?" The rest of the page is the answer._

## Context

_Three to five sentences. Before the consolidation (commit `acd91c9`, "refactor: unify HTML response rewriting into a single AngleSharp pass") every HTML-mutating concern — xref resolution, locale link prefixing, base-URL stamping — was its own `IResponseProcessor`, and each one parsed the response body into its own AngleSharp document, mutated it, and serialized it back out. That meant an N-parse/serialize cycle per response with N DOM copies floating around, and it left the cross-processor ordering question — "does locale prefixing see xref-resolved hrefs or raw `xref:` attributes?" — implicit in the DI registration order. A string-only chain would paper over the problem (each stage reparses anyway); a single-monolith-rewriter would drop the extensibility story. Pennington chose a two-tier shape: keep the generic string-in/string-out contract for body-level concerns that don't need a DOM, and give DOM-shaped concerns one shared parse they all participate in._

## How it works

### Tier A: `IResponseProcessor` (generic body capture)

_Two to four paragraphs. `ResponseProcessingMiddleware` wraps the response body stream, captures it into memory, runs every `IResponseProcessor` whose `ShouldProcess` returns `true` in ascending `Order`, then flushes the final string back to the socket. The processor contract is intentionally narrow — `int Order`, `bool ShouldProcess(HttpContext)`, `Task<string> ProcessAsync(string responseBody, HttpContext)` — so anything that wants to touch the body can participate without taking an AngleSharp dependency._

_Name the built-in processors and what each is shaped like: `HtmlResponseRewritingProcessor` at `Order => 10` (the host for Tier B), `LiveReloadScriptProcessor` at `Order => 20` injecting the reconnect script before `</body>` in dev, `DiagnosticOverlayProcessor` at `Order => 30` rendering the collected `DiagnosticContext` into a corner panel in dev, and `CssClassCollectorProcessor` (MonorailCSS) which scrapes utility classes out of the emitted HTML so the stylesheet endpoint can regenerate. Point out that three of those are body-level concerns — string inject, panel append, regex scan — that gain nothing from a parsed DOM, so forcing them through AngleSharp would just waste work._

```csharp:xmldocid
T:Pennington.Infrastructure.IResponseProcessor
```

### Tier B: `IHtmlResponseRewriter` (shared AngleSharp pass)

_Two to four paragraphs. Tier B is the inside of a single Tier A processor — `HtmlResponseRewritingProcessor`. It holds one `IBrowsingContext` (`BrowsingContext.New(Configuration.Default)`), calls each rewriter's `ShouldApply(HttpContext)` first, and if none apply it returns the body untouched so the parser never fires. If at least one applies, it runs all of them through a two-phase pipeline: `PreParseAsync(string, HttpContext)` over the raw HTML for constructs AngleSharp can't parse (notably `<xref:uid>` tag syntax, which is not valid HTML), then `ApplyAsync(IDocument, HttpContext)` over a single shared document that every rewriter mutates in turn. The document is serialized exactly once at the end, via `document.ToHtml()`._

_Surface the invariant in one sentence: "N rewriters, one parse, one serialize, one DOM." That is the design's load-bearing property — adding a fourth DOM-shaped concern (say, a heading-anchor lowercaser) costs a method call, not another parse/serialize round trip._

```csharp:xmldocid
T:Pennington.Infrastructure.IHtmlResponseRewriter
```

### Why two tiers, not one

_Two or three paragraphs. This is the core of the "why". Collapsing everything into `IHtmlResponseRewriter` would be wrong: live-reload script injection is a string insert that doesn't care about DOM structure, the diagnostic overlay is an append-before-`</body>` that survives without parsing, and MonorailCSS class collection is a regex scan that would gain nothing from AngleSharp and would happily run on partially-valid HTML the parser would otherwise try to fix. Conversely, collapsing everything into `IResponseProcessor` (the pre-`acd91c9` state) paid N-1 extra parses for no reason and left the cross-processor ordering implicit. The split matches the natural fault line — "does this concern care about HTML structure?" — and the two contracts stay narrow on their respective sides of that line._

_The body-capture middleware itself is deliberately generic: it doesn't know anything about HTML. `HtmlResponseRewritingProcessor.ShouldProcess` is what gates the HTML-specific branch, checking `Content-Type`, `StatusCode in [200, 300)`, and whether any rewriter's `ShouldApply` returned true. That gate keeps the AngleSharp dependency from running on `application/json`, `404`s, or opted-out pages, which is exactly the sort of thing you'd want a generic middleware to delegate rather than special-case._

### Why order is load-bearing

_Two paragraphs. The three built-in rewriters run in a specific outside-in order because each produces or consumes a link shape the next one transforms:_

_- **`XrefHtmlRewriter` at `Order => 10`.** Resolves `<xref:uid>` tags (in `PreParseAsync`) and `href="xref:uid"` attributes (in `ApplyAsync`) into canonical root-relative paths. Runs first so everything downstream sees real URLs, not symbolic ones._

_- **`LocaleLinkHtmlRewriter` at `Order => 20`.** Prefixes internal links with the active locale segment (`/` → `/fr/`). Must run after xref resolution — otherwise `xref:some.uid` wouldn't be a path yet and the prefixer would have nothing to operate on — and before base-URL rewriting, so the locale sits inside the base URL, not outside it._

_- **`BaseUrlHtmlRewriter` at `Order => 30`.** Prefixes root-relative URLs with the configured base URL and stamps `data-base-url` on `<body>`. Runs last so it is the outermost transport layer: xref and locale rewriters operate on logical `/`-rooted paths without having to strip a base URL first._

_Close the subsection by noting that reversing any two of these breaks one of the others' invariants — which is precisely the sort of bug the monolithic-per-rewriter-parse-and-serialize model used to hide behind DI ordering._

## Trade-offs

- **Cost:** _A rewriter can't assume it's running on unmodified HTML — earlier rewriters may have rewritten the links it cares about. Authors have to think about where in the 10/20/30 pipeline their rewriter belongs, which is strictly more cognitive load than "it runs after the previous one."_
- **Cost:** _The shared document is mutable and shared, so a rewriter that over-selects (for example, `document.QuerySelectorAll("a")` when it only cared about external links) can silently interfere with neighboring rewriters. Narrow selectors aren't optional; they're how the pipeline stays composable._
- **Alternative considered:** _A chain of pure string-to-string processors, each reparsing as needed. Rejected because it either duplicates parsing work (the original shape) or pushes every HTML-shaped concern into ad-hoc regex, which is fragile against real-world HTML and doesn't play well with AngleSharp's tolerance for malformed input._
- **Alternative considered:** _A single monolithic `HtmlRewriter` with hardcoded stages. Rejected because it closes the extensibility surface — every new concern (custom anchor rules, image lazy-loading, table classification) would have to land in core instead of a library consumer's assembly. The interface boundary at `IHtmlResponseRewriter` is what keeps the third-party story open._
- **Consequence:** _`ShouldApply` is the cheapest gate in the pipeline — check it first, keep it boolean, never parse inside it. If every rewriter returns `false`, the orchestrator skips parsing entirely, which is the fast path for non-HTML responses and for endpoints where rewriting is a no-op (static JSON, search-index files, the SPA envelope endpoint)._

## Further reading

- Reference: [Response processing interfaces](xref:reference.extension-points.response-processing) — the member-by-member catalog of `IResponseProcessor`, `IHtmlResponseRewriter`, and the three built-in rewriters.
- How-to: [Write a response processor](xref:how-to.extensibility.response-processor) — when you need to touch the raw body.
- How-to: [Write an HTML rewriter](xref:how-to.extensibility.html-rewriter) — when you need the shared DOM pass.
- Related explanation: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build) — why the same processor chain runs against both live requests and the static-build crawler.
