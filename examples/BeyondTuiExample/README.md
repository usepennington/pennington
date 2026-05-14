# BeyondTuiExample

Opts a DocSite host into the dev-time TUI dashboard via `AddPenningtonTui`. One line on top of the standard DocSite — under `dotnet run` (with a real terminal) the host paints a full-screen XenoAtom dashboard over stdout; under `dotnet run -- build`, when stdout is redirected, or when `DOTNET_WATCH=1`, the hosted service no-ops so static publish, CI logs, and `dotnet watch` continue to work normally.

## What the TUI surfaces

`AddPenningtonTui` registers a hosted service (`PenningtonTuiHostedService`) that brings up four panels:

- **Main tab** — process status, app URL (resolved from `IServerAddressesFeature`), a ring buffer of recent HTTP requests, a ring buffer of log lines, and the file-change log.
- **Content tab** — every registered `IContentService`'s discovered TOC entries, refreshed when `IFileWatcher` reports a change. The refresh is debounced by `PenningtonTuiOptions.FileChangeDebounce` (default 500 ms) so a `:w` in vim that touches many files batches into one TOC rebuild.
- **Diagnostics tab** — fed by the shared `IAuditCache`. Whatever the framework's `AuditRunner` produces from registered `IBuildAuditor`s (translation audit, xref audit, link audit, overlap audit) and `IRenderedAuditor`s appears here in real time. Same diagnostic data as the build report.
- **Log replacement** — in dev mode the TUI claims stdout, so every `ILoggerProvider` is replaced with `TuiLoggerProvider` (writing into the Logs panel) so Kestrel's startup banner and request lines don't paint over frames.

The hosted service skips all of that when:

- `--` argv[1] starts with `build` (static publish — diagnostics go to the build report).
- `DOTNET_WATCH=1` is set (dotnet watch owns the terminal).
- `Console.IsOutputRedirected` is true (CI, container, `> log.txt`) — default `ILogger` Console output prints normal line-mode log entries instead.

## Concepts

- `AddPenningtonTui` registering `PenningtonTuiHostedService` + four-tab dashboard
- Debounced `IFileWatcher` subscription rebuilding the Content tab
- Shared `IAuditCache` feeding the Diagnostics tab — the *same* data the dev overlay (`#penn-diag-root`) and the build report read
- Dev-only — build mode, dotnet watch, and non-TTY stdout are all detected at startup

## See also

No dedicated how-to or reference page exists yet — the framework source is the authoritative surface:

- `src/Pennington.Tui/PenningtonTuiExtensions.cs` — `AddPenningtonTui` registration + the three early-exit gates.
- `src/Pennington.Tui/PenningtonTuiHostedService.cs` — hosted-service lifecycle (`OnApplicationStarted`, `RunTuiLoop`, `OnFileChanged`).
- `src/Pennington.Tui/PenningtonTuiOptions.cs` — `FileChangeDebounce`, buffer sizes, `LogMinLevel`.
- `src/Pennington.Tui/Views/TuiApp.cs` — XenoAtom rendering entry point (Main / Content / Diagnostics tabs).

## Referenced from

This example exists as a working reference for the `Pennington.Tui` package. A dedicated how-to (likely `how-to/dev-loop/tui-dashboard.md`) is a known follow-up.
