namespace Pennington.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages a cached service instance that auto-invalidates when watched files change.
/// </summary>
public sealed class FileWatchDependencyFactory<T> : IDisposable where T : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private T? _instance;
    private readonly Lock _lock = new();

    /// <summary>Initializes the factory and subscribes to file-change notifications.</summary>
    public FileWatchDependencyFactory(IFileWatcher fileWatcher, IServiceProvider serviceProvider, ILogger<FileWatchDependencyFactory<T>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        fileWatcher.SubscribeToChanges(InvalidateInstance);
    }

    /// <summary>Returns the cached instance, constructing one via DI on first access.</summary>
    public T GetInstance()
    {
        lock (_lock)
        {
            if (_instance is not null) return _instance;
            _logger.LogDebug("Creating new instance of {Type}", typeof(T).Name);
            _instance = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            return _instance;
        }
    }

    /// <summary>Disposes the current instance (if any) so the next call to <see cref="GetInstance"/> builds a fresh one.</summary>
    public void InvalidateInstance()
    {
        lock (_lock)
        {
            if (_instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _instance = null;
            _logger.LogDebug("Invalidated instance of {Type}", typeof(T).Name);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _instance = null;
        }
    }
}