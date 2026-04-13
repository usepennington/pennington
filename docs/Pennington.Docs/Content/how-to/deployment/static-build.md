---
title: "Build a static site"
description: "Run the build verb, understand the crawler-based OutputGenerationService, and read the BuildReport for broken links and failed pages."
section: "deployment"
order: 10
tags: []
uid: how-to.deployment.static-build
isDraft: true
search: false
llms: false
---

> **In this page.** Running the app with `build [baseUrl] [outputDirectory]`, understanding the crawler-based `OutputGenerationService`, and reading the `BuildReport` for broken links and failed pages.
>
> **Not in this page.** Platform-specific upload steps (see the per-host pages in this section).

## When to use this

- You have a working Pennington site running under `dotnet run` and need to produce a directory of static files for hosting.
- Use this page before any host-specific deployment recipe (GitHub Pages, Netlify, S3, etc.) — those pages assume the output directory already exists.

## Assumptions

- You have an existing Pennington site whose `Program.cs` ends with `await app.RunOrBuildAsync(args)`, `await app.RunDocSiteAsync(args)`, or `await app.RunBlogSiteAsync(args)`.
- You can run `dotnet run --project <yourSite>` and the site serves locally without errors.
- You understand that every page is produced by a real HTTP round-trip against the running host, so whatever works in dev is what ships.

To copy a working setup, see [`examples/MinimalExample`](https://github.com/Phil-Scott-DEV/Pennington/tree/main/examples/MinimalExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Confirm the entry point forwards `args`

The `build` verb is dispatched by `RunOrBuildAsync` (or the `RunDocSiteAsync` / `RunBlogSiteAsync` wrappers). The final line of `Program.cs` must forward the process `args`:

```csharp
await app.RunOrBuildAsync(args);
```

If `args` is not passed through, the host sees no `build` token and falls back to `app.RunAsync()` — the build is silently a no-op dev run.

### 2. Invoke the `build` verb

From the project directory, run:

```shell
dotnet run --project <yourSite> -- build
```

Positional arguments are parsed by `OutputOptions.FromArgs`:

- `args[0]` must equal `build` (case-insensitive). Anything else returns a no-op `OutputOptions` so `dotnet test` and `dotnet watch` don't misread positional args.
- `args[1]` is `BaseUrl` (default `"/"`). Use a sub-path like `/pennington/` when hosting under a prefix.
- `args[2]` is `OutputDirectory` (default `"output"`).

### 3. Supply a base URL and output directory when needed

```shell
dotnet run --project <yourSite> -- build /pennington/ ./dist
```

The same invocation under `dotnet watch` or tests would be ignored — the `build` sentinel in `args[0]` is what flips the host into generate mode.

### 4. Understand what the crawler does

`RunOrBuildAsync` calls `app.StartAsync()` so the full ASP.NET host comes up exactly as in `dotnet run`, then resolves `OutputGenerationService` and calls `GenerateAsync(app.Urls.First())`. Phases executed against the running host:

- Discover content pages via every `IContentService.DiscoverAsync()` and MapGet routes via `EndpointDataSource`.
- Clean the output directory when `CleanOutput` is true, then copy content assets and wwwroot/RCL static web assets.
- HTTP-fetch all HTML content pages in parallel, then MapGet routes (e.g. `/styles.css`) last so CSS class collectors see every HTML page first.
- Fetch the sentinel `/__pennington-404-generator` to materialize `404.html`.
- Run `LinkVerificationService` over every generated HTML page to collect broken internal links.

Because output is produced by real HTTP responses, response processors, `IHtmlResponseRewriter`s, Razor SSR, Markdig extensions, and `MonorailCSS` class collection all run identically to dev serve.

### 5. Read the `BuildReport` written to stdout

`RunOrBuildAsync` calls `report.WriteTo(Console.Out)` after generation. Expect output shaped like:

```
Build Complete — 42 pages in 3.7s
  41 pages generated
  1 pages skipped (draft)
  2 warnings

WARNINGS
  /guides/intro/: 1 broken links found:
    /guides/intro/ links to /guides/missing-page/ (Target route not found)
```

Check for:

- `N pages failed` — fatal render/fetch errors. Each failure is listed under `ERRORS` with source file and message.
- `N broken links found` — `LinkVerificationService` couldn't match an internal `href` to any generated route or copied asset.
- `N pages skipped (draft)` — pages whose front matter has `isDraft: true`; this is informational.

### 6. Check the process exit code in CI

When `BuildReport.HasErrors` is true (any `Error`-severity diagnostic, any broken link, or any failed page), `RunOrBuildAsync` sets `Environment.ExitCode = 1`. A CI pipeline can gate deployment on the exit code directly:

```shell
dotnet run --project <yourSite> -- build https://example.com/ ./dist
```

A non-zero exit means the build artifact is unsafe to publish.

---

## Verify

- The output directory exists and contains `index.html`, `404.html`, and `styles.css` (or whatever stylesheet path you configured).
- The stdout `Build Complete` line reports the expected page count, zero failed pages, and zero broken links.
- `echo $?` (bash) / `$LASTEXITCODE` (PowerShell) is `0`.

## Related

- Reference: [OutputOptions and CLI arguments](/reference/generation/output-options/)
- Reference: [BuildReport fields](/reference/generation/build-report/)
- Background: [Why dev-serve and build share one HTTP path](/explanation/architecture/unified-build-pipeline/)
