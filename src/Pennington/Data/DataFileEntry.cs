namespace Pennington.Data;

using System.IO.Abstractions;
using Infrastructure;

/// <summary>
/// Singleton holder for a single registered data file. The first call to <see cref="GetValue"/>
/// loads and deserializes the file via <see cref="DataFileLoader"/>; subsequent calls return the
/// cached value until <see cref="IFileWatcher"/> reports the file has changed, at which point
/// the next access reloads it.
/// </summary>
/// <typeparam name="T">The deserialization target type.</typeparam>
public sealed class DataFileEntry<T> : IDataFile
{
    private readonly string _absolutePath;
    private readonly IFileSystem _fileSystem;
    private readonly Lock _lock = new();
    private Lazy<T> _value;

    /// <summary>
    /// Creates and registers the file watcher; the file itself is not read until <see cref="GetValue"/>
    /// is called.
    /// </summary>
    /// <param name="name">Logical name; the lookup key for <see cref="IDataFiles.Get{T}"/>.</param>
    /// <param name="path">Path to the data file. Resolved against the current working directory if relative.</param>
    /// <param name="fileSystem">Abstraction used to read the file.</param>
    /// <param name="fileWatcher">Watcher used to invalidate the cache when the file changes.</param>
    public DataFileEntry(string name, string path, IFileSystem fileSystem, IFileWatcher fileWatcher)
    {
        Name = name;
        _absolutePath = fileSystem.Path.GetFullPath(path);
        _fileSystem = fileSystem;
        _value = new Lazy<T>(Load);

        var directory = fileSystem.Path.GetDirectoryName(_absolutePath)
            ?? throw new ArgumentException($"Data file path has no directory component: {path}", nameof(path));
        var pattern = fileSystem.Path.GetFileName(_absolutePath);

        fileWatcher.AddPathWatch(directory, pattern, OnChange, includeSubdirectories: false);
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public Type ValueType => typeof(T);

    /// <inheritdoc/>
    public object GetValue() => _value.Value!;

    private T Load() => DataFileLoader.Load<T>(_absolutePath, _fileSystem);

    private void OnChange(string changedPath, WatcherChangeTypes changeType)
    {
        // The watcher's pattern is filename-scoped, so any callback here means our file changed.
        lock (_lock)
        {
            _value = new Lazy<T>(Load);
        }
    }
}
