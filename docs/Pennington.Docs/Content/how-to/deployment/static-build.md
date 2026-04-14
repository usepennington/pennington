---
title: "Build a static site"
description: "Produce a deployable `output/` directory by running the same app in build mode and reading the `BuildReport` for failures."
uid: how-to.deployment.static-build
order: 10
sectionLabel: Publishing & Deployment
tags: [build, deployment, output-generation, build-report]
---

> **In this page.** _Paraphrase TOC "Covers": invoking the host with `build [baseUrl] [outputDirectory]`, what the crawler-based `OutputGenerationService` actually does, and how to read the `BuildReport` it prints to stdout. Two sentences max._
>
> **Not in this page.** _Paraphrase TOC "Does not cover": platform-specific upload steps live on the per-host how-to pages — link out to [Deploy to GitHub Pages](xref:how-to.deployment.github-pages) as the canonical next step and [Host under a sub-path (base URL)](xref:how-to.deployment.base-url) for non-root deployments._

## When to use this

_Two sentences. Frame the arrival state: reader has a working Pennington site running under `dotnet run` and now needs a folder of static HTML to hand to a static host. Emphasize that there is no separate "build project" — the same `Program.cs` that serves live produces the output by crawling itself over HTTP, so everything the reader sees locally is exactly what lands on disk._

## Assumptions

_Three bullets. Keep prerequisites minimal — if the list grows, the page is a tutorial._

- You have a working Pennington site that serves under `dotnet run` (see [Create your first Pennington site](xref:tutorials.getting-started.first-site) if not)
- Your host composes `RunOrBuildAsync` (directly, or via `RunDocSiteAsync` / `RunBlogSiteAsync`)
- You can write to a local directory — the build deletes and re-creates `output/` by default

To copy a working setup, see [`examples/SubPathDeployableExample`](https://github.com/usepennington/pennington/tree/main/examples/SubPathDeployableExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Five steps. Imperative verbs. Respect the unified dev+build invariant — do not describe a parallel "offline renderer". Anchor every C# fence to a production symbol in `Pennington.Generation` or to the `SubPathDeployableExample` helper shims; use plain fences for CLI invocations._

### 1. Confirm the host calls `RunOrBuildAsync`

_One sentence. `RunOrBuildAsync` is the single switch: no args means dev serve, `build` as the first arg means crawl-and-write. Most apps already route through it via `RunDocSiteAsync` / `RunBlogSiteAsync`; show the minimal `Program.cs` tail so readers can confirm._

```csharp:path
examples/SubPathDeployableExample/Program.cs
```

_Optional one-liner: if you need custom exit-code semantics (for example, failing CI on broken links but not on warnings), replace the call with the `BuildHost.RunOrBuildAsync` body — it is the same switch written out longhand._

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

### 2. Invoke the build verb

_Two sentences. `dotnet run -- build` parses into `OutputOptions` via `FromArgs`; `args[0]` must equal `build` or the call becomes a no-op and you get a dev run instead. Show all three supported argument shapes so the reader picks the one that matches their deployment._

```text
# defaults: BaseUrl = "/", OutputDirectory = "output"
dotnet run -- build

# positional: base URL, then output dir
dotnet run -- build /my-site dist

# named flags (order-independent, preferred for scripts)
dotnet run -- build --base-url=/my-site --output=dist
```

_Show the parser for completeness — this is the single source of truth for the CLI surface:_

```csharp:xmldocid
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

### 3. Understand what the crawler does

_Two sentences. `OutputGenerationService` starts the real ASP.NET host, opens an `HttpClient` against `app.Urls.First()`, and issues a GET for every route discovered by `IContentService.DiscoverAsync` plus every `MapGet` endpoint. Because every page ships through the live response-processor pipeline, xref resolution, locale prefixing, base-URL rewriting, MonorailCSS class collection, and diagnostics all behave identically in dev and build — this is the deliberate invariant, not an accident._

```csharp:xmldocid
T:Pennington.Generation.OutputGenerationService
```

_Cross-link: the deeper reasoning lives in [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build). Do not describe a separate offline renderer; there is none._

### 4. Read the `BuildReport` printed to stdout

_Two sentences. When the crawl finishes, `RunOrBuildAsync` writes a human-readable report and sets a non-zero exit code when `HasErrors` is true (any error diagnostic, failed page, or broken internal link). The interesting collections are `GeneratedPages`, `SkippedPages` (drafts), `FailedPages`, `BrokenLinks`, and `Diagnostics`._

```csharp:xmldocid
T:Pennington.Generation.BuildReport
```

_If CI needs a custom presentation (e.g. GitHub Actions summary), print the report yourself — this wrapper is short enough to inline:_

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)
```

### 5. Fix what the report flags before shipping

_Two sentences. `BrokenLinks` surface internal hrefs that did not resolve to a generated page (usually a typo or a moved file that no xref caught); `FailedPages` surface routes whose parse or render raised, each carrying the originating `ContentRoute` so you can jump to the source. Warnings (unknown xrefs, missing trailing slashes on Razor pages) are advisory — they do not fail the build on their own, but `HasErrors` will flip when a warning is actually a broken link._

```csharp:xmldocid
P:Pennington.Generation.BuildReport.BrokenLinks
P:Pennington.Generation.BuildReport.FailedPages
P:Pennington.Generation.BuildReport.HasErrors
```

---

## Verify

_Terse. Three bullets the reader can eyeball without rereading the steps._

- `dotnet run -- build` exits `0` and `output/index.html` exists — open it in a browser and every internal link resolves
- The stdout report ends with `Build Complete — N pages in Xs`, with `0 failed` and `0 broken links found`
- `output/404.html` exists (the crawler fetches the internal `/__pennington-404-generator` sentinel to materialize it)

## Related

_Three cross-quadrant links. Reference for the CLI surface and the report shape, background for why dev and build share a code path. Do not link the next how-to in this section — generated automatically from `order:`._

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [Build report fields](xref:reference.diagnostics.build-report)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
