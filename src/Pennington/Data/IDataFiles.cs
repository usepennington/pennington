namespace Pennington.Data;

/// <summary>
/// Typed, name-keyed access to data files registered with <see cref="DataFileServiceExtensions.AddDataFile{T}"/>.
/// Data files reload automatically when the underlying file changes on disk.
/// </summary>
public interface IDataFiles
{
    /// <summary>
    /// Retrieves the loaded value for the data file registered under <paramref name="name"/>.
    /// </summary>
    /// <typeparam name="T">The type the data file was registered with.</typeparam>
    /// <param name="name">Logical name supplied when the data file was registered.</param>
    /// <exception cref="KeyNotFoundException">No data file is registered with this name.</exception>
    /// <exception cref="InvalidCastException">The data file is registered with a different type than <typeparamref name="T"/>.</exception>
    T Get<T>(string name);

    /// <summary>
    /// Tries to retrieve the loaded value for the data file registered under <paramref name="name"/>.
    /// Returns <c>false</c> when no data file is registered with that name OR when the registered
    /// type does not match <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type the data file was registered with.</typeparam>
    /// <param name="name">Logical name supplied when the data file was registered.</param>
    /// <param name="value">The loaded value when the lookup succeeds.</param>
    bool TryGet<T>(string name, out T? value);

    /// <summary>The names of all registered data files.</summary>
    IEnumerable<string> Names { get; }
}
