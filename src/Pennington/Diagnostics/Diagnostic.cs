namespace Pennington.Diagnostics;

/// <summary>
/// A diagnostic produced during HTTP request handling.
/// Route-agnostic — the route is known by the request context, not the diagnostic.
/// </summary>
/// <param name="Severity">Severity of the diagnostic.</param>
/// <param name="Message">Human-readable description of the problem.</param>
/// <param name="Source">Optional identifier for the component that raised the diagnostic.</param>
public sealed record Diagnostic(DiagnosticSeverity Severity, string Message, string? Source = null);