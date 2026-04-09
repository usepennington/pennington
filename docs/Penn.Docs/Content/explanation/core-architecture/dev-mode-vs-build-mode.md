---
title: "Dev Mode vs Build Mode"
description: "How the same ASP.NET app serves as both a live dev server and a static site generator — covering RunOrBuildAsync dispatch, the HTTP self-crawl strategy for generation, response processor parity between modes, and phase ordering (HTML before CSS)"
uid: "penn.explanation.dev-mode-vs-build-mode"
order: 20
---

Explain the dual-personality architecture: the same ASP.NET application runs as a live development server (with hot reload, drafts visible, diagnostics overlay) and as a static site generator (HTTP-crawling itself to produce files). Discuss the design choice to generate static sites by starting the app, making HTTP requests to itself, and saving the responses — this ensures that response processors, middleware, and Razor rendering all execute identically in both modes, eliminating dev/prod parity bugs. Explain the phase ordering constraint in `OutputGenerationService`: HTML pages must be fetched before the CSS endpoint because `CssClassCollectorProcessor` needs to observe all HTML to know which utility classes to include in the stylesheet. Discuss `RunOrBuildAsync` as the mode switch and how CLI arguments (`build [baseUrl] [outputDir]`) control it.
