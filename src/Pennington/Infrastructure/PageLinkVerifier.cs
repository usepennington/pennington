namespace Pennington.Infrastructure;

using Content;
using Generation;
using Microsoft.AspNetCore.Routing;
using Pipeline;
using Routing;

/// <summary>
/// Holds a <see cref="LinkVerificationService"/> built from the current corpus of known
/// routes and copied assets. File-watched so the known-paths set refreshes when content
/// changes. Lets <see cref="PageLinkAuditProcessor"/> verify per-page links without
/// running a corpus-wide rendered audit.
/// </summary>
public sealed class PageLinkVerifier : IFileWatchAware
{
    private readonly AsyncLazy<LinkVerificationService> _verifierLazy;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>Creates the verifier; the underlying <see cref="LinkVerificationService"/> is built lazily on first request.</summary>
    public PageLinkVerifier(
        IEnumerable<IContentService> contentServices,
        IEnumerable<IContentEmitter> contentEmitters,
        EndpointDataSource endpointDataSource,
        OutputOptions outputOptions)
    {
        _verifierLazy = new AsyncLazy<LinkVerificationService>(
            () => BuildAsync(contentServices, contentEmitters, endpointDataSource, outputOptions));
    }

    /// <summary>Returns the current verifier; rebuilds on first access after a file change.</summary>
    public Task<LinkVerificationService> GetVerifierAsync() => _verifierLazy.Value;

    private static async Task<LinkVerificationService> BuildAsync(
        IEnumerable<IContentService> contentServices,
        IEnumerable<IContentEmitter> contentEmitters,
        EndpointDataSource endpointDataSource,
        OutputOptions outputOptions)
    {
        var knownRoutes = new List<ContentRoute>();
        var copiedAssetPaths = new List<string>();

        await foreach (var item in contentServices.DiscoverAllAsync())
        {
            // Llms-only items have no HTML page at the canonical URL — a link from a
            // regular page to that URL would 404, so leaving them out of knownRoutes
            // lets the verifier flag the broken reference.
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

        // Mirror OutputGenerationService.CreateContentFilesAsync so files emitted via
        // GetContentToCreateAsync (per-subtree llms.txt, sitemap, …) are treated as
        // known assets rather than broken links.
        foreach (var emitter in contentServices.WithStandaloneEmitters(contentEmitters))
        {
            foreach (var item in await emitter.GetContentToCreateAsync())
            {
                copiedAssetPaths.Add(item.OutputPath.Value);
            }
        }

        knownRoutes.AddRange(MapGetRouteDiscovery.Discover(endpointDataSource));

        return new LinkVerificationService(knownRoutes, copiedAssetPaths, outputOptions.BaseUrl.Value);
    }
}
