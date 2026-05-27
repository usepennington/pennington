namespace Pennington.Search;

/// <summary>Configuration for search index generation.</summary>
public sealed class SearchIndexOptions
{
    /// <summary>Default priority assigned to indexed documents. Default: 5.</summary>
    public int DefaultPriority { get; set; } = 5;

    /// <summary>
    /// Per-area search priority (area slug to priority); higher ranks first. Documents whose area is
    /// absent from the map use <see cref="DefaultPriority"/>. The DocSite derives this from its
    /// configured area order so results lean toward earlier areas (e.g. tutorials/how-to over
    /// reference) when matches are otherwise comparable. Default: empty (uniform priority).
    /// </summary>
    public Dictionary<string, int> AreaPriorities { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Number of leading characters of a stemmed term used as its shard key. Lower values
    /// produce fewer, larger shards; higher values produce more, smaller shards. Default: 2.
    /// </summary>
    public int ShardPrefixLength { get; set; } = 2;

    /// <summary>
    /// Query-time synonyms. Each entry maps a term to the additional terms it should also match.
    /// Keys and values are stemmed at build time and shipped in the index entrypoint, so authors
    /// write natural words (e.g. <c>"config" =&gt; ["configuration"]</c>). Default: empty.
    /// </summary>
    public Dictionary<string, string[]> Synonyms { get; set; } = [];

    /// <summary>
    /// Facet dimensions to generate for client-side filtering. Default: <see cref="SearchFacetField.Area"/>
    /// only — content areas are few and stable, so they make clean filter chips. Section and tag
    /// faceting are opt-in because their vocabularies are large enough to bury the filter bar in
    /// chips; combine the flags to re-enable (e.g. <c>Area | Section | Tags</c>).
    /// </summary>
    public SearchFacetField Facets { get; set; } = SearchFacetField.Area;

    /// <summary>
    /// Upper bound on the edit distance the client applies for typo-tolerant matching. The client
    /// also scales the budget down for short terms; this caps it. Set to 0 to require exact
    /// matches. Default: 2.
    /// </summary>
    public int MaxEditDistance { get; set; } = 2;
}