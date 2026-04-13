namespace Pennington.Infrastructure;

/// <summary>Thread-safe async lazy initialization with retry on failure.</summary>
public sealed class AsyncLazy<T>
{
    private readonly Func<Task<T>> _factory;
    private Task<T>? _task;
    private readonly Lock _lock = new();

    public AsyncLazy(Func<Task<T>> factory) => _factory = factory;

    public Task<T> Value
    {
        get
        {
            lock (_lock)
            {
                if (_task is { IsFaulted: true } or null)
                    _task = Task.Run(_factory);
                return _task;
            }
        }
    }

    public void Reset()
    {
        lock (_lock) { _task = null; }
    }
}