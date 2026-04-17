namespace Pennington.Search;

/// <summary>Configuration for search index generation.</summary>
public sealed class SearchIndexOptions
{
    /// <summary>
    /// CSS selector identifying the main content element inside the rendered page HTML
    /// (e.g. "#main-content", "article", "main"). The matched element's text is used as
    /// the search body. When null, the entire &lt;body&gt; is used.
    /// </summary>
    public string? ContentSelector { get; set; }

    /// <summary>Default priority assigned to indexed documents. Default: 5.</summary>
    public int DefaultPriority { get; set; } = 5;

    /// <summary>
    /// When true (default), <c>&lt;pre&gt;</c> blocks are dropped from the indexed body.
    /// Keeps the per-locale index JSON smaller and reduces noisy matches on syntax-highlighted
    /// code, which emits hundreds of span-wrapped tokens per snippet. Inline <c>&lt;code&gt;</c>
    /// spans are always kept — identifiers in prose are worth indexing.
    /// </summary>
    public bool ExcludeCodeBlocks { get; set; } = true;
}