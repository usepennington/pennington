namespace Pennington.Generation;

using Diagnostics;
using Routing;

/// <summary>
/// Single diagnostic entry captured during a static build.
/// </summary>
/// <param name="Severity">Severity level of the diagnostic.</param>
/// <param name="Route">Route the diagnostic relates to, if any.</param>
/// <param name="Message">Human-readable message.</param>
/// <param name="Exception">Optional exception captured with the diagnostic.</param>
/// <param name="SourceFile">Optional source file path associated with the diagnostic.</param>
public record BuildDiagnostic(
    DiagnosticSeverity Severity,
    ContentRoute? Route,
    string Message,
    Exception? Exception = null,
    string? SourceFile = null);