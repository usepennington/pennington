namespace Pennington.Content;

using System.Collections.Frozen;
using System.Collections.Immutable;
using Infrastructure;

/// <summary>
/// Aggregates <see cref="FolderMetadata"/> rows from every <see cref="IFolderMetadataProvider"/>
/// registered as an <see cref="IContentService"/> and exposes a sync lookup keyed by
/// canonical folder URL prefix. Registered via <c>AddFileWatched&lt;FolderMetadataRegistry&gt;()</c>
/// so the aggregated table is dropped when any provider's underlying file source changes.
/// <para>
/// The aggregation is async (each provider exposes <c>GetFolderMetadataAsync</c>) but the
/// consumers (<see cref="Navigation.NavigationBuilder"/>) are sync — so the registry kicks
/// off the load on a thread-pool thread at construction via <see cref="AsyncLazy{T}"/> and
/// the sync API waits on that task. Running on the pool avoids the inline sync-over-async
/// re-entrance that deadlocks under xUnit's task scheduler.
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
        // Kick off the load on the pool eagerly so the first sync TryGet has the result
        // ready (or close to it) instead of paying the cold-start cost on the calling thread.
        _byPrefix.Start();
    }

    /// <summary>Creates a registry pre-populated with the supplied rows. Test-only.</summary>
    internal FolderMetadataRegistry(IEnumerable<FolderMetadata> rows)
    {
        var frozen = rows
            .ToDictionary(r => r.FolderUrlPrefix, r => r, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _byPrefix = new AsyncLazy<FrozenDictionary<string, FolderMetadata>>(() => Task.FromResult(frozen));
        _byPrefix.Start();
    }

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>
    /// Returns the folder metadata for <paramref name="folderUrlPrefix"/> (canonical <c>/foo/bar/</c> form),
    /// or <c>null</c> when no sidecar declares anything for that folder.
    /// </summary>
    public FolderMetadata? TryGet(string folderUrlPrefix)
        => _byPrefix.Task.GetAwaiter().GetResult().TryGetValue(folderUrlPrefix, out var md) ? md : null;

    /// <summary>All registered folder-metadata rows, ordered by descending prefix length for longest-match scans.</summary>
    public ImmutableList<FolderMetadata> All => _byPrefix.Task.GetAwaiter().GetResult().Values
        .OrderByDescending(m => m.FolderUrlPrefix.Length)
        .ThenBy(m => m.FolderUrlPrefix, StringComparer.Ordinal)
        .ToImmutableList();

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
