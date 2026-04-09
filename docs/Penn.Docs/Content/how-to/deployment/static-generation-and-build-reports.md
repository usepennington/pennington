---
title: "Static Generation and Build Reports"
description: "Run static site generation with dotnet run build, understand the 9-phase generation process, interpret BuildReport output (generated pages, errors, warnings, broken links), and configure CI exit codes"
uid: "penn.how-to.static-generation-and-build-reports"
order: 20
---

## Beat 1: Run the Build Command

Show the command to trigger static site generation and explain how `RunOrBuildAsync` detects build mode from the first CLI argument.

### What to show
- The build command: `dotnet run -- build / ./output`
- Reference `M:Penn.Infrastructure.PennExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` (:path `src/Penn/Infrastructure/PennExtensions.cs`) — the entry point that checks `args[0].Equals("build", StringComparison.OrdinalIgnoreCase)`
- Show the detection flow: if first arg is `build`, it calls `app.StartAsync()`, resolves `T:Penn.Generation.OutputGenerationService` from DI, calls `M:Penn.Generation.OutputGenerationService.GenerateAsync(System.String)`, then `app.StopAsync()`
- If the first arg is not `build`, it falls through to `app.RunAsync()` for normal dev server mode
- Reference `M:Penn.Generation.OutputOptions.FromArgs(System.String[])` (:path `src/Penn/Generation/OutputOptions.cs`) — parses `args[1]` as `P:Penn.Generation.OutputOptions.BaseUrl` and `args[2]` as `P:Penn.Generation.OutputOptions.OutputDirectory`

### Key points
- The same `Program.cs` serves both dev mode and static build — no separate build tool needed
- The `OutputOptions` are registered as a singleton in DI during `AddPenn`, parsed from `Environment.GetCommandLineArgs()` (:path `src/Penn/Infrastructure/PennExtensions.cs`, line where `OutputOptions.FromArgs` is called)
- Default output directory is `output`, default base URL is `/`

## Beat 2: The Nine Generation Phases

Walk through each phase of `OutputGenerationService.GenerateAsync` so the reader understands what the console output means and what order things happen in.

### What to show
- Reference `M:Penn.Generation.OutputGenerationService.GenerateAsync(System.String)` (:path `src/Penn/Generation/OutputGenerationService.cs`)
- **Phase 1: Collect content pages** — iterates all `T:Penn.Content.IContentService` registrations, calling `M:Penn.Content.IContentService.DiscoverAsync` to build a list of `PageToGenerate` records
- **Phase 2: Discover MapGet routes** — scans `EndpointDataSource` for GET endpoints (like `/styles.css`, `/search-index.json`, `/sitemap.xml`) that are not Blazor component routes or parameterized routes
- **Phase 3: Clean and recreate output directory** — unconditionally deletes and recreates `P:Penn.Generation.OutputOptions.OutputDirectory`. Note: `P:Penn.Generation.OutputOptions.CleanOutput` exists (default `true`) but is not currently checked — the output directory is always cleaned
- **Phase 4: Copy static assets** — copies non-markdown content files from content services (`GetContentToCopyAsync`), wwwroot files, and RCL static web assets (Penn.UI scripts, etc.)
- **Phase 5: Create dynamic content files** — calls `GetContentToCreateAsync` on each content service for dynamically generated files
- **Phase 6: Fetch HTML content pages** — HTTP-crawls the running app in parallel using `HttpClient`, writes responses as static files. This is where markdown-rendered pages become HTML files
- **Phase 7: Fetch MapGet routes last** — fetches CSS, search index, sitemap, etc. **after** all HTML. This ordering matters because the CSS class collector (`CssClassCollector`) needs to observe all HTML before generating the stylesheet
- **Phase 8: Generate 404.html** — fetches `/__penn-404-generator` to produce a custom 404 page via the app's fallback route
- **Phase 9: Verify internal links** — runs `T:Penn.Infrastructure.LinkVerificationService` (:path `src/Penn/Infrastructure/LinkVerificationService.cs`) across all fetched HTML, checking every internal link against the set of known routes

### Key points
- Phase ordering is critical: HTML before CSS ensures the utility-first CSS generator sees all classes before producing the stylesheet
- All page fetching happens through the running app's HTTP pipeline, so middleware (including `BaseUrlRewritingProcessor`) runs on every response
- Link verification happens post-fetch across the complete set of generated pages, catching cross-page broken links

## Beat 3: Understand the BuildReport

