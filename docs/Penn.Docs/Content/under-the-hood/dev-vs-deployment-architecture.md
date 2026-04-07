---
title: "Development vs Deployment Architecture"
description: "How Penn runs as a live dev server and a static site generator from the same codebase"
uid: "penn.under-the-hood.dev-vs-deployment-architecture"
order: 3000
---

Penn is simultaneously a live ASP.NET Core development server and a static site generator. The same content pipeline, the same rendering logic, the same middleware chain -- the difference between the two modes is a single CLI argument. During development you run `dotnet watch` and get on-demand rendering with hot reload. When you are ready to deploy you run `dotnet run -- build` and get a directory of static HTML files. There is no separate build tool. There is no template mismatch. The HTML you see in your browser during development is the same HTML that ships to production.

After reading this page you will understand how `RunOrBuildAsync` switches between modes, how `OutputGenerationService` crawls the running application to produce static files, and why the self-crawl approach guarantees parity between development and production.

## Development Mode

In development mode, Penn is a standard ASP.NET Core application running Blazor SSR. You start it with `dotnet watch` (or `dotnet run`) and open a browser.

Every incoming request flows through the full content pipeline: content services discover pages, the parser extracts front matter and markdown, and the renderer produces HTML. The response passes through a chain of `IResponseProcessor` implementations registered in DI:

- **`BaseUrlRewritingProcessor`** -- rewrites root-relative URLs when a base URL is configured (usually a no-op during development since the base URL defaults to `/`)
- **`XrefResolvingProcessor`** -- resolves `xref:` links to their target URLs
- **`LiveReloadScriptProcessor`** -- injects a WebSocket script that triggers browser refresh on file changes (only active when the `DOTNET_WATCH` environment variable is set)
- **`DiagnosticOverlayProcessor`** -- adds an overlay showing warnings and errors for the current page

Content is cached after first access. `MarkdownContentService` uses `AsyncLazy` for its metadata and `FileWatchDependencyFactory<T>` manages service instances like `XrefResolver`, `SearchIndexService`, and `SitemapService` that need to be recreated when source files change. The `FileWatcher` monitors content directories and notifies subscribers, which invalidate cached instances so the next request gets fresh data. You never restart the server to see content changes.

Draft pages are visible during development. Items with `IsDraft: true` in their front matter go through the entire pipeline and render normally, so you can preview unpublished content in context. They are only filtered out during static generation.

For a deeper look at the file watching and cache invalidation mechanics, see <xref:penn.under-the-hood.hot-reload-architecture>.

## Static Build Mode

In build mode, the same ASP.NET Core application boots up, but it never waits for browser requests. Instead, it starts Kestrel, crawls itself over HTTP, writes the responses to disk, and shuts down.

The command is:

```shell
dotnet run -- build [baseUrl] [outputDir]
```

The two optional arguments are parsed by `OutputOptions.FromArgs`:

- **`baseUrl`** (default `/`) -- prepended to all URLs in the generated HTML. Essential for subdirectory deployments like GitHub Pages where your site lives at `/my-project/` rather than the root.
- **`outputDir`** (default `output`) -- the directory where static files are written.

These values are registered as a singleton `OutputOptions` during `AddPenn` and are available to both `OutputGenerationService` (which needs the output directory) and `BaseUrlRewritingProcessor` (which needs the base URL to rewrite links in the generated HTML).

For practical deployment instructions, see <xref:penn.getting-started.deploying-to-github-pages>.

## RunOrBuildAsync: The Mode Switch

The entire mode decision lives in one extension method on `WebApplication`:

```csharp
public static async Task RunOrBuildAsync(this WebApplication app, string[] args)
{
    StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);

    if (args.Length > 0 && args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
    {
        await app.StartAsync();
        var generator = app.Services.GetRequiredService<OutputGenerationService>();
        var addresses = app.Urls.Any() ? app.Urls : ["http://localhost:5000"];
        var report = await generator.GenerateAsync(addresses.First());
        await app.StopAsync();

        report.WriteTo(Console.Out);
        if (report.HasErrors)
        {
            Environment.ExitCode = 1;
        }
    }
    else
    {
        await app.RunAsync();
    }
}
```

