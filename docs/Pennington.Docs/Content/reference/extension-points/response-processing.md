---
title: "Response processing interfaces"
description: "The IResponseProcessor and IHtmlResponseRewriter contracts, their execution order, and the three built-in HTML rewriters."
section: "extension-points"
order: 30
tags: []
uid: reference.extension-points.response-processing
isDraft: true
search: false
llms: false
---

> **In this page.** The `IResponseProcessor` and `IHtmlResponseRewriter` contracts, their execution order, and the three built-in HTML rewriters (`XrefHtmlRewriter`, `LocaleLinkHtmlRewriter`, `BaseUrlHtmlRewriter`).
>
> **Not in this page.** ASP.NET middleware ordering at large — see the ASP.NET Core documentation.

## Summary

The two-tier response rewriting contract: `IResponseProcessor` transforms a full response body string; `IHtmlResponseRewriter` participates in a shared AngleSharp DOM pass owned by `HtmlResponseRewritingProcessor`.
Namespace `Pennington.Infrastructure`, driven by `ResponseProcessingMiddleware` which is wired by `UsePennington`.

## Declaration

### `IResponseProcessor`

```csharp:xmldocid
T:Pennington.Infrastructure.IResponseProcessor
```

### `IHtmlResponseRewriter`

```csharp:xmldocid
T:Pennington.Infrastructure.IHtmlResponseRewriter
```

## Members — `IResponseProcessor`

| Name | Type | Description |
|---|---|---|
| `Order` | `int` | Ascending sort key applied by `ResponseProcessingMiddleware` before processors run. |
| `ShouldProcess` | `bool ShouldProcess(HttpContext)` | Cheap gate; if it returns false the processor is skipped for this response. |
| `ProcessAsync` | `Task<string> ProcessAsync(string responseBody, HttpContext)` | Receives the captured response body string and returns the rewritten body. |

## Members — `IHtmlResponseRewriter`

| Name | Type | Description |
|---|---|---|
| `Order` | `int` | Ascending sort key applied by `HtmlResponseRewritingProcessor` within the shared DOM pass. |
| `ShouldApply` | `bool ShouldApply(HttpContext)` | Gate checked before parsing; if every rewriter returns false, the DOM is not parsed. |
| `PreParseAsync` | `Task<string> PreParseAsync(string html, HttpContext)` | Raw-string pre-pass before AngleSharp parses the body. Default member — pass-through. |
| `ApplyAsync` | `Task ApplyAsync(IDocument document, HttpContext)` | Mutates the shared parsed document; the orchestrator serializes once after every rewriter has run. |

## Execution order

`ResponseProcessingMiddleware` captures the response body, then runs every `IResponseProcessor` whose `ShouldProcess` returns true in ascending `Order`. Diagnostic headers are written after processors run.

`HtmlResponseRewritingProcessor` (`IResponseProcessor` at `Order` 10) owns the HTML rewriting sub-pipeline. It runs each applicable `IHtmlResponseRewriter` in ascending `Order`, in two phases: `PreParseAsync` on the raw string, then `ApplyAsync` on a single parsed `IDocument`.

## Built-in HTML rewriters

Registered by `AddPennington`. Ordering is load-bearing: xref resolution produces canonical paths, locale prefixing transforms them for the active locale, and base-URL prefixing is the outermost transport layer.

| Order | Name | Purpose |
|---|---|---|
| 10 | `XrefHtmlRewriter` | Resolves `xref:uid` cross-references. `PreParseAsync` substitutes raw `<xref:uid>` tags (invalid HTML); `ApplyAsync` rewrites `href="xref:uid"` attributes. Delegates to `XrefResolvingService`. |
| 20 | `LocaleLinkHtmlRewriter` | Prefixes internal anchor links with the active locale (e.g., `/about` -> `/fr/about`) when the request is a non-default locale. Skips external links, already-prefixed paths, framework paths, and static-asset extensions. |
| 30 | `BaseUrlHtmlRewriter` | Prefixes root-relative `href`, `src`, and `action` attributes with the configured `OutputOptions.BaseUrl`, and stamps `data-base-url` on `<body>`. Skipped when the base URL is empty or `/`. |

## Example

```csharp:xmldocid
T:Pennington.Infrastructure.XrefHtmlRewriter
```

A built-in rewriter implements `IHtmlResponseRewriter` with an explicit `Order`, a `ShouldApply` gate, and the two phases; registration is done by the host in `AddPennington`.

## See also

- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Related reference: [`LocalizationOptions`](/reference/options/localization-options)
- Background: [The response-processing pipeline](/explanation/core/response-processing)