After generation completes, `BuildReportBuilder.Build()` produces a `BuildReport`. Explain every property and what it means for the reader's build.

### What to show
- Reference `T:Penn.Generation.BuildReport` (:path `src/Penn/Generation/BuildReport.cs`)
- `P:Penn.Generation.BuildReport.GeneratedPages` (`ImmutableList<ContentRoute>`) — pages that were successfully fetched and written
- `P:Penn.Generation.BuildReport.SkippedPages` (`ImmutableList<ContentRoute>`) — pages skipped (e.g., drafts). Note: in the current implementation, drafts are filtered during `DiscoverAsync` before reaching the generation phase, so this list is typically empty. The property exists for future use or custom `IContentService` implementations that call `BuildReportBuilder.AddSkippedPage`
- `P:Penn.Generation.BuildReport.FailedPages` (`ImmutableList<ContentRoute>`) — pages that returned non-success HTTP status codes or threw exceptions during fetch
- `P:Penn.Generation.BuildReport.Diagnostics` (`ImmutableList<BuildDiagnostic>`) — all info, warning, and error diagnostics collected during the build
- `P:Penn.Generation.BuildReport.BrokenLinks` (`ImmutableList<BrokenLink>`) — internal links that point to pages not in the generated set
- `P:Penn.Generation.BuildReport.Duration` (`TimeSpan`) — total build time
- `P:Penn.Generation.BuildReport.HasErrors` — computed property: true when any diagnostic has `DiagnosticSeverity.Error`, or `BrokenLinks.Count > 0`, or `FailedPages.Count > 0`
- `P:Penn.Generation.BuildReport.TotalPages` — computed: `GeneratedPages.Count + SkippedPages.Count + FailedPages.Count`
- Reference `T:Penn.Generation.BuildDiagnostic` (:path `src/Penn/Generation/BuildDiagnostic.cs`) — record with `Severity` (`T:Penn.Diagnostics.DiagnosticSeverity`: `Info`, `Warning`, `Error`), optional `Route`, `Message`, optional `Exception`, optional `SourceFile`
- Reference `T:Penn.Generation.BrokenLink` (:path `src/Penn/Generation/BrokenLink.cs`) — record with `SourcePage` (the page containing the link), `Url` (the broken target), `Type` (`T:Penn.Generation.LinkType`: `Internal`, `External`, `Anchor`, `Image`), and `Reason`

### Key points
- `HasErrors` is the single boolean that determines the build's pass/fail status
- Broken links are classified as errors (they contribute to `HasErrors`)
- Diagnostics can also arrive from per-request response headers (`X-Penn-Diagnostic`), which individual middleware or renderers can emit during page generation

## Beat 4: Read the Console Output

Show what `BuildReport.WriteTo` prints to the console and how to interpret each section.

### What to show
- Reference `M:Penn.Generation.BuildReport.WriteTo(System.IO.TextWriter)` (:path `src/Penn/Generation/BuildReport.cs`)
- The summary line: `Build Complete — {TotalPages} pages in {Duration}s` followed by counts for generated, skipped (draft), and failed pages, plus warning count
- The ERRORS section: lists each error diagnostic with its route's `CanonicalPath`, the message, and the source file path
- The WARNINGS section: lists warning diagnostics and broken links. Broken links show the format: `{SourcePage.CanonicalPath} links to {Url} ({Reason})`
- Show a sample output with 12 pages generated, 1 draft skipped, 1 warning for a broken xref

### Key points
- The summary line always prints, even on success
- Errors are listed before warnings
- `M:Penn.Generation.BuildReport.ToFormattedString` returns the same output as a string (useful for logging or artifact capture)

## Beat 5: Diagnose Common Build Issues

Walk through the most common diagnostics and how to fix them.

### What to show
- **Broken internal link**: the `BrokenLink` record shows `SourcePage` and `Url`. Fix: correct the link target or create the missing page
- **Broken xref**: appears as a warning diagnostic with message referencing the unresolved UID and source file. Fix: check the `uid` in the target page's front matter
- **Failed page (HTTP error)**: appears in `FailedPages` with an error diagnostic like `"HTTP 500 fetching /some-page/"`. Fix: run the dev server and navigate to that page to see the full error
- **CSS class not generating**: a utility class used only in JavaScript or a component parameter won't be seen by the CSS class collector. Fix: add the class to a `ContentPaths` configuration or use it in markup
- **Static asset copy failure**: appears as a warning with the source and target paths. Fix: check file permissions or path length limits

