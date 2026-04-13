---
title: "Dev mode and build mode share one code path"
description: "The deliberate decision to run the same HTTP pipeline whether serving live or generating static output."
section: "core"
order: 20
tags: []
uid: explanation.core.dev-vs-build
isDraft: true
search: false
llms: false
---

> **In this page.** The deliberate decision to run the same HTTP pipeline whether serving live or generating static output — the static build is a crawler driven by `OutputGenerationService` hitting the running app — and why this keeps dev fidelity and publish output in lockstep.
>
> **Not in this page.** Static-build CLI arguments (see Reference).

## The question

Why does Pennington refuse to ship a separate "offline build" renderer and instead run the static build as an HTTP crawler against its own running host?

## Context

- Most static site generators split into two code paths: a dev server that hot-reloads, and an offline build that renders to disk through a different code path.
- That split is where dev/prod drift is born — a Markdig extension, a middleware, a response rewriter, or a Razor layout that behaves one way under the dev server and another way under the build.
- Pennington's pipeline is dense with response-stage work that only materializes late: MonorailCSS class collection, locale link rewriting, xref resolution, base-URL prefixing, diagnostics headers, Razor SSR.
- Duplicating all of that in an offline renderer would be the single largest source of "works in dev, broken on publish" bugs in the project.
- The chosen invariant: there is exactly one rendering path, and the static build consumes it through the same contract the browser does — HTTP GET.

## How it works

### One entry point, two modes

- `RunOrBuildAsync(app, args)` in `src/Pennington/Infrastructure/PenningtonExtensions.cs` is the single entry point shared by every Pennington host.
- When `args[0] != "build"`, it calls `app.RunAsync()` — ordinary `dotnet run` dev serve.
- When `args[0] == "build"`, it calls `app.StartAsync()` so the full ASP.NET host comes up identically, resolves `OutputGenerationService`, invokes `GenerateAsync(app.Urls.First())`, then `StopAsync`.
- The branch is a dozen lines. Nothing about the pipeline, DI graph, middleware order, or endpoints changes between the two modes.

### The static build is a crawler, not a renderer

- `OutputGenerationService.GenerateAsync` (`src/Pennington/Generation/OutputGenerationService.cs`) constructs an `HttpClient` with `AllowAutoRedirect = false` pointed at `app.Urls.First()`.
- Pages are discovered from two sources: `IContentService.DiscoverAsync` for content routes, and the live `EndpointDataSource` for `MapGet` routes such as `/styles.css`, `/sitemap.xml`, per-locale `/search-index-{code}.json`.
- Each page is fetched in parallel via `HttpClient.GetAsync` and written to `OutputOptions.OutputDirectory`. HTML pages, JSON, XML, and binary assets are each handled by the same fetcher.
- The sentinel `NotFoundGeneratorPath = "/__pennington-404-generator"` is fetched last to materialize `404.html` from the catch-all fallback.

### Ordering is deliberate

- HTML content pages are fetched first (Phase 6), then `MapGet` routes last (Phase 7). `/styles.css` must see the class set collected from every rendered HTML page before MonorailCSS generates the stylesheet.
- Static assets from content services and the composite `WebRootFileProvider` (wwwroot plus RCLs) are copied directly in Phase 4 — not fetched over HTTP — so they land before any HTML response can reference them and before the link verifier runs in Phase 9.
- Because every HTML response flows through the real `ResponseProcessingMiddleware`, the `IHtmlResponseRewriter` chain (`XrefHtmlRewriter` → `LocaleLinkHtmlRewriter` → `BaseUrlHtmlRewriter`) runs in the same order on disk as it does in the browser.

### Diagnostics ride the same channel

- Per-request diagnostics are emitted by handlers into `DiagnosticContext` and flushed as `X-Pennington-Diagnostic` response headers by `ResponseProcessingMiddleware`.
- `OutputGenerationService.ParseDiagnosticHeaders` reads those headers off the crawler's `HttpResponseMessage` and threads them into the `BuildReport`.
- In dev, the same headers feed `DiagnosticOverlayProcessor` and show up as an overlay in the browser.
- There is no "build-only" diagnostic surface and no "dev-only" error path — the channel is HTTP headers either way.

## Trade-offs

- **Cost: the host must boot to build.** A build cannot be a pure in-memory transform; Kestrel binds a port, the full DI container spins up, every registered service initializes. For a tiny site this is slower than a direct renderer would be.
- **Cost: parallelism lives inside `HttpClient`, not inside a bespoke scheduler.** Fine-grained build orchestration (per-page cache keys, incremental output, dependency graphs) has to be expressed in terms of HTTP round-trips rather than as an in-process DAG.
- **Alternative considered and rejected: a second renderer that bypasses the HTTP pipeline.** It would be faster and simpler to write once, but every response-stage feature (middleware, rewriters, SSR, CSS collection, diagnostics) would then need two implementations kept in sync. The project's stated invariant is "do not propose designs that add a separate offline build renderer that bypasses the HTTP pipeline."
- **Consequence: if it works in dev, it works on publish.** Equivalently, if it is broken on publish, the same bug is reproducible with `dotnet run` and a browser — there is no build-only failure mode to debug in the dark.

## Further reading

- Reference: [Static build CLI arguments](/reference/generation/output-options)
- Reference: [Response processors and HTML rewriters](/reference/infrastructure/response-processors)
- How-to: [Publish a site to static output](/how-to/generation/publish-static-output)
- Explanation: [The content pipeline](/explanation/core/content-pipeline)
