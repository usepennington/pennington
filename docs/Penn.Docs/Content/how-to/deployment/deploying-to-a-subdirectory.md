---
title: "Deploying to a Subdirectory"
description: "Configure base URL rewriting for /repo-name/ subdirectory deployments — covering BaseUrlRewritingProcessor, OutputOptions.BaseUrl, CLI arguments, and local testing"
uid: "penn.how-to.deploying-to-a-subdirectory"
order: 10
---

## Beat 1: The Problem — Root-Relative Paths Break Under a Subdirectory

Explain why a Penn site that works at `http://localhost:5000/` breaks when hosted at `https://phil.github.io/beacon/`. Every root-relative URL (`href="/getting-started/"`, `src="/styles.css"`) points to the host root, skipping the `/beacon/` prefix entirely. This affects links, scripts, stylesheets, images, and client-side search paths.

### What to show
- A before/after comparison: the same `<a href="/getting-started/">` that works at root but 404s at `/beacon/`
- Mention this is the universal problem for GitHub Pages project sites, Azure subdirectory apps, and reverse-proxy mounts

### Key points
- Penn generates root-relative URLs by default (no base URL prefix)
- The fix must rewrite every URL attribute in every generated HTML page
- Penn solves this with a two-part system: `OutputOptions.BaseUrl` for build-time configuration and `BaseUrlRewritingProcessor` for runtime rewriting

## Beat 2: Build with a Base URL — CLI Argument Syntax

Show the build command that passes a base URL. Walk through how `OutputOptions.FromArgs` (`T:Penn.Generation.OutputOptions`, method `M:Penn.Generation.OutputOptions.FromArgs(System.String[])`) parses the CLI arguments: `args[1]` becomes `P:Penn.Generation.OutputOptions.BaseUrl` and `args[2]` becomes `P:Penn.Generation.OutputOptions.OutputDirectory`.

### What to show
- The build command: `dotnet run -- build /beacon/ ./output`
- Reference `M:Penn.Generation.OutputOptions.FromArgs(System.String[])` (:path `src/Penn/Generation/OutputOptions.cs`) to show the argument parsing: `args[1]` maps to `BaseUrl`, `args[2]` maps to `OutputDirectory`, with defaults of `/` and `output`
- Reference `P:Penn.Generation.OutputOptions.BaseUrl` (type `T:Penn.Routing.UrlPath`) and `P:Penn.Generation.OutputOptions.OutputDirectory` (type `T:Penn.Routing.FilePath`)

### Key points
- The argument order is `build [baseUrl] [outputDir]` — base URL comes first
- Both arguments are optional: omitting them gives `/` and `output` respectively
- The base URL must include leading and trailing slashes (`/beacon/`, not `beacon`)

## Beat 3: How BaseUrlRewritingProcessor Rewrites HTML

Walk through what happens to every HTML response during build. The `BaseUrlRewritingProcessor` (`T:Penn.Infrastructure.BaseUrlRewritingProcessor`) implements `T:Penn.Infrastructure.IResponseProcessor` and uses AngleSharp to parse the HTML DOM, then rewrites `href`, `src`, and `action` attributes on every element.

### What to show
- Reference `T:Penn.Infrastructure.BaseUrlRewritingProcessor` (:path `src/Penn/Infrastructure/BaseUrlRewritingProcessor.cs`)
- Reference the constructor that reads `P:Penn.Generation.OutputOptions.BaseUrl` and trims the trailing slash
- Reference `M:Penn.Infrastructure.BaseUrlRewritingProcessor.ShouldProcess(Microsoft.AspNetCore.Http.HttpContext)` — skips processing when `_baseUrl` is empty or `/`, only runs on `text/html` and `application/json` responses with 2xx status
- Reference `M:Penn.Infrastructure.BaseUrlRewritingProcessor.ProcessAsync(System.String,Microsoft.AspNetCore.Http.HttpContext)` — delegates to `RewriteHtmlAsync` for HTML content
- Show the private `RewriteHtmlAsync` method: it uses `_browsingContext.OpenAsync` (AngleSharp) to parse the HTML, sets `data-base-url` on `<body>`, then iterates `[href], [src], [action]` elements calling `RewriteAttribute`
- Show the private `RewriteAttribute` method: for any attribute starting with `/` (but not `//`), it prepends `_baseUrl`
- Show a concrete before/after: `href="/getting-started/"` becomes `href="/beacon/getting-started/"`, `src="/styles.css"` becomes `src="/beacon/styles.css"`

