---
title: "Request-scoped diagnostics"
description: "The per-request diagnostic accumulator (DiagnosticContext), the Diagnostic record, the DiagnosticSeverity enum, and the dev-mode overlay plus X-Pennington-Diagnostic header that surface them."
sectionLabel: "Diagnostics"
order: 407020
tags: [diagnostics, request-context, dev-overlay, response-headers]
uid: reference.diagnostics.request-context
---

The scoped accumulator and record types that collect per-request warnings, errors, and info messages, plus the two dev-mode transports (`X-Pennington-Diagnostic` header, on-page overlay) that surface them to the author. The accumulator and record types live in namespace `Pennington.Diagnostics` (`src/Pennington/Diagnostics/`); the header emission and overlay processor live in `Pennington.Infrastructure` (`src/Pennington/Infrastructure/`).

## `DiagnosticContext`

Scoped accumulator registered in DI as `Scoped` — a fresh instance per HTTP request, backed by a private `List<Diagnostic>` with no thread-safety. Consumers resolve it via `context.RequestServices.GetService<DiagnosticContext>()` (or constructor injection) and call one of the `Add*` methods; the middleware and overlay read `Diagnostics`, `HasAny`, and `HasErrors` during response flush.

### Members

- **`Add(Diagnostic diagnostic)`** — appends a pre-constructed `Diagnostic` to the request's list. Used when the caller already has a `Diagnostic` instance (for example, one forwarded from a helper service).
- **`AddError(string message, string? source = null)`** — appends a new `Diagnostic` with `Severity = DiagnosticSeverity.Error`. Causes `HasErrors` to return `true` for the remainder of the request.
- **`AddWarning(string message, string? source = null)`** — appends a new `Diagnostic` with `Severity = DiagnosticSeverity.Warning`. Contributes to the overlay warning count but does not flip `HasErrors`.
- **`AddInfo(string message, string? source = null)`** — appends a new `Diagnostic` with `Severity = DiagnosticSeverity.Info`. Surfaces in the overlay and headers but does not flip `HasErrors`.
- **`Diagnostics`** (property) — read-only view of the diagnostics accumulated so far, in insertion order. Read by `ResponseProcessingMiddleware.WriteDiagnosticHeaders` and `DiagnosticOverlayProcessor.ProcessAsync`.
- **`HasAny`** (property) — `true` when at least one diagnostic has been appended. Gate before enumerating the list to emit `X-Pennington-Diagnostic` headers.
- **`HasErrors`** (property) — `true` when at least one appended diagnostic has `Severity = Error`. A context with only warnings/info returns `true` from `HasAny` and `false` from `HasErrors`.

## `Diagnostic`

Immutable record carrying one diagnostic event. Route-agnostic — the request that produced it supplies the route context via `HttpContext`; the record itself only carries severity, message, and an optional `Source` label used as the overlay pill subtitle and the third pipe-delimited segment of the `X-Pennington-Diagnostic` header.

### Parameters

<FieldList>
<Field Name="Severity" Type="DiagnosticSeverity">
The severity band controlling how the overlay colors the entry and whether `HasErrors` flips.
</Field>
<Field Name="Message" Type="string">
Human-readable body rendered into the overlay panel and after the first pipe in the `X-Pennington-Diagnostic` header value.
</Field>
<Field Name="Source" Type="string?" Default="null">
Optional label identifying the producer (for example, `"XrefResolver"`); rendered as the small subtitle next to the severity pill in the overlay and appended after a second pipe in the header value when non-null.
</Field>
</FieldList>

## `DiagnosticSeverity`

Three-value enum in ascending severity order. The overlay's aggregate badge color picks the highest present severity (`Error` > `Warning` > `Info`); `DiagnosticContext.HasErrors` fires only on `Error`.

### Values

| Name | Value | Description |
|---|---|---|
| `Info` | `0` | Informational diagnostic; counted toward the badge total but renders in blue and never flips `HasErrors`. |
| `Warning` | `1` | Recoverable issue (for example, an unresolved xref); contributes to the overlay warning count and the amber badge color. |
| `Error` | `2` | Fatal issue for the request's output; flips `HasErrors` and renders with the red badge color. |

## Dev-mode overlay and `X-Pennington-Diagnostic` header

Two transports surface the accumulated diagnostics, both wired inside `UsePennington` via the response-processor pipeline.

| Transport | Type | Availability | Shape |
|---|---|---|---|
| Response header | `X-Pennington-Diagnostic` | Every request that has `HasAny` | One header value per diagnostic, pipe-delimited: `Severity|Message` (or `Severity|Message|Source` when `Source` is non-null). Emitted by `ResponseProcessingMiddleware.WriteDiagnosticHeaders` immediately before the buffered body is flushed. |
| On-page overlay | `DiagnosticOverlayProcessor` (`Order = 30`, `IResponseProcessor`) | Requests where `DOTNET_WATCH` is set, status is `2xx`, and content type contains `text/html` | Floating badge injected before `</body>` summarizing error/warning counts; clicking expands a panel listing every diagnostic. Re-renders on the `spa:diagnostics` DOM event for SPA navigations. |

## Example

The canonical in-repo consumer is `XrefResolvingService`, which reports unresolved uids. Any service or response processor that resolves `DiagnosticContext` and calls `AddWarning` / `AddError` / `AddInfo` during request handling flows entries into the `X-Pennington-Diagnostic` response header and the dev overlay without further wiring. The `examples/ExtensibilityLabExample` lab registers a scoped `IResponseProcessor` that follows this shape:

```csharp:xmldocid,bodyonly
T:ExtensibilityLabExample.DiagnosticsEmittingProcessor
```

## See also

- Related reference: [Build report fields](xref:reference.api.build-report)
- Related reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- How-to: [Write a response processor](xref:how-to.extensibility.response-processor)
