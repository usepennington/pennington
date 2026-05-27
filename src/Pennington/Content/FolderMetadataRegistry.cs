namespace Pennington.Content;

using System.Collections.Frozen;
using Infrastructure;

/// <summary>
/// Aggregates <see cref="FolderMetadata"/> rows from every <see cref="IFolderMetadataProvider"/>
/// registered as an <see cref="IContentService"/> and exposes them as an async-loaded
/// snapshot keyed by canonical folder URL prefix. Registered via
/// <c>AddFileWatched&lt;FolderMetadataRegistry&gt;()</c> so the aggregated table is dropped
/// when any provider's underlying file source changes.
/// <para>
/// Callers <c>await registry.GetSnapshotAsync()</c> once at the top of their build path
/// (e.g. <see cref="Navigation.NavigationBuilder.BuildTreeAsync"/>) and pass the snapshot
/// down through sync recursion. The async load runs on a thread-pool thread; no caller
/// blocks on sync-over-async.
/// </para>
/// </summary>
public sealed class FolderMetadataRegistry : IFileWatchAware
{
    private readonly AsyncLazy<FrozenDictionary<string, FolderMetadata>> _byPrefix;

    /// <summary>Creates a registry that lazily aggregates folder metadata across all registered content services.</summary>
    public FolderMetadataRegistry(IEnumerable<IContentService> contentServices)
    {
        var services = contentServices.ToList();
        _byPrefix = new AsyncLazy<FrozenDictionary<string, FolderMetadata>>(() => LoadAsync(services));
    }

    /// <summary>Creates a registry pre-populated with the supplied rows. Test-only.</summary>
    internal FolderMetadataRegistry(IEnumerable<FolderMetadata> rows)
    {
        var frozen = rows
            .ToDictionary(r => r.FolderUrlPrefix, r => r, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _byPrefix = new AsyncLazy<FrozenDictionary<string, FolderMetadata>>(() => Task.FromResult(frozen));
    }

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>
    /// Returns the aggregated folder-metadata snapshot, keyed by canonical folder
    /// URL prefix (<c>/foo/bar/</c> form). Materializes on first call; subsequent
    /// calls return the same task. The instance is dropped on file-watch
    /// invalidation, so the next access rebuilds.
    /// </summary>
    public Task<FrozenDictionary<string, FolderMetadata>> GetSnapshotAsync() => _byPrefix.Task;

    private static async Task<FrozenDictionary<string, FolderMetadata>> LoadAsync(IReadOnlyList<IContentService> services)
    {
        var acc = new Dictionary<string, FolderMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var service in services)
        {
            if (service is not IFolderMetadataProvider provider)
            {
                continue;
            }

            var rows = await provider.GetFolderMetadataAsync().ConfigureAwait(false);
            foreach (var row in rows)
            {
                acc[row.FolderUrlPrefix] = row;
            }
        }

        return acc.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }
}
