namespace Pennington.Diagnostics;

/// <summary>
/// Scoped service that accumulates diagnostics for a single HTTP request.
/// Registered as scoped in DI — fresh instance per request, no thread-safety needed.
/// </summary>
public sealed class DiagnosticContext
{
    private readonly List<Diagnostic> _diagnostics = [];

    public void Add(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);

    public void AddWarning(string message, string? source = null)
        => _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, message, source));

    public void AddError(string message, string? source = null)
        => _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, message, source));

    public void AddInfo(string message, string? source = null)
        => _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Info, message, source));

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;
    public bool HasAny => _diagnostics.Count > 0;
    public bool HasErrors => _diagnostics.Exists(d => d.Severity is DiagnosticSeverity.Error);
}