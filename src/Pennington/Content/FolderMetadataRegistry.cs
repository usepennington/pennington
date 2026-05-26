namespace Pennington.Content;

using System.Collections.Frozen;
using System.Collections.Immutable;
using Infrastructure;

/// <summary>
/// Aggregates <see cref="FolderMetadata"/> rows from every <see cref="IFolderMetadataProvider"/>
/// registered as an <see cref="IContentService"/> and exposes a sync lookup keyed by
/// canonical folder URL prefix. Registered via <c>AddFileWatched&lt;FolderMetadataRegistry&gt;()</c>
/// so the aggregated table is dropped when any provider's underlying file source changes.
/// </summary>
public sealed class FolderMetadataRegistry : IFileWatchAware
{
    private readonly Lazy<FrozenDictionary<string, FolderMetadata>> _byPrefix;

    /// <summary>Creates a registry that lazily aggregates folder metadata across all registered content services.</summary>
    public FolderMetadataRegistry(IEnumerable<IContentService> contentServices)
    {
        var services = contentServices.ToList();
        _byPrefix = new Lazy<FrozenDictionary<string, FolderMetadata>>(() => Load(services));
    }

    /// <summary>Creates a registry pre-populated with the supplied rows. Test-only.</summary>
    internal FolderMetadataRegistry(IEnumerable<FolderMetadata> rows)
    {
        var frozen = rows
            .ToDictionary(r => r.FolderUrlPrefix, r => r, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _byPrefix = new Lazy<FrozenDictionary<string, FolderMetadata>>(() => frozen);
    }

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>
    /// Returns the folder metadata for <paramref name="folderUrlPrefix"/> (canonical <c>/foo/bar/</c> form),
    /// or <c>null</c> when no sidecar declares anything for that folder.
    /// </summary>
    public FolderMetadata? TryGet(string folderUrlPrefix)
        => _byPrefix.Value.TryGetValue(folderUrlPrefix, out var md) ? md : null;

    /// <summary>All registered folder-metadata rows, ordered by descending prefix length for longest-match scans.</summary>
    public ImmutableList<FolderMetadata> All => _byPrefix.Value.Values
        .OrderByDescending(m => m.FolderUrlPrefix.Length)
        .ThenBy(m => m.FolderUrlPrefix, StringComparer.Ordinal)
        .ToImmutableList();

    private static FrozenDictionary<string, FolderMetadata> Load(IReadOnlyList<IContentService> services)
    {
        var acc = new Dictionary<string, FolderMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var service in services)
        {
            if (service is not IFolderMetadataProvider provider)
            {
                continue;
            }

            // Sync-block on the async API. Each provider's underlying implementation
            // does plain file I/O behind an AsyncLazy — there's no real async work to
            // wait on, just initialization. Registry construction is rare (file-watched).
            var rows = provider.GetFolderMetadataAsync().GetAwaiter().GetResult();
            foreach (var row in rows)
            {
                acc[row.FolderUrlPrefix] = row;
            }
        }

        return acc.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }
}
