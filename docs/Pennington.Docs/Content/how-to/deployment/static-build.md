---
title: "Build a static site"
description: "Run the build verb, let OutputGenerationService crawl the running host, and read the BuildReport for broken links and failed pages."
section: "deployment"
order: 10
tags: []
uid: how-to.deployment.static-build
isDraft: true
search: false
llms: false
---

> **In this page.** Running the app with `build [baseUrl] [outputDirectory]`, what `OutputGenerationService` crawls, and reading the `BuildReport` for broken links and failed pages.
>
> **Not in this page.** Platform-specific upload steps — see [Deploy to GitHub Pages](/how-to/deployment/github-pages) and [Adapt the deploy workflow for other hosts](/how-to/deployment/adapt-for-other-hosts).

## When to use this

- You have a working Pennington site running under `dotnet run` and need to produce a directory of static files for hosting.
- Use this page before any host-specific deployment recipe (GitHub Pages, Netlify, S3, etc.) — those pages assume the output directory already exists.

## Assumptions

- You have an existing Pennington site whose `Program.cs` ends with `await app.RunOrBuildAsync(args)`, `await app.RunDocSiteAsync(args)`, or `await app.RunBlogSiteAsync(args)`.
- You can run `dotnet run --project <yourSite>` and the site serves locally without errors.
- You understand that every page is produced by a real HTTP round-trip against the running host, so whatever works in dev is what ships.

To copy a working setup, see [`examples/MinimalExample`](https://github.com/usepennington/pennington/tree/main/examples/MinimalExample). Do not walk through the whole example — this page is a recipe, not a tour.

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

Positional arguments after `--`:

- First token must be `build` (case-insensitive). Anything else is a no-op so `dotnet test` and `dotnet watch` don't misread positional args.
- Second token is `BaseUrl` (default `/`). Use a sub-path like `/pennington/` when hosting under a prefix.
- Third token is `OutputDirectory` (default `output`).

### 3. Supply a base URL and output directory when needed

```shell
dotnet run --project <yourSite> -- build /pennington/ ./dist
```

The same invocation under `dotnet watch` or tests would be ignored — the `build` sentinel in `args[0]` is what flips the host into generate mode.

### 4. Understand what the crawler does

`RunOrBuildAsync` starts the full ASP.NET host exactly as in `dotnet run`, then `OutputGenerationService` crawls it:

- Discovers every content page and `MapGet` route.
- Cleans the output directory (when `CleanOutput` is true), copies content assets and static web assets.
- Fetches every HTML page over HTTP in parallel, then fetches asset routes (e.g. `/styles.css`) last so CSS class collection sees every page first.
- Fetches `/__pennington-404-generator` to materialize `404.html`.
- Verifies internal links across every generated page.

Dev serve and build share one pipeline — whatever works in `dotnet run` is what ships. See [Unified dev and build path](/explanation/core/dev-vs-build) for the why.

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
- `N broken links found` — an internal `href` didn't match any generated route or copied asset.
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

- How-to: [Deploy to GitHub Pages](/how-to/deployment/github-pages)
- How-to: [Adapt the deploy workflow for other hosts](/how-to/deployment/adapt-for-other-hosts)
- Reference: [`OutputOptions` and CLI arguments](/reference/options/auxiliary-options)
- Reference: [Build report fields](/reference/diagnostics/build-report)
- Background: [Dev mode and build mode share one code path](/explanation/core/dev-vs-build)
