# BeyondTranslationAuditExample

Wires `AddTranslationAudit` into a two-locale DocSite. Spanish translations under `Content/es/` deliberately omit `getting-started.md` so the auditor produces a "missing es translation" warning visible both in the dev overlay (`#penn-diag-root` badge in the bottom-right when a page has diagnostics) and in `dotnet run -- build` diagnostics.

## Concepts

- `AddTranslationAudit` registering an `IBuildAuditor`
- Audit cache shared with the dev overlay — same data behind both surfaces (look for the `#penn-diag-root` badge, not `[class*=overlay]`)
- Repository auto-discovery from the current working directory

## Repository auto-discovery

`TranslationAuditOptions.RepositoryPath` defaults to the host's `Directory.GetCurrentDirectory()`. `LibGit2GitHistoryReader` walks upward from that path looking for a `.git` directory. The behavior splits into three cases:

- **Inside a git repo (normal):** every translation pair runs both checks — *missing* (translation file is absent on disk) and *outdated* (translation file's most recent commit predates the source file's most recent commit).
- **No git repo found:** the auditor logs `no git repository found at or above '<path>'. Translation status will treat every file as untracked.` once at startup, the *missing* check still runs (it's pure filesystem), but every *outdated* check is skipped because both `sourceCommit` and `translationCommit` resolve to `null`. The build report and overlay both still surface the "missing" diagnostics.
- **Repo open fails:** caught and logged; same effective behavior as "no git repo found".

A reader copying just this folder into a non-git scratch directory still sees the example's central teaching (the "missing es translation" warning), just without staleness detection.

## See also

No dedicated how-to or reference page exists yet — the framework source is the authoritative surface:

- `src/Pennington.TranslationAudit/TranslationAuditor.cs` — `IBuildAuditor` implementation that buckets pages by canonical path and classifies each translation as Up-to-date, Outdated, or Missing.
- `src/Pennington.TranslationAudit/TranslationAuditOptions.cs` — configuration surface (`IncludedLocales`, `ReportMissing`, `MissingSeverity`, `OutdatedSeverity`, `RepositoryPath`).
- `src/Pennington.TranslationAudit/LibGit2GitHistoryReader.cs` — the LibGit2Sharp adapter that powers staleness checks.

## Referenced from

This example exists as a working reference for the `Pennington.TranslationAudit` package. A dedicated docs-site how-to (likely `how-to/discovery/audit-translations.md`) is a known follow-up.
