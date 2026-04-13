namespace Pennington.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class FileWatchedServiceExtensions
{
    /// <summary>
    /// Register a service whose instance is managed by <see cref="FileWatchDependencyFactory{T}"/>.
    /// The factory (singleton) recreates the instance when watched files change.
    /// The service (transient) always returns the current instance from the factory.
    /// </summary>
    public static IServiceCollection AddFileWatched<TService, TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<FileWatchDependencyFactory<TImplementation>>(sp =>
            new FileWatchDependencyFactory<TImplementation>(
                sp.GetRequiredService<IFileWatcher>(), sp,
                sp.GetRequiredService<ILogger<FileWatchDependencyFactory<TImplementation>>>()));

        services.AddTransient<TService>(sp =>
            sp.GetRequiredService<FileWatchDependencyFactory<TImplementation>>().GetInstance());

        return services;
    }

    /// <summary>
    /// Register a concrete service whose instance is managed by <see cref="FileWatchDependencyFactory{T}"/>.
    /// </summary>
    public static IServiceCollection AddFileWatched<T>(this IServiceCollection services)
        where T : class
    {
        services.AddSingleton<FileWatchDependencyFactory<T>>(sp =>
            new FileWatchDependencyFactory<T>(
                sp.GetRequiredService<IFileWatcher>(), sp,
                sp.GetRequiredService<ILogger<FileWatchDependencyFactory<T>>>()));

        services.AddTransient(sp =>
            sp.GetRequiredService<FileWatchDependencyFactory<T>>().GetInstance());

        return services;
    }
}