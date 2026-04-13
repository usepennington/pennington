---
title: "The response-processing pipeline"
description: "Why HTML rewriting is consolidated into a single AngleSharp pass, how IResponseProcessor differs from IHtmlResponseRewriter, and the order the built-in rewriters run."
section: "core"
order: 40
tags: []
uid: explanation.core.response-processing
isDraft: true
search: false
llms: false
---

> **In this page.** Why HTML rewriting is consolidated into a single AngleSharp pass, how `IResponseProcessor` differs from `IHtmlResponseRewriter`, and the order the built-in rewriters run.
>
> **Not in this page.** Writing a rewriter — see the How-Tos.

## The question

- One question: "Why does Pennington funnel every response through one middleware, but split HTML rewriting into a second, narrower pipeline underneath it?"

## Context

- Pennington renders once and serves the same HTML in dev and build (crawler re-issues HTTP against the running host).
- That means rewriting concerns — cross-reference resolution, locale prefixing, base-URL prefixing, MonorailCSS class harvesting, live-reload injection, diagnostic overlay — all ride on the response body, not the render tree.
- Earliest shape: each concern was its own `IResponseProcessor`, each parsed the body with its own AngleSharp `IBrowsingContext`, each serialized it back to a string.
- Commit `4617f64` consolidated HTML rewriting into one AngleSharp pass; the generic processor tier stayed, a narrower rewriter tier grew under it.

## How it works

### Two tiers, not one

- **Tier A — `IResponseProcessor`:** captures the full body as a string, runs a processor's `ProcessAsync(string, HttpContext)`, moves on. Body is opaque; processors can be non-HTML (class collection, script injection, overlay).
- **Tier B — `IHtmlResponseRewriter`:** shares a single parsed `IDocument` with its siblings. Rewriters never see the raw string except for an optional `PreParseAsync` regex pre-pass.
- One `IResponseProcessor` — `HtmlResponseRewritingProcessor` at Order 10 — owns Tier B. It parses once, runs every applicable `IHtmlResponseRewriter` in order, serializes once.

```csharp:xmldocid
T:Pennington.Infrastructure.IResponseProcessor
```

```csharp:xmldocid
T:Pennington.Infrastructure.IHtmlResponseRewriter
```

### Why consolidate into one AngleSharp pass

- Parsing HTML is the expensive step; serializing it back to a string is almost as costly. N independent rewriters meant N parse cycles and N serialize cycles for every page response.
- Each parser also owned a private `IBrowsingContext`, so DOM state was copied between passes — a mutation by rewriter A was observed by rewriter B only after round-tripping through the serializer.
- Consolidation means one parse, one shared document, one serialize. The only escape hatch is `PreParseAsync`, which exists specifically because raw `<xref:uid>` tags are not valid HTML and must be substituted before AngleSharp ever sees them.

### Why `IResponseProcessor` stayed generic

- Not every response concern is HTML-shaped. `CssClassCollectorProcessor` harvests class names from the served HTML but does not rewrite it. `LiveReloadScriptProcessor` and `DiagnosticOverlayProcessor` are dev-only string injections that run on top of the already-rewritten document. MonorailCSS lives in a separate project and shouldn't depend on `IHtmlResponseRewriter`.
- Keeping `IResponseProcessor` as the outer contract means any middleware-shaped concern plugs in without having to justify sharing the DOM.

### The built-in rewriters, and why they run 10 -> 20 -> 30

| Order | Rewriter | Concern |
| --- | --- | --- |
| 10 | `XrefHtmlRewriter` | Resolves `xref:uid` tags and attributes to canonical root-relative paths. |
| 20 | `LocaleLinkHtmlRewriter` | Prefixes internal anchors with the active locale (e.g. `/about/` -> `/fr/about/`). |
| 30 | `BaseUrlHtmlRewriter` | Prefixes all root-relative `href`, `src`, `action` with the deployment base URL. |

- The ordering is load-bearing, and it reads as "inside -> out":
  - Xref runs first because it emits the canonical path (`/guides/foo/`). Locale and base-URL downstream need something path-shaped to operate on; a raw `xref:guide.foo` would be opaque to them.
  - Locale runs second because it decides whether a logical root-relative path should become locale-qualified. It must see the already-resolved path but must not see a path that has been prefixed with a deployment base. `LocaleLinkHtmlRewriter` checks for an existing locale prefix before rewriting — `/preview/about` would confuse that check.
  - Base URL runs last because it is the transport layer. Every rewriter above it reasons in logical root-relative paths; `BaseUrlHtmlRewriter` stamps them for the actual deployment. It also writes `data-base-url` on `<body>` so client-side code can reproduce the prefix without re-deriving it.
- The invariant: each rewriter's input contract is "root-relative logical paths." Reordering breaks it.

### The raw-string pre-pass

- `IHtmlResponseRewriter.PreParseAsync` runs before AngleSharp parses. Only xref uses it today, because `<xref:some.uid>` is not well-formed HTML — AngleSharp would either drop it or mangle it.
- The default member is pass-through, so rewriters that only touch the DOM ignore it.
- This is the one shared-string hand-off in the rewriter tier, and it exists strictly to paper over input that the shared parser cannot represent.

## Trade-offs

- **Cost — shared mutable DOM.** Rewriters see each other's mutations. A rewriter that insists on pristine input has no hiding place; it has to tolerate upstream changes or run first.
- **Cost — two contracts to learn.** Newcomers must pick Tier A or Tier B, and picking wrong surfaces as a subtle bug (e.g., an HTML concern written as `IResponseProcessor` incurs its own parse cycle instead of joining the shared one).
- **Alternative considered — single interface with a DOM-aware base class.** Rejected because non-HTML concerns (MonorailCSS class collection, live-reload script string injection) would pay for DOM parsing they don't need.
- **Alternative considered — N independent processors (the pre-`4617f64` shape).** Rejected because the cost grew linearly with rewriter count, and the independent parses made ordering semantics muddy: every rewriter saw a freshly-parsed document, not the output of the previous one.
- **Consequence — ordering is semantic, not cosmetic.** The 10/20/30 slots encode a layering invariant. New rewriters should fit inside that layering (stay below 30, above 10) rather than picking a free integer.

## Further reading

- Reference: [Response-processing extension points](/reference/extension-points/response-processing)
- How-to: [Write an HTML rewriter](/how-to/extensibility/html-rewriter)
- How-to: [Write a response processor](/how-to/extensibility/response-processor)
- Explanation: [Unified dev-and-build code path](/explanation/core/dev-and-build)
