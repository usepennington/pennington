---
title: Build report fields
description: BuildReport, BuildDiagnostic, BrokenLink, severity levels, and how to read Duration/GeneratedPages/FailedPages.
section: diagnostics
order: 10
tags: []
uid: reference.diagnostics.build-report
isDraft: true
search: false
llms: false
---

> **In this page.** `BuildReport`, `BuildDiagnostic`, `BrokenLink`, severity levels, and how to read `Duration`/`GeneratedPages`/`FailedPages`.
>
> **Not in this page.** What each warning means at the per-rule level.

## Summary

The report returned by the static build, capturing every generated page, diagnostic, broken link, and the total duration.
Namespace `Pennington.Generation`; defined in `src/Pennington/Generation/BuildReport.cs`.

## Declaration

```csharp:xmldocid
T:Pennington.Generation.BuildReport
```

## Properties

| Name | Type | Description |
|---|---|---|
| `BrokenLinks` | `ImmutableList<BrokenLink>` | Internal links that could not be resolved during generation. |
| `Diagnostics` | `ImmutableList<BuildDiagnostic>` | All info/warning/error diagnostics emitted during the build. |
| `Duration` | `TimeSpan` | Wall-clock duration of the generation run. |
| `FailedPages` | `ImmutableList<ContentRoute>` | Routes whose parse or render stage produced a `FailedItem`. |
| `GeneratedPages` | `ImmutableList<ContentRoute>` | Routes successfully written to the output directory. |
| `HasErrors` | `bool` | `true` when any diagnostic is `Error`, any broken link exists, or any page failed. |
| `SkippedPages` | `ImmutableList<ContentRoute>` | Routes skipped because their front matter set `IsDraft = true`. |
| `TotalPages` | `int` | `GeneratedPages.Count + SkippedPages.Count + FailedPages.Count`. |

## Methods

### `WriteTo`

```csharp:xmldocid
M:Pennington.Generation.BuildReport.WriteTo(System.IO.TextWriter)
```

Writes the formatted summary, errors, and warnings sections to the provided `TextWriter`.

### `ToFormattedString`

```csharp:xmldocid
M:Pennington.Generation.BuildReport.ToFormattedString
```

Returns the formatted report as a string by invoking `WriteTo` against a `StringWriter`.

## `BuildDiagnostic`

Record type carrying a single diagnostic event. Namespace `Pennington.Generation`; defined in `src/Pennington/Generation/BuildDiagnostic.cs`.

```csharp:xmldocid
T:Pennington.Generation.BuildDiagnostic
```

| Name | Type | Default | Description |
|---|---|---|---|
| `Exception` | `Exception?` | `null` | Optional underlying exception captured when the diagnostic was raised. |
| `Message` | `string` | — | Human-readable diagnostic message. |
| `Route` | `ContentRoute?` | — | Route that produced the diagnostic, or `null` for build-wide diagnostics. |
| `Severity` | `DiagnosticSeverity` | — | Severity level of this diagnostic. |
| `SourceFile` | `string?` | `null` | Optional source file path associated with the diagnostic. |

## `BrokenLink`

Record type representing one unresolved link found during link verification. Namespace `Pennington.Generation`; defined in `src/Pennington/Generation/BrokenLink.cs`.

```csharp:xmldocid
T:Pennington.Generation.BrokenLink
```

| Name | Type | Description |
|---|---|---|
| `Reason` | `string` | Reason the link could not be resolved. |
| `SourcePage` | `ContentRoute` | Route of the page containing the broken link. |
| `Type` | `LinkType` | Classification of the link. |
| `Url` | `string` | Raw URL value that failed resolution. |

### `LinkType`

```csharp:xmldocid
T:Pennington.Generation.LinkType
```

| Member | Description |
|---|---|
| `Anchor` | In-page fragment link. |
| `External` | Absolute link to an external origin. |
| `Image` | Image asset reference. |
| `Internal` | Link to another page in the same site. |

## `DiagnosticSeverity`

Enum used by `BuildDiagnostic.Severity`. Namespace `Pennington.Diagnostics`; defined in `src/Pennington/Diagnostics/DiagnosticSeverity.cs`.

```csharp:xmldocid
T:Pennington.Diagnostics.DiagnosticSeverity
```

| Member | Description |
|---|---|
| `Error` | Build-breaking condition; contributes to `HasErrors`. |
| `Info` | Informational message; does not affect `HasErrors`. |
| `Warning` | Non-fatal problem; surfaced under the WARNINGS section in formatted output. |

## See also

- Related reference: [`PenningtonOptions`](/reference/options/pennington-options)
- How-to: [Run a static build and read the report](/how-to/build/run-static-build)
- Background: [Diagnostics and the build pipeline](/explanation/diagnostics/build-pipeline)
