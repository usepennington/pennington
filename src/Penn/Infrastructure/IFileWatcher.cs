namespace Pennington.Infrastructure;

/// <summary>
/// Watches file system paths for changes and notifies subscribers.
/// </summary>
public interface IFileWatcher : IDisposable
{
    /// <summary>Watch a path for file changes matching a pattern.</summary>
    void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true);

    /// <summary>Subscribe to be notified when any watched file changes.</summary>
    void SubscribeToChanges(Action onUpdate);
}
