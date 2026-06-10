---
title: "Request-scoped diagnostics"
description: "The per-request diagnostic accumulator (DiagnosticContext), the Diagnostic record, the DiagnosticSeverity enum, and the dev-mode overlay plus X-Pennington-Diagnostic header that surface them."
sectionLabel: "Diagnostics"
order: 1
tags: [diagnostics, request-context, dev-overlay, response-headers]
uid: reference.diagnostics.request-context
---

The scoped accumulator and record types that collect per-request warnings, errors, and info messages, plus the two transports that surface them: the `X-Pennington-Diagnostic` response header (emitted on every request, including during a static build, where it feeds the [build report](xref:reference.api.build-report)) and the dev-only on-page overlay. The accumulator and record types live in `Pennington.Diagnostics`; the header emission and overlay processor live in `Pennington.Infrastructure`.

## `DiagnosticContext`

Per-request accumulator registered as `Scoped`. Not thread-safe; resolved via DI for the lifetime of one request.

### Members

| Member | Description |
|---|---|
| `Add(Diagnostic diagnostic)` | Appends a pre-constructed `Diagnostic`. |
| `AddError(string message, string? source = null)` | Appends a `Diagnostic` with `Severity = Error`. |
| `AddWarning(string message, string? source = null)` | Appends a `Diagnostic` with `Severity = Warning`. |
| `AddInfo(string message, string? source = null)` | Appends a `Diagnostic` with `Severity = Info`. |
| `Diagnostics` | Read-only view of the diagnostics accumulated so far, in insertion order. |
| `HasAny` | `true` when at least one diagnostic has been appended. |
| `HasErrors` | `true` when at least one appended diagnostic has `Severity = Error`. |

## `Diagnostic`

Immutable record carrying one diagnostic event. Route-agnostic — `HttpContext` supplies the route context.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Severity` | `DiagnosticSeverity` | — | Severity band controlling overlay color and the `HasErrors` flag. |
| `Message` | `string` | — | Human-readable body rendered into the overlay panel and, percent-encoded, into the second segment of the `X-Pennington-Diagnostic` header value. |
| `Source` | `string?` | `null` | Optional producer label (for example, `"XrefResolver"`); rendered as the overlay pill subtitle and, percent-encoded, appended as a third header segment when non-null. |

## `DiagnosticSeverity`

Three-value enum.

### Values

| Name | Value | Description |
|---|---|---|
| `Warning` | `0` | Recoverable issue (for example, an unresolved xref); contributes to the overlay warning count and the amber badge color. |
| `Error` | `1` | Failure that indicates broken content or misconfiguration; flips `HasErrors` and renders with the red badge color. |
| `Info` | `2` | Informational notice about degraded but non-broken behavior; does not contribute to the error or warning counts. |

## `X-Pennington-Diagnostic` header and dev-mode overlay

Two transports surface the accumulated diagnostics, both wired inside `UsePennington` via the response-processor pipeline. The header is emitted on every request; the overlay is dev-only.

| Transport | Type | Availability | Shape |
|---|---|---|---|
| Response header | `X-Pennington-Diagnostic` | Every request where `HasAny` is `true`, including during a static build (the build pipeline parses these headers and folds them into the [build report](xref:reference.api.build-report)) | One header value per diagnostic, pipe-delimited: `Severity|Message` (or `Severity|Message|Source` when `Source` is non-null). Each segment is independently percent-encoded with `Uri.EscapeDataString` so non-ASCII values (accented locale names, content titles) survive the ASCII-only header writer — a machine parser must split on `|` then `Uri.UnescapeDataString` each segment. |
| On-page overlay | `DiagnosticOverlayProcessor` (`Order = 30`, `IResponseProcessor`) | Dev-serve requests (the host was not launched with the build verb) where status is `2xx` and content type contains `text/html` | Floating badge injected before `</body>` summarizing error/warning counts; clicking expands a panel listing every diagnostic. Re-renders on the `spa:diagnostics` DOM event for SPA navigations. |

## Example

```csharp:symbol,bodyonly
examples/ExtensibilityLabExample/DiagnosticsEmittingProcessor.cs > DiagnosticsEmittingProcessor
```

## See also

- Related reference: [Build report fields](xref:reference.api.build-report)
- Related reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- How-to: [Write a response processor](xref:how-to.response-pipeline.response-processor)
