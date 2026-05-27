namespace Pennington.Pipeline;

using Routing;

/// <summary>
/// Single shared corpus projection: walks every indexable route once,
/// captures its post-pipeline HTML once, parses the DOM once, and yields a
/// stream of <see cref="RenderedPage"/> records. Every site-wide aggregator
/// (search index, llms.txt, build-time link audit) folds over this stream
/// instead of independently fanning out across the corpus.
/// <para>
/// <b>Critical lifecycle invariant:</b> this projection is for build-time and
/// file-watched-sidecar consumers only. Do <i>not</i> consume it from any
/// <c>IResponseProcessor</c>, middleware, or other component on the request
/// path. Materialization triggers parallel HTTP self-fetches that re-enter
/// the request pipeline; consuming this from a request would deadlock the
/// same way <c>PageLinkVerifier</c>'s emitter walk did (see commit b719d73).
/// Request-path link verification stays on
/// <c>Infrastructure.PageLinkVerifier</c>, which consults
/// <c>IContentService.DiscoverAllAsync</c> only.
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
