namespace Pennington.Artifacts;

using System.Collections.Immutable;
using Pipeline;

/// <summary>
/// The corpus-derived artifact tier: a service whose URLs and bytes derive from the rendered site
/// (via <see cref="ISiteProjection"/>) or from configuration — search shards, llms.txt files,
/// book PDFs, well-known verification files. Registered ONLY under this interface, never as
/// <see cref="Content.IContentService"/>, so request-path discovery walkers
/// (<see cref="Content.PageResolver"/>), the projection's own input set, sitemap, and the record
/// registry structurally cannot trigger its potentially expensive discovery.
/// <para>
/// One byte path serves both surfaces: <see cref="ResolveAsync"/> answers dev requests through
/// the artifact router and produces the static build's output for every route
/// <see cref="DiscoverAsync"/> enumerates — dev/build parity by construction. Routes that should
/// exist only in dev (e.g. live book previews) are resolvable via <see cref="Claims"/> without
/// being enumerated.
/// </para>
/// </summary>
public interface IArtifactContentService
{
    /// <summary>
    /// URL territories this service serves. Options-derived and consulted on every request —
    /// must be cheap and must not trigger discovery, the projection, or any lazy corpus work.
    /// </summary>
    ImmutableList<ArtifactClaim> Claims { get; }

    /// <summary>
    /// Returns the bytes for <paramref name="relativePath"/> (no leading slash, e.g.
    /// <c>search/en/index.json</c>), or null to decline so the request falls through to content
    /// routing. May materialize the projection, build an index, or run Chromium on demand.
    /// </summary>
    Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates every artifact route the static build should write, as
    /// <see cref="GeneratedSource"/> items. May consume the projection — the build invokes this
    /// outside any request, after the page crawl has primed the render cache. Never called on
    /// the request path.
    /// </summary>
    IAsyncEnumerable<DiscoveredItem> DiscoverAsync();
}
