namespace Pennington.Generation;

/// <summary>
/// A build-time auditor that inspects discovered content and produces
/// <see cref="BuildDiagnostic"/>s. Auditors run during dev-mode startup and
/// on file changes (results feed the diagnostic overlay), and again at build
/// time (results land in <see cref="BuildReport.Diagnostics"/>).
/// <para>
/// Implement and register via <c>services.AddTransient&lt;IBuildAuditor, MyAuditor&gt;()</c>.
/// Auditors should be cheap to invoke repeatedly because the runner re-runs every
/// registered auditor on every content-tree change.
/// </para>
/// </summary>
public interface IBuildAuditor
{
    /// <summary>
    /// Stable identifier for the auditor (e.g. <c>translation.audit</c>). Surfaced on
    /// <see cref="BuildDiagnostic"/>s via the source label so users can filter or
    /// suppress by code in CI dashboards.
    /// </summary>
    string Code { get; }

    /// <summary>Runs the auditor against <paramref name="context"/> and returns its diagnostics.</summary>
    Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(BuildAuditContext context, CancellationToken cancellationToken);
}
