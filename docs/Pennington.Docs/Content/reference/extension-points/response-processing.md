---
title: "Response processing interfaces"
description: "The two response-rewriting contracts (IResponseProcessor, IHtmlResponseRewriter) with execution order and the three built-in rewriters that ship in Pennington."
sectionLabel: "Extension Points"
order: 405030
tags: [response-processing, html-rewriting, extension-points, middleware]
uid: reference.extension-points.response-processing
---

> **In this page.** `IResponseProcessor`, `IHtmlResponseRewriter`, execution order, and the three built-in rewriters (`XrefHtmlRewriter`, `LocaleLinkHtmlRewriter`, `BaseUrlHtmlRewriter`).
>
> **Not in this page.** Middleware ordering in ASP.NET at large.

## Summary

_**One sentence: what it is.** The two interfaces that rewrite outgoing HTTP response bodies — `IResponseProcessor` captures the full body as a string, `IHtmlResponseRewriter` participates in a shared single-parse AngleSharp pipeline — together with the three built-in rewriters that resolve xrefs, add locale prefixes, and apply the deployment base URL._
_**One sentence: where it lives.** Namespace `Pennington.Infrastructure` (`src/Pennington/Infrastructure/`) for the two contracts, `HtmlResponseRewritingProcessor`, `XrefHtmlRewriter`, and `BaseUrlHtmlRewriter`; `Pennington.Localization` (`src/Pennington/Localization/`) for `LocaleLinkHtmlRewriter`._

## `IResponseProcessor`

```csharp:xmldocid
T:Pennington.Infrastructure.IResponseProcessor
```

_The outer tier: a full-response string rewriter invoked by `ResponseProcessingMiddleware` for every response that survives `ShouldProcess`. Implementations are sorted by `Order` ascending and each receives the complete response body produced by earlier processors. `HtmlResponseRewritingProcessor` is one such processor (Order 10); `LiveReloadScriptProcessor` (Order 20, dev only), `DiagnosticOverlayProcessor` (Order 30, dev only), and `CssClassCollectorProcessor` (from `Pennington.MonorailCss`) are the other built-ins._

### Members

_Alphabetical._

| Name | Signature | Description |
|---|---|---|
| `Order` | `int { get; }` | Sort key within the processor pipeline; lower values run first and see the response body before higher-order processors. |
| `ProcessAsync` | `Task<string> ProcessAsync(string responseBody, HttpContext context)` | Returns the rewritten response body; the returned string replaces `responseBody` for the next processor in the chain and is ultimately flushed to the wire. |
| `ShouldProcess` | `bool ShouldProcess(HttpContext context)` | Cheap pre-capture gate; when every registered processor returns `false`, `ResponseProcessingMiddleware` does not buffer the response at all. |

## `IHtmlResponseRewriter`

```csharp:xmldocid
T:Pennington.Infrastructure.IHtmlResponseRewriter
```

_The inner tier: a participant in the unified HTML rewriting pipeline owned by `HtmlResponseRewritingProcessor`. Every registered rewriter shares one `IDocument` per response — the DOM is parsed once, each rewriter mutates it in `Order`, then the document is serialized once. A two-phase shape separates non-HTML constructs (handled by `PreParseAsync` before the parser runs) from DOM mutations (`ApplyAsync`)._

### Members

_Alphabetical._

| Name | Signature | Description |
|---|---|---|
| `ApplyAsync` | `Task ApplyAsync(IDocument document, HttpContext context)` | Mutates the shared parsed document; the orchestrator serializes it exactly once after every applicable rewriter has run. |
| `Order` | `int { get; }` | Sort key within the HTML rewriting pipeline; the built-ins occupy Order 10 (xref), 20 (locale), and 30 (base URL). |
| `PreParseAsync` | `Task<string> PreParseAsync(string html, HttpContext context)` (default pass-through) | Regex or string pass over the raw HTML before AngleSharp parses it; exists for constructs that are not valid HTML (such as raw `<xref:uid>` tags) and therefore cannot be expressed in the DOM. |
| `ShouldApply` | `bool ShouldApply(HttpContext context)` | Cheap gate checked before parsing; returning `false` skips both `PreParseAsync` and `ApplyAsync` for this response, and when every rewriter returns `false` the orchestrator skips parsing entirely. |

