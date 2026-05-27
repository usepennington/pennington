namespace Pennington.Infrastructure;

using Content;
using Generation;
using Microsoft.AspNetCore.Routing;
using Pipeline;
using Routing;

/// <summary>
/// Shared builder for <see cref="LinkVerificationService"/> instances. Both the in-pipeline
/// per-page verifier (<see cref="PageLinkVerifier"/>) and the corpus-wide build auditor
/// (<see cref="LinkAuditor"/>) collect the same <c>(knownRoutes, copiedAssetPaths, MapGet)</c>
/// triple from the content surface; only the build auditor additionally folds in emitter
/// outputs. The <c>includeEmitterOutputs</c> flag on <see cref="BuildAsync"/> selects between
/// the two modes.
/// <para>
/// In-pipeline consumers must pass <c>false</c>: emitters fan out through the shared site
/// projection, which self-fetches every page, which re-enters the per-request
/// <see cref="PageLinkAuditProcessor"/> — pulling that work onto a verifier-build task
/// deadlocks.
/// </para>
/// </summary>
public static class LinkVerificationServiceBuilder
{
    /// <summary>Builds a verifier from the content services, endpoint table, and output options. Pass <paramref name="includeEmitterOutputs"/>=true only from build-mode callers.</summary>
    public static async Task<LinkVerificationService> BuildAsync(
        IEnumerable<IContentService> contentServices,
        IEnumerable<IContentEmitter> contentEmitters,
        EndpointDataSource endpointDataSource,
        OutputOptions outputOptions,
        bool includeEmitterOutputs,
        CancellationToken cancellationToken = default)
    {
        var knownRoutes = new List<ContentRoute>();
        var copiedAssetPaths = new List<string>();

        await foreach (var item in contentServices.DiscoverAllAsync(cancellationToken))
        {
            // Llms-only items have no HTML page at the canonical URL — leaving them out of
            // knownRoutes lets the verifier flag the broken reference instead of silently
            // allowing it.
            if (item.Source is LlmsOnlySource)
            {
                continue;
            }

            knownRoutes.Add(item.Route);
        }

        foreach (var copy in await contentServices.CollectContentToCopyAsync())
        {
            copiedAssetPaths.Add(copy.OutputPath.Value);
        }

        if (includeEmitterOutputs)
        {
            // Mirrors OutputGenerationService.CreateContentFilesAsync so files emitted via
            // GetContentToCreateAsync (e.g. per-subtree llms.txt files) are treated as known
            // assets rather than broken links.
            foreach (var emitter in contentServices.WithStandaloneEmitters(contentEmitters))
            {
                foreach (var emitted in await emitter.GetContentToCreateAsync())
                {
                    copiedAssetPaths.Add(emitted.OutputPath.Value);
                }
            }
        }

        knownRoutes.AddRange(MapGetRouteDiscovery.Discover(endpointDataSource));

        return new LinkVerificationService(knownRoutes, copiedAssetPaths, outputOptions.BaseUrl.Value);
    }
}
