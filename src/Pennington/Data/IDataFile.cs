namespace Pennington.Data;

/// <summary>
/// Non-generic facade over a single registered data file. Used by <see cref="DataFiles"/>
/// to enumerate every <see cref="DataFileEntry{T}"/> the container has registered without
/// reflecting over the closed generic types.
/// </summary>
public interface IDataFile
{
    /// <summary>Logical name supplied at registration; lookup key for <see cref="IDataFiles.Get{T}"/>.</summary>
    string Name { get; }

    /// <summary>The closed generic type the entry was registered with.</summary>
    Type ValueType { get; }

    /// <summary>Returns the current loaded value, refreshed if the underlying file has changed since last access.</summary>
    object GetValue();
}
