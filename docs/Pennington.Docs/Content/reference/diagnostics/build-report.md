---
title: "Build report fields"
description: "Field-by-field catalog of BuildReport, BuildDiagnostic, and BrokenLink — the record OutputGenerationService returns after a static build."
uid: reference.diagnostics.build-report
order: 10
sectionLabel: Diagnostics
tags: [diagnostics, build, reference]
---

> **In this page.** _One sentence. Field-by-field catalog of `BuildReport`, `BuildDiagnostic`, `BrokenLink`, and the `DiagnosticSeverity` levels they carry, plus how to read `Duration`, `GeneratedPages`, and `FailedPages`._
>
> **Not in this page.** _One sentence. The meaning of individual warning messages at the per-rule level is not covered here; nor is the request-scoped `DiagnosticContext` used during rendering — see [Request-scoped diagnostics](xref:reference.diagnostics.request-context)._

## Summary

_**One sentence: what it is.** The immutable record returned by `OutputGenerationService.GenerateAsync` summarizing a static build: per-route diagnostics, broken-link findings, the three page-state lists (`GeneratedPages`, `SkippedPages`, `FailedPages`), and total wall-clock `Duration`._
_**One sentence: where they live.** Namespace `Pennington.Generation` at `src/Pennington/Generation/BuildReport.cs`, `BuildDiagnostic.cs`, and `BrokenLink.cs`; `DiagnosticSeverity` lives in `Pennington.Diagnostics` at `src/Pennington/Diagnostics/DiagnosticSeverity.cs`._

## Overview

