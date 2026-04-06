namespace Penn.Infrastructure;

using System.Collections.Immutable;
using Penn.Content;
using Penn.Pipeline;

/// <summary>
/// Resolves cross-reference UIDs to URLs and titles.
/// Builds a case-insensitive lookup from all registered content services on first use.
/// </summary>
public sealed class XrefResolver
{
    private readonly IEnumerable<IContentService> _contentServices;
    private ImmutableDictionary<string, CrossReference>? _cache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public XrefResolver(IEnumerable<IContentService> contentServices)
    {
        _contentServices = contentServices;
    }

    public async Task<CrossReference?> ResolveAsync(string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
            return null;

        var lookup = await GetLookupAsync();
        return lookup.GetValueOrDefault(uid);
    }

    public void Invalidate() => _cache = null;

    private async Task<ImmutableDictionary<string, CrossReference>> GetLookupAsync()
    {
        if (_cache is { } cached)
            return cached;

        await _lock.WaitAsync();
        try
        {
            if (_cache is { } cachedAfterLock)
                return cachedAfterLock;

            var builder = ImmutableDictionary.CreateBuilder<string, CrossReference>(StringComparer.OrdinalIgnoreCase);

            foreach (var service in _contentServices)
            {
                var refs = await service.GetCrossReferencesAsync();
                foreach (var xref in refs)
                {
                    if (!string.IsNullOrWhiteSpace(xref.Uid))
                        builder[xref.Uid] = xref;
                }
            }

            _cache = builder.ToImmutable();
            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }
}
