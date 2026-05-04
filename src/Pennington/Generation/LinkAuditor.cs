namespace Pennington.Generation;

using System.Collections.Immutable;
using Content;
using Diagnostics;
using Infrastructure;
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
    private readonly EndpointDataSource _endpointDataSource;
    private readonly OutputOptions _outputOptions;

    /// <summary>Stable identifier surfaced on every diagnostic this auditor emits.</summary>
    public string Code => "content.links";

    /// <summary>Wires the auditor to the content discovery surface, the endpoint table, and the output options.</summary>
    public LinkAuditor(
        IEnumerable<IContentService> contentServices,
        EndpointDataSource endpointDataSource,
        OutputOptions outputOptions)
    {
        _contentServices = contentServices;
        _endpointDataSource = endpointDataSource;
        _outputOptions = outputOptions;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(RenderedAuditContext context, CancellationToken cancellationToken)
    {
        var knownRoutes = new List<Routing.ContentRoute>();
        var copiedAssetPaths = new List<string>();

        foreach (var service in _contentServices)
        {
            await foreach (var item in service.DiscoverAsync().WithCancellation(cancellationToken))
            {
                // Llms-only items have no HTML page at the canonical URL — a
                // link from a regular page to that URL would 404, so leaving
                // them out of knownRoutes lets the link verifier flag the
                // broken reference instead of silently allowing it.
                if (item.Source is Pipeline.LlmsOnlySource) continue;
                knownRoutes.Add(item.Route);
            }

            foreach (var copy in await service.GetContentToCopyAsync())
            {
                copiedAssetPaths.Add(copy.OutputPath.Value);
            }
        }

        knownRoutes.AddRange(MapGetRouteDiscovery.Discover(_endpointDataSource));

        var verifier = new LinkVerificationService(knownRoutes, copiedAssetPaths, _outputOptions.BaseUrl.Value);
        var diagnostics = ImmutableList.CreateBuilder<BuildDiagnostic>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var page in context.Pages)
        {
            // Locale fallbacks reuse the same source file, but the rendered HTML
            // differs per locale (translated chrome around English body), so do not
            // dedupe on source file. Dedupe on canonical path instead — that's the
            // unit each rendered page renders at.
            if (!visited.Add(page.Route.CanonicalPath.Value)) continue;

            var html = await context.GetRenderedHtmlAsync(page.Route, cancellationToken);
            if (html is null) continue;

            foreach (var result in verifier.VerifyLinks(page.Route, html))
            {
                if (result.Value is not BrokenLinkResult broken) continue;
                diagnostics.Add(new BuildDiagnostic(
                    Severity: DiagnosticSeverity.Warning,
                    Route: page.Route,
                    Message: $"Broken link to {broken.Url} ({broken.Reason})",
                    SourceFile: $"{Code}/{broken.Type}/{broken.Url}"));
            }
        }

        return diagnostics.ToImmutable();
    }
}
