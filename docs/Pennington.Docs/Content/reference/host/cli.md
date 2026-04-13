---
title: "CLI and build arguments"
description: "The build [baseUrl] [outputDirectory] positional arguments, dev-mode behavior, and environment variables consulted."
section: "host"
order: 20
tags: []
uid: reference.host.cli
isDraft: true
search: false
llms: false
---

> **In this page.** The `build [baseUrl] [outputDirectory]` arguments, dev-mode behavior, and environment variables consulted (e.g., `DOTNET_WATCH`).
>
> **Not in this page.** Platform-specific deployment (see How-Tos).

## Summary

- One sentence: what it is. The CLI surface Pennington recognises when a host calls `RunOrBuildAsync(args)` — one verb (`build`) with two positional arguments, plus a small set of runtime environment variables consulted by middleware.
- One sentence: where it lives. `Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync` dispatches on `args[0]`; positional argument parsing lives in `Pennington.Generation.OutputOptions.FromArgs`.

## Declaration

- `RunOrBuildAsync`:

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

- `OutputOptions.FromArgs`:

```csharp:xmldocid
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

## Arguments

- Verb detection is case-insensitive on `args[0]`; any value other than `build` falls through to `app.RunAsync()` (dev serve).
- When the verb is not `build`, positional args are ignored — `OutputOptions.FromArgs` returns defaults so test runners and `dotnet watch` flags cannot be misread as a base URL.
- Positional order is fixed; there are no named flags, no `--help`, no `--version`.

| Position | Name | Type | Default | Description |
|---|---|---|---|---|
| `args[0]` | verb | `string` | — | Must equal `build` (case-insensitive) to trigger static generation; any other value (or empty) runs the host in dev serve mode. |
| `args[1]` | `BaseUrl` | `UrlPath` | `/` | Prefix applied to internal links by `BaseUrlHtmlRewriter`; written into `OutputOptions.BaseUrl`. |
| `args[2]` | `OutputDirectory` | `FilePath` | `output` | Filesystem directory that receives crawled pages and assets; written into `OutputOptions.OutputDirectory`. |

## Dev-mode behaviour

- `args.Length == 0` or `args[0] != "build"` → `app.RunAsync()`; no crawl, no output directory write.
- `StaticWebAssetsLoader.UseStaticWebAssets` runs in both modes before the branch.
- `OutputOptions` is still registered in DI during dev serve (with `OutputDirectory = "output"`, `BaseUrl = "/"`) so components that depend on it resolve, but nothing is written to disk.
- `args[0] == "build"` → `app.StartAsync()` → `OutputGenerationService.GenerateAsync(app.Urls.First())` → `app.StopAsync()` → `BuildReport.WriteTo(Console.Out)`; if `report.HasErrors`, sets `Environment.ExitCode = 1`.
- Host address for the build crawl comes from `app.Urls`; if the collection is empty it falls back to `http://localhost:5000`.

## Environment variables consulted

- Pennington itself reads exactly one environment variable at runtime: `DOTNET_WATCH`. Standard ASP.NET variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`, etc.) are consumed by the host — not by Pennington code — and are out of scope for this page.

| Name | Read by | Effect when set (non-empty) |
|---|---|---|
| `DOTNET_WATCH` | `LiveReloadServer.UsePenningtonLiveReload` | Maps the `/__pennington/reload` WebSocket endpoint; otherwise the call is a no-op. |
| `DOTNET_WATCH` | `LiveReloadScriptProcessor` (`IResponseProcessor`, `Order = 20`) | Injects the reconnection script before `</body>` in HTML responses. |
| `DOTNET_WATCH` | `DiagnosticOverlayProcessor` (`IResponseProcessor`, `Order = 30`) | Injects the diagnostic overlay into HTML responses. |

## Exit codes

| Code | Condition |
|---|---|
| `0` | `build` completed and `BuildReport.HasErrors` is `false`; or dev serve exited normally. |
| `1` | `build` completed and `BuildReport.HasErrors` is `true` (set via `Environment.ExitCode`). |

## Example

```csharp:xmldocid,bodyonly
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

Invocation shapes accepted by `RunOrBuildAsync`:

- `dotnet run --project docs/Pennington.Docs` — dev serve.
- `dotnet run --project docs/Pennington.Docs -- build` — build into `./output` with base URL `/`.
- `dotnet run --project docs/Pennington.Docs -- build /docs/` — build into `./output` with base URL `/docs/`.
- `dotnet run --project docs/Pennington.Docs -- build /docs/ dist` — build into `./dist` with base URL `/docs/`.

## See also

- Related reference: [`OutputOptions`](/reference/options/auxiliary-options)
- Related reference: [Host integration extensions](/reference/host/extensions)
- Background: [Unified dev-and-build code path](/explanation/unified-build-path)
