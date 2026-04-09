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

    public FileWatchDependencyFactory(IFileWatcher fileWatcher, IServiceProvider serviceProvider, ILogger<FileWatchDependencyFactory<T>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        fileWatcher.SubscribeToChanges(InvalidateInstance);
    }

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
