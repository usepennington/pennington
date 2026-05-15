---
title: "Request-scoped diagnostics"
description: "The per-request diagnostic accumulator (DiagnosticContext), the Diagnostic record, the DiagnosticSeverity enum, and the dev-mode overlay plus X-Pennington-Diagnostic header that surface them."
sectionLabel: "Diagnostics"
order: 407020
tags: [diagnostics, request-context, dev-overlay, response-headers]
uid: reference.diagnostics.request-context
---

The scoped accumulator and record types that collect per-request warnings, errors, and info messages, plus the two dev-mode transports (`X-Pennington-Diagnostic` header, on-page overlay) that surface them to the author. The accumulator and record types live in `Pennington.Diagnostics`; the header emission and overlay processor live in `Pennington.Infrastructure`.

## `DiagnosticContext`

Per-request accumulator registered as `Scoped`. Not thread-safe; resolved via DI for the lifetime of one request.

### Members

| Member | Description |
|---|---|
| `Add(Diagnostic diagnostic)` | Appends a pre-constructed `Diagnostic`. |
| `AddError(string message, string? source = null)` | Appends a `Diagnostic` with `Severity = Error`. Flips `HasErrors` to `true`. |
| `AddWarning(string message, string? source = null)` | Appends a `Diagnostic` with `Severity = Warning`. |
| `Diagnostics` | Read-only view of the diagnostics accumulated so far, in insertion order. |
| `HasAny` | `true` when at least one diagnostic has been appended. |
| `HasErrors` | `true` when at least one appended diagnostic has `Severity = Error`. |

## `Diagnostic`

Immutable record carrying one diagnostic event. Route-agnostic — `HttpContext` supplies the route context.

### Parameters

| Name | Type | Default | Description |
|---|---|---|---|
| `Severity` | `DiagnosticSeverity` | — | Severity band controlling overlay color and the `HasErrors` flag. |
| `Message` | `string` | — | Human-readable body rendered into the overlay panel and after the first pipe in the `X-Pennington-Diagnostic` header value. |
| `Source` | `string?` | `null` | Optional producer label (for example, `"XrefResolver"`); rendered as the overlay pill subtitle and appended after a second pipe in the header value when non-null. |

## `DiagnosticSeverity`

Two-value enum in ascending severity order.

### Values

| Name | Value | Description |
|---|---|---|
| `Warning` | `0` | Recoverable issue (for example, an unresolved xref); contributes to the overlay warning count and the amber badge color. |
| `Error` | `1` | Fatal issue for the request's output; flips `HasErrors` and renders with the red badge color. |

## Dev-mode overlay and `X-Pennington-Diagnostic` header

Two transports surface the accumulated diagnostics, both wired inside `UsePennington` via the response-processor pipeline.

| Transport | Type | Availability | Shape |
|---|---|---|---|
| Response header | `X-Pennington-Diagnostic` | Every request where `HasAny` is `true` | One header value per diagnostic, pipe-delimited: `Severity|Message` (or `Severity|Message|Source` when `Source` is non-null). |
| On-page overlay | `DiagnosticOverlayProcessor` (`Order = 30`, `IResponseProcessor`) | Requests where `DOTNET_WATCH` is set, status is `2xx`, and content type contains `text/html` | Floating badge injected before `</body>` summarizing error/warning counts; clicking expands a panel listing every diagnostic. Re-renders on the `spa:diagnostics` DOM event for SPA navigations. |

## Example

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.DiagnosticsEmittingProcessor
```

## See also

- Related reference: [Build report fields](xref:reference.api.build-report)
- Related reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- How-to: [Write a response processor](xref:how-to.response-pipeline.response-processor)
