namespace Pennington.Infrastructure;

using Microsoft.Extensions.Logging;

/// <summary>
/// Owns every <see cref="IFileWatcher"/> call on behalf of the application's
/// <see cref="IFileWatchAware"/> services. On construction it registers an OS-level watcher for
/// each declared <see cref="FileWatchScope"/> and subscribes once to the change stream; every
/// subsequent change is fanned out to all <see cref="IFileWatchAware"/> instances.
/// </summary>
public sealed class FileWatchDispatcher
{
    private readonly IReadOnlyList<IFileWatchAware> _aware;
    private readonly ILogger<FileWatchDispatcher>? _logger;

    /// <summary>
    /// Wires the watches: registers every declared scope and subscribes to the change stream.
    /// </summary>
    public FileWatchDispatcher(
        IEnumerable<IFileWatchAware> aware,
        IFileWatcher fileWatcher,
        ILogger<FileWatchDispatcher>? logger = null)
    {
        _aware = aware.ToList();
        _logger = logger;

        // Ensure every declared directory has an OS watcher. The callback is a no-op — the
        // global subscription below catches the event; this call exists only to create the
        // underlying FileSystemWatcher.
        foreach (var watchAware in _aware)
        {
            foreach (var scope in watchAware.WatchScopes)
            {
                fileWatcher.AddPathWatch(scope.Path, scope.Pattern, static (_, _) => { }, scope.IncludeSubdirectories);
            }
        }

        fileWatcher.SubscribeToChanges(Dispatch);
    }

    private void Dispatch(FileChangeNotification change)
    {
        int refreshed = 0, recreated = 0, ignored = 0;
        foreach (var watchAware in _aware)
        {
            switch (watchAware.OnFileChanged(change))
            {
                case FileWatchResponse.Refreshed: refreshed++; break;
                case FileWatchResponse.Recreate: recreated++; break;
                default: ignored++; break;
            }
        }

        _logger?.LogDebug(
            "{Path} changed -> {Refreshed} refreshed, {Recreated} recreated, {Ignored} ignored",
            change.FullPath, refreshed, recreated, ignored);
    }
}