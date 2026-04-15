---
title: "CLI and build arguments"
description: "The argument and environment-variable surface for RunOrBuildAsync — positional args, named flags, and the variables consulted when the host boots."
sectionLabel: "Host Integration"
order: 406020
tags: [host, cli, build, arguments]
uid: reference.host.cli
---

The command-line surface `RunOrBuildAsync` dispatches on — one positional verb (`build`) followed by an optional base URL and output directory, or equivalent `--base-url` / `--output` named flags. Parsed by `Pennington.Generation.OutputOptions.FromArgs` in `src/Pennington/Generation/OutputOptions.cs` and consumed by `OutputGenerationService.GenerateAsync`; any invocation whose first argument is not `build` falls through to `app.RunAsync()` with default `OutputOptions`.

## Commands

| Command | Arguments | Effect |
|---|---|---|
| _(none)_ | — | Dev-serve: `app.RunAsync()`. `OutputOptions.FromArgs` returns defaults (`BaseUrl = "/"`, `OutputDirectory = "output"`) but the crawler never runs. |
| `build` | `[baseUrl] [outputDirectory]` positional, or `--base-url` / `--output` named flags | Static build: `app.StartAsync()`, resolve `OutputGenerationService`, HTTP-crawl the running host, write each response to `OutputOptions.OutputDirectory`, print `BuildReport`, set `Environment.ExitCode = 1` on errors, `app.StopAsync()`. |
| _anything else_ | — | Dev-serve fallback. Non-`build` `args[0]` is treated as unknown; positional args are not interpreted as a base URL or output directory (guards against `dotnet test` / `dotnet watch` emitting stray positional args). |

## Positional arguments

| Position | Name | Default | Description |
|---|---|---|---|
| `args[1]` | `baseUrl` | `/` | The URL sub-path the site will be served from; materialized as `OutputOptions.BaseUrl` (a `UrlPath`). Promoted to `args[2]`'s slot if `--base-url` was already supplied. |
| `args[2]` | `outputDirectory` | `output` | The filesystem directory to write the generated site into; materialized as `OutputOptions.OutputDirectory` (a `FilePath`). Promoted if `--output` was already supplied. |

## Named flags

| Flag | Value form | Maps to | Notes |
|---|---|---|---|
| `--base-url` | `--base-url /sub` or `--base-url=/sub` | `OutputOptions.BaseUrl` | Read by `TryReadFlag` in `OutputOptions.FromArgs`. Case-insensitive match. |
| `--output` | `--output dist` or `--output=dist` | `OutputOptions.OutputDirectory` | Read by `TryReadFlag`. Case-insensitive match. |

## Environment variables

| Variable | Consumer | Effect when set |
|---|---|---|
| `DOTNET_WATCH` | `LiveReloadServer`, `LiveReloadScriptProcessor`, `DiagnosticOverlayProcessor`, `LiveReloadExtensions.UsePenningtonLiveReload` | Enables the `/__pennington/reload` WebSocket, injects the reconnection script before `</body>`, renders the dev-mode diagnostic overlay. Unset at build and in production → every live-reload code path is a no-op. Set automatically by `dotnet watch`. |
| `ASPNETCORE_URLS` | ASP.NET Core host | Standard ASP.NET binding. `RunOrBuildAsync` resolves `app.Urls.First()` after `StartAsync`, falling back to `http://localhost:5000` only when `app.Urls` is empty, so overriding this variable moves the crawler target. |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core host | Standard environment selection; Pennington itself does not branch on it, but downstream templates (DocSite/BlogSite) may read `app.Environment.IsDevelopment()`. |

## Exit codes

- `0` — `build` completed without errors (`BuildReport.HasErrors == false`), or dev-serve exited cleanly.
- `1` — `build` completed but the `BuildReport` contains at least one error diagnostic or failed page (`BuildReport.HasErrors == true`). Set explicitly by `RunOrBuildAsync` after writing the report.

## Example

```csharp:xmldocid,bodyonly
M:GettingStartedMinimalSiteExample.Stage3.Run(System.String[])
```

`dotnet run` against this host serves live; `dotnet run -- build` generates to `./output/` at base URL `/`; `dotnet run -- build /sub dist` or `dotnet run -- build --base-url=/sub --output=dist` generates to `./dist/` at base URL `/sub`.

## See also

- Related reference: [DI and middleware extension methods](xref:reference.host.extensions)
- Related reference: [Build report fields](xref:reference.diagnostics.build-report)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
