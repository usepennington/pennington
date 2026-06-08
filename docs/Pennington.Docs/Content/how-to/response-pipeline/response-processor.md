---
title: "Inject HTML before </body> on every page"
description: "Implement IResponseProcessor to splice a feedback widget, banner, or analytics tag before </body> on every rendered HTML page."
uid: how-to.response-pipeline.response-processor
order: 2
sectionLabel: "Response Pipeline"
tags: [response-pipeline, extensibility, middleware, html-injection]
---

To inject a feedback widget, banner, or analytics tag before `</body>` on every rendered page, implement `IResponseProcessor`. The processor receives the full response body as a string and returns the replacement — useful when the goal is to insert a pre-serialized HTML fragment, log an outgoing payload, or append a non-HTML footer. When the work is DOM-shaped (anchor rewrites, attribute additions, element injection at a CSS selector), implement `IHtmlResponseRewriter` instead so every rewriter shares one AngleSharp parse. See <xref:how-to.response-pipeline.html-rewriter>.

The recipe references `examples/ExtensibilityLabExample/FeedbackWidgetProcessor.cs`, which injects a "Was this helpful?" aside before `</body>` against a bare `AddPennington` host.

## Before you begin

- An existing Pennington site (see <xref:tutorials.getting-started.first-site> if not).
- The response pipeline buffers the full response body before the processor runs. This is fine for HTML pages but unsuitable for large binary streams — gate those out in `ShouldProcess`.

## Write the processor

Implement <xref:reference.api.i-response-processor> as a sealed class. Two rules carry the page:

- `ShouldProcess` runs before the body is buffered. Returning `false` skips body capture entirely, so this is where filtering by status code, content type, or request path belongs. The example accepts only 2xx HTML responses, letting static assets, JSON endpoints, and redirects pass through untouched.
- `ProcessAsync` receives the full captured body as a string and returns the replacement. The example locates the last `</body>` with `LastIndexOf` and inserts the widget HTML there, falling back to append-at-end when the tag is absent so content still reaches the browser.

```csharp:symbol
examples/ExtensibilityLabExample/FeedbackWidgetProcessor.cs
```

## Pick an Order value

Slot into the `Order` sequence so the processor sees the HTML state it expects. Anything below `10` would see un-resolved `<xref:...>` placeholders that `HtmlResponseRewritingProcessor` expands. The example uses `500` so the widget is inserted after every built-in pass has run. For the full table of shipped `Order` values, see <xref:reference.api.i-response-processor>.

## Register the processor

Every registered `IResponseProcessor` is picked up and ordered by its `Order` value, so a single `AddSingleton` is the entire wiring step.

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

- Run `dotnet run --project examples/ExtensibilityLabExample` and visit `/`. The rendered HTML contains `<aside class="feedback-widget" data-extensibility-lab="feedback-widget">` immediately before `</body>`; fetch `/styles.css` and the aside is absent.
- Static build: `dotnet run --project examples/ExtensibilityLabExample -- build output` — grep `output/index.html` for `data-extensibility-lab="feedback-widget"` to confirm the processor runs during publish as well as dev.

## Related

- Reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- Background: [The response-processing pipeline](xref:explanation.core.response-processing)
- Related how-to: [Write an HTML rewriter](xref:how-to.response-pipeline.html-rewriter)
