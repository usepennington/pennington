---
title: "Write a response processor"
description: "Implement IResponseProcessor to mutate the full HTTP response body, gate work with ShouldProcess, and slot into the built-in Order chain."
uid: how-to.extensibility.response-processor
order: 40
sectionLabel: Extensibility
tags: [response-pipeline, extensibility, middleware, html-injection]
---

> **In this page.** _Paraphrase the TOC "Covers" line: implementing `IResponseProcessor`, picking an `Order` that slots cleanly into the built-in 10/20/30 chain, gating expensive work with `ShouldProcess`, and mutating the response body inside `ProcessAsync`._
>
> **Not in this page.** _Paraphrase the TOC "Does not cover": HTML-structured edits. When the goal is DOM work — anchor rewrites, attribute additions, element injection at a selector — implement `IHtmlResponseRewriter` instead so every rewriter shares one AngleSharp pass. See [Write an HTML rewriter](/how-to/extensibility/html-rewriter)._

## When to use this

_Two sentences. Frame the reader's goal: they need to transform the whole response body as a string — inject a pre-serialized HTML fragment, log an outgoing payload, or append a non-HTML footer — where parsing the DOM would be overkill or actively wrong (non-HTML responses). Point them sideways to `IHtmlResponseRewriter` for selector-driven HTML edits so nobody lands here to write a regex over `<a href>`._

## Assumptions

_Three bullets. Each is realistic prior state, not a tutorial step._

- You have an existing Pennington site (see the [Getting Started tutorial](/tutorials/getting-started/first-site) if not).
- You understand that `ResponseProcessingMiddleware` buffers the full response body before your processor runs — this is fine for HTML pages but unsuitable for large binary streams, so gate those out in `ShouldProcess`.
- You know which of the built-in processors your work needs to run before or after: `HtmlResponseRewritingProcessor` at `Order` 10, `LiveReloadScriptProcessor` at 20, `DiagnosticOverlayProcessor` at 30.

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `FeedbackWidgetProcessor.cs` injects a "Was this helpful?" aside before `</body>` and is registered against a bare `AddPennington` host in `Program.cs`.

---

## Steps

### 1. Implement `IResponseProcessor`

_One sentence. The contract is three members: `Order`, `ShouldProcess(HttpContext)`, and `ProcessAsync(string responseBody, HttpContext)` returning the replacement body. Start from the shipped example — it injects a feedback widget before `</body>` and falls back to appending when the closing tag is missing._

```csharp:xmldocid
T:Pennington.Infrastructure.IResponseProcessor
```

```csharp:xmldocid
T:ExtensibilityLabExample.FeedbackWidgetProcessor
```

### 2. Pick an `Order`

_Two sentences. The built-ins occupy 10 (`HtmlResponseRewritingProcessor`), 20 (`LiveReloadScriptProcessor`, dev-only), and 30 (`DiagnosticOverlayProcessor`, dev-only). Slot yours using the same tidy 10/20/30/40/50 sequence — `40` runs after all three built-ins so your output is not subject to further rewriting, while a value below `10` would see the un-resolved `<xref:...>` placeholders that `HtmlResponseRewritingProcessor` expands._

```csharp:xmldocid
P:ExtensibilityLabExample.FeedbackWidgetProcessor.Order
```

### 3. Gate work with `ShouldProcess`

_Two sentences. `ShouldProcess` runs before the body is buffered; returning `false` skips body capture entirely, so this is where you filter by status code, content type, or request path. The example accepts only 2xx HTML responses — static assets, JSON endpoints, and redirects pass through untouched._

```csharp:xmldocid
M:ExtensibilityLabExample.FeedbackWidgetProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)
```

### 4. Mutate the body in `ProcessAsync`

_Two sentences. `ProcessAsync` receives the full captured body as a string and must return the replacement string — an empty return will empty the response. The example finds the last `</body>` with `LastIndexOf` and splices the widget HTML in, falling back to append-at-end when the tag is absent so the content still reaches the browser._

```csharp:xmldocid
M:ExtensibilityLabExample.FeedbackWidgetProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)
```

### 5. Register the processor with DI

_One sentence. Add a singleton registration for `IResponseProcessor` — `ResponseProcessingMiddleware` resolves every registered implementation and sorts by `Order` on each request._

```csharp
builder.Services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>();
```

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/`.
- Expect the rendered HTML to contain `<aside class="feedback-widget" data-extensibility-lab="feedback-widget">` immediately before `</body>`; fetch `/styles.css` and confirm the aside is absent (non-HTML content type gated out by `ShouldProcess`).
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep `output/index.html` for `data-extensibility-lab="feedback-widget"` to confirm the processor runs during publish as well as dev.

## Related

- Reference: [Response processing interfaces](/reference/extension-points/response-processing)
- Background: [The response-processing pipeline](/explanation/core/response-processing)
- Related how-to: [Write an HTML rewriter](/how-to/extensibility/html-rewriter)
