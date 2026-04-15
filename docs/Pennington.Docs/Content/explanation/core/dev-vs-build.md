---
title: "Dev mode and build mode share one code path"
description: "Why the static build is an HTTP crawler against the running app, not a second renderer — keeping dev fidelity and publish output in lockstep."
sectionLabel: "Core Architecture"
order: 301020
tags: [build, architecture, static-site, invariants]
uid: explanation.core.dev-vs-build
---

Why doesn't Pennington have a separate offline build step — one that reads markdown and writes HTML without starting a web server — when `dotnet run -- build` boots the entire ASP.NET host first?

## Context

Most static site generators are built as compilers: read content files, transform them, write HTML. That shape is intuitive, and it was on the table for Pennington too. The problem with a separate publish renderer becomes visible not at first feature but at second: locale middleware runs in dev, so it needs a second implementation in the offline path; response processors run in dev, so they need it too; Blazor SSR for islands, the xref rewriter, the CSS class collector — each one accrues a corresponding "also do this in build" edit. The two implementations then diverge over time, invisibly, until a feature that works perfectly in development silently produces different output in publish.

Pennington keeps one host. Dev mode is that host serving requests; build mode is a crawler pointed at the same host. The invariant the rest of this page unfolds is simple: there is exactly one HTTP pipeline, and the static build is a consumer of it.

## How it works

### Dev serve: the ASP.NET host is the renderer

When you run `dotnet run`, `RunOrBuildAsync` detects the absence of a `build` argument and calls `app.RunAsync()`. Every request that lands at `localhost:5000` flows through the full middleware stack: locale routing, live reload, `ResponseProcessingMiddleware` capturing and rewriting the body, Blazor SSR for any island components, and the Markdig extensions inside `MarkdownContentRenderer`. The rendered HTML that arrives in your browser is the pipeline output, unchanged.

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

Nothing in this path is marked "dev-only." The diagnostic overlay and live-reload script injection are response processors ordered behind environment gates — they are not separate code paths. The renderer you see at `localhost:5000` is the renderer, full stop.

### Build mode: a crawler pointed at the same host

When `args[0] == "build"`, `RunOrBuildAsync` calls `app.StartAsync()` instead — the same Kestrel host, bound to a real port. It then resolves `OutputGenerationService` and hands it the first bound address. From that point, the service is an HTTP client: it opens an `HttpClient` against that URL and issues GETs.

URL discovery comes from two sources. Every registered `IContentService` exposes `DiscoverAsync`, which returns the set of content routes it knows about. The live `EndpointDataSource` covers `MapGet` handlers — `/styles.css`, `/sitemap.xml`, the per-locale `/search-index-{code}.json` endpoints, and anything else the host has wired up explicitly. Each response is written to `OutputOptions.OutputDirectory` using the route's `OutputFile` mapping.

The 404 page is a small special case: the service fetches a sentinel URL (`"/__pennington-404-generator"`) that no route matches, so the catch-all fallback fires and its output is written as `404.html`. The mechanism is still an HTTP GET; it is still the same pipeline.

```csharp:xmldocid
T:Pennington.Generation.OutputGenerationService
```

### The shared pipeline

Because the build is HTTP-driven, every cross-cutting system runs identically in both modes. `ResponseProcessingMiddleware` captures and rewrites bodies. `IHtmlResponseRewriter` resolves xref links and applies locale prefixes and the base URL. `CssClassCollectorProcessor` observes HTML class names across content pages before `/styles.css` is fetched — a deliberate serialization: the crawler issues content-page GETs first and `MapGet` handler GETs last, so class collection completes before the stylesheet is materialized. That phase ordering lives in `OutputGenerationService.GenerateAsync` and nowhere else.

The consequence is that output drift has no place to hide. The pipeline that produced `localhost:5000/foo` is the pipeline that produced `output/foo/index.html`. If a feature works in dev, it works in build. If it breaks in build, it would have broken in dev first.

### Why not a separate renderer?

The alternative — a pure in-process renderer that drives Markdig directly, writes files, skips the HTTP round-trip — is faster for small sites and architecturally tidier if your feature set is frozen. The tradeoff is that every capability built on top of ASP.NET stops being free. Locale middleware, response processors, Blazor SSR for islands, the per-locale search endpoints, the diagnostic-header transport — each would require a second implementation in the offline path. Each new feature becomes two edits and two chances for the implementations to diverge.

The HTTP overhead of build mode is measurable on very small sites and mostly irrelevant on anything larger. The architectural cost of a second renderer compounds with every feature added. Pennington takes the HTTP overhead.

## Trade-offs

- **Cost — the build boots the full host.** Generation is not a pure function of your content directory; it starts Kestrel, binds a port, and loads every service `AddPennington` registers. For tiny sites this is measurable overhead. In exchange, nothing that works in dev fails in publish.
- **Alternative considered — an offline renderer.** A second code path reading markdown and driving Markdig directly would skip the HTTP round-trip. It was rejected because the engine's value is in the response-processor chain (xref, locale, base URL, CSS collection, diagnostics); a renderer that bypasses that chain is a renderer that silently drops half the feature surface.
- **Consequence — every feature pays one integration tax, not two.** A new response processor, rewriter, or endpoint works in build the moment it works in dev, with no "also wire this into the static generator" step. That is the invariant, and designs that split dev-serve and build-publish into separate implementations work against it.
- **Consequence — the `/styles.css` (and other MapGet) endpoints must tolerate being fetched after content pages.** The crawler deliberately serializes content-first, MapGet-last so class collection completes before the stylesheet is materialized. If you add an endpoint whose correctness depends on fetch order, you are fighting this invariant.

## Further reading

- Reference: [Build report fields](xref:reference.diagnostics.build-report)
- Reference: [CLI and build arguments](xref:reference.host.cli)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
