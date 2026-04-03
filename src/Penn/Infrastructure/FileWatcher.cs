namespace Penn.Infrastructure;

using Microsoft.Extensions.Logging;

/// <summary>
/// Manages FileSystemWatcher instances and notifies subscribers of changes.
/// </summary>
public sealed class FileWatcher : IFileWatcher
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new();
    private readonly List<Action> _subscribers = [];
    private readonly ILogger<FileWatcher>? _logger;
    private bool _disposed;

    public FileWatcher(ILogger<FileWatcher>? logger = null)
    {
        _logger = logger;
    }

    public void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true)
    {
        var fullPath = Path.GetFullPath(path);
        var key = $"{fullPath}|{filePattern}";

        if (_watchers.ContainsKey(key)) return;
        if (!Directory.Exists(fullPath))
        {
            _logger?.LogWarning("Watch path does not exist: {Path}", fullPath);
            return;
        }

        var watcher = new FileSystemWatcher(fullPath, filePattern)
        {
            IncludeSubdirectories = includeSubdirectories,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime
        };

        watcher.Changed += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Changed); NotifySubscribers(); };
        watcher.Created += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Created); NotifySubscribers(); };
        watcher.Deleted += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Deleted); NotifySubscribers(); };
        watcher.Renamed += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Renamed); NotifySubscribers(); };

        watcher.EnableRaisingEvents = true;
        _watchers[key] = watcher;
    }

    public void SubscribeToChanges(Action onUpdate)
    {
        _subscribers.Add(onUpdate);
    }

    private void NotifySubscribers()
    {
        foreach (var subscriber in _subscribers)
        {
            try { subscriber(); }
            catch (Exception ex) { _logger?.LogError(ex, "Error notifying file watch subscriber"); }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
    }
}
