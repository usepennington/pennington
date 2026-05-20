namespace Pennington.Generation;

/// <summary>
/// A build-time auditor that needs the post-pipeline rendered HTML for each
/// page. Runs after every <see cref="IBuildAuditor"/> in <see cref="AuditRunner"/>
/// and writes into the same <see cref="IAuditCache"/>, so consumers (the dev
/// overlay, the build report) don't need to know which interface produced a
/// given diagnostic.
/// <para>
/// Prefer <see cref="IBuildAuditor"/> when a structural check on
/// <see cref="BuildAuditContext.Pages"/> is enough — this seam is for checks
/// (broken links, accessibility passes, etc.) that genuinely need rendered
/// markup. Each rendered audit costs an HTTP self-dispatch per page.
/// </para>
/// </summary>
public interface IRenderedAuditor
{
    /// <summary>Stable identifier for the auditor (e.g. <c>content.links</c>).</summary>
    string Code { get; }

    /// <summary>Runs the auditor against <paramref name="context"/> and returns its diagnostics.</summary>
    Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(RenderedAuditContext context, CancellationToken cancellationToken);
}