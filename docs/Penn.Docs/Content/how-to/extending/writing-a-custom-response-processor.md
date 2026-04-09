---
title: "Writing a Custom Response Processor"
description: "Implement IResponseProcessor (Order, ShouldProcess, ProcessAsync) to transform HTML responses — covering the middleware capture pattern, ordering semantics, and common use cases like analytics injection or custom link rewriting"
uid: "penn.how-to.writing-a-custom-response-processor"
order: 40
---

## Beat 1: The Problem — Page-Wide Changes Without Template Surgery

Introduce the scenario: you want to inject a feedback widget (a `<script>` tag and a floating button `<div>`) into every page of Forge's documentation. Editing every Razor template is fragile and error-prone. Pennington's response processor pipeline lets you transform HTML after rendering but before it reaches the browser — a clean cross-cutting concern.

### What to show
- The desired outcome: a small floating feedback button in the bottom-right of every HTML page, injected via a `<script>` and `<div>` before `</body>`
- The constraint: the injection must happen on HTML pages only, must skip JSON endpoints like `/search-index.json`, and must work identically in both dev server and static builds

### Key points
- Response processors are Pennington's post-rendering transformation layer — they operate on the fully rendered HTML string
- Processors run during both `dotnet run` (dev) and `dotnet run build` (static generation) — what you see in dev is what you get in the build
- This pattern applies to any cross-cutting HTML transformation: analytics scripts, banner injection, custom link rewriting, accessibility annotations

## Beat 2: The IResponseProcessor Interface

Walk through the three members of `T:Pennington.Infrastructure.IResponseProcessor` at `:path:src/Pennington/Infrastructure/IResponseProcessor.cs`.

### What to show
- `P:Pennington.Infrastructure.IResponseProcessor.Order` — `int` that controls execution sequence. Lower values run first. The middleware sorts all processors by `Order` ascending before applying them
- `M:Pennington.Infrastructure.IResponseProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)` — predicate that receives the `HttpContext` after the response has been generated. Return `true` to process this response, `false` to skip. Use this to filter by content type, status code, or request path
- `M:Pennington.Infrastructure.IResponseProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)` — receives the full response body as a string and the `HttpContext`. Returns the (potentially modified) response body string. The output of one processor becomes the input of the next

### Key points
- The interface is intentionally minimal — three members for a complete response transformation capability
- `ShouldProcess` runs before the response body is read, so it can inspect headers and status without incurring the cost of body string conversion for non-applicable responses
- Processors are chained: the output of `ProcessAsync` from one processor feeds into the next, in `Order` sequence

## Beat 3: The Built-In Processor Ordering

Map out the existing processors and their `Order` values to understand where a custom processor should slot in.

### What to show
- The built-in processor chain registered in `M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})` at `:path:src/Pennington/Infrastructure/PenningtonExtensions.cs`:
  - `T:Pennington.Infrastructure.XrefResolvingProcessor` — `Order: -10`. Resolves `xref:uid` links to actual URLs. Runs first so subsequent processors see resolved links
  - `T:Pennington.Infrastructure.BaseUrlRewritingProcessor` — `Order: 0`. Rewrites root-relative URLs to include the configured base URL (for subdirectory deployments)
  - `T:Pennington.Localization.LocaleLinkRewritingProcessor` — `Order: 50`. Rewrites internal links to include locale prefix for non-default locales
  - `T:Pennington.Infrastructure.LiveReloadScriptProcessor` — `Order: 1000`. Injects the live reload WebSocket script (dev mode only)
  - `T:Pennington.Infrastructure.DiagnosticOverlayProcessor` — `Order: 10000`. Injects the diagnostic overlay widget (dev mode only)

### Key points
- The ordering forms a logical pipeline: resolve references first (-10), then rewrite URLs (0, 50), then inject dev-mode scripts last (1000, 10000)
- A feedback widget at `Order: 500` slots after all URL processing but before dev-mode injections — the widget's HTML will have correct URLs and will not interfere with live reload
- The gap between built-in values (0 to 1000) provides ample room for custom processors

## Beat 4: How ResponseProcessingMiddleware Drives the Chain

Show the middleware internals to build confidence in the execution model.

### What to show
- `T:Pennington.Infrastructure.ResponseProcessingMiddleware` at `:path:src/Pennington/Infrastructure/ResponseProcessingMiddleware.cs`:
  - `M:Pennington.Infrastructure.ResponseProcessingMiddleware.InvokeAsync(Microsoft.AspNetCore.Http.HttpContext,System.Collections.Generic.IEnumerable{Pennington.Infrastructure.IResponseProcessor})` captures the response body into a `MemoryStream`
  - Filters processors: `processors.Where(p => p.ShouldProcess(context)).OrderBy(p => p.Order)` — only applicable processors run, in order
  - If any processors apply: reads body to string, runs each processor's `ProcessAsync` in sequence, writes the modified body back
  - If no processors apply: copies the original stream directly — zero overhead for non-HTML responses
  - Sets `ContentLength = null` after modification (body length may change)

### Key points
- The middleware captures the response body once and passes the string through the chain — no repeated I/O
- Processors that do not apply (per `ShouldProcess`) are skipped entirely — they add no cost to non-matching requests
- The middleware is registered by `M:Pennington.Infrastructure.PenningtonExtensions.UsePennington(Microsoft.AspNetCore.Builder.WebApplication)` via `app.UseMiddleware<ResponseProcessingMiddleware>()`

