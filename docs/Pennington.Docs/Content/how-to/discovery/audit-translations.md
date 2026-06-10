---
title: "Flag missing and outdated translations in the build report and dev overlay"
description: "Register AddTranslationAudit so missing and stale per-locale translations surface in the build report and the dev overlay, gated by git commit history."
uid: how-to.discovery.audit-translations
order: 6
sectionLabel: "Content Discovery"
tags: [localization, translations, audit, build, diagnostics]
---

Once a site ships in more than one language, translations drift — a page gets reworded in the default locale and its counterpart silently falls behind, or a new page never gets translated at all. `AddTranslationAudit` registers an `IBuildAuditor` that pairs every default-locale page with its translations and reports the gaps, both per-page in the dev overlay and site-wide in the build report. Set the locales up first with <xref:how-to.discovery.localization>.

## Before you begin

- A multi-locale site with default-locale content and at least one locale subfolder (see <xref:how-to.discovery.localization>).
- A git repository. The auditor reads commit dates to decide whether a translation is *outdated*; without git it still reports *missing* files but skips staleness checks.

## Install the package

`AddTranslationAudit` ships in its own package, separate from the core library and the site templates. Add it to the host project:

```bash
dotnet add package Pennington.TranslationAudit
```

## Register the auditor

`AddTranslationAudit` needs no other wiring — the auditor flows through the same audit cache the dev overlay reads, so one call next to the rest of your service registration is enough. The repository auto-discovers from the current working directory.

```csharp
builder.Services.AddTranslationAudit();
```

## Configure what gets audited

`AddTranslationAudit` takes an optional configuration action exposing `TranslationAuditOptions`:

```csharp
builder.Services.AddTranslationAudit(options =>
{
    options.IncludedLocales = ["es"];
    options.OutdatedSeverity = DiagnosticSeverity.Error;
});
```

- `RepositoryPath` — absolute path to the git repository root. Defaults to `null`, which auto-discovers by walking up from the current directory.
- `IncludedLocales` — the locale codes to audit. Defaults to `null`, which reports every non-default locale registered in `LocalizationOptions`.
- `MissingSeverity` — severity for "no translation file exists". Defaults to `DiagnosticSeverity.Warning`.
- `OutdatedSeverity` — severity for "translation predates the source's last commit". Defaults to `DiagnosticSeverity.Warning`.
- `ReportMissing` — set to `false` to suppress the missing-file diagnostics and audit only staleness. Defaults to `true`.

## Result

The audit surfaces in two places from one registration. During `dotnet run -- build`, each gap is a line in the build report's diagnostics:

```text
Build Complete — 11 pages in 0.5s
  11 pages generated
  1 warnings
WARNINGS
  /getting-started/: Missing Español (es) translation for "Getting Started" (/getting-started/).
```

During `dotnet run`, the same diagnostics attach per-page: visiting a page with a missing or outdated translation lights up the dev overlay badge (`#penn-diag-root`, bottom-right) with the count.

When no git repository is found, the auditor logs `no git repository found at or above '<path>'. Translation status will treat every file as untracked.` once at startup. The *missing* check still runs because it only touches the filesystem; every *outdated* check is skipped because both commit lookups resolve to `null`.

## Verify

- Run `dotnet run --project examples/BeyondTranslationAuditExample -- build` and confirm the build report lists the missing `es` translation.
- Run `dotnet run --project examples/BeyondTranslationAuditExample`, visit `/es/getting-started/`, and confirm the overlay badge shows one warning.

## Related

- Tutorial: [Add a second locale to your site](xref:tutorials.beyond-basics.add-a-locale)
- How-to: [Serve the site in multiple languages](xref:how-to.discovery.localization)
- Background: [Locale-aware URLs and content fallback](xref:explanation.localization.urls-and-fallback)
