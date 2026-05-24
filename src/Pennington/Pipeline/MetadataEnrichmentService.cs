namespace Pennington.Pipeline;

/// <summary>
/// Runs the registered <see cref="IMetadataEnricher"/> pipeline over a
/// <see cref="ParsedItem"/>, merging every contribution into
/// <see cref="ParsedItem.Derived"/>. A no-op when no enrichers are registered.
/// </summary>
public sealed class MetadataEnrichmentService
{
    private readonly IReadOnlyList<IMetadataEnricher> _enrichers;

    /// <summary>Creates the service over the registered enrichers (registration order).</summary>
    public MetadataEnrichmentService(IEnumerable<IMetadataEnricher> enrichers)
    {
        ArgumentNullException.ThrowIfNull(enrichers);
        _enrichers = enrichers.ToList();
    }

    /// <summary>
    /// Returns <paramref name="item"/> with derived metadata from every enricher merged
    /// into <see cref="ParsedItem.Derived"/>. Returns the item unchanged when no enricher
    /// is registered or none contributes a value.
    /// </summary>
    public async Task<ParsedItem> EnrichAsync(ParsedItem item)
    {
        if (_enrichers.Count == 0)
        {
            return item;
        }

        var merged = new Dictionary<string, object?>(item.Derived);
        foreach (var enricher in _enrichers)
        {
            foreach (var (key, value) in await enricher.EnrichAsync(item))
            {
                merged[key] = value;
            }
        }

        return merged.Count == 0 ? item : item with { Derived = merged };
    }
}
