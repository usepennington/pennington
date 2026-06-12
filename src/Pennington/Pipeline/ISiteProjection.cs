namespace Pennington.Pipeline;

using Routing;

/// <summary>
/// Single shared corpus projection: walks every indexable route once,
/// captures its post-pipeline HTML once, parses the DOM once, and yields a
/// stream of <see cref="RenderedPage"/> records. Every site-wide aggregator
/// (search index, llms.txt, build-time link audit) folds over this stream
/// instead of independently fanning out across the corpus.
/// <para>
/// <b>Critical lifecycle invariant (runtime-enforced):</b> this projection is for
/// build-time and artifact-service consumers only. Materialization triggers parallel
/// HTTP self-fetches that re-enter the request pipeline, so no component that runs
/// during a content page's render or response processing may await it — that is the
/// task-cycle deadlock from commit b719d73. The implementation fails fast instead of
/// hanging: every projection-issued fetch is stamped via
/// <c>Infrastructure.CorpusFetchScope</c>, and consuming the projection from such a
/// request (or from inside its own materialization) throws a descriptive
/// <see cref="InvalidOperationException"/>. Artifact services may consume it freely —
/// their claimed URLs are disjoint from the page corpus the projection fetches.
/// Request-path link verification stays on <c>Infrastructure.PageLinkVerifier</c>,
/// which consults <c>IContentService.DiscoverAllAsync</c> and artifact claims only.
/// </para>
/// </summary>
public interface ISiteProjection
{
    /// <summary>
    /// Yields every renderable page in deterministic discovery order.
    /// Materializes lazily on first enumeration; subsequent calls replay the
    /// cached array until the instance is dropped by file-watch invalidation.
    /// </summary>
    IAsyncEnumerable<RenderedPage> GetPagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the projected page at <paramref name="canonicalPath"/>, or
    /// <c>null</c> when no page matches. Triggers full materialization on
    /// first call; cheap on subsequent calls.
    /// </summary>
    Task<RenderedPage?> GetPageAsync(UrlPath canonicalPath, CancellationToken cancellationToken = default);
}
