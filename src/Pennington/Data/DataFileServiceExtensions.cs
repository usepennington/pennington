namespace Pennington.Data;

using System.IO.Abstractions;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// DI helpers for registering YAML/JSON data files that hot-reload when the underlying
/// file changes on disk.
/// </summary>
public static class DataFileServiceExtensions
{
    /// <summary>
    /// Registers <paramref name="path"/> as a data file accessible through <see cref="IDataFiles"/>
    /// under the lookup key <paramref name="name"/>. Format is inferred from the file extension
    /// (<c>.yml</c>, <c>.yaml</c>, <c>.json</c>). Edits to the file invalidate the cached value
    /// so the next read returns the fresh content.
    /// </summary>
    /// <typeparam name="T">The deserialization target type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">Logical lookup key. Case-insensitive; must be unique across all registered data files.</param>
    /// <param name="path">Path to the data file. Resolved against the current working directory if relative.</param>
    public static IServiceCollection AddDataFile<T>(this IServiceCollection services, string name, string path)
    {
        // Register straight into IDataFile so multiple AddDataFile<T> calls with the same
        // T but different names each get their own singleton — registering DataFileEntry<T>
        // as a typed singleton would let the second call shadow the first.
        services.AddSingleton<IDataFile>(sp => new DataFileEntry<T>(
            name,
            path,
            sp.GetRequiredService<IFileSystem>(),
            sp.GetRequiredService<IFileWatcher>()));

        services.TryAddSingleton<IDataFiles, DataFiles>();

        return services;
    }
}
