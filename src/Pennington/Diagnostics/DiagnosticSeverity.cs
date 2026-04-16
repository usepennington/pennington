namespace Pennington.Diagnostics;

/// <summary>Severity levels for a request-scoped <see cref="Diagnostic"/>.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Informational note; safe to ignore in production.</summary>
    Info,
    /// <summary>Potential issue that does not block rendering.</summary>
    Warning,
    /// <summary>Failure that indicates broken content or misconfiguration.</summary>
    Error,
}