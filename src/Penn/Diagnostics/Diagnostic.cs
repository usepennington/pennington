namespace Pennington.Diagnostics;

/// <summary>
/// A diagnostic produced during HTTP request handling.
/// Route-agnostic — the route is known by the request context, not the diagnostic.
/// </summary>
public sealed record Diagnostic(DiagnosticSeverity Severity, string Message, string? Source = null);
