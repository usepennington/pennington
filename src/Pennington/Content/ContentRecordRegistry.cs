namespace Pennington.Content;

using System.Collections.Frozen;
using Infrastructure;

/// <summary>
/// Aggregates the <see cref="ContentRecord"/>s projected by every registered
/// <see cref="IContentService"/> into a snapshot keyed by canonical path, so consumers can resolve
/// the typed front matter for a route without re-walking the services. Registered via
/// <c>AddFileWatched&lt;ContentRecordRegistry&gt;()</c> so the table is dropped when any service's
/// source changes.
/// <para>
/// Search faceting and structured-data emission both join the rendered corpus back to its records
/// through this registry: the route a page was served at resolves to the
/// <see cref="ContentRecord.Metadata"/> that carries its capabilities
/// (<see cref="Search.IHasSearchFacets"/>, <see cref="StructuredData.IHasStructuredData"/>, ...).
/// </para>
/// </summary>
public sealed class ContentRecordRegistry : IFileWatchAware
{
    private readonly AsyncLazy<FrozenDictionary<string, ContentRecord>> _byPath;

    /// <summary>Creates a registry that lazily aggregates records across all registered content services.</summary>
    public ContentRecordRegistry(IEnumerable<IContentService> contentServices)
    {
        var services = contentServices.ToList();
        _byPath = new AsyncLazy<FrozenDictionary<string, ContentRecord>>(() => LoadAsync(services));
    }

    /// <summary>Creates a registry pre-populated with the supplied records. Test-only.</summary>
    internal ContentRecordRegistry(IEnumerable<ContentRecord> records)
    {
        var frozen = BuildMap(records);
        _byPath = new AsyncLazy<FrozenDictionary<string, ContentRecord>>(() => Task.FromResult(frozen));
    }

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>
    /// Returns the aggregated record snapshot keyed by canonical path with slashes trimmed —
    /// matching <see cref="Pipeline.SiteProjection"/>'s route key so the two join cleanly.
    /// Materializes on first call; dropped on file-watch invalidation so the next access rebuilds.
    /// </summary>
    public Task<FrozenDictionary<string, ContentRecord>> GetSnapshotAsync() => _byPath.Task;

    private static async Task<FrozenDictionary<string, ContentRecord>> LoadAsync(IReadOnlyList<IContentService> services)
    {
        var acc = new Dictionary<string, ContentRecord>(StringComparer.OrdinalIgnoreCase);
        await foreach (var record in services.GetAllRecordsAsync())
        {
            // First writer wins on a duplicate route — matching SiteProjection's first-wins dedup by
            // canonical path, so the record this registry resolves is the same page the projection
            // renders and indexes when two services collide on a route.
            acc.TryAdd(Key(record.Route.CanonicalPath.Value), record);
        }
        return acc.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static FrozenDictionary<string, ContentRecord> BuildMap(IEnumerable<ContentRecord> records)
    {
        var acc = new Dictionary<string, ContentRecord>(StringComparer.OrdinalIgnoreCase);
        foreach (var record in records)
        {
            acc.TryAdd(Key(record.Route.CanonicalPath.Value), record);
        }
        return acc.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static string Key(string canonicalPath) => canonicalPath.Trim('/');
}
