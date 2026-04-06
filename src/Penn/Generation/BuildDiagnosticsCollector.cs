namespace Penn.Generation;

using System.Collections.Concurrent;
using Penn.Routing;

/// <summary>
/// Thread-safe collector for diagnostics that arise during request handling
/// (e.g., rendering-time errors from code block preprocessors).
/// Registered as a singleton; drained by the build service after all pages are fetched.
/// </summary>
public sealed class BuildDiagnosticsCollector
{
    private readonly ConcurrentBag<BuildDiagnostic> _diagnostics = new();

    public void AddWarning(ContentRoute? route, string message, string? sourceFile = null)
        => _diagnostics.Add(new BuildDiagnostic(new DiagnosticWarning(route, message, sourceFile)));

    public void AddError(ContentRoute? route, string message, string? sourceFile = null)
        => _diagnostics.Add(new BuildDiagnostic(new DiagnosticError(route, message, SourceFile: sourceFile)));

    public IReadOnlyList<BuildDiagnostic> Drain()
    {
        var items = _diagnostics.ToList();
        _diagnostics.Clear();
        return items;
    }
}