| Type | Role |
|---|---|
| [`BuildReport`](#buildreport) | _One sentence: top-level result record aggregating diagnostics, broken links, generated / skipped / failed page routes, and the total build duration._ |
| [`BuildDiagnostic`](#builddiagnostic) | _One sentence: single per-route or per-file finding tagged with a `DiagnosticSeverity` level, an optional `ContentRoute`, a message string, an optional exception, and an optional source-file path._ |
| [`BrokenLink`](#brokenlink) | _One sentence: record describing one link in a rendered page that failed verification, tagged with its source page, the target URL, a `LinkType`, and a human-readable reason._ |
| [`DiagnosticSeverity`](#diagnosticseverity) | _One sentence: enum with `Info`, `Warning`, and `Error` levels shared with the request-scoped `DiagnosticContext`._ |

## `BuildReport`

### Declaration

```csharp:xmldocid
T:Pennington.Generation.BuildReport
```

_One sentence: sealed class with read-only `ImmutableList<T>` collections and a `TimeSpan` duration; instances are produced only by `BuildReportBuilder.Build()` at the end of `OutputGenerationService.GenerateAsync`._

### Properties

| Name | Type | Description |
|---|---|---|
| `BrokenLinks` | `ImmutableList<BrokenLink>` | _One sentence: every link in a generated page that failed verification — unreachable internal targets, missing anchors, images that 404; populated by the `LinkVerificationService` pass that runs after each page is rendered._ |
| `Diagnostics` | `ImmutableList<BuildDiagnostic>` | _One to two sentences: every `Info`, `Warning`, and `Error` emitted during discovery, parse, render, and generation. Filtered by severity when `WriteTo` splits the output into ERRORS and WARNINGS sections._ |
| `Duration` | `TimeSpan` | _One sentence: wall-clock time between `BuildReportBuilder` construction and the call to `Build()`, spanning discovery through the final HTTP crawl write._ |
| `FailedPages` | `ImmutableList<ContentRoute>` | _One to two sentences: routes that reached the generate stage as `FailedItem` or whose generation raised an exception captured via `AddError(route, …)`. Each entry corresponds to at least one `Diagnostics` entry with `Severity is DiagnosticSeverity.Error`._ |
| `GeneratedPages` | `ImmutableList<ContentRoute>` | _One sentence: routes successfully rendered and written to the output directory, appended by `BuildReportBuilder.AddGeneratedPage` once the crawler's HTTP GET returns a non-failure response._ |
| `HasErrors` | `bool` | _One to two sentences: computed property — `true` when any diagnostic is `DiagnosticSeverity.Error`, when `BrokenLinks` is non-empty, or when `FailedPages` is non-empty. `RunOrBuildAsync` sets a non-zero process exit code when this is `true`._ |
| `SkippedPages` | `ImmutableList<ContentRoute>` | _One sentence: routes deliberately skipped during generation — currently draft pages (`IFrontMatter.IsDraft == true`) — distinguished from `FailedPages` so CI can differentiate intentional omissions from build failures._ |
| `TotalPages` | `int` | _One sentence: computed sum of `GeneratedPages.Count + SkippedPages.Count + FailedPages.Count`, used by `WriteTo` in the summary line._ |

### Methods

#### `WriteTo(TextWriter)`

```csharp:xmldocid
M:Pennington.Generation.BuildReport.WriteTo(System.IO.TextWriter)
```

_Two to three sentences: writes a human-readable summary to the supplied `TextWriter` — a one-line `Build Complete — N pages in X.Xs` header, followed by per-category counts, then an `ERRORS` section (each entry prefixed with its `ContentRoute.CanonicalPath` and source file where available) and a `WARNINGS` section including the broken-link rollup. This is the format `PenningtonExtensions.RunOrBuildAsync` prints to `Console.Out` at the end of a build._

#### `ToFormattedString()`

```csharp:xmldocid
M:Pennington.Generation.BuildReport.ToFormattedString
```

_One sentence: convenience wrapper around `WriteTo` that renders to a `StringWriter` and returns the resulting string, suitable for embedding the report in test assertions or CI log artifacts._

## `BuildDiagnostic`

### Declaration

```csharp:xmldocid
T:Pennington.Generation.BuildDiagnostic
```

_One sentence: positional record with one required severity / message pair and three optional fields; instances are appended by `BuildReportBuilder.AddInfo`, `AddWarning`, and `AddError`, or synthesized externally and passed to `AddDiagnostic`._

### Properties

| Name | Type | Description |
|---|---|---|
| `Exception` | `Exception?` | _One sentence: optional exception captured when the diagnostic originated from a catch block — e.g., parser or renderer failures demoted to `FailedItem` — so callers can inspect the full stack trace beyond the `Message`._ |
| `Message` | `string` | _One sentence: human-readable diagnostic text rendered verbatim by `BuildReport.WriteTo`; messages are unstructured strings — no per-rule codes — and are not intended for machine parsing._ |
| `Route` | `ContentRoute?` | _One to two sentences: the route this diagnostic is attached to, when the failure was traceable to a discovered content item. `null` indicates a site-wide or source-file-scoped finding (e.g., a malformed YAML front-matter block with no successfully constructed route)._ |
| `Severity` | `DiagnosticSeverity` | _One sentence: one of `Info`, `Warning`, or `Error`; `Error` additionally triggers a `FailedPages` append when the `ContentRoute` overload of `AddError` is used._ |
| `SourceFile` | `string?` | _One sentence: optional source-file path used when the diagnostic pre-dates a resolved `ContentRoute` — surfaced by `WriteTo` as a `Source:` or `File:` line under the route or message._ |

## `BrokenLink`

### Declaration

```csharp:xmldocid
T:Pennington.Generation.BrokenLink
```

_One sentence: positional record describing one failed link-verification finding; instances are appended by `BuildReportBuilder.AddBrokenLink` after `LinkVerificationService` inspects each rendered page._

### Properties

| Name | Type | Description |
|---|---|---|
| `Reason` | `string` | _One sentence: human-readable explanation of why the link failed verification (for example, missing trailing slash, unreachable URL, unresolved anchor fragment); no structured reason codes are exposed._ |
| `SourcePage` | `ContentRoute` | _One sentence: the route of the rendered page that contains the broken link, used by `BuildReport.WriteTo` to prefix the broken-link rollup entries with `CanonicalPath`._ |
| `Type` | `LinkType` | _One sentence: one of `Internal`, `External`, `Anchor`, or `Image`, classifying the link so callers can filter findings by link kind._ |
| `Url` | `string` | _One sentence: the verbatim target URL as it appeared in the rendered HTML, before any rewriting by `BaseUrlHtmlRewriter` or `XrefHtmlRewriter` normalization._ |

### Related enum — `LinkType`

```csharp:xmldocid
T:Pennington.Generation.LinkType
```

_One sentence: enum with `Internal`, `External`, `Anchor`, and `Image` members; populated by `LinkVerificationService` when it classifies each `<a href>` or `<img src>` it inspects._

## `DiagnosticSeverity`

### Declaration

```csharp:xmldocid
T:Pennington.Diagnostics.DiagnosticSeverity
```

### Values

| Name | Semantics in `BuildReport` |
|---|---|
| `Info` | _One sentence: informational diagnostic — written to neither the ERRORS nor the WARNINGS section of `WriteTo` output, retained on `Diagnostics` for programmatic access only._ |
| `Warning` | _One sentence: listed under `WARNINGS` by `WriteTo`, counted in the summary line's `N warnings` tally, and does not on its own flip `HasErrors` to `true`._ |
| `Error` | _One sentence: listed under `ERRORS` by `WriteTo`, flips `HasErrors` to `true`, and — when added via the `AddError(ContentRoute, …)` overload — also appends the route to `FailedPages`._ |

## Example

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)
```

_One sentence: minimal post-build handler that prints the report via `WriteTo(Console.Out)` and sets `Environment.ExitCode = 1` when `HasErrors` is `true`._

## See also

- Related reference: [Request-scoped diagnostics](xref:reference.diagnostics.request-context)
- Related reference: [CLI and build arguments](xref:reference.host.cli)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- Background: TODO — the Diagnostics explanation page on why `BuildReport` and the request-scoped `DiagnosticContext` are split surfaces is not yet in the TOC.