## Beat 5: Create the FeedbackWidgetProcessor

Implement `T:Pennington.Infrastructure.IResponseProcessor` for the feedback widget injection.

### What to show
- Class declaration: `public sealed class FeedbackWidgetProcessor : IResponseProcessor`
- `P:Pennington.Infrastructure.IResponseProcessor.Order` returns `500` — after xref resolution (-10), base URL rewriting (0), and locale link rewriting (50), but before live reload (1000)
- `M:Pennington.Infrastructure.IResponseProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)` implementation:
  - Check `context.Response.ContentType` starts with `"text/html"`
  - Check `context.Response.StatusCode` is `>= 200` and `< 300`
  - Exclude specific paths: `context.Request.Path` is not `/search-index.json`
  - Return `true` only when all conditions pass
- `M:Pennington.Infrastructure.IResponseProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)` implementation:
  - Find `</body>` using `LastIndexOf` (same pattern as `T:Pennington.Infrastructure.LiveReloadScriptProcessor` at `:path:src/Pennington/Infrastructure/LiveReloadScriptProcessor.cs`)
  - Insert a `<script>` tag and a `<div>` with a floating feedback button before `</body>`
  - Return the modified string
- ~30 lines total

### Key points
- Model the `ShouldProcess` guard on the built-in processors — check content type and status code
- Use `LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase)` for case-insensitive matching, consistent with `T:Pennington.Infrastructure.LiveReloadScriptProcessor`
- The processor sees already-resolved HTML (xrefs replaced, URLs rewritten) because it runs at `Order: 500`

## Beat 6: Register via DI

Add the processor to the DI container and explain auto-discovery.

### What to show
- In `Program.cs`: `services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>()`
- Explain that `T:Pennington.Infrastructure.ResponseProcessingMiddleware` receives `IEnumerable<IResponseProcessor>` via DI constructor injection — all registered `IResponseProcessor` implementations are automatically discovered and included in the chain
- No special registration API needed — standard DI is sufficient

### Key points
- Unlike highlighters (`options.Highlighting.AddHighlighter<T>()`) and islands (`options.Islands.Register<T>(name)`), response processors use plain DI registration
- The middleware resolves `IEnumerable<IResponseProcessor>` on each request, so processors registered at any point before `app.Build()` are included
- Singleton lifetime is typical since processors should be stateless (request-specific data comes from `HttpContext`)

## Beat 7: Run and Verify in Dev Mode

Navigate the site and confirm the feedback widget appears on HTML pages but not on JSON endpoints.

### What to show
- Run `dotnet run` and navigate to any documentation page — the floating feedback button appears in the bottom-right corner
- View page source to confirm the injected `<script>` and `<div>` appear before `</body>`
- Navigate to `/search-index.json` — no script injection (the `ShouldProcess` guard excluded it)
- Navigate between pages via SPA navigation — the widget persists because it was injected into the initial page load HTML (SPA navigation does not re-run response processors for the base page)

### Key points
- Response processors run on the full page HTML, not on SPA data fetches — the `/_spa-data/` endpoint returns JSON, which `ShouldProcess` correctly skips
- For widgets that need to react to SPA navigation, listen for the `spa:diagnostics` custom event or similar client-side events

## Beat 8: Verify in Static Builds

Confirm the processor runs during static generation for output parity.

### What to show
- Run `dotnet run build` to generate the static site
- Open a generated HTML file (e.g., `_output/index.html`) in a text editor — the feedback script and div are present before `</body>`
- The output matches what was seen in dev mode — complete parity

### Key points
- Static generation uses the same ASP.NET pipeline as the dev server — every request goes through `T:Pennington.Infrastructure.ResponseProcessingMiddleware`
- This is a design guarantee: processors do not need conditional logic for dev vs. build
- If a processor should only run in dev mode (like `T:Pennington.Infrastructure.LiveReloadScriptProcessor`), check `Environment.GetEnvironmentVariable("DOTNET_WATCH")` in `ShouldProcess`

## Beat 9: Order Sensitivity and Debugging

Demonstrate the impact of ordering and how to diagnose processor issues.

### What to show
- Temporarily change `Order` to `-20` (before `T:Pennington.Infrastructure.XrefResolvingProcessor` at -10). The processor now runs on HTML that still contains unresolved `xref:uid` placeholders — the feedback widget HTML itself could contain xref links that would never be resolved. Reset to `500`
- Temporarily change `Order` to `2000` (after `T:Pennington.Infrastructure.LiveReloadScriptProcessor` at 1000). Now the feedback widget appears after the live reload script in the HTML — functionally fine, but demonstrates that ordering affects HTML position when both processors inject at the same `</body>` point
- Show the diagnostic headers: `T:Pennington.Infrastructure.ResponseProcessingMiddleware` writes `X-Pennington-Diagnostic` headers that contain content diagnostics (format: `Severity|Message|Source`) — these track content issues like unresolved xrefs, not processor execution order

### Key points
- Rule of thumb: processors that add content should run at `Order 100-900` (after URL processing, before dev tools)
- Processors that rewrite URLs or resolve links should run at negative `Order` values
- Dev-mode-only processors should use high `Order` values (1000+) so they see the final content
