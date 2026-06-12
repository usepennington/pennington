namespace Pennington.Search;

using System.Collections.Immutable;
using Artifacts;
using Pipeline;
using Routing;

/// <summary>
/// Artifact-tier façade over <see cref="SearchArtifactService"/>: claims the sharded index
/// territory under <c>/search/</c>, serves shards in dev through the artifact router, and
/// enumerates the same files for the static build — one byte path for both surfaces.
/// Transient so each resolution captures the current file-watched service.
/// </summary>
public sealed class SearchArtifactContentService : IArtifactContentService
{
    private static readonly ImmutableList<ArtifactClaim> ClaimList =
        [new ArtifactClaim("search", new PrefixClaim(new UrlPath("/search/"), ".json"), "sharded search index")];

    private readonly SearchArtifactService _service;

    /// <summary>Creates the façade over the given <see cref="SearchArtifactService"/>.</summary>
    public SearchArtifactContentService(SearchArtifactService service) => _service = service;

    /// <inheritdoc/>
    public ImmutableList<ArtifactClaim> Claims => ClaimList;

    /// <inheritdoc/>
    public async Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken)
    {
        var bytes = await _service.GetArtifactAsync(relativePath);
        return bytes is null ? null : new ArtifactContent(bytes, "application/json; charset=utf-8");
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var files = await _service.GetArtifactFilesAsync();
        foreach (var path in files.Keys)
        {
            yield return new DiscoveredItem(
                new ContentRoute
                {
                    CanonicalPath = new UrlPath("/" + path),
                    OutputFile = new FilePath(path),
                },
                new GeneratedSource("application/json"));
        }
    }
}
