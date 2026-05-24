---
title: "CLI and build arguments"
description: "The argument and environment-variable surface for RunOrBuildAsync — positional args, named flags, and the variables consulted when the host boots."
sectionLabel: "Host Integration"
order: 406020
tags: [host, cli, build, arguments]
uid: reference.host.cli
---

The command-line surface that `RunOrBuildAsync` dispatches on: one positional verb (`build`) followed by an optional base URL and output directory, or the equivalent `--base-url` / `--output` named flags. Parsing lives in `OutputOptions.FromArgs`; any invocation whose first argument is not `build` falls through to `app.RunAsync()` with default `OutputOptions`.

## Commands

| Command | Arguments | Effect |
|---|---|---|
| _(none)_ | — | Dev-serve. |
| `build` | `[baseUrl] [outputDirectory]` positional, or `--base-url` / `--output` named flags | Static build; writes to `OutputOptions.OutputDirectory`, prints `BuildReport`, sets `Environment.ExitCode = 1` when the report has errors. |
| _anything else_ | — | Dev-serve. Non-`build` `args[0]` is treated as unknown; positional args are not interpreted as base URL or output directory. |

## Positional arguments

| Position | Name | Default | Description |
|---|---|---|---|
| `args[1]` | `baseUrl` | `/` | The URL sub-path the site will be served from; materialized as `OutputOptions.BaseUrl` (a `UrlPath`). When `--base-url` is supplied, the parser advances past it and reads this argument from `args[2]` instead. |
| `args[2]` | `outputDirectory` | `output` | The filesystem directory to write the generated site into; materialized as `OutputOptions.OutputDirectory` (a `FilePath`). When `--output` is supplied, the parser advances past it. |

## Named flags

| Flag | Value form | Maps to | Notes |
|---|---|---|---|
| `--base-url` | `--base-url /sub` or `--base-url=/sub` | `OutputOptions.BaseUrl` | Read by `TryReadFlag` in `OutputOptions.FromArgs`. Case-insensitive match. |
| `--output` | `--output dist` or `--output=dist` | `OutputOptions.OutputDirectory` | Read by `TryReadFlag`. Case-insensitive match. |

## Environment variables

| Variable | Consumer | Effect when set |
|---|---|---|
| `ASPNETCORE_URLS` | ASP.NET Core host | Standard ASP.NET binding. `RunOrBuildAsync` resolves `app.Urls.First()` after `StartAsync`, falling back to `http://localhost:5000` only when `app.Urls` is empty. |

`ASPNETCORE_ENVIRONMENT` has no Pennington-specific effect: dev tooling (live reload, diagnostic overlay) gates on the `build` command-line argument, not on this variable.

## Listening port

Pennington uses the standard ASP.NET Core host port-binding mechanisms — `--urls`, `ASPNETCORE_URLS`, or `launchSettings.json`. The library adds middleware and endpoints on top of whatever URL Kestrel is told to listen on.

## Exit codes

- `0` — `build` completed without errors (`BuildReport.HasErrors == false`), or dev-serve exited cleanly.
- `1` — `build` completed but the `BuildReport` contains at least one error diagnostic or failed page (`BuildReport.HasErrors == true`). Set explicitly by `RunOrBuildAsync` after writing the report.

## Example

```csharp:symbol,bodyonly
examples/GettingStartedMinimalSiteExample/Stage3_UsePennington.cs > Stage3.Run
```

`dotnet run` against this host serves live; `dotnet run -- build` generates to `./output/` at base URL `/`; `dotnet run -- build /sub dist` or `dotnet run -- build --base-url=/sub --output=dist` generates to `./dist/` at base URL `/sub`.

## See also

- Related reference: [DI and middleware extension methods](xref:reference.host.extensions)
- Related reference: [Build report fields](xref:reference.api.build-report)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- How-to: [Host under a sub-path (base URL)](xref:how-to.deployment.base-url)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