### Key points
- The build report always includes the source file path when available, making it easy to locate the problem
- Per-request diagnostics (from the `X-Penn-Diagnostic` response header) can surface renderer-specific warnings that only appear during build

## Beat 6: CI Exit Codes and Build Failure

Show how `RunOrBuildAsync` translates `BuildReport.HasErrors` into a process exit code, and how this integrates with CI systems.

### What to show
- Reference `M:Penn.Infrastructure.PennExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])` (:path `src/Penn/Infrastructure/PennExtensions.cs`) — after `GenerateAsync` returns, it calls `report.WriteTo(Console.Out)`, then checks `P:Penn.Generation.BuildReport.HasErrors`: if true, sets `Environment.ExitCode = 1`
- Show that `HasErrors` is true when:
  - Any `BuildDiagnostic` has `DiagnosticSeverity.Error`
  - `BrokenLinks.Count > 0`
  - `FailedPages.Count > 0`
- Warnings alone do **not** cause a non-zero exit code
- Show a GitHub Actions workflow step: `dotnet run --project docs/Beacon.Docs -- build / ./output` — if the exit code is 1, the step fails and the workflow stops

### Key points
- Broken links are treated as errors, not warnings, for exit code purposes
- To fail on warnings too, the reader would need to inspect the `BuildReport` programmatically after `GenerateAsync` returns and set `Environment.ExitCode` based on custom criteria
- The build report is written to stdout; capture it with shell redirection or use `M:Penn.Generation.BuildReport.ToFormattedString` for programmatic access

## Beat 7: Capture the Build Report as a CI Artifact

Show how to persist the build output and report for debugging failed builds in CI.

### What to show
- A GitHub Actions workflow that:
  - Runs the build: `dotnet run --project docs/Beacon.Docs -- build / ./output`
  - Uploads `./output` as a build artifact (useful for inspecting generated HTML)
  - Optionally redirects build output to a file for artifact upload: `dotnet run ... 2>&1 | tee build-report.txt`
- Reference `T:Penn.Generation.BuildReportBuilder` (:path `src/Penn/Generation/BuildReportBuilder.cs`) — the builder that accumulates diagnostics during generation. Methods: `M:Penn.Generation.BuildReportBuilder.AddWarning(Penn.Routing.ContentRoute,System.String)`, `M:Penn.Generation.BuildReportBuilder.AddError(Penn.Routing.ContentRoute,System.String,System.Exception)`, `M:Penn.Generation.BuildReportBuilder.AddBrokenLink(Penn.Generation.BrokenLink)`, `M:Penn.Generation.BuildReportBuilder.AddGeneratedPage(Penn.Routing.ContentRoute)`, `M:Penn.Generation.BuildReportBuilder.AddSkippedPage(Penn.Routing.ContentRoute)`, `M:Penn.Generation.BuildReportBuilder.Build`

### Key points
- The output directory itself is the most useful artifact — you can open generated HTML files to inspect rewritten links, check the search index, and verify the sitemap
- The `BuildReportBuilder` is internal to the generation process; the reader interacts with the final `BuildReport` returned by `GenerateAsync`
- For advanced CI pipelines, parse the console output or use `ToFormattedString` to include the report text in a PR comment or Slack notification

## Beat 8: Priority Ordering — Why HTML Before CSS Matters

Explain the deliberate ordering of Phase 6 (HTML content) before Phase 7 (MapGet routes) and why it matters for utility-first CSS generation.

### What to show
- Reference the doc comment on `T:Penn.Generation.OutputGenerationService` (:path `src/Penn/Generation/OutputGenerationService.cs`): "Pages are fetched in priority order: HTML content first, then MapGet routes (like /styles.css) last. This ensures CSS class collectors have observed all HTML before the stylesheet is generated."
- Phase 6 fetches all content pages in parallel via `FetchPagesAsync`
- Phase 7 fetches MapGet routes (including `/styles.css`) **after** Phase 6 completes
- The MonorailCSS integration collects CSS classes as HTML is rendered; the `/styles.css` endpoint then produces a stylesheet containing only the classes actually used

### Key points
- If CSS were generated before HTML, utility classes referenced in markdown content or Razor components would be missing from the stylesheet
- This ordering is automatic — the reader doesn't need to configure it, but understanding it explains why `/styles.css` is always generated last
- The search index (`/search-index.json`) and sitemap (`/sitemap.xml`) are also MapGet routes fetched in Phase 7, though their ordering relative to CSS doesn't matter
