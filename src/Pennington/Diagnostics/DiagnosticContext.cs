namespace Pennington.Diagnostics;

/// <summary>
/// Scoped service that accumulates diagnostics for a single HTTP request.
/// Registered as scoped in DI — fresh instance per request, no thread-safety needed.
/// </summary>
public sealed class DiagnosticContext
{
    private readonly List<Diagnostic> _diagnostics = [];

    /// <summary>Appends a pre-built diagnostic to the context.</summary>
    public void Add(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);

    /// <summary>Records a warning-severity diagnostic with the given message and optional source label.</summary>
    public void AddWarning(string message, string? source = null)
        => _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, message, source));

    /// <summary>Records an error-severity diagnostic with the given message and optional source label.</summary>
    public void AddError(string message, string? source = null)
        => _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, message, source));

    /// <summary>Records an info-severity diagnostic with the given message and optional source label.</summary>
    public void AddInfo(string message, string? source = null)
        => _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Info, message, source));

    /// <summary>Diagnostics accumulated for the current request, in insertion order.</summary>
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    /// <summary>True when at least one diagnostic has been recorded.</summary>
    public bool HasAny => _diagnostics.Count > 0;

    /// <summary>True when at least one recorded diagnostic has <see cref="DiagnosticSeverity.Error"/> severity.</summary>
    public bool HasErrors => _diagnostics.Exists(d => d.Severity is DiagnosticSeverity.Error);
}