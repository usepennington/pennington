namespace Pennington.Infrastructure;

/// <summary>Thread-safe async lazy initialization with retry on failure.</summary>
public sealed class AsyncLazy<T>
{
    private readonly Func<Task<T>> _factory;
    private Task<T>? _task;
    private readonly Lock _lock = new();

    /// <summary>Initializes the instance with a factory invoked on first access.</summary>
    public AsyncLazy(Func<Task<T>> factory) => _factory = factory;

    /// <summary>Task that resolves to the lazily produced value; retries automatically if the previous attempt faulted.</summary>
    public Task<T> Value
    {
        get
        {
            lock (_lock)
            {
                if (_task is { IsFaulted: true } or null)
                {
                    _task = Task.Run(_factory);
                }

                return _task;
            }
        }
    }

    /// <summary>Discards any cached value so the next access runs the factory again.</summary>
    public void Reset()
    {
        lock (_lock) { _task = null; }
    }
}