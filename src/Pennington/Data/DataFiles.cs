namespace Pennington.Data;

/// <summary>
/// Default <see cref="IDataFiles"/> implementation that aggregates every registered
/// <see cref="IDataFile"/> by name. Constructed once per DI scope; the underlying
/// entries handle their own hot-reload, so this aggregator does not need to refresh.
/// </summary>
public sealed class DataFiles : IDataFiles
{
    private readonly Dictionary<string, IDataFile> _byName;

    /// <summary>Creates the registry from every <see cref="IDataFile"/> registered in DI.</summary>
    /// <exception cref="InvalidOperationException">Two data files share the same name (case-insensitive).</exception>
    public DataFiles(IEnumerable<IDataFile> entries)
    {
        _byName = new Dictionary<string, IDataFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            if (!_byName.TryAdd(entry.Name, entry))
            {
                throw new InvalidOperationException(
                    $"Two data files registered with the same name '{entry.Name}'. Names must be unique.");
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> Names => _byName.Keys;

    /// <inheritdoc/>
    public T Get<T>(string name)
    {
        if (!_byName.TryGetValue(name, out var entry))
        {
            throw new KeyNotFoundException(
                $"No data file registered with name '{name}'. Registered names: {string.Join(", ", _byName.Keys)}");
        }

        if (entry.ValueType != typeof(T))
        {
            throw new InvalidCastException(
                $"Data file '{name}' is registered as {entry.ValueType.Name} but was requested as {typeof(T).Name}.");
        }

        return (T)entry.GetValue();
    }

    /// <inheritdoc/>
    public bool TryGet<T>(string name, out T? value)
    {
        if (_byName.TryGetValue(name, out var entry) && entry.ValueType == typeof(T))
        {
            value = (T)entry.GetValue();
            return true;
        }

        value = default;
        return false;
    }
}
