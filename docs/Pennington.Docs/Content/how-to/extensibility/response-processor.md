---
title: "Write a response processor"
description: "Implement IResponseProcessor to mutate the full HTTP response body, gate work with ShouldProcess, and slot into the built-in Order chain."
uid: how-to.extensibility.response-processor
order: 203040
sectionLabel: Extensibility
tags: [response-pipeline, extensibility, middleware, html-injection]
---

Implement `IResponseProcessor` to transform the full response body as a string — injecting a pre-serialized HTML fragment, logging an outgoing payload, or appending a non-HTML footer. When the goal is DOM work such as anchor rewrites, attribute additions, or element injection at a CSS selector, implement `IHtmlResponseRewriter` instead so every rewriter shares one AngleSharp parse. See [Write an HTML rewriter](xref:how-to.extensibility.html-rewriter).

## Before you begin

- An existing Pennington site. See the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not.
- `ResponseProcessingMiddleware` buffers the full response body before the processor runs. This is fine for HTML pages but unsuitable for large binary streams — gate those out in `ShouldProcess`.
- The built-in processors and their `Order` values: `HtmlResponseRewritingProcessor` at 10, `LiveReloadScriptProcessor` at 20 (dev only), `DiagnosticOverlayProcessor` at 30 (dev only).

The `ExtensibilityLabExample` project provides a working reference — `FeedbackWidgetProcessor.cs` injects a "Was this helpful?" aside before `</body>` and is registered in `Program.cs` against a bare `AddPennington` host.

---

## Steps

<Steps>
<Step StepNumber="1">

**Implement `IResponseProcessor`**

The contract is three members — `Order`, `ShouldProcess(HttpContext)`, and `ProcessAsync(string responseBody, HttpContext)` — and the shipped example at `examples/ExtensibilityLabExample/FeedbackWidgetProcessor.cs` injects a feedback widget before `</body>`, falling back to append when the closing tag is missing. The next three steps fence each member in turn.

</Step>
<Step StepNumber="2">

**Pick an `Order`**

The built-ins occupy 10 (`HtmlResponseRewritingProcessor`), 20 (`LiveReloadScriptProcessor`, dev-only), and 30 (`DiagnosticOverlayProcessor`, dev-only). Slot into the same 10/20/30/40/50 sequence — `40` runs after all three built-ins so the output is not rewritten further, while anything below `10` would see the un-resolved `<xref:...>` placeholders that `HtmlResponseRewritingProcessor` expands.

```csharp:xmldocid
P:ExtensibilityLabExample.FeedbackWidgetProcessor.Order
```

</Step>
<Step StepNumber="3">

**Gate work with `ShouldProcess`**

`ShouldProcess` runs before the body is buffered — returning `false` skips body capture entirely, so this is where filtering by status code, content type, or request path belongs. The example accepts only 2xx HTML responses, letting static assets, JSON endpoints, and redirects pass through untouched.

```csharp:xmldocid
M:ExtensibilityLabExample.FeedbackWidgetProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)
```

</Step>
<Step StepNumber="4">

**Mutate the body in `ProcessAsync`**

`ProcessAsync` receives the full captured body as a string and returns the replacement string — an empty return empties the response. The example locates the last `</body>` with `LastIndexOf` and splices the widget HTML in, falling back to append-at-end when the tag is absent so content still reaches the browser.

```csharp:xmldocid
M:ExtensibilityLabExample.FeedbackWidgetProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)
```

</Step>
<Step StepNumber="5">

**Register the processor with DI**

Add a singleton registration against `IResponseProcessor` — `ResponseProcessingMiddleware` resolves every registered implementation and sorts by `Order` on each request.

```csharp
builder.Services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>();
```

</Step>
</Steps>

---

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/`.
- Expect the rendered HTML to contain `<aside class="feedback-widget" data-extensibility-lab="feedback-widget">` immediately before `</body>`; fetch `/styles.css` and confirm the aside is absent (non-HTML content type gated out by `ShouldProcess`).
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep `output/index.html` for `data-extensibility-lab="feedback-widget"` to confirm the processor runs during publish as well as dev.

## Related

- Reference: [Response processing interfaces](xref:reference.extension-points.response-processing)
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)
- Related how-to: [Write an HTML rewriter](xref:how-to.extensibility.html-rewriter)
