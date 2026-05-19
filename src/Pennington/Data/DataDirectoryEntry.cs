namespace Pennington.Data;

using System.IO.Abstractions;
using Infrastructure;

/// <summary>
/// Singleton holder for a directory of data files, aggregating every <c>.yml</c>, <c>.yaml</c>,
/// and <c>.json</c> file into one list of <typeparamref name="TItem"/>. The first call to
/// <see cref="GetValue"/> enumerates the directory and deserializes each file via
/// <see cref="DataFileLoader"/>; subsequent calls return the cached list until
/// <see cref="IFileWatcher"/> reports a change in the directory, at which point the next access
/// reloads it.
/// </summary>
/// <typeparam name="TItem">The element type each file deserializes into.</typeparam>
public sealed class DataDirectoryEntry<TItem> : IDataFile
{
    private static readonly string[] SupportedExtensions = [".yml", ".yaml", ".json"];

    private readonly FileWatchedValue<IReadOnlyList<TItem>> _value;

    /// <summary>
    /// Creates and registers the directory watcher; the directory itself is not read until
    /// <see cref="GetValue"/> is called.
    /// </summary>
    /// <param name="name">Logical name; the lookup key for <see cref="IDataFiles.Get{T}"/>.</param>
    /// <param name="path">Path to the directory. Resolved against the current working directory if relative.</param>
    /// <param name="fileSystem">Abstraction used to enumerate and read the files.</param>
    /// <param name="fileWatcher">Watcher used to invalidate the cache when any file in the directory changes.</param>
    public DataDirectoryEntry(string name, string path, IFileSystem fileSystem, IFileWatcher fileWatcher)
    {
        Name = name;
        var absolutePath = fileSystem.Path.GetFullPath(path);

        _value = new FileWatchedValue<IReadOnlyList<TItem>>(
            fileWatcher, absolutePath, "*.*",
            () => Load(absolutePath, fileSystem));
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public Type ValueType => typeof(IReadOnlyList<TItem>);

    /// <inheritdoc/>
    public object GetValue() => _value.Value;

    private static IReadOnlyList<TItem> Load(string absolutePath, IFileSystem fileSystem)
    {
        if (!fileSystem.Directory.Exists(absolutePath))
            throw new DirectoryNotFoundException($"Data directory not found: {absolutePath}");

        var files = fileSystem.Directory.EnumerateFiles(absolutePath)
            .Where(f => SupportedExtensions.Contains(fileSystem.Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => fileSystem.Path.GetFileName(f), StringComparer.Ordinal);

        var items = new List<TItem>();
        foreach (var file in files)
            items.AddRange(DataFileLoader.LoadMany<TItem>(file, fileSystem));

        return items;
    }
}