The first line -- `StaticWebAssetsLoader.UseStaticWebAssets` -- ensures that static web assets from Razor Class Libraries (like Penn.UI's scripts and styles) are available in both modes. Without this, RCL assets would only resolve correctly during development.

After that, the logic branches on a single check: does the first argument equal `"build"`? If yes, the app starts Kestrel, resolves `OutputGenerationService` from DI, calls `GenerateAsync` with the first listening address, then stops the server and writes the build report to the console. If the report contains errors, the exit code is set to 1 so CI pipelines fail appropriately.

If the argument is not `"build"`, the app runs normally with `app.RunAsync()` -- the standard ASP.NET Core blocking run that keeps the server alive until you stop it.

The key insight: `Program.cs` is identical for both modes. There is no conditional service registration, no build-only configuration. Every service, every middleware, every response processor is registered the same way regardless of which mode runs.

## OutputGenerationService: The Nine Phases

`OutputGenerationService` is where the static build actually happens. It creates an `HttpClient` pointed at the running application and executes nine phases in sequence.

### Phase 1: Collect content pages

The service iterates every registered `IContentService` implementation and calls `DiscoverAsync()` on each. This produces a list of `PageToGenerate` records, each carrying a `ContentRoute` that contains both a canonical URL (for the HTTP request) and an output file path (for writing to disk).

### Phase 2: Discover MapGet routes

`DiscoverMapGetRoutes()` inspects the ASP.NET `EndpointDataSource` to find all registered GET endpoints that are not content pages. It filters out parameterized routes (anything containing `{`), Blazor framework routes (`_framework`, `_blazor`), static file endpoints, fallback routes, and component-based routes. What remains are routes like `/styles.css`, `/search-index.json`, and `/sitemap.xml`. These are collected separately because they must be fetched *after* all HTML pages.

### Phase 3: Clean output directory

If the output directory exists, it is deleted and recreated. This ensures a clean build with no stale artifacts from previous runs.

### Phase 4: Copy static assets

`CopyStaticAssetsAsync` handles two categories of files. First, it asks each `IContentService` for non-markdown files to copy -- images, downloads, and other assets that live alongside content. Second, it copies everything from the web root file provider, including RCL static assets discovered through the `CompositeFileProvider`.

### Phase 5: Create dynamic content files

`CreateContentFilesAsync` asks each `IContentService` for programmatically generated files via `GetContentToCreateAsync()`. Each item provides an output path and a content generator function that returns a byte array.

### Phase 6: Fetch HTML content pages

This is the self-crawl. `FetchPagesAsync` uses `Parallel.ForEachAsync` to make HTTP GET requests to the running application for every content page discovered in Phase 1. Each response is written to the output directory as a file. The method handles three cases: redirects (producing a small HTML file with a `<meta http-equiv="refresh">` tag), successful responses (written as text or binary depending on content type), and failures (recorded in the build report). Per-page diagnostics are extracted from `X-Penn-Diagnostic` response headers that the `ResponseProcessingMiddleware` attaches.

### Phase 7: Fetch MapGet routes

The same `FetchPagesAsync` mechanism runs again, but now for the MapGet routes collected in Phase 2. This happens after all HTML pages have been fetched. The ordering is deliberate: MonorailCSS collects utility class names from rendered HTML as pages are served. By the time `/styles.css` is requested, every HTML page has been processed and the CSS generator has a complete picture of which classes the site uses.

### Phase 8: Generate 404 page

The service fetches `/__penn-404-generator` -- a URL that does not match any route and triggers the fallback handler. The response is saved as `404.html` in the output root. Static hosting platforms like GitHub Pages serve this file when a visitor requests a URL that does not exist.

### Phase 9: Verify internal links

`LinkVerificationService` performs static analysis over all the HTML fetched in Phases 6 and 7. It extracts `href` and `src` attributes, checks each internal link against the set of all known routes, and reports broken links. No additional HTTP requests are made -- this is pure string analysis over already-fetched content. Broken links are added to the `BuildReport` so they surface in the console output and fail CI.

## The Build Report

After all nine phases complete, `OutputGenerationService` returns a `BuildReport`. The report accumulates results across every phase:

- **Generated pages** -- pages that were successfully fetched and written to disk
- **Skipped pages** -- draft pages filtered from the build
- **Failed pages** -- pages that returned error HTTP status codes
- **Diagnostics** -- warnings and errors from response processors, propagated through `X-Penn-Diagnostic` headers
- **Broken links** -- internal links that do not resolve to any known route

`BuildReport.WriteTo(Console.Out)` prints a human-readable summary: page count, generation time, errors, warnings, and broken link details. If `HasErrors` is true (any errors, broken links, or failed pages), the exit code is set to 1. This is the integration point with CI -- a build with broken links or rendering failures will not silently pass.

## Development vs Build Comparison

| Aspect | Development | Static Build |
|---|---|---|
| Server lifetime | Continuous until stopped | Start, generate, stop |
| Page rendering | On-demand per request | All pages in parallel |
| Caching | File-watch invalidation via `FileWatchDependencyFactory` | Not needed (single pass) |
| Drafts | Visible for preview | Filtered from output |
| Base URL rewriting | Skipped (base URL is `/`) | Applied when base URL is set |
| Live reload | Active (WebSocket + script injection) | Not applicable |
| Diagnostic overlay | Injected into responses | Not injected |
| Link verification | Not performed | Full cross-site verification |
| Output | HTTP responses to browser | Static files on disk |

The central guarantee: since the static build fetches pages by making real HTTP requests to the running application, the generated HTML passes through the same middleware pipeline and response processors that a browser sees during development. The rendering path is not approximated or reimplemented -- it is the same code, producing the same output, delivered through a different mechanism.

## What Ends Up in the Output Directory

A typical build produces a directory structure like this:

```
output/
  index.html
  404.html
  styles.css
  search-index.json
  sitemap.xml
  getting-started/
    index.html
    installation/
      index.html
    your-first-site/
      index.html
  under-the-hood/
    content-processing-pipeline/
      index.html
    hot-reload-architecture/
      index.html
  _spa-data/
    getting-started/
      index.json
    under-the-hood/
      content-processing-pipeline.json
  images/
    logo.png
```

Each content page produces a directory with an `index.html` file, giving you clean URLs (`/getting-started/` instead of `/getting-started.html`). Static assets -- images, downloads, web root files, and RCL assets -- are copied to their corresponding paths. Generated resources like `styles.css`, `search-index.json`, and `sitemap.xml` come from MapGet routes. If SPA navigation is configured, JSON data envelopes appear under `_spa-data/`. Redirect pages are tiny HTML files containing a `<meta http-equiv="refresh">` tag.

## The Complete Lifecycle

To see how both modes handle the same content, trace a single markdown file through each path.

**Development mode.** You save `getting-started/installation.md`. The `FileWatcher` detects the change and notifies subscribers. `FileWatchDependencyFactory` invalidates cached instances of `XrefResolver`, `SearchIndexService`, and `SitemapService`. The `LiveReloadServer` sends a reload message over the WebSocket connection. Your browser refreshes and requests `/getting-started/installation/`. The request hits the content pipeline: `MarkdownContentService` discovers the file, `MarkdownContentParser` extracts front matter and body, `MarkdownContentRenderer` runs Markdig to produce HTML. The `ResponseProcessingMiddleware` runs the processor chain -- xref resolution, live reload script injection, diagnostic overlay. The browser renders the page. Total time: milliseconds.

**Static build mode.** You run `dotnet run -- build /my-project/ output`. The application starts Kestrel. `OutputGenerationService` begins Phase 1, calling `DiscoverAsync()` on `MarkdownContentService`, which finds `installation.md` and produces a `ContentRoute` mapping `/getting-started/installation/` to `getting-started/installation/index.html`. In Phase 6, an `HttpClient` sends a GET request to `http://localhost:5000/getting-started/installation/`. The request flows through the same pipeline -- discover, parse, render -- and then through the response processors. The `BaseUrlRewritingProcessor` rewrites root-relative URLs to include `/my-project/`. The response is written to `output/getting-started/installation/index.html`. After all pages are fetched, Phase 9 verifies that every internal link in the page resolves to a known route. The build report is printed and the application shuts down.

Same file. Same pipeline. Same rendering. Different delivery.

## Next Steps

- <xref:penn.under-the-hood.hot-reload-architecture> -- how file watching, cache invalidation, and live reload collaborate during development
- <xref:penn.under-the-hood.content-processing-pipeline> -- the discover/parse/render pipeline that both modes share
- <xref:penn.getting-started.deploying-to-github-pages> -- practical guide to building and deploying with a base URL
