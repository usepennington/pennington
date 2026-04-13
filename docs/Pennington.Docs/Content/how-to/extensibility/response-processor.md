---
title: "Write a response processor"
description: "Implement IResponseProcessor, decide an Order, use ShouldProcess to gate work, and mutate the HTTP response body."
section: "extensibility"
order: 40
tags: []
uid: how-to.extensibility.response-processor
isDraft: true
search: false
llms: false
---

> **In this page.** Implementing `IResponseProcessor`, deciding an `Order`, using `ShouldProcess` to gate work, and mutating the HTTP response body.
>
> **Not in this page.** HTML-specific rewriting — prefer `IHtmlResponseRewriter` (next).

## When to use this

- You need to observe or mutate the full response string (HTML, JSON, or other) for every matching request.
- Reach for `IHtmlResponseRewriter` instead whenever your work is DOM-level HTML mutation — it shares a single AngleSharp parse with the built-in rewriters.
- Typical fits: injecting a raw script/widget before `</body>`, scanning non-HTML payloads (e.g. JSON scraping), writing custom diagnostic headers.

## Assumptions

- You have an existing Pennington site wired with `AddPennington` + `UsePennington`.
- You understand that `ResponseProcessingMiddleware` buffers the whole response body into memory before processors run.
- You know the three built-in `IResponseProcessor` orders (`10` HTML rewriting, `20` live-reload, `30` diagnostic overlay) so you can slot yours in.

To copy a working setup, see [`examples/ForgePortalExample`](https://github.com/Phillip-Haydon/Penn/tree/main/examples/ForgePortalExample) — it registers `FeedbackWidgetProcessor` that inserts a floating button before `</body>`.

---

## Steps

### 1. Create the processor class

Implement `IResponseProcessor` with `Order`, `ShouldProcess`, and `ProcessAsync`. Keep work off the hot path by returning early from `ShouldProcess`.

```csharp:xmldocid
T:ForgePortalExample.FeedbackWidgetProcessor
```

### 2. Pick an `Order`

- `10` HTML rewriting (`HtmlResponseRewritingProcessor`) runs first so xref/locale/base-URL rewrites happen before your observer sees the body.
- `20` `LiveReloadScriptProcessor` (dev only) injects the WebSocket script before `</body>`.
- `30` `DiagnosticOverlayProcessor` (dev only) injects the diagnostics badge.
- Pick a number outside those slots. Use something well above `30` (e.g. `100`, `500`) so your processor runs after rewriting and after live-reload/overlay injection — this means you see the final HTML exactly as the browser will.

### 3. Gate with `ShouldProcess`

Check `Response.ContentType` and `StatusCode` before doing any work. `ResponseProcessingMiddleware` buffers the response into a `MemoryStream`, so a cheap predicate here avoids an unnecessary copy when no processor applies.

```csharp:xmldocid
M:ForgePortalExample.FeedbackWidgetProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)
```

### 4. Mutate the body in `ProcessAsync`

Return the new string. The middleware re-encodes as UTF-8 and clears `ContentLength` so your edit length does not need to match the original.

```csharp:xmldocid
M:ForgePortalExample.FeedbackWidgetProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)
```

### 5. Register the processor with DI

Register as `IResponseProcessor`. `ResponseProcessingMiddleware` resolves the full `IEnumerable<IResponseProcessor>`, filters by `ShouldProcess`, and sorts by `Order` per request.

```csharp
builder.Services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>();
```

---

## Verify

- Run `dotnet run` and request any HTML page.
- Expect your processor's effect to be visible in the rendered response (e.g. the injected widget appears above `</body>`).
- Confirm non-HTML endpoints (`/search-index-*.json`, `/sitemap.xml`) are untouched — `ShouldProcess` should return `false` for them.

## Related

- Reference: [Response processing interfaces](/reference/extension-points/response-processing)
- Background: [The response-processing pipeline](/explanation/core/response-processing)
- Next recipe: [Write an HTML rewriter](/how-to/extensibility/html-rewriter) — when you need DOM mutation, not raw string editing.
