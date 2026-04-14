---
title: Request-scoped diagnostics
description: DiagnosticContext, Diagnostic, DiagnosticSeverity, and how the dev-mode overlay surfaces them.
section: diagnostics
order: 20
tags: []
uid: reference.diagnostics.request-context
isDraft: true
search: false
llms: false
---

> **In this page.** `DiagnosticContext`, `Diagnostic`, `DiagnosticSeverity`, and how the dev-mode overlay surfaces them.
>
> **Not in this page.** Wiring custom log sinks.

## Summary

The per-request diagnostic accumulator: a scoped `DiagnosticContext` collects `Diagnostic` records (each carrying a `DiagnosticSeverity`) produced while handling a single HTTP request.
Namespace `Pennington.Diagnostics`; registered as scoped in DI by `AddPennington` and consumed by `ResponseProcessingMiddleware` (headers) and `DiagnosticOverlayProcessor` (dev overlay).

## Declaration

### `DiagnosticSeverity`

```csharp:xmldocid
T:Pennington.Diagnostics.DiagnosticSeverity
```

### `Diagnostic`

```csharp:xmldocid
T:Pennington.Diagnostics.Diagnostic
```

### `DiagnosticContext`

```csharp:xmldocid
T:Pennington.Diagnostics.DiagnosticContext
```

## Members --- `DiagnosticSeverity`

| Name | Value | Description |
|---|---|---|
| `Info` | `0` | Informational entry; surfaces in the overlay as a blue pill. |
| `Warning` | `1` | Recoverable condition; surfaces in the overlay as an amber pill and contributes to the overlay badge warning count. |
| `Error` | `2` | Failure condition; surfaces in the overlay as a red pill, contributes to the error count, and sets `DiagnosticContext.HasErrors`. |

## Members --- `Diagnostic`

A `sealed record` with positional parameters.

| Name | Type | Default | Description |
|---|---|---|---|
| `Severity` | `DiagnosticSeverity` | --- | Required. The entry's severity. |
| `Message` | `string` | --- | Required. Human-readable message text; HTML-escaped when rendered in the overlay. |
| `Source` | `string?` | `null` | Optional label identifying the component that produced the diagnostic. |

## Members --- `DiagnosticContext`

A `sealed class` holding a `List<Diagnostic>`. Instance members are listed alphabetically.

### `Add`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.Add(Pennington.Diagnostics.Diagnostic)
```

Appends a fully constructed `Diagnostic` to the accumulator.

### `AddError`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.AddError(System.String,System.String)
```

Appends a new `Diagnostic` with `Severity = Error`.

### `AddInfo`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.AddInfo(System.String,System.String)
```

Appends a new `Diagnostic` with `Severity = Info`.

### `AddWarning`

```csharp:xmldocid
M:Pennington.Diagnostics.DiagnosticContext.AddWarning(System.String,System.String)
```

Appends a new `Diagnostic` with `Severity = Warning`.

### `Diagnostics`

```csharp:xmldocid
P:Pennington.Diagnostics.DiagnosticContext.Diagnostics
```

Read-only view over the accumulated entries in insertion order.

### `HasAny`

```csharp:xmldocid
P:Pennington.Diagnostics.DiagnosticContext.HasAny
```

`true` when at least one diagnostic has been added.

### `HasErrors`

```csharp:xmldocid
P:Pennington.Diagnostics.DiagnosticContext.HasErrors
```

`true` when at least one accumulated diagnostic has `Severity = Error`.

## Lifetime and registration

| Aspect | Value |
|---|---|
| DI lifetime | `Scoped` --- one instance per HTTP request. |
| Registered by | `services.AddPennington(...)` in `PenningtonExtensions`. |
| Thread safety | Not thread-safe; one instance per request, mutated on the request thread. |
| Resolution pattern | `context.RequestServices.GetService<DiagnosticContext>()` or constructor-injected into scoped services. |

## How the dev-mode overlay surfaces entries

| Surface | Mechanism |
|---|---|
| `X-Pennington-Diagnostic` response header | `ResponseProcessingMiddleware` emits one header per accumulated entry, formatted `Severity|Message` or `Severity|Message|Source`. Emitted on every response where `HasAny` is true, regardless of mode. |
| Floating HTML overlay | `DiagnosticOverlayProcessor` (`IResponseProcessor`, `Order = 30`) injects a badge plus expandable panel before `</body>` on successful `text/html` responses. Gated on the `DOTNET_WATCH` environment variable being set. |
| Badge counts | The overlay badge shows the total error count, then warning count; if neither is present it shows the info count. Dot color is red for any error, amber for any warning, otherwise blue. |
| Entry pills | Each expanded entry renders a severity pill (`error` / `warning` / `info`), the optional `Source`, and the message (HTML-escaped). |
| SPA navigation updates | The overlay listens for the `spa:diagnostics` DOM event and replaces the displayed entries with `event.detail`; entries delivered this way come from the SPA data endpoint rather than the response header. |

## See also

- Related reference: [Build report fields](/reference/diagnostics/build-report)
- Related reference: [Response processing interfaces](/reference/extension-points/response-processing)
- Related reference: [DI and middleware extension methods](/reference/host/extensions)
