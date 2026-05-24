namespace Pennington.Pipeline;

/// <summary>
/// Contributes derived (non-authored) metadata for a parsed content item — reading
/// time, git last-modified, GitHub permalinks, and the like. Contributions are merged
/// into <see cref="ParsedItem.Derived"/> by <see cref="MetadataEnrichmentService"/>.
/// Register implementations with <c>AddTransient&lt;IMetadataEnricher, T&gt;()</c>.
/// </summary>
public interface IMetadataEnricher
{
    /// <summary>
    /// Returns key/value pairs to merge into the item's derived metadata. Return an
    /// empty dictionary to contribute nothing. Later-registered enrichers override
    /// earlier ones on key collision.
    /// </summary>
    Task<IReadOnlyDictionary<string, object?>> EnrichAsync(ParsedItem item);
}
