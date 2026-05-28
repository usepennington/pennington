namespace Pennington.Infrastructure;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages FileSystemWatcher instances and notifies subscribers of changes.
/// </summary>
public sealed class FileWatcher : IFileWatcher
{
    // 50ms is below human perception of save latency and well above the Windows
    // FileSystemWatcher duplicate-event interval (typically <10ms for LastWrite +
    // size + dir-metadata burst on a single save).
    private static readonly TimeSpan CoalesceWindow = TimeSpan.FromMilliseconds(50);

    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<string, IFileSystemWatcher> _watchers = new();
    private readonly List<Action> _subscribers = [];
    private readonly List<Action<FileChangeNotification>> _pathAwareSubscribers = [];
    private readonly ConcurrentDictionary<string, long> _lastPublishedTimestamps = new(StringComparer.OrdinalIgnoreCase);
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
        _logger?.LogDebug("Adding file watch: {Path} with pattern {Pattern}", path, filePattern);
        var fullPath = _fileSystem.Path.GetFullPath(path);
        var key = $"{fullPath}|{filePattern}";

        if (_watchers.ContainsKey(key))
        {
            return;
        }

        if (!_fileSystem.Directory.Exists(fullPath))
        {
            _logger?.LogWarning("Watch path does not exist: {Path}", fullPath);
            return;
        }

        var watcher = _fileSystem.FileSystemWatcher.New(fullPath, filePattern);
        watcher.IncludeSubdirectories = includeSubdirectories;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;

        watcher.Changed += (_, e) => Publish(e.FullPath, WatcherChangeTypes.Changed, onFileChanged);
        watcher.Created += (_, e) => Publish(e.FullPath, WatcherChangeTypes.Created, onFileChanged);
        watcher.Deleted += (_, e) => Publish(e.FullPath, WatcherChangeTypes.Deleted, onFileChanged);
        watcher.Renamed += (_, e) => Publish(e.FullPath, WatcherChangeTypes.Renamed, onFileChanged);

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

    private void Publish(string fullPath, WatcherChangeTypes changeType, Action<string, WatcherChangeTypes> onFileChanged)
    {
        if (!ShouldPublish(fullPath, changeType))
        {
            return;
        }
        onFileChanged(fullPath, changeType);
        NotifySubscribers(fullPath, changeType);
    }

    private bool ShouldPublish(string fullPath, WatcherChangeTypes changeType)
    {
        var fileName = _fileSystem.Path.GetFileName(fullPath) ?? string.Empty;
        if (IsEditorTempFile(fileName))
        {
            _logger?.LogTrace("Suppressed (backup-file): {Path} {ChangeType}", fullPath, changeType);
            return false;
        }

        var key = $"{fullPath}|{(int)changeType}";
        var now = Stopwatch.GetTimestamp();
        var publish = true;
        _lastPublishedTimestamps.AddOrUpdate(
            key,
            addValueFactory: _ => now,
            updateValueFactory: (_, prev) =>
            {
                if (Stopwatch.GetElapsedTime(prev) < CoalesceWindow)
                {
                    publish = false;
                    // Preserve the original timestamp so a steady sub-window stream
                    // stays suppressed instead of perpetually resetting the gate.
                    return prev;
                }
                return now;
            });
        if (!publish)
        {
            _logger?.LogTrace("Suppressed (coalesced): {Path} {ChangeType}", fullPath, changeType);
        }
        return publish;
    }

    /// <summary>
    /// True for filenames that editors create transiently around a save — vim/nano/VS Code
    /// backup (<c>foo~</c>), vim swap (<c>.swp</c>/<c>.swo</c>/<c>.swx</c>) and pre-write
    /// probe (<c>4913</c>), emacs lock (<c>.#foo</c>), Office lock (<c>~$foo.docx</c>),
    /// and generic <c>.tmp</c>/<c>.bak</c>. Filtering these at the watcher source spares
    /// every <see cref="IFileWatchAware"/> from receiving noise events for files no
    /// content service consumes.
    /// </summary>
    internal static bool IsEditorTempFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        if (fileName[^1] == '~')
        {
            return true;
        }

        if (fileName.StartsWith(".#", StringComparison.Ordinal)
            || fileName.StartsWith("~$", StringComparison.Ordinal))
        {
            return true;
        }

        if (fileName.Equals("4913", StringComparison.Ordinal))
        {
            return true;
        }

        var lastDot = fileName.LastIndexOf('.');
        if (lastDot < 0)
        {
            return false;
        }

        var ext = fileName.AsSpan(lastDot);
        return ext.Equals(".swp", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".swo", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".swx", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".tmp", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".bak", StringComparison.OrdinalIgnoreCase);
    }

    private void NotifySubscribers(string fullPath, WatcherChangeTypes changeType)
    {
        foreach (var subscriber in _subscribers)
        {
            try { subscriber(); }
            catch (Exception ex) { _logger?.LogError(ex, "Error notifying file watch subscriber"); }
        }
        if (_pathAwareSubscribers.Count == 0)
        {
            return;
        }

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
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var watcher in _watchers.Values)
        {
            // On Windows, disposing a FileSystemWatcher with a pending event can throw.
            // Isolate each disposal so one bad watcher doesn't skip the rest.
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error disposing file system watcher");
            }
        }
        _watchers.Clear();
    }
}