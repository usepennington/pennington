---
title: "Inject HTML before </body> on every page"
description: "Implement IResponseProcessor to splice a feedback widget, banner, or analytics tag before </body> on every rendered HTML page."
uid: how-to.response-pipeline.response-processor
order: 210020
sectionLabel: "Response Pipeline"
tags: [response-pipeline, extensibility, middleware, html-injection]
---

To inject a feedback widget, banner, or analytics tag before `</body>` on every rendered page, implement `IResponseProcessor`. The processor receives the full response body as a string and returns the replacement — useful when the goal is to splice a pre-serialized HTML fragment, log an outgoing payload, or append a non-HTML footer. When the work is DOM-shaped (anchor rewrites, attribute additions, element injection at a CSS selector), implement `IHtmlResponseRewriter` instead so every rewriter shares one AngleSharp parse. See <xref:how-to.response-pipeline.html-rewriter>.

## Before you begin

- An existing Pennington site. See the <xref:tutorials.getting-started.first-site> tutorial if not.
- `ResponseProcessingMiddleware` buffers the full response body before the processor runs. This is fine for HTML pages but unsuitable for large binary streams — gate those out in `ShouldProcess`.
- The built-in processors and their `Order` values: `HtmlResponseRewritingProcessor` at 10, `LiveReloadScriptProcessor` at 20 (dev only), `DiagnosticOverlayProcessor` at 30 (dev only).

The `ExtensibilityLabExample` project provides a working reference — `FeedbackWidgetProcessor.cs` injects a "Was this helpful?" aside before `</body>` and is registered in `Program.cs` against a bare `AddPennington` host.

## Implement the processor

`IResponseProcessor` has three members. The shipped example at `examples/ExtensibilityLabExample/FeedbackWidgetProcessor.cs` exercises all of them in one sealed type.

`ShouldProcess` runs before the body is buffered — returning `false` skips body capture entirely, so this is where filtering by status code, content type, or request path belongs. The example accepts only 2xx HTML responses, letting static assets, JSON endpoints, and redirects pass through untouched.

```csharp:xmldocid
M:ExtensibilityLabExample.FeedbackWidgetProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)
```

`ProcessAsync` receives the full captured body as a string and returns the replacement string — an empty return empties the response. The example locates the last `</body>` with `LastIndexOf` and splices the widget HTML in, falling back to append-at-end when the tag is absent so content still reaches the browser.

```csharp:xmldocid
M:ExtensibilityLabExample.FeedbackWidgetProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)
```

## Pick an Order value

The built-ins occupy 10 (`HtmlResponseRewritingProcessor`), 20 (`LiveReloadScriptProcessor`, dev-only), and 30 (`DiagnosticOverlayProcessor`, dev-only). Slot into the same 10/20/30/40/50 sequence — `40` runs after all three built-ins so the output is not rewritten further, while anything below `10` would see the un-resolved `<xref:...>` placeholders that `HtmlResponseRewritingProcessor` expands.

```csharp:xmldocid
P:ExtensibilityLabExample.FeedbackWidgetProcessor.Order
```

## Register the implementation

`ResponseProcessingMiddleware` resolves every registered `IResponseProcessor` from the container and sorts by `Order` on each request, so a single `AddSingleton` is the entire wiring step.

```csharp
builder.Services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>();
```

## Result

Every `text/html` response carries the widget aside immediately before its closing `</body>` tag:

```html
    <aside class="feedback-widget" data-extensibility-lab="feedback-widget">
      <p><strong>Was this helpful?</strong>
        <button type="button" data-feedback="yes">Yes</button>
        <button type="button" data-feedback="no">No</button>
      </p>
    </aside>
  </body>
</html>
```

Non-HTML endpoints (`/styles.css`, `/sitemap.xml`) are unmodified because `ShouldProcess` returns `false` for them.

## Verify

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/`.
- Expect the rendered HTML to contain `<aside class="feedback-widget" data-extensibility-lab="feedback-widget">` immediately before `</body>`; fetch `/styles.css` and confirm the aside is absent (non-HTML content type gated out by `ShouldProcess`).
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep `output/index.html` for `data-extensibility-lab="feedback-widget"` to confirm the processor runs during publish as well as dev.

## Related

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)
- Related how-to: [Write an HTML rewriter](xref:how-to.response-pipeline.html-rewriter)
