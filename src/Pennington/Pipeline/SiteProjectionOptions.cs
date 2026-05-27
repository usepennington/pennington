namespace Pennington.Pipeline;

/// <summary>
/// Configuration for <see cref="ISiteProjection"/>: the shared corpus projection
/// consumed by every site-wide aggregator (search index, llms.txt, link audit).
/// </summary>
public sealed class SiteProjectionOptions
{
    /// <summary>
    /// CSS selector identifying the main content element inside the rendered
    /// page HTML (e.g. <c>#main-content</c>, <c>article</c>, <c>main</c>). When
    /// null, the entire <c>&lt;body&gt;</c> is used. Layouts that wrap content
    /// in a navigation/footer chrome should set this so the chrome does not
    /// leak into the search index or llms.txt sidecars.
    /// </summary>
    public string? ContentSelector { get; set; }

    /// <summary>
    /// When true (default), <c>&lt;pre&gt;</c> blocks are dropped from the
    /// indexed body. Keeps per-locale search shards small and reduces noisy
    /// matches on syntax-highlighted code that emits hundreds of tokens per
    /// snippet. Inline <c>&lt;code&gt;</c> spans are always kept.
    /// </summary>
    public bool ExcludeCodeBlocks { get; set; } = true;
}
