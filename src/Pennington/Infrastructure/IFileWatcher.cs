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

    /// <summary>Subscribe to be notified when any watched file changes, with the changed path and change type.</summary>
    void SubscribeToChanges(Action<FileChangeNotification> onUpdate);
}

/// <summary>A single file-change notification carrying the full path and the type of change.</summary>
/// <param name="FullPath">Absolute path to the file that changed.</param>
/// <param name="ChangeType">Kind of change reported by <see cref="FileSystemWatcher"/>.</param>
public readonly record struct FileChangeNotification(string FullPath, WatcherChangeTypes ChangeType);