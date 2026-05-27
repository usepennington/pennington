namespace Pennington.Infrastructure;

using System.Runtime.CompilerServices;

/// <summary>
/// Thread-safe asynchronous lazy initialization. The factory is queued onto the
/// thread pool on first access; subsequent accesses return the same task. A faulted
/// task is evicted so the next access retries.
/// <para>
/// Use <see cref="GetAwaiter"/> (i.e. <c>await asyncLazy</c>) from async contexts.
/// Pennington has no sync-over-async on this type in production code; if a new
/// consumer reaches for <c>asyncLazy.Task.GetAwaiter().GetResult()</c>, make its
/// caller async instead — that's the pattern that exhausts thread-pool budget and
/// occasionally deadlocks under hostile schedulers.
/// </para>
/// </summary>
public sealed class AsyncLazy<T>
{
    private readonly Func<Task<T>> _factory;
    private Task<T>? _task;
    private readonly Lock _lock = new();

    /// <summary>Initializes the instance with a factory invoked on first access.</summary>
    public AsyncLazy(Func<Task<T>> factory) => _factory = factory;

    /// <summary>
    /// Returns the underlying task. Starts the factory on a thread-pool thread the
    /// first time it is accessed; subsequent accesses replay the same task. A
    /// previously-faulted task is evicted so the next access retries.
    /// </summary>
    public Task<T> Task
    {
        get
        {
            lock (_lock)
            {
                if (_task is { IsFaulted: true } or null)
                {
                    _task = System.Threading.Tasks.Task.Run(_factory);
                }

                return _task;
            }
        }
    }

    /// <summary>Lets the lazy be <c>await</c>ed directly: <c>var value = await asyncLazy;</c>.</summary>
    public TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();
}
