namespace Pennington.Generation;

using System.Collections.Immutable;
using System.Diagnostics;
using Pennington.Diagnostics;
using Pennington.Routing;

public sealed class BuildReportBuilder
{
    private readonly List<BuildDiagnostic> _diagnostics = [];
    private readonly List<BrokenLink> _brokenLinks = [];
    private readonly List<ContentRoute> _generatedPages = [];
    private readonly List<ContentRoute> _skippedPages = [];
    private readonly List<ContentRoute> _failedPages = [];
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public void AddDiagnostic(BuildDiagnostic diagnostic) => _diagnostics.Add(diagnostic);

    public void AddInfo(ContentRoute route, string message)
        => _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Info, route, message));

    public void AddWarning(ContentRoute route, string message)
        => _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Warning, route, message));

    public void AddWarning(string message, string? sourceFile = null)
        => _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Warning, null, message, SourceFile: sourceFile));

    public void AddError(ContentRoute route, string message, Exception? exception = null)
    {
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Error, route, message, exception));
        _failedPages.Add(route);
    }

    public void AddError(string message, Exception? exception = null, string? sourceFile = null)
    {
        _diagnostics.Add(new BuildDiagnostic(DiagnosticSeverity.Error, null, message, exception, sourceFile));
    }

    public void AddBrokenLink(BrokenLink link) => _brokenLinks.Add(link);

    public void AddGeneratedPage(ContentRoute route) => _generatedPages.Add(route);

    public void AddSkippedPage(ContentRoute route) => _skippedPages.Add(route);

    public BuildReport Build()
    {
        _stopwatch.Stop();
        return new BuildReport(
            diagnostics: [.. _diagnostics],
            brokenLinks: [.. _brokenLinks],
            generatedPages: [.. _generatedPages],
            skippedPages: [.. _skippedPages],
            failedPages: [.. _failedPages],
            duration: _stopwatch.Elapsed
        );
    }
}
