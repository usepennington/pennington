namespace Pennington.Generation;

using System.Collections.Immutable;
using Artifacts;
using Content;
using Diagnostics;
using Pipeline;

/// <summary>
/// <see cref="IBuildAuditor"/> that polices the artifact tier's declared URL territories:
/// warns when a content route falls inside an <see cref="ArtifactClaim"/> (the artifact router
/// would shadow it in dev and the build output could collide), and when two claims from
/// different owners overlap (resolution order would silently decide the winner). Rides the
/// audit pipeline, so the same warnings reach the dev overlay, <c>diag warnings</c>, and the
/// build report. Consults claims and route discovery only — never artifact discovery, which
/// may materialize the projection.
/// </summary>
public sealed class ClaimConflictAuditor : IBuildAuditor
{
    private readonly IEnumerable<IContentService> _contentServices;
    private readonly IEnumerable<IArtifactContentService> _artifactServices;

    /// <summary>Stable identifier surfaced on every diagnostic this auditor emits.</summary>
    public string Code => "artifacts.namespace";

    /// <summary>Wires the auditor to the content discovery surface and the artifact tier.</summary>
    public ClaimConflictAuditor(
        IEnumerable<IContentService> contentServices,
        IEnumerable<IArtifactContentService> artifactServices)
    {
        _contentServices = contentServices;
        _artifactServices = artifactServices;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(BuildAuditContext context, CancellationToken cancellationToken)
    {
        var claims = _artifactServices.SelectMany(s => s.Claims).ToImmutableList();
        if (claims.IsEmpty)
        {
            return [];
        }

        var diagnostics = ImmutableList.CreateBuilder<BuildDiagnostic>();

        await foreach (var item in _contentServices.DiscoverAllAsync(cancellationToken))
        {
            // Llms-only routes have no HTML page to shadow.
            if (item.Source is LlmsOnlySource)
            {
                continue;
            }

            var path = item.Route.CanonicalPath.Value;
            var claimed = claims.FirstOrDefault(c => c.Matches(path));
            if (claimed is not null)
            {
                diagnostics.Add(new BuildDiagnostic(
                    Severity: DiagnosticSeverity.Warning,
                    Route: item.Route,
                    Message: $"Content route '{path}' falls inside the artifact territory '{claimed.Pattern}' "
                        + $"({claimed.Owner}: {claimed.Description}). The artifact router shadows it in dev when the "
                        + $"artifact resolves, and the static build may collide on the output file.",
                    SourceFile: Code));
            }
        }

        foreach (var (a, b) in CrossOwnerOverlaps(claims))
        {
            diagnostics.Add(new BuildDiagnostic(
                Severity: DiagnosticSeverity.Warning,
                Route: null,
                Message: $"Artifact territories overlap across owners: '{a.Pattern}' ({a.Owner}) and "
                    + $"'{b.Pattern}' ({b.Owner}). Registration order decides which serves a contested path.",
                SourceFile: Code));
        }

        return diagnostics.ToImmutable();
    }

    /// <summary>
    /// Pairwise overlap check between claims of different owners. Static analysis only — prefix
    /// containment, suffix containment, and an exact path matched by another owner's claim.
    /// Prefix-vs-suffix pairs are undecidable without enumerating paths and are skipped.
    /// </summary>
    private static IEnumerable<(ArtifactClaim A, ArtifactClaim B)> CrossOwnerOverlaps(ImmutableList<ArtifactClaim> claims)
    {
        for (var i = 0; i < claims.Count; i++)
        {
            for (var j = i + 1; j < claims.Count; j++)
            {
                var a = claims[i];
                var b = claims[j];
                if (string.Equals(a.Owner, b.Owner, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (Overlap(a, b))
                {
                    yield return (a, b);
                }
            }
        }
    }

    private static bool Overlap(ArtifactClaim a, ArtifactClaim b) => (a.Shape.Value, b.Shape.Value) switch
    {
        (ExactClaim e, _) => b.Matches(e.Path.Value),
        (_, ExactClaim e) => a.Matches(e.Path.Value),
        (PrefixClaim x, PrefixClaim y) =>
            (x.Prefix.Value.StartsWith(y.Prefix.Value, StringComparison.OrdinalIgnoreCase)
                || y.Prefix.Value.StartsWith(x.Prefix.Value, StringComparison.OrdinalIgnoreCase))
            && (x.Suffix is null || y.Suffix is null || string.Equals(x.Suffix, y.Suffix, StringComparison.OrdinalIgnoreCase)),
        (SuffixClaim x, SuffixClaim y) =>
            x.Suffix.EndsWith(y.Suffix, StringComparison.OrdinalIgnoreCase)
            || y.Suffix.EndsWith(x.Suffix, StringComparison.OrdinalIgnoreCase),
        _ => false,
    };
}