## Built-in rewriters

_One row per built-in `IHtmlResponseRewriter`, in execution order. Each is registered inside `AddPennington` and picked up by `HtmlResponseRewritingProcessor` via constructor injection._

| Rewriter | Order | Purpose |
|---|---|---|
| `XrefHtmlRewriter` | 10 | Resolves `<xref:uid>` tags in `PreParseAsync` and `href="xref:uid"` attributes in `ApplyAsync`, delegating both phases to `XrefResolvingService`. |
| `LocaleLinkHtmlRewriter` | 20 | Prefixes internal anchor `href`s with the active locale (e.g. `/about` → `/fr/about`) when the request is serving a non-default locale. |
| `BaseUrlHtmlRewriter` | 30 | Prefixes root-relative `href`, `src`, and `action` attributes with the configured base URL and stamps `data-base-url` on `<body>`. |

### `XrefHtmlRewriter`

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

_Runs first so the canonical paths it emits (e.g. `/about/`) are visible to the later locale and base-URL rewriters. `ShouldApply` returns `true` unconditionally; unresolved uids are recorded on the request's `DiagnosticContext` and surface in the dev overlay._

### `LocaleLinkHtmlRewriter`

```csharp:xmldocid
T:Pennington.Localization.LocaleLinkHtmlRewriter
```

_Runs second so it operates on the logical root-relative paths xref emits, before base-URL prefixing transforms them. `ShouldApply` returns `true` only when `LocalizationOptions.IsMultiLocale` and the current request's locale differs from `DefaultLocale`; anchors carrying `data-locale` (language switchers), external links, paths with existing locale prefixes, framework paths (`/_content/…`), and static-asset paths (paths ending in a file extension) are skipped._

### `BaseUrlHtmlRewriter`

```csharp:xmldocid
T:Pennington.Infrastructure.BaseUrlHtmlRewriter
```

_Runs last — the outermost transport layer — so xref resolution and locale prefixing both operate on paths without a deployment prefix. `ShouldApply` returns `true` only when `OutputOptions.BaseUrl` is set to a non-empty value other than `/`; it rewrites every `href`, `src`, and `action` attribute that starts with `/` (but not `//`) and stamps `data-base-url` on `<body>` so client-side code can apply the same prefix to dynamically-generated links._

## Execution order

_Lookup table of the built-in `Order` values. Lower runs first; ties are broken by DI registration order._

1. `XrefHtmlRewriter` — `Order = 10`.
2. `LocaleLinkHtmlRewriter` — `Order = 20`.
3. `BaseUrlHtmlRewriter` — `Order = 30`.

_The orchestrator `HtmlResponseRewritingProcessor` itself has `Order = 10` within the outer `IResponseProcessor` pipeline and sits alongside `LiveReloadScriptProcessor` (20, dev only) and `DiagnosticOverlayProcessor` (30, dev only)._

## Example

_One minimal example pulled from `examples/ExtensibilityLabExample/AnchorLowercaseRewriter.cs` — the canonical custom `IHtmlResponseRewriter` used by the extensibility how-tos. Shown at the type level so a reader recognizes the full interface surface (`Order`, `ShouldApply`, `PreParseAsync`, `ApplyAsync`) in one place. Implementation walkthrough lives in the how-to._

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.AnchorLowercaseRewriter
```

_Reference shape for a custom `IHtmlResponseRewriter` that participates in the shared AngleSharp pass. For a custom outer-tier `IResponseProcessor`, see `T:ExtensibilityLabExample.FeedbackWidgetProcessor`._

## See also

- How-to: [Write a response processor](xref:how-to.extensibility.response-processor)
- How-to: [Write an HTML rewriter](xref:how-to.extensibility.html-rewriter)
- Related reference: [Routing types](xref:reference.extension-points.routing)
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)
