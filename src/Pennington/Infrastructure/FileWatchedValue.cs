namespace Pennington.Infrastructure;

/// <summary>
/// A lazily-loaded value that reloads on next access when a file in its <see cref="Scope"/>
/// changes. Implements <see cref="IFileWatchAware"/> so <see cref="FileWatchDispatcher"/> drives
/// the reload — this type holds no <see cref="IFileWatcher"/> subscription of its own.
/// </summary>
/// <typeparam name="T">The cached value type.</typeparam>
public sealed class FileWatchedValue<T> : IFileWatchAware
{
    private readonly Func<T> _load;
    private readonly Lock _lock = new();
    private Lazy<T> _value;

    /// <summary>Creates the holder; the value is not loaded until <see cref="Value"/> is first read.</summary>
    /// <param name="scope">The directory and pattern whose changes trigger a reload.</param>
    /// <param name="load">Factory that produces the value.</param>
    public FileWatchedValue(FileWatchScope scope, Func<T> load)
    {
        Scope = scope;
        _load = load;
        _value = new Lazy<T>(load);
    }

    /// <summary>The directory and pattern this value reloads for.</summary>
    public FileWatchScope Scope { get; }

    /// <summary>The current value, loaded on first access and reloaded after a change in <see cref="Scope"/>.</summary>
    public T Value => _value.Value;

    /// <inheritdoc/>
    public IReadOnlyList<FileWatchScope> WatchScopes => [Scope];

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change)
    {
        if (!Scope.Matches(change)) return FileWatchResponse.Ignore;

        lock (_lock)
        {
            _value = new Lazy<T>(_load);
        }
        return FileWatchResponse.Refreshed;
    }
}
