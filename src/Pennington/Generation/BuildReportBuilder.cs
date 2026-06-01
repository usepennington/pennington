namespace Pennington.Generation;

using System.Diagnostics;
using Diagnostics;
using Routing;

/// <summary>
/// Mutable accumulator for a single static-build run that produces a finalized <see cref="BuildReport"/>.
/// </summary>
internal sealed class BuildReportBuilder
{
    private readonly List<BuildDiagnostic> _diagnostics = [];
    private readonly List<ContentRoute> _generatedPages = [];
    private readonly List<ContentRoute> _skippedPages = [];
    private readonly List<ContentRoute> _failedPages = [];
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    /// <summary>Appends a pre-built diagnostic to the report.</summary>
    public void AddDiagnostic(BuildDiagnostic diagnostic) => _diagnostics.Add(diagnostic);

    /// <summary>Records a warning diagnostic attached to a specific route.</summary>
    public void AddWarning(ContentRoute route, string message)
        => _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Warning, route, message));

    /// <summary>Records a warning diagnostic not tied to a specific route, optionally with a source file.</summary>
    public void AddWarning(string message, string? sourceFile = null)
        => _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Warning, null, message, SourceFile: sourceFile));

    /// <summary>Records an error diagnostic for a route and marks the route as failed.</summary>
    public void AddError(ContentRoute route, string message, Exception? exception = null)
    {
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Error, route, message, exception));
        _failedPages.Add(route);
    }

    /// <summary>Records an error diagnostic not tied to a specific route.</summary>
    public void AddError(string message, Exception? exception = null, string? sourceFile = null)
    {
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Error, null, message, exception, sourceFile));
    }

    /// <summary>Marks <paramref name="route"/> as successfully generated.</summary>
    public void AddGeneratedPage(ContentRoute route) => _generatedPages.Add(route);

    /// <summary>Marks <paramref name="route"/> as skipped (e.g. because it is a draft).</summary>
    public void AddSkippedPage(ContentRoute route) => _skippedPages.Add(route);

    /// <summary>Stops the timer and returns an immutable <see cref="BuildReport"/> for the run.</summary>
    public BuildReport Build()
    {
        _stopwatch.Stop();
        return new BuildReport(
            diagnostics: [.. _diagnostics],
            generatedPages: [.. _generatedPages],
            skippedPages: [.. _skippedPages],
            failedPages: [.. _failedPages],
            duration: _stopwatch.Elapsed
        );
    }
}