---
title: "Response Processor Chain"
description: "Reference for IResponseProcessor interface (Order, ShouldProcess, ProcessAsync), ResponseProcessingMiddleware behavior, all 6 built-in processors in execution order, and the X-Penn-Diagnostic header protocol"
uid: "penn.reference.response-processor-chain"
order: 10
---

Document the `IResponseProcessor` interface: `Order` (int, lower runs first), `ShouldProcess(HttpContext)` (bool), `ProcessAsync(body, context)` (returns transformed string). Document `ResponseProcessingMiddleware` behavior: captures response body via stream substitution, runs all processors where `ShouldProcess` returns true in `Order` sequence, writes final result. List all 6 built-in processors in execution order with their order values, what they do, and their `ShouldProcess` conditions: `BaseUrlRewritingProcessor`, `XrefResolvingProcessor`, `LiveReloadScriptProcessor` (dev only, checks `DOTNET_WATCH` env var), `LocaleLinkRewritingProcessor`, `DiagnosticOverlayProcessor` (dev only), `CssClassCollectorProcessor`. Document the `X-Penn-Diagnostic` response header format.