### Key points
- AngleSharp ensures robust HTML parsing — no regex-based rewriting
- The processor also sets `data-base-url` on `<body>`, which the client-side search and SPA navigation scripts read to construct correct URLs
- The `IResponseProcessor` pipeline (`T:Penn.Infrastructure.ResponseProcessingMiddleware`, :path `src/Penn/Infrastructure/ResponseProcessingMiddleware.cs`) runs all processors in `P:Penn.Infrastructure.IResponseProcessor.Order` sequence; `BaseUrlRewritingProcessor` has `Order = 0`

## Beat 4: Inspect the Rewritten Output

Have the reader open a generated HTML file from the `./output` directory and verify the rewriting. Show specific before/after examples across different element types.

### What to show
- Open `./output/beacon/index.html` and point out rewritten navigation links, stylesheet references, and script sources
- Show `<body data-base-url="/beacon">` attribute that was injected
- Show a navigation link: `href="/beacon/getting-started/"`
- Show a stylesheet: `href="/beacon/styles.css"`
- Show a search result link in the JSON: `/beacon/guides/configuration/`

### Key points
- Every root-relative URL in the HTML has been prefixed
- The `data-base-url` body attribute enables client-side JavaScript (search, SPA navigation) to construct correct paths at runtime
- JSON responses are also processed by `ShouldProcess` (it checks for `application/json`), but the current `ProcessAsync` only transforms HTML — JSON passes through unchanged

## Beat 5: Set CanonicalBaseUrl for Absolute URLs

Distinguish between the build base URL (relative path prefix) and `CanonicalBaseUrl` (full absolute URL). The build base URL rewrites relative links; `CanonicalBaseUrl` is used for sitemaps, RSS feeds, JSON-LD structured data, and social meta tags.

### What to show
- Reference `P:Penn.DocSite.DocSiteOptions.CanonicalBaseUrl` (:path `src/Penn.DocSite/DocSiteOptions.cs`) — set to `"https://phil.github.io/beacon/"`
- Reference `P:Penn.Infrastructure.PennOptions.CanonicalBaseUrl` (:path `src/Penn/Infrastructure/PennOptions.cs`) — the core-level equivalent, propagated from `DocSiteOptions`
- Show how `CanonicalBaseUrl` flows to `T:Penn.Feeds.SitemapBuilder` and `T:Penn.Feeds.RssFeedBuilder` via `M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})` (:path `src/Penn/Infrastructure/PennExtensions.cs`)
- Show how DocSite layout uses `CanonicalBaseUrl` for JSON-LD structured data (:path `src/Penn.DocSite/Components/Layout/Pages.razor`)

### Key points
- `CanonicalBaseUrl` is optional — but without it, sitemaps have relative URLs and structured data is skipped
- The build base URL and `CanonicalBaseUrl` serve different purposes: one for relative path rewriting, one for absolute URL generation
- Set `CanonicalBaseUrl` to the full public URL including the subdirectory path

## Beat 6: Test Locally Before Deploying

Show how to serve the built output locally at the correct subdirectory path to verify everything works before pushing to CI.

### What to show
- Serve the `./output` directory with a static file server mounted at `/beacon/`
- Command example: `npx serve ./output -l 5000` (then navigate to `http://localhost:5000/beacon/`)
- Verify: navigation links work, search modal opens and returns results with correct URLs, SPA transitions (if enabled) preserve the base path, sitemap.xml contains full absolute URLs

### Key points
- Testing locally catches base URL issues before they reach production
- The search index URLs are relative in the JSON but the client-side `SearchManager` (in `scripts.js`) reads `data-base-url` from the body to prepend the base path when constructing result links
- SPA navigation also reads `data-base-url` to handle client-side routing correctly

## Beat 7: GitHub Actions Workflow

Provide a complete, portable GitHub Actions workflow that builds the site with the repository name as the base URL and deploys to GitHub Pages.

### What to show
- A workflow YAML that:
  - Checks out the code
  - Sets up .NET
  - Runs `dotnet run --project docs/Beacon.Docs -- build /${{ github.event.repository.name }}/ ./output`
  - Deploys `./output` to GitHub Pages
- Point out that `${{ github.event.repository.name }}` makes the workflow portable — it works for any repository without hardcoding the base path
- Reference `M:Penn.Infrastructure.PennExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` (:path `src/Penn/Infrastructure/PennExtensions.cs`) — the method that detects `build` as the first argument and triggers `T:Penn.Generation.OutputGenerationService`

### Key points
- The workflow is portable across repositories because it uses the dynamic repository name
- `RunOrBuildAsync` sets `Environment.ExitCode = 1` when `P:Penn.Generation.BuildReport.HasErrors` is true, so the GitHub Actions step will fail automatically on build errors
- The output directory (`./output`) becomes the GitHub Pages deployment artifact
