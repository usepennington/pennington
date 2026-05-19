namespace Pennington.Infrastructure;

/// <summary>
/// A lazily-loaded value that is recomputed on next access whenever a watched path changes.
/// The factory does not run until <see cref="Value"/> is first read; a file-change
/// notification drops the cached value so the following read reloads it.
/// </summary>
/// <typeparam name="T">The cached value type.</typeparam>
public sealed class FileWatchedValue<T>
{
    private readonly Func<T> _load;
    private readonly Lock _lock = new();
    private Lazy<T> _value;

    /// <summary>
    /// Registers a path watch; the value is not loaded until <see cref="Value"/> is first read.
    /// </summary>
    /// <param name="fileWatcher">Watcher used to invalidate the cached value.</param>
    /// <param name="watchPath">Directory to watch for changes.</param>
    /// <param name="watchPattern">File pattern within <paramref name="watchPath"/> that triggers a reload.</param>
    /// <param name="load">Factory that produces the value.</param>
    /// <param name="includeSubdirectories">Whether changes in subdirectories also trigger a reload.</param>
    public FileWatchedValue(
        IFileWatcher fileWatcher,
        string watchPath,
        string watchPattern,
        Func<T> load,
        bool includeSubdirectories = false)
    {
        _load = load;
        _value = new Lazy<T>(load);
        fileWatcher.AddPathWatch(watchPath, watchPattern, OnChange, includeSubdirectories);
    }

    /// <summary>The current value, loaded on first access and reloaded after a watched change.</summary>
    public T Value => _value.Value;

    private void OnChange(string changedPath, WatcherChangeTypes changeType)
    {
        // The watch is scoped to one path/pattern, so any callback means our input changed.
        lock (_lock)
        {
            _value = new Lazy<T>(_load);
        }
    }
}
