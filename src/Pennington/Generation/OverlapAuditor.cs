namespace Pennington.Generation;

using System.Collections.Immutable;
using Content;
using Diagnostics;

/// <summary>
/// <see cref="IBuildAuditor"/> that flags markdown content sources whose
/// <see cref="IMarkdownContentSource.AbsoluteContentRoot"/> directories overlap
/// without an explicit <see cref="IMarkdownContentSource.ExcludePaths"/> carve-out.
/// Wraps <see cref="MarkdownSourceOverlapDetector"/> so the same warnings reach
/// the dev overlay (via <see cref="AuditCache"/>) and the build report.
/// </summary>
public sealed class OverlapAuditor : IBuildAuditor
{
    private readonly IEnumerable<IContentService> _contentServices;

    /// <summary>Stable identifier surfaced on every diagnostic this auditor emits.</summary>
    public string Code => "content.overlap";

    /// <summary>Wires the auditor to the registered content services.</summary>
    public OverlapAuditor(IEnumerable<IContentService> contentServices)
    {
        _contentServices = contentServices;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(BuildAuditContext context, CancellationToken cancellationToken)
    {
        var sources = _contentServices.OfType<IMarkdownContentSource>();
        var diagnostics = MarkdownSourceOverlapDetector.DetectOverlaps(sources)
            .Select(message => new BuildDiagnostic(
                Severity: DiagnosticSeverity.Warning,
                Route: null,
                Message: message,
                SourceFile: Code))
            .ToImmutableList();
        return Task.FromResult<IReadOnlyList<BuildDiagnostic>>(diagnostics);
    }
}
