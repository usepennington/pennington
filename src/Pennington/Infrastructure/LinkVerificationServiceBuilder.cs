namespace Pennington.Infrastructure;

using Artifacts;
using Content;
using Generation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Pipeline;
using Routing;

/// <summary>
/// Shared builder for <see cref="LinkVerificationService"/> instances. Both the in-pipeline
/// per-page verifier (<see cref="PageLinkVerifier"/>) and the corpus-wide build auditor
/// (<see cref="LinkAuditor"/>) collect the same <c>(knownRoutes, copiedAssetPaths, MapGet)</c>
/// triple from the content surface. Artifact-tier URLs are folded in one of two ways, selected
/// by <c>enumerateArtifactRoutes</c>:
/// <list type="bullet">
/// <item><c>false</c> (request path): only the cheap, options-derived
/// <see cref="IArtifactContentService.Claims"/> are folded — a link into a claimed territory is
/// trusted. Artifact discovery fans out through the site projection, which must never run on
/// the request path (see <see cref="ISiteProjection"/>).</item>
/// <item><c>true</c> (build mode): exact artifact routes are enumerated into the known set and
/// no claims are folded, so a typo inside a claimed territory is still flagged.</item>
/// </list>
/// </summary>
public static class LinkVerificationServiceBuilder
{
    /// <summary>
    /// Builds a verifier from the content services, artifact services, endpoint table, and
    /// output options. Pass <paramref name="enumerateArtifactRoutes"/>=true only from build-mode
    /// callers. Pass <paramref name="webRootFileProvider"/> (the host's <c>WebRootFileProvider</c>)
    /// so wwwroot/RCL assets — copied by the build but owned by no content service — are treated
    /// as known assets.
    /// </summary>
    public static async Task<LinkVerificationService> BuildAsync(
        IEnumerable<IContentService> contentServices,
        IEnumerable<IArtifactContentService> artifactServices,
        EndpointDataSource endpointDataSource,
        OutputOptions outputOptions,
        bool enumerateArtifactRoutes,
        IFileProvider? webRootFileProvider = null,
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

        if (webRootFileProvider != null)
        {
            // wwwroot + RCL assets are copied by OutputGenerationService.CopyStaticAssetsAsync via the
            // same walk; fold them in so absolute references to wwwroot files (the documented home for
            // shared assets) resolve instead of being flagged as broken.
            foreach (var asset in StaticWebAssetWalker.Walk(webRootFileProvider))
            {
                copiedAssetPaths.Add(asset.RelativePath);
            }
        }

        IEnumerable<ArtifactClaim>? claims = null;
        if (enumerateArtifactRoutes)
        {
            // Mirrors OutputGenerationService.WriteArtifactsAsync so every artifact route the
            // build writes (per-subtree llms.txt files, search shards, PDFs) is a known path.
            foreach (var service in artifactServices)
            {
                await foreach (var item in service.DiscoverAsync().WithCancellation(cancellationToken))
                {
                    knownRoutes.Add(item.Route);
                }
            }
        }
        else
        {
            claims = artifactServices.SelectMany(service => service.Claims).ToList();
        }

        knownRoutes.AddRange(MapGetRouteDiscovery.Discover(endpointDataSource));

        return new LinkVerificationService(knownRoutes, copiedAssetPaths, outputOptions.BaseUrl.Value, claims);
    }
}
