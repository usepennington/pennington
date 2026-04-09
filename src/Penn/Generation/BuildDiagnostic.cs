namespace Pennington.Generation;

using Pennington.Diagnostics;
using Pennington.Routing;

public record BuildDiagnostic(
    DiagnosticSeverity Severity,
    ContentRoute? Route,
    string Message,
    Exception? Exception = null,
    string? SourceFile = null);
