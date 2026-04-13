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
}