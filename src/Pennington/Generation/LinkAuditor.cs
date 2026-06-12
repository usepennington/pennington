namespace Pennington.Generation;

using System.Collections.Immutable;
using Artifacts;
using Content;
using Diagnostics;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// <see cref="IRenderedAuditor"/> that fetches each TOC page through the live
/// pipeline and runs <see cref="LinkVerificationService"/> over its rendered
/// HTML, surfacing broken internal links in the dev overlay (per page) and
/// the build report.
/// </summary>
public sealed class LinkAuditor : IRenderedAuditor
{
    private readonly IEnumerable<IContentService> _contentServices;
    private readonly IEnumerable<IArtifactContentService> _artifactServices;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly OutputOptions _outputOptions;
    private readonly IWebHostEnvironment _environment;

    /// <summary>Stable identifier surfaced on every diagnostic this auditor emits.</summary>
    public string Code => "content.links";

    /// <summary>Wires the auditor to the content discovery surface, the artifact tier, the endpoint table, the output options, and the host environment (for wwwroot/RCL assets).</summary>
    public LinkAuditor(
        IEnumerable<IContentService> contentServices,
        IEnumerable<IArtifactContentService> artifactServices,
        EndpointDataSource endpointDataSource,
        OutputOptions outputOptions,
        IWebHostEnvironment environment)
    {
        _contentServices = contentServices;
        _artifactServices = artifactServices;
        _endpointDataSource = endpointDataSource;
        _outputOptions = outputOptions;
        _environment = environment;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(RenderedAuditContext context, CancellationToken cancellationToken)
    {
        var verifier = await LinkVerificationServiceBuilder.BuildAsync(
            _contentServices,
            _artifactServices,
            _endpointDataSource,
            _outputOptions,
            enumerateArtifactRoutes: true,
            _environment.WebRootFileProvider,
            cancellationToken);
        var diagnostics = ImmutableList.CreateBuilder<BuildDiagnostic>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var route in context.Pages)
        {
            // Locale fallbacks reuse the same source file, but the rendered HTML
            // differs per locale (translated chrome around English body), so do not
            // dedupe on source file. Dedupe on canonical path instead — that's the
            // unit each rendered page renders at.
            if (!visited.Add(route.CanonicalPath.Value))
            {
                continue;
            }

            var html = await context.GetRenderedHtmlAsync(route, cancellationToken);
            if (html is null)
            {
                continue;
            }

            foreach (var result in verifier.VerifyLinks(route, html))
            {
                if (result.Value is not BrokenLinkResult broken)
                {
                    continue;
                }

                diagnostics.Add(new BuildDiagnostic(
                    Severity: DiagnosticSeverity.Warning,
                    Route: route,
                    Message: $"Broken link to {broken.Url} ({broken.Reason})",
                    SourceFile: $"{Code}/{broken.Type}/{broken.Url}"));
            }
        }

        return diagnostics.ToImmutable();
    }
}