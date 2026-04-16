namespace Pennington.Roslyn.Utilities;

/// <summary>Run async code synchronously without deadlocks.</summary>
public static class AsyncHelpers
{
    private static readonly TaskFactory TaskFactory = new(
        CancellationToken.None,
        TaskCreationOptions.None,
        TaskContinuationOptions.None,
        TaskScheduler.Default);

    /// <summary>Synchronously runs an async delegate on the default task scheduler and returns its result, bypassing the current synchronization context.</summary>
    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        => TaskFactory
            .StartNew(func)
            .Unwrap()
            .GetAwaiter()
            .GetResult();

    /// <summary>Synchronously runs an async delegate on the default task scheduler, bypassing the current synchronization context.</summary>
    public static void RunSync(Func<Task> func)
        => TaskFactory
            .StartNew(func)
            .Unwrap()
            .GetAwaiter()
            .GetResult();
}