namespace Pennington.Infrastructure;

using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages FileSystemWatcher instances and notifies subscribers of changes.
/// </summary>
public sealed class FileWatcher : IFileWatcher
{
    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<string, IFileSystemWatcher> _watchers = new();
    private readonly List<Action> _subscribers = [];
    private readonly List<Action<FileChangeNotification>> _pathAwareSubscribers = [];
    private readonly ILogger<FileWatcher>? _logger;
    private bool _disposed;

    /// <summary>Initializes the watcher with a filesystem abstraction and optional logger.</summary>
    public FileWatcher(IFileSystem fileSystem, ILogger<FileWatcher>? logger = null)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true)
    {
        _logger?.LogInformation("Adding file watch: {Path} with pattern {Pattern}", path, filePattern);
        var fullPath = _fileSystem.Path.GetFullPath(path);
        var key = $"{fullPath}|{filePattern}";

        if (_watchers.ContainsKey(key)) return;
        if (!_fileSystem.Directory.Exists(fullPath))
        {
            _logger?.LogWarning("Watch path does not exist: {Path}", fullPath);
            return;
        }

        var watcher = _fileSystem.FileSystemWatcher.New(fullPath, filePattern);
        watcher.IncludeSubdirectories = includeSubdirectories;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;

        watcher.Changed += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Changed); NotifySubscribers(e.FullPath, WatcherChangeTypes.Changed); };
        watcher.Created += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Created); NotifySubscribers(e.FullPath, WatcherChangeTypes.Created); };
        watcher.Deleted += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Deleted); NotifySubscribers(e.FullPath, WatcherChangeTypes.Deleted); };
        watcher.Renamed += (_, e) => { onFileChanged(e.FullPath, WatcherChangeTypes.Renamed); NotifySubscribers(e.FullPath, WatcherChangeTypes.Renamed); };

        watcher.EnableRaisingEvents = true;
        _watchers[key] = watcher;
    }

    /// <inheritdoc/>
    public void SubscribeToChanges(Action onUpdate)
    {
        _subscribers.Add(onUpdate);
    }

    /// <inheritdoc/>
    public void SubscribeToChanges(Action<FileChangeNotification> onUpdate)
    {
        _pathAwareSubscribers.Add(onUpdate);
    }

    private void NotifySubscribers(string fullPath, WatcherChangeTypes changeType)
    {
        foreach (var subscriber in _subscribers)
        {
            try { subscriber(); }
            catch (Exception ex) { _logger?.LogError(ex, "Error notifying file watch subscriber"); }
        }
        if (_pathAwareSubscribers.Count == 0) return;
        var notification = new FileChangeNotification(fullPath, changeType);
        foreach (var subscriber in _pathAwareSubscribers)
        {
            try { subscriber(notification); }
            catch (Exception ex) { _logger?.LogError(ex, "Error notifying path-aware file watch subscriber"); }
        }
    }

    /// <inheritdoc/>
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