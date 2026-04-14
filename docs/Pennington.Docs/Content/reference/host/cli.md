---
title: "CLI and build arguments"
description: "The argument and environment-variable surface for RunOrBuildAsync — positional args, named flags, and the variables consulted when the host boots."
sectionLabel: "Host Integration"
order: 20
tags: [host, cli, build, arguments]
uid: reference.host.cli
---

> **In this page.** The `build [baseUrl] [outputDirectory]` argument shape, the `--base-url` / `--output` named flags, dev-mode behavior when `args[0]` is anything other than `build`, and environment variables Pennington consults at runtime (notably `DOTNET_WATCH`).
>
> **Not in this page.** Platform-specific deployment mechanics — see the how-tos under `/how-to/deployment/*` (GitHub Pages, self-host, sub-path hosting, adaptation sheet).

## Summary

The command-line surface `RunOrBuildAsync` dispatches on — one positional verb (`build`) followed by an optional base URL and output directory, or equivalent `--base-url` / `--output` named flags. Parsed by `Pennington.Generation.OutputOptions.FromArgs` in `src/Pennington/Generation/OutputOptions.cs` and consumed by `OutputGenerationService.GenerateAsync`; any invocation whose first argument is not `build` falls through to `app.RunAsync()` with default `OutputOptions`.

## Commands

_The dispatch table — which `args[0]` value triggers which code path inside `PenningtonExtensions.RunOrBuildAsync`._

| Command | Arguments | Effect |
|---|---|---|
| _(none)_ | — | Dev-serve: `app.RunAsync()`. `OutputOptions.FromArgs` returns defaults (`BaseUrl = "/"`, `OutputDirectory = "output"`) but the crawler never runs. |
| `build` | `[baseUrl] [outputDirectory]` positional, or `--base-url` / `--output` named flags | Static build: `app.StartAsync()`, resolve `OutputGenerationService`, HTTP-crawl the running host, write each response to `OutputOptions.OutputDirectory`, print `BuildReport`, set `Environment.ExitCode = 1` on errors, `app.StopAsync()`. |
| _anything else_ | — | Dev-serve fallback. Non-`build` `args[0]` is treated as unknown; positional args are not interpreted as a base URL or output directory (guards against `dotnet test` / `dotnet watch` emitting stray positional args). |

## Declaration

```csharp:xmldocid
M:Pennington.Generation.OutputOptions.FromArgs(System.String[])
```

_The single source of truth for CLI parsing. `RunOrBuildAsync` receives the same `args` array and checks only `args[0] == "build"`; all other interpretation of `args[1..]` happens here._

## Positional arguments

_Applied only when `args[0].Equals("build", OrdinalIgnoreCase)`. Named flags take precedence; the first positional fills the slot corresponding to the first unset flag, the second fills the next unset slot._

| Position | Name | Default | Description |
|---|---|---|---|
| `args[1]` | `baseUrl` | `/` | The URL sub-path the site will be served from; materialized as `OutputOptions.BaseUrl` (a `UrlPath`). Promoted to `args[2]`'s slot if `--base-url` was already supplied. |
| `args[2]` | `outputDirectory` | `output` | The filesystem directory to write the generated site into; materialized as `OutputOptions.OutputDirectory` (a `FilePath`). Promoted if `--output` was already supplied. |

## Named flags

_Equivalent to positionals; may be space- or `=`-joined. When a flag and its positional counterpart both appear, the flag wins. Unknown flags are ignored (matches existing silence on stray dev-mode args like `--urls=…`)._

| Flag | Value form | Maps to | Notes |
|---|---|---|---|
| `--base-url` | `--base-url /sub` or `--base-url=/sub` | `OutputOptions.BaseUrl` | Read by `TryReadFlag` in `OutputOptions.FromArgs`. Case-insensitive match. |
| `--output` | `--output dist` or `--output=dist` | `OutputOptions.OutputDirectory` | Read by `TryReadFlag`. Case-insensitive match. |

## Environment variables

_Variables the Pennington runtime reads at startup. None are set by Pennington itself; all are consulted via `Environment.GetEnvironmentVariable`._

| Variable | Consumer | Effect when set |
|---|---|---|
| `DOTNET_WATCH` | `LiveReloadServer`, `LiveReloadScriptProcessor`, `DiagnosticOverlayProcessor`, `LiveReloadExtensions.UsePenningtonLiveReload` | Enables the `/__pennington/reload` WebSocket, injects the reconnection script before `</body>`, renders the dev-mode diagnostic overlay. Unset at build and in production → every live-reload code path is a no-op. Set automatically by `dotnet watch`. |
| `ASPNETCORE_URLS` | ASP.NET Core host | Standard ASP.NET binding. `RunOrBuildAsync` resolves `app.Urls.First()` after `StartAsync`, falling back to `http://localhost:5000` only when `app.Urls` is empty, so overriding this variable moves the crawler target. |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core host | Standard environment selection; Pennington itself does not branch on it, but downstream templates (DocSite/BlogSite) may read `app.Environment.IsDevelopment()`. |

## Exit codes

_Set on `Environment.ExitCode` after `BuildReport.WriteTo(Console.Out)`. Dev-serve invocations return whatever `app.RunAsync()` yields._

- `0` — `build` completed without errors (`BuildReport.HasErrors == false`), or dev-serve exited cleanly.
- `1` — `build` completed but the `BuildReport` contains at least one error diagnostic or failed page (`BuildReport.HasErrors == true`). Set explicitly by `RunOrBuildAsync` after writing the report.

## Example

_A minimal host that defers to `RunOrBuildAsync`, exercising every branch described above from one `args` array._

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
