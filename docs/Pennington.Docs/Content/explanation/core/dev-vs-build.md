---
title: "Dev mode and build mode share one code path"
description: "Why the static build is an HTTP crawler against the running app, not a second renderer — keeping dev fidelity and publish output in lockstep."
sectionLabel: "Core Architecture"
order: 301020
tags: [build, architecture, static-site, invariants]
uid: explanation.core.dev-vs-build
---

> **In this page.** The deliberate decision to run the same HTTP pipeline whether serving live or generating static output — the static build is a crawler driven by `OutputGenerationService` hitting the running app — and why this keeps dev fidelity and publish output in lockstep.
>
> **Not in this page.** The `build` command's CLI arguments and flag shapes — those belong in the reference page on host CLI arguments.

## The question

_One sentence phrased as the reader's question. Something like: "Why doesn't Pennington have a separate 'offline build' step that reads markdown and writes HTML — why does `dotnet run -- build` start the whole ASP.NET host first?" Keep it to one sentence; this is the hook, not the answer._

## Context

_Two to five sentences. Open by noting that most static site generators are built as standalone compilers — read files, transform, write output — and that a separate publish renderer is a natural first design. Sketch why that shape goes wrong over time: every feature that works in dev needs a second implementation for build, and the two implementations drift. Mention that Pennington started with one host and chose to keep it: dev mode is the host serving requests, and build mode is a crawler pointed at the same host. Close by previewing the invariant that the rest of the page unfolds — there is exactly one HTTP pipeline, and the build is a consumer of it._

## How it works

_Narrative stays close to the mechanism: dev serves the pipeline, build crawls the pipeline, they are the same pipeline. Anchor the prose with one or two signatures from `Pennington.Generation` so the reader sees the crawler shape. Do not drift into option tables or "to configure X, do Y" — those live in reference and how-tos._

### Dev serve: the ASP.NET host IS the renderer

_A few paragraphs. When the user runs `dotnet run`, `RunOrBuildAsync` takes the non-build branch and hands control to `app.RunAsync()`. Every page request flows through the full middleware stack: locale routing, live reload, the response-processing middleware (which captures the body for rewriters and CSS collection), Blazor SSR, the Markdig extensions inside `MarkdownContentRenderer`. The rendered HTML lands in the browser. Note that there is nothing "dev-only" about this path — the diagnostic overlay and live-reload script injection are processors ordered behind gates, not separate code paths. The renderer the reader sees at `localhost:5000` is the renderer, full stop._

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

_Optional — keep this fence if the method body makes the dev-vs-build fork concrete in one glance; drop it if the prose already stood up on its own._

### Build mode: a crawler pointed at the same host

_Explain that when `args[0] == "build"`, `RunOrBuildAsync` calls `app.StartAsync()` — the same Kestrel host, bound to a real URL. It then resolves `OutputGenerationService` and hands it the first bound address. The service opens an `HttpClient` against that URL and issues GETs. It discovers URLs from two sources: every registered `IContentService.DiscoverAsync` and the live `EndpointDataSource` (for `MapGet` handlers like `/styles.css`, `/sitemap.xml`, and the per-locale `/search-index-{code}.json` endpoints). Each response is written to `OutputOptions.OutputDirectory` using the route's `OutputFile`. The 404 page is materialized by fetching a sentinel URL — `NotFoundGeneratorPath = "/__pennington-404-generator"` — that nothing else resolves, so the catch-all fallback fires and its HTML is written as `404.html`._

```csharp:xmldocid
T:Pennington.Generation.OutputGenerationService
```

_Pull the type so the reader sees the contract — one `GenerateAsync(string appUrl)` method. This is the entire build API surface. If it reads as obvious from prose, drop the fence._

### The shared pipeline

_Continue. Because the build is HTTP-driven, every cross-cutting system runs identically in both modes: `ResponseProcessingMiddleware` captures and rewrites bodies, `IHtmlResponseRewriter` resolves `<xref:uid>` and applies locale prefixes and the base URL, `CssClassCollectorProcessor` observes HTML class names before `/styles.css` is fetched last, `SearchIndexService` and `LlmsTxtService` emit their endpoints on request. The crawler fetches HTML pages first and MapGet handlers last so the CSS collector has seen every page before the stylesheet is generated — a phase ordering that exists in `OutputGenerationService.GenerateAsync` and nowhere else. The phrase "output drift" simply has no place to hide: the pipeline that produced `localhost:5000/foo` is byte-for-byte the pipeline that produced `output/foo/index.html`._

### Why not a separate renderer?

_A few sentences. Walk the reader through the alternative that was on the table: a pure in-process renderer that reads markdown, drives Markdig directly, and writes files — no host, no HTTP, faster. It loses, because every feature that depends on ASP.NET — locale middleware, response processors, Blazor SSR for islands, the diagnostic-header transport, the per-locale search endpoints — would need a second implementation in the offline path. Each new feature becomes a multi-site edit and two chances to drift. The HTTP overhead of build mode is small; the architectural cost of a second renderer is not._

## Trade-offs

- **Cost — the build boots the full host.** Generation is not a pure function of your content directory; it starts Kestrel, binds a port, and loads every service `AddPennington` registers. For tiny sites this is measurable overhead. In exchange, nothing that works in dev fails in publish.
- **Alternative considered — an offline renderer.** A second code path reading markdown and driving Markdig directly would skip the HTTP round-trip. It was rejected because the engine's value is in the response-processor chain (xref, locale, base URL, CSS collection, diagnostics); a renderer that bypasses that chain is a renderer that silently drops half the feature surface.
- **Consequence — every feature pays one integration tax, not two.** A new response processor, rewriter, or endpoint works in build the moment it works in dev, with no "also wire this into the static generator" step. That is the invariant; do not propose designs that split dev-serve and build-publish into separate implementations.
- **Consequence — the `/styles.css` (and other MapGet) endpoints must tolerate being fetched after content pages.** The crawler deliberately serializes content-first, MapGet-last so class collection completes before the stylesheet is materialized. If you add an endpoint whose correctness depends on fetch order, you are fighting this invariant.

## Further reading

- Reference: [Build report fields](xref:reference.diagnostics.build-report)
- Reference: [CLI and build arguments](xref:reference.host.cli)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
