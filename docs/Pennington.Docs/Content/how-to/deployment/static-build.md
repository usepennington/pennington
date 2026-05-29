---
title: "Build a static site"
description: "Produce a deployable `output/` directory by running the same app in build mode and reading the `BuildReport` for failures."
uid: how-to.deployment.static-build
order: 1
sectionLabel: "Publishing & Deployment"
tags: [build, deployment, output-generation, build-report]
---

To turn a working Pennington site into a folder of static HTML for a static host, run the app in build mode. For why the same `Program.cs` works in both dev and build, see <xref:explanation.core.dev-vs-build>; for platform-specific upload steps, see <xref:how-to.deployment.github-pages>; for sub-path sites, see <xref:how-to.deployment.base-url>.

## Before you begin
- A working Pennington site that serves under `dotnet run` (see <xref:tutorials.getting-started.first-site> if not).
- The host composes `RunOrBuildAsync` directly or via `RunDocSiteAsync` / `RunBlogSiteAsync` (most apps do — confirm `Program.cs` ends with one of those calls).
- A writable local directory — the build deletes and re-creates `output/` by default.

---

## Steps

<Steps>
<Step StepNumber="1">

**Invoke the build verb**

Pass `build` as the first argument to `dotnet run`. The argument is parsed into `OutputOptions` via `FromArgs`; without it, the app starts as a dev server instead. Three argument shapes are supported:

```bash
# defaults: BaseUrl = "/", OutputDirectory = "output"
dotnet run -- build

# positional: base URL, then output dir
dotnet run -- build /my-site dist

# named flags (order-independent, preferred for scripts)
dotnet run -- build --base-url=/my-site --output=dist
```

See <xref:reference.host.cli> for the full grammar.

</Step>
<Step StepNumber="2">

**Read the `BuildReport` printed to stdout**

When the crawl finishes, `RunOrBuildAsync` writes a human-readable report and exits with a non-zero code when `HasErrors` is true. The key collections are `GeneratedPages`, `SkippedPages` (drafts), `FailedPages`, `BrokenLinks`, and `Diagnostics`; see <xref:reference.api.build-report> for the full field list. `BrokenLinks` and `FailedPages` are what blocks shipping; fix the listed routes before deploying.

For custom CI presentation (a GitHub Actions summary, a Slack message), use `BuildHost.PrintBuildReport` in `examples/SubPathDeployableExample/BuildHost.cs` as a starting point.

</Step>
</Steps>

---

## Verify

- `dotnet run -- build` exits `0` and `output/index.html` exists — open it in a browser and every internal link resolves
- The stdout report opens with `Build Complete — N pages in Xs` followed by `N pages generated`, shows no `ERRORS` or `WARNINGS` section, and the process exits `0`
- `output/404.html` exists (the crawler fetches the internal `/__pennington-404-generator` sentinel to materialize it)

## Related

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [Build report fields](xref:reference.api.build-report)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
