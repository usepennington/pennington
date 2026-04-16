namespace Pennington.Infrastructure;

using System.Collections.Immutable;
using Content;
using Pipeline;

/// <summary>
/// Resolves cross-reference UIDs to URLs and titles.
/// Builds a case-insensitive lookup from all registered content services on first use.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes, ensuring fresh data from content services.
/// </summary>
public sealed class XrefResolver
{
    private readonly AsyncLazy<ImmutableDictionary<string, CrossReference>> _lookupLazy;

    /// <summary>Initializes the resolver and prepares lazy aggregation of UID entries across content services.</summary>
    public XrefResolver(IEnumerable<IContentService> contentServices)
    {
        _lookupLazy = new AsyncLazy<ImmutableDictionary<string, CrossReference>>(
            () => BuildLookupAsync(contentServices));
    }

    /// <summary>Returns the <see cref="CrossReference"/> for <paramref name="uid"/>, or <c>null</c> when not found.</summary>
    public async Task<CrossReference?> ResolveAsync(string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
            return null;

        var lookup = await _lookupLazy.Value;
        return lookup.GetValueOrDefault(uid);
    }

    private static async Task<ImmutableDictionary<string, CrossReference>> BuildLookupAsync(
        IEnumerable<IContentService> contentServices)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CrossReference>(StringComparer.OrdinalIgnoreCase);

        foreach (var service in contentServices)
        {
            var refs = await service.GetCrossReferencesAsync();
            foreach (var xref in refs)
            {
                // First-write-wins: default-locale routes are discovered before
                // fallback routes emitted by MarkdownContentService.DiscoverRoutesWithFallbacks,
                // so the canonical entry is preserved and later fallback insertions are ignored.
                if (!string.IsNullOrWhiteSpace(xref.Uid) && !builder.ContainsKey(xref.Uid))
                    builder.Add(xref.Uid, xref);
            }
        }

        return builder.ToImmutable();
    }
}