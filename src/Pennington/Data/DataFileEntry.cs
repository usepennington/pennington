namespace Pennington.Data;

using System.IO.Abstractions;
using Infrastructure;

/// <summary>
/// Singleton holder for a single registered data file. The first call to <see cref="GetValue"/>
/// loads and deserializes the file via <see cref="DataFileLoader"/>; subsequent calls return the
/// cached value until <see cref="FileWatchDispatcher"/> reports the file has changed, at which
/// point the next access reloads it.
/// </summary>
/// <typeparam name="T">The deserialization target type.</typeparam>
public sealed class DataFileEntry<T> : IDataFile
{
    private readonly FileWatchedValue<T> _value;

    /// <summary>Creates the entry; the file itself is not read until <see cref="GetValue"/> is called.</summary>
    /// <param name="name">Logical name; the lookup key for <see cref="IDataFiles.Get{T}"/>.</param>
    /// <param name="path">Path to the data file. Resolved against the current working directory if relative.</param>
    /// <param name="fileSystem">Abstraction used to read the file.</param>
    public DataFileEntry(string name, string path, IFileSystem fileSystem)
    {
        Name = name;
        var absolutePath = fileSystem.Path.GetFullPath(path);

        var directory = fileSystem.Path.GetDirectoryName(absolutePath)
            ?? throw new ArgumentException($"Data file path has no directory component: {path}", nameof(path));
        var pattern = fileSystem.Path.GetFileName(absolutePath);

        _value = new FileWatchedValue<T>(
            new FileWatchScope(directory, pattern),
            () => DataFileLoader.Load<T>(absolutePath, fileSystem));
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public Type ValueType => typeof(T);

    /// <inheritdoc/>
    public object GetValue() => _value.Value!;

    /// <inheritdoc/>
    public IReadOnlyList<FileWatchScope> WatchScopes => _value.WatchScopes;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => _value.OnFileChanged(change);
}