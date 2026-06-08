---
title: "CLI and build arguments"
description: "The argument and environment-variable surface for RunOrBuildAsync — positional args, named flags, and the variables consulted when the host boots."
sectionLabel: "Host Integration"
order: 2
tags: [host, cli, build, arguments]
uid: reference.host.cli
---

The command-line arguments and environment variables `RunOrBuildAsync` dispatches on. It builds a System.CommandLine root command with two verbs — `build` (generate the static site) and `diag` (read-only inspection) — and dev-serves when neither verb is present. `--help`, `-h`, `-?`, and `--version` print and exit without booting the host. Build arguments — an optional base URL and output directory, or the equivalent `--base-url` / `--output` named flags — are parsed by `OutputOptions.FromArgs`.

## Commands

| Command | Arguments | Effect |
|---|---|---|
| _(none)_ | — | Dev-serve. |
| `build` | `[baseUrl] [outputDirectory]` positional, or `--base-url` / `--output` named flags | Static build; writes to `OutputOptions.OutputDirectory`, prints `BuildReport`, sets `Environment.ExitCode = 1` when the report has errors. |
| `diag <subcommand>` | one of `info`, `toc`, `routes`, `warnings`, `translation`, `frontmatter`, `llms`, `standard-site` | Read-only inspection. Runs the host headless (in-process, no socket bind), writes text to stdout, and exits. `diag --help` lists the subcommands. |
| `--help` / `--version` | — | Print usage (or the package version) and exit without serving. `-h` and `-?` are help aliases. |
| _anything else_ | — | Dev-serve. An unrecognized `args[0]` is not interpreted as `build`/`diag` arguments. |

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
| `ASPNETCORE_URLS` | ASP.NET Core host | Standard ASP.NET binding for dev-serve. Build mode replaces Kestrel with `TestServer` at service-registration time, so this variable has no effect under `build`. |

`ASPNETCORE_ENVIRONMENT` has no Pennington-specific effect: dev tooling (live reload, diagnostic overlay) gates on the `build` command-line argument, not on this variable.

## Listening port

In dev mode, Pennington uses the standard ASP.NET Core host port-binding mechanisms — `--urls`, `ASPNETCORE_URLS`, or `launchSettings.json` — and the library adds middleware and endpoints on top of whatever URL Kestrel is told to listen on. Build mode does not bind a port: Kestrel is replaced with `TestServer` at service-registration time, and the crawler dispatches requests in-memory through the same middleware pipeline.

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
