---
title: "Request-scoped diagnostics"
description: "The per-request diagnostic accumulator (DiagnosticContext), the Diagnostic record, the DiagnosticSeverity enum, and the dev-mode overlay plus X-Pennington-Diagnostic header that surface them."
sectionLabel: "Diagnostics"
order: 20
tags: [diagnostics, request-context, dev-overlay, response-headers]
uid: reference.diagnostics.request-context
---

> **In this page.** `DiagnosticContext`, `Diagnostic`, `DiagnosticSeverity`, and how the dev-mode overlay surfaces them.
>
> **Not in this page.** Wiring custom log sinks.

## Summary

_**One sentence: what it is.** The scoped accumulator and record types that collect per-request warnings/errors/info messages, plus the two dev-mode transports (`X-Pennington-Diagnostic` header, on-page overlay) that surface them to the author._
_**One sentence: where it lives.** Namespace `Pennington.Diagnostics` (`src/Pennington/Diagnostics/`) for the accumulator and record types; `Pennington.Infrastructure` (`src/Pennington/Infrastructure/`) for the `ResponseProcessingMiddleware` header emission and the `DiagnosticOverlayProcessor` surface._

## `DiagnosticContext`

```csharp:xmldocid
T:Pennington.Diagnostics.DiagnosticContext
```

_Scoped accumulator registered in DI as `Scoped` — a fresh instance per HTTP request, backed by a private `List<Diagnostic>` with no thread-safety. Consumers resolve it via `context.RequestServices.GetService<DiagnosticContext>()` (or constructor injection) and call one of the `Add*` methods; the middleware and overlay read `Diagnostics`, `HasAny`, and `HasErrors` during response flush._

### Members

_Alphabetical._

### `Add`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.Add(Pennington.Diagnostics.Diagnostic)
```

_Appends a pre-constructed `Diagnostic` to the request's list. Used when the caller already has a `Diagnostic` instance (for example, one forwarded from a helper service)._

### `AddError`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.AddError(System.String,System.String)
```

_Appends a new `Diagnostic` with `Severity = DiagnosticSeverity.Error` and the supplied `message` / optional `source`. An error causes `HasErrors` to return `true` for the remainder of the request._

### `AddInfo`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.AddInfo(System.String,System.String)
```

_Appends a new `Diagnostic` with `Severity = DiagnosticSeverity.Info` and the supplied `message` / optional `source`. Info entries surface in the overlay and headers but do not flip `HasErrors`._

### `AddWarning`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.AddWarning(System.String,System.String)
```

_Appends a new `Diagnostic` with `Severity = DiagnosticSeverity.Warning` and the supplied `message` / optional `source`. Warnings count toward the overlay badge's warning total but do not flip `HasErrors`._

### `Diagnostics`

```csharp:xmldocid
P:Pennington.Diagnostics.DiagnosticContext.Diagnostics
```

_Read-only view of the diagnostics accumulated so far, in insertion order. Read by `ResponseProcessingMiddleware.WriteDiagnosticHeaders` and `DiagnosticOverlayProcessor.ProcessAsync`._

### `HasAny`

```csharp:xmldocid
P:Pennington.Diagnostics.DiagnosticContext.HasAny
```

_Returns `true` when at least one diagnostic has been appended during this request. The middleware uses this as a cheap gate before enumerating the list to emit `X-Pennington-Diagnostic` headers._

### `HasErrors`

```csharp:xmldocid
P:Pennington.Diagnostics.DiagnosticContext.HasErrors
```

_Returns `true` when at least one appended diagnostic has `Severity = DiagnosticSeverity.Error`. Independent of `HasAny`; a context with only warnings/info returns `true` from `HasAny` and `false` from `HasErrors`._

## `Diagnostic`

```csharp:xmldocid
T:Pennington.Diagnostics.Diagnostic
```

_Immutable record carrying one diagnostic event. Route-agnostic — the request that produced it supplies the route context via `HttpContext`; the record itself only carries severity, message, and an optional `Source` label used as the overlay pill subtitle and the third pipe-delimited segment of the `X-Pennington-Diagnostic` header._

### Parameters

_Positional record parameters, in declaration order._

| Name | Type | Default | Description |
|---|---|---|---|
| `Severity` | `DiagnosticSeverity` | — | The severity band controlling how the overlay colors the entry and whether `HasErrors` flips. |
| `Message` | `string` | — | Human-readable body rendered into the overlay panel and after the first pipe in the `X-Pennington-Diagnostic` header value. |
| `Source` | `string?` | `null` | Optional label identifying the producer (e.g. `"XrefResolver"`); rendered as the small subtitle next to the severity pill in the overlay and appended after a second pipe in the header value when non-null. |

## `DiagnosticSeverity`

```csharp:xmldocid
T:Pennington.Diagnostics.DiagnosticSeverity
```

_Three-value enum in ascending severity order. The overlay's aggregate badge color picks the highest present severity (`Error` > `Warning` > `Info`); `DiagnosticContext.HasErrors` fires only on `Error`._

### Values

_Declaration order._

| Name | Value | Description |
|---|---|---|
| `Info` | `0` | Informational diagnostic; counted toward the badge total but renders in blue and never flips `HasErrors`. |
| `Warning` | `1` | Recoverable issue (for example, an unresolved xref); contributes to the overlay warning count and the amber badge color. |
| `Error` | `2` | Fatal issue for the request's output; flips `HasErrors` and renders with the red badge color. |

## Dev-mode overlay and `X-Pennington-Diagnostic` header

_Two transports surface the accumulated diagnostics, both wired inside `UsePennington` via the response-processor pipeline._

| Transport | Type | Availability | Shape |
|---|---|---|---|
| Response header | `X-Pennington-Diagnostic` | Every request that has `HasAny` | One header value per diagnostic, pipe-delimited: `Severity|Message` (or `Severity|Message|Source` when `Source` is non-null). Emitted by `ResponseProcessingMiddleware.WriteDiagnosticHeaders` just before the buffered body is flushed. |
| On-page overlay | `DiagnosticOverlayProcessor` (`Order = 30`, `IResponseProcessor`) | Requests where `DOTNET_WATCH` is set, status is `2xx`, and content type contains `text/html` | Floating badge injected before `</body>` summarizing error/warning counts; clicking expands a panel listing every diagnostic. Re-renders on the `spa:diagnostics` DOM event for SPA navigations. |

## Example

_No `examples/` project exercises `DiagnosticContext` directly today; the canonical in-repo consumer is `XrefResolvingService` reporting unresolved uids. A minimal xmldocid-backed example belongs in an extensibility sample before this page lands._

TODO: add an example method under `examples/ExtensibilityLabExample/` that injects `DiagnosticContext` and calls `AddWarning`, then replace this block with a `csharp:xmldocid,bodyonly` fence targeting it.

## See also

- Related reference: [Build report fields](/reference/diagnostics/build-report)
- Related reference: [Response processing interfaces](/reference/extension-points/response-processing)
- How-to: [Write a response processor](/how-to/extensibility/response-processor)
- Background: [Diagnostics and the dev overlay](/explanation/operations/diagnostics)
