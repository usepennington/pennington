---
title: "Build a static site"
description: "Produce a deployable `output/` directory by running the same app in build mode and reading the `BuildReport` for failures."
uid: how-to.deployment.static-build
order: 204010
sectionLabel: Publishing & Deployment
tags: [build, deployment, output-generation, build-report]
---

To turn a working Pennington site running under `dotnet run` into a folder of static HTML for a static host, run the app in build mode. There is no separate build project — the same `Program.cs` that serves the site locally crawls itself over HTTP and writes the result to disk, so the locally tested site is exactly what ships.

For platform-specific upload steps, see <xref:how-to.deployment.github-pages>. For sites hosted under a sub-path, see <xref:how-to.deployment.base-url>.

## Assumptions

- A working Pennington site that serves under `dotnet run` (see <xref:tutorials.getting-started.first-site> if not)
- The host composes `RunOrBuildAsync` (directly, or via `RunDocSiteAsync` / `RunBlogSiteAsync`)
- A writable local directory — the build deletes and re-creates `output/` by default

---

## Steps

<Steps>
<Step StepNumber="1">

**Confirm the host calls `RunOrBuildAsync`**

`RunOrBuildAsync` is the single switch: no arguments means dev serve, `build` as the first argument triggers the crawl-and-write path. Most apps already route through it via `RunDocSiteAsync` or `RunBlogSiteAsync`. The tail of `Program.cs` confirms it.

```csharp:path
examples/SubPathDeployableExample/Program.cs
```

For custom exit-code semantics — for example, failing CI on broken links but not on warnings — replace the call with the explicit switch written out longhand:

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

</Step>
<Step StepNumber="2">

**Invoke the build verb**

Pass `build` as the first argument to `dotnet run`. The argument is parsed into `OutputOptions` via `FromArgs`; without it, the app starts as a dev server instead. Three argument shapes are supported:

```text
# defaults: BaseUrl = "/", OutputDirectory = "output"
dotnet run -- build

# positional: base URL, then output dir
dotnet run -- build /my-site dist

# named flags (order-independent, preferred for scripts)
dotnet run -- build --base-url=/my-site --output=dist
```

`OutputOptions.FromArgs` is the single source of truth for the CLI surface; see <xref:reference.host.cli> for the full grammar.

</Step>
<Step StepNumber="3">

**Understand what the crawler does**

`OutputGenerationService` starts the real ASP.NET host, opens an `HttpClient` against the first bound URL, and issues a GET for every route discovered by `IContentService.DiscoverAsync` plus every `MapGet` endpoint. Every page passes through the live response-processor pipeline — xref resolution, locale prefixing, base-URL rewriting, MonorailCSS class collection, and diagnostics behave identically in dev and build. This is a deliberate invariant — the reasoning is covered in <xref:explanation.core.dev-vs-build>.

</Step>
<Step StepNumber="4">

**Read the `BuildReport` printed to stdout**

When the crawl finishes, `RunOrBuildAsync` writes a human-readable report and exits with a non-zero code when `HasErrors` is true — triggered by any error diagnostic, failed page, or broken internal link. The key collections are `GeneratedPages`, `SkippedPages` (drafts), `FailedPages`, `BrokenLinks`, and `Diagnostics`; see <xref:reference.diagnostics.build-report> for the full field list.

For a custom CI presentation such as a GitHub Actions summary, print the report directly:

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)
```

</Step>
<Step StepNumber="5">

**Fix what the report flags before shipping**

`BrokenLinks` surfaces internal hrefs that did not resolve to a generated page — usually a typo or a moved file that no xref caught. `FailedPages` surfaces routes whose parse or render raised an exception, each carrying the originating `ContentRoute` so the source is easy to locate. Warnings are advisory and do not set `HasErrors` on their own, but a warning that represents a broken link flips the flag.

</Step>
</Steps>

---

## Verify

- `dotnet run -- build` exits `0` and `output/index.html` exists — open it in a browser and every internal link resolves
- The stdout report ends with `Build Complete — N pages in Xs`, with `0 failed` and `0 broken links found`
- `output/404.html` exists (the crawler fetches the internal `/__pennington-404-generator` sentinel to materialize it)

## Related

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [Build report fields](xref:reference.diagnostics.build-report)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
