namespace Pennington.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// DI helpers for registering services whose lifetimes are bound to file-change invalidation.
/// </summary>
public static class FileWatchedServiceExtensions
{
    /// <summary>
    /// Register a service whose instance is managed by <see cref="FileWatchDependencyFactory{T}"/>.
    /// The factory (singleton) recreates the instance when the implementation's
    /// <see cref="IFileWatchAware.OnFileChanged"/> returns <see cref="FileWatchResponse.Recreate"/>.
    /// The service (transient) always returns the current instance from the factory.
    /// </summary>
    public static IServiceCollection AddFileWatched<TService, TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService, IFileWatchAware
    {
        services.AddSingleton<FileWatchDependencyFactory<TImplementation>>();

        services.AddTransient<TService>(sp =>
            sp.GetRequiredService<FileWatchDependencyFactory<TImplementation>>().GetInstance());

        services.AddSingleton<IFileWatchAware>(sp =>
            sp.GetRequiredService<FileWatchDependencyFactory<TImplementation>>());

        return services;
    }

    /// <summary>
    /// Register a concrete service whose instance is managed by <see cref="FileWatchDependencyFactory{T}"/>.
    /// </summary>
    public static IServiceCollection AddFileWatched<T>(this IServiceCollection services)
        where T : class, IFileWatchAware
    {
        services.AddSingleton<FileWatchDependencyFactory<T>>();

        services.AddTransient(sp =>
            sp.GetRequiredService<FileWatchDependencyFactory<T>>().GetInstance());

        services.AddSingleton<IFileWatchAware>(sp =>
            sp.GetRequiredService<FileWatchDependencyFactory<T>>());

        return services;
    }
}