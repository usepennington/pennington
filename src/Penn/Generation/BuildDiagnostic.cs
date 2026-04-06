namespace Penn.Generation;

using Penn.Diagnostics;
using Penn.Routing;

public record BuildDiagnostic(
    DiagnosticSeverity Severity,
    ContentRoute? Route,
    string Message,
    Exception? Exception = null,
    string? SourceFile = null);
