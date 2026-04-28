namespace Pennington.MonorailCss;

/// <summary>
/// Thread-safe collector for CSS class names discovered during request processing.
/// Classes accumulate across requests and are never cleared at runtime — stale classes
/// are harmless (MonorailCSS ignores unknown tokens) and are removed on the next build.
/// </summary>
public class CssClassCollector
{
    private static readonly HashSet<string> Classes = [];
    private static readonly ReaderWriterLockSlim ProcessingLock = new(LockRecursionPolicy.SupportsRecursion);

    /// <summary>Registers CSS class names for the given URL.</summary>
    public void AddClasses(string url, IEnumerable<string> classes)
    {
        // This is called from within middleware processing, so we're already holding the write lock
        foreach (var cls in classes)
        {
            Classes.Add(cls);
        }
    }

    /// <summary>Acquires the write lock for adding classes.</summary>
    public void BeginProcessing()
    {
        ProcessingLock.EnterWriteLock();
    }

    /// <summary>Releases the write lock.</summary>
    public void EndProcessing()
    {
        ProcessingLock.ExitWriteLock();
    }

    /// <summary>Returns a snapshot of all collected CSS class names.</summary>
    public IReadOnlyCollection<string> GetClasses()
    {
        ProcessingLock.EnterReadLock();
        try
        {
            return Classes.ToList().AsReadOnly();
        }
        finally
        {
            ProcessingLock.ExitReadLock();
        }
    }
}