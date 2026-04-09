---
title: "The Response Processor Pipeline"
description: "Why Pennington post-processes HTTP responses instead of modifying content at render time — the middleware stream capture pattern, ordering semantics (base URL rewriting before xref resolution), DiagnosticContext flow from processors to headers to build reports"
uid: "penn.explanation.response-processor-pipeline"
order: 10
---

Explain why Pennington post-processes complete HTTP responses rather than modifying content at render time in Razor or Markdig. The core reason: response processors run on the final HTML regardless of how it was produced (markdown, Razor page, programmatic content), ensuring uniform behavior. Discuss the middleware stream capture pattern — `ResponseProcessingMiddleware` substitutes the response stream with a `MemoryStream`, lets the rest of the pipeline write to it, then reads the buffered body, runs processors, and writes the final result to the real stream. Explain ordering semantics and why they matter: `BaseUrlRewritingProcessor` must run before `XrefResolvingProcessor` because xref resolution produces relative URLs that might need base URL rewriting. Discuss the `DiagnosticContext` flow: processors can emit diagnostics (warnings, errors) that flow to `X-Pennington-Diagnostic` response headers in dev mode and aggregate into the `BuildReport` during static generation. Explain why `CssClassCollectorProcessor` must run on every HTML response (to observe all utility classes) but the CSS endpoint must be fetched after all HTML is processed.
