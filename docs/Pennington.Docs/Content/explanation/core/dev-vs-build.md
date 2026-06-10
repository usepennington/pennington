---
title: "Dev mode and build mode share one code path"
description: "Why the static build is a crawler against the same ASP.NET pipeline as dev, not a second renderer — keeping dev fidelity and publish output in lockstep."
sectionLabel: "Core Architecture"
order: 3
tags: [build, architecture, static-site, invariants]
uid: explanation.core.dev-vs-build
---

Why doesn't Pennington have a separate offline build step — one that reads markdown and writes HTML without starting a web server — when `dotnet run -- build` boots the entire ASP.NET host first?

## Context

Most static site generators are built as compilers: read content files, transform them, write HTML. That shape is intuitive, and it was on the table for Pennington too. The problem with a separate publish renderer surfaces not at the first feature but at the second. Locale middleware runs in dev, so it needs a second implementation in the offline path; response processors run in dev, so they need it too; Blazor SSR for islands, the xref rewriter, the CSS discovery pipeline — each one accrues a corresponding "also do this in build" edit. The two implementations then diverge over time, invisibly, until a feature that works in development produces different output in publish.

Pennington keeps one host. Dev mode is that host serving requests over Kestrel; build mode is a crawler that drives the same host's request pipeline in process. There is exactly one ASP.NET pipeline, and the static build is a consumer of it. The rest of this page works through what that buys.

## How it works

### Dev serve: the ASP.NET host is the renderer

Running `dotnet run` causes `RunOrBuildAsync` to detect the absence of a `build` argument and call `app.RunAsync()`. Every request that lands at `localhost:5000` flows through the full middleware stack: locale routing, live reload, `ResponseProcessingMiddleware` capturing and rewriting the body, Blazor SSR for any island components, and the Markdig extensions inside `MarkdownContentRenderer`. The rendered HTML that arrives in the browser is the pipeline output, unchanged.

Nothing in this path is marked "dev-only." The diagnostic overlay and live-reload script injection are response processors ordered behind environment gates — not separate code paths. The renderer behind `localhost:5000` is the same renderer the build uses.

### Build mode: a crawler driving the same pipeline

When the first argument is `build`, Pennington replaces Kestrel with an in-memory test host at service-registration time, then starts that host without a socket bind, a dev-cert prompt, or a port. The crawler dispatches requests straight into the same `RequestDelegate` Kestrel would have invoked in dev — there is no network round-trip, but every request runs the full pipeline.

URL discovery comes from two sources. Every registered `IContentService` exposes `DiscoverAsync`, which returns the set of content routes it knows about. The live `EndpointDataSource` covers `MapGet` handlers — `/styles.css`, `/sitemap.xml`, the per-locale `/search/{locale}/...` artifacts, and anything else the host has wired up explicitly. Each response is written to `OutputOptions.OutputDirectory` using the route's `OutputFile` mapping.

The 404 page is a small special case: the crawler fetches a URL that no route matches, so the catch-all fallback fires and its output is written as `404.html`. The mechanism remains a GET against the same pipeline.

### The shared pipeline

Because the build drives requests through the same pipeline, every cross-cutting system runs identically in both modes. `ResponseProcessingMiddleware` captures and rewrites bodies. `IHtmlResponseRewriter` resolves xref links and applies locale prefixes and the base URL. The [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) discovery pipeline scans loaded assemblies and watched source files at startup, so the class registry is already populated before the crawler starts; content-page GETs run first and `MapGet` GETs last as a separate ordering rule, ensuring `/styles.css` and other generated endpoints see a fully-warm system.

The consequence is that dev and build cannot drift apart. The pipeline that produced `localhost:5000/foo` is the pipeline that produced `output/foo/index.html`. A feature that works in dev works in build, and one that breaks in build would have broken in dev first.

### Why not a separate renderer?

The alternative — a pure in-process renderer that drives Markdig directly, writes files, skips the request pipeline entirely — is faster for small sites and simpler to maintain if the feature set never grows. The tradeoff is that every capability built on top of ASP.NET would have to be reimplemented for the offline path. Locale middleware, response processors, Blazor SSR for islands, the per-locale search artifacts, the diagnostic-header transport — each would require a second implementation. Each new feature becomes two edits and two chances for the implementations to diverge.

Build mode does add a fixed cost per page — routing and the middleware stack run on every URL rather than being bypassed — but with no socket round-trip that cost is a thin slice of each page's render time and stays flat as the site grows. The in-memory `BuildHtmlCache` further collapses the disk-write, search-index, and llms.txt passes to one render per URL. The cost of maintaining a second renderer, by contrast, grows with every feature added. Pennington accepts the per-request overhead to avoid it.

## Further reading

- Reference: [Build report fields](xref:reference.api.build-report)
- Reference: [CLI and build arguments](xref:reference.host.cli)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
