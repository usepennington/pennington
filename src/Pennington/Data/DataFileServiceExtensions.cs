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
        // T but different names each get their own singleton.
        services.AddSingleton<IDataFile>(sp => new DataFileEntry<T>(
            name, path, sp.GetRequiredService<IFileSystem>()));

        AddDataFilesCore(services);
        return services;
    }

    /// <summary>
    /// Registers every <c>.yml</c>, <c>.yaml</c>, and <c>.json</c> file in <paramref name="path"/>
    /// as a single aggregated <see cref="IReadOnlyList{T}"/> accessible through <see cref="IDataFiles"/>
    /// under the lookup key <paramref name="name"/>. Each file contributes one record, or several
    /// when its root is an array; files are ordered by name. Edits, additions, and removals in the
    /// directory invalidate the cached value so the next read returns the fresh content.
    /// </summary>
    /// <typeparam name="TItem">The element type each file deserializes into.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">Logical lookup key. Case-insensitive; must be unique across all registered data files.</param>
    /// <param name="path">Path to the directory. Resolved against the current working directory if relative.</param>
    public static IServiceCollection AddDataDirectory<TItem>(this IServiceCollection services, string name, string path)
    {
        services.AddSingleton<IDataFile>(sp => new DataDirectoryEntry<TItem>(
            name, path, sp.GetRequiredService<IFileSystem>()));

        AddDataFilesCore(services);
        return services;
    }

    // Registers the shared DataFiles aggregator once — exposed as IDataFiles and as the single
    // IFileWatchAware for the data-file subsystem. Idempotent across repeated AddDataFile calls.
    private static void AddDataFilesCore(IServiceCollection services)
    {
        services.TryAddSingleton<DataFiles>();
        services.TryAddSingleton<IDataFiles>(sp => sp.GetRequiredService<DataFiles>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFileWatchAware, DataFiles>(
            sp => sp.GetRequiredService<DataFiles>()));
    }
}
