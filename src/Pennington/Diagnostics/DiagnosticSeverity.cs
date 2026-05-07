namespace Pennington.Diagnostics;

/// <summary>Severity levels for a request-scoped <see cref="Diagnostic"/>.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Potential issue that does not block rendering.</summary>
    Warning,
    /// <summary>Failure that indicates broken content or misconfiguration.</summary>
    Error,
    /// <summary>Informational notice about degraded but non-broken behavior.</summary>
    Info,
}