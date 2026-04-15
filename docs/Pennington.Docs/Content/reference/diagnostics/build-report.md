---
title: "Build report fields"
description: "Field-by-field catalog of BuildReport, BuildDiagnostic, and BrokenLink — the record OutputGenerationService returns after a static build."
uid: reference.diagnostics.build-report
order: 407010
sectionLabel: Diagnostics
tags: [diagnostics, build, reference]
---

`BuildReport` is the immutable record returned by `OutputGenerationService.GenerateAsync` summarizing a static build: per-route diagnostics, broken-link findings, the three page-state lists (`GeneratedPages`, `SkippedPages`, `FailedPages`), and total wall-clock `Duration`. Namespace `Pennington.Generation` at `src/Pennington/Generation/BuildReport.cs`, `BuildDiagnostic.cs`, and `BrokenLink.cs`; `DiagnosticSeverity` lives in `Pennington.Diagnostics` at `src/Pennington/Diagnostics/DiagnosticSeverity.cs`.

## Overview

| Type | Role |
|---|---|
| [`BuildReport`](#buildreport) | Top-level result record aggregating diagnostics, broken links, generated/skipped/failed page routes, and the total build duration. |
| [`BuildDiagnostic`](#builddiagnostic) | Single per-route or per-file finding tagged with a `DiagnosticSeverity` level, an optional `ContentRoute`, a message string, an optional exception, and an optional source-file path. |
| [`BrokenLink`](#brokenlink) | Record describing one link in a rendered page that failed verification, tagged with its source page, the target URL, a `LinkType`, and a human-readable reason. |
| [`DiagnosticSeverity`](#diagnosticseverity) | Enum with `Info`, `Warning`, and `Error` levels shared with the request-scoped `DiagnosticContext`. |

## `BuildReport`

Sealed record with read-only `ImmutableList<T>` collections and a `TimeSpan` duration; instances are produced only by `BuildReportBuilder.Build()` at the end of `OutputGenerationService.GenerateAsync`.

### Properties

| Name | Type | Description |
|---|---|---|
| `BrokenLinks` | `ImmutableList<BrokenLink>` | Every link in a generated page that failed verification — unreachable internal targets, missing anchors, images that 404; populated by the `LinkVerificationService` pass that runs after each page is rendered. |
| `Diagnostics` | `ImmutableList<BuildDiagnostic>` | Every `Info`, `Warning`, and `Error` emitted during discovery, parse, render, and generation. Filtered by severity when `WriteTo` splits the output into ERRORS and WARNINGS sections. |
| `Duration` | `TimeSpan` | Wall-clock time between `BuildReportBuilder` construction and the call to `Build()`, spanning discovery through the final HTTP crawl write. |
| `FailedPages` | `ImmutableList<ContentRoute>` | Routes that reached the generate stage as `FailedItem` or whose generation raised an exception captured via `AddError(route, …)`. Each entry corresponds to at least one `Diagnostics` entry with `Severity is DiagnosticSeverity.Error`. |
| `GeneratedPages` | `ImmutableList<ContentRoute>` | Routes successfully rendered and written to the output directory, appended by `BuildReportBuilder.AddGeneratedPage` once the crawler's HTTP GET returns a non-failure response. |
| `HasErrors` | `bool` | Computed property — `true` when any diagnostic is `DiagnosticSeverity.Error`, when `BrokenLinks` is non-empty, or when `FailedPages` is non-empty. `RunOrBuildAsync` sets a non-zero process exit code when this is `true`. |
| `SkippedPages` | `ImmutableList<ContentRoute>` | Routes deliberately skipped during generation — currently draft pages (`IFrontMatter.IsDraft == true`) — distinguished from `FailedPages` so CI can differentiate intentional omissions from build failures. |
| `TotalPages` | `int` | Computed sum of `GeneratedPages.Count + SkippedPages.Count + FailedPages.Count`, used by `WriteTo` in the summary line. |

### Methods

#### `WriteTo(TextWriter writer)`

Writes a human-readable summary to the supplied `TextWriter` — a one-line `Build Complete — N pages in X.Xs` header, followed by per-category counts, then an `ERRORS` section (each entry prefixed with its `ContentRoute.CanonicalPath` and source file where available) and a `WARNINGS` section including the broken-link rollup. This is the format `PenningtonExtensions.RunOrBuildAsync` prints to `Console.Out` at the end of a build.

#### `ToFormattedString()`

Convenience wrapper around `WriteTo` that renders to a `StringWriter` and returns the resulting string, suitable for embedding the report in test assertions or CI log artifacts.

## `BuildDiagnostic`

Positional record with one required severity/message pair and three optional fields; instances are appended by `BuildReportBuilder.AddInfo`, `AddWarning`, and `AddError`, or synthesized externally and passed to `AddDiagnostic`.

### Properties

| Name | Type | Description |
|---|---|---|
| `Exception` | `Exception?` | Optional exception captured when the diagnostic originated from a catch block — parser or renderer failures demoted to `FailedItem` — so callers can inspect the full stack trace beyond the `Message`. |
| `Message` | `string` | Human-readable diagnostic text rendered verbatim by `BuildReport.WriteTo`; messages are unstructured strings with no per-rule codes and are not intended for machine parsing. |
| `Route` | `ContentRoute?` | The route this diagnostic is attached to, when the failure was traceable to a discovered content item. `null` indicates a site-wide or source-file-scoped finding (for example, a malformed YAML front-matter block with no successfully constructed route). |
| `Severity` | `DiagnosticSeverity` | One of `Info`, `Warning`, or `Error`; `Error` additionally triggers a `FailedPages` append when the `ContentRoute` overload of `AddError` is used. |
| `SourceFile` | `string?` | Optional source-file path used when the diagnostic pre-dates a resolved `ContentRoute` — surfaced by `WriteTo` as a `Source:` or `File:` line under the route or message. |

## `BrokenLink`

Positional record describing one failed link-verification finding; instances are appended by `BuildReportBuilder.AddBrokenLink` after `LinkVerificationService` inspects each rendered page.

### Properties

| Name | Type | Description |
|---|---|---|
| `Reason` | `string` | Human-readable explanation of why the link failed verification — for example, missing trailing slash, unreachable URL, or unresolved anchor fragment; no structured reason codes are exposed. |
| `SourcePage` | `ContentRoute` | The route of the rendered page that contains the broken link, used by `BuildReport.WriteTo` to prefix the broken-link rollup entries with `CanonicalPath`. |
| `Type` | `LinkType` | One of `Internal`, `External`, `Anchor`, or `Image`, classifying the link so callers can filter findings by link kind. |
| `Url` | `string` | The verbatim target URL as it appeared in the rendered HTML, before any rewriting by `BaseUrlHtmlRewriter` or `XrefHtmlRewriter` normalization. |

### Related enum — `LinkType`

Enum with `Internal`, `External`, `Anchor`, and `Image` members; populated by `LinkVerificationService` when it classifies each `<a href>` or `<img src>` it inspects.

## `DiagnosticSeverity`

### Values

| Name | Semantics in `BuildReport` |
|---|---|
| `Info` | Informational diagnostic — written to neither the ERRORS nor the WARNINGS section of `WriteTo` output, retained on `Diagnostics` for programmatic access only. |
| `Warning` | Listed under `WARNINGS` by `WriteTo`, counted in the summary line's `N warnings` tally, and does not on its own flip `HasErrors` to `true`. |
| `Error` | Listed under `ERRORS` by `WriteTo`, flips `HasErrors` to `true`, and — when added via the `AddError(ContentRoute, …)` overload — also appends the route to `FailedPages`. |

## Example

```csharp:xmldocid,bodyonly
M:SubPathDeployableExample.BuildHost.PrintBuildReport(Pennington.Generation.BuildReport)
```

Minimal post-build handler that prints the report via `WriteTo(Console.Out)` and sets `Environment.ExitCode = 1` when `HasErrors` is `true`.

## See also

- Related reference: [Request-scoped diagnostics](xref:reference.diagnostics.request-context)
- Related reference: [CLI and build arguments](xref:reference.host.cli)
- How-to: [Build a static site](xref:how-to.deployment.static-build)
- Background: TODO — the Diagnostics explanation page on why `BuildReport` and the request-scoped `DiagnosticContext` are split surfaces is not yet in the TOC.
