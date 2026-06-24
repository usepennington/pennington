namespace Pennington.Infrastructure;

using System.IO.Abstractions;
using System.Threading;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages FileSystemWatcher instances and notifies subscribers of changes.
/// </summary>
/// <remarks>
/// Notifications are debounced on the <em>trailing</em> edge: a file's raw OS events (the
/// truncate/write/close burst a single save produces, plus the duplicate events Windows raises)
/// are collapsed into one notification fired only after the file has been quiet for
/// <see cref="DebounceWindow"/>. Because the editor has closed the handle by then, every
/// subscriber that re-reads the file — every <see cref="IFileWatchAware"/> cache, the live-reload
/// server — observes the finished file instead of one mid-write (which would otherwise throw a
/// sharing violation or read a momentarily-truncated body).
/// </remarks>
public sealed class FileWatcher : IFileWatcher
{
    // Trailing-edge quiet window. Comfortably above the Windows FileSystemWatcher duplicate-event
    // burst (typically <10ms on a single save) so the burst collapses to one notification, and
    // well below LiveReloadServer's 300ms reload debounce so caches refresh before the browser
    // re-requests the page.
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(100);

    private readonly IFileSystem _fileSystem;
    private readonly TimeProvider _clock;
    private readonly Dictionary<string, IFileSystemWatcher> _watchers = new();
    private readonly List<Action> _subscribers = [];
    private readonly List<Action<FileChangeNotification>> _pathAwareSubscribers = [];
    private readonly Lock _debounceLock = new();
    private readonly Dictionary<string, PendingChange> _pending = new(StringComparer.Ordinal);
    private readonly ILogger<FileWatcher>? _logger;
    private bool _disposed;

    /// <summary>A change buffered until its file goes quiet; the latest event for a key wins.</summary>
    private sealed record PendingChange(
        ITimer Timer,
        string FullPath,
        WatcherChangeTypes ChangeType,
        Action<string, WatcherChangeTypes> OnFileChanged);

    /// <summary>Initializes the watcher with a filesystem abstraction, clock, and optional logger.</summary>
    public FileWatcher(IFileSystem fileSystem, TimeProvider? clock = null, ILogger<FileWatcher>? logger = null)
    {
        _fileSystem = fileSystem;
        _clock = clock ?? TimeProvider.System;
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

    /// <summary>
    /// Buffers a raw OS event behind the trailing-edge debounce. Editor backup/temp files are
    /// dropped immediately (no subscriber consumes them); everything else (re)arms a per-key timer
    /// so a burst of events for one file collapses into a single delivery once the file is quiet.
    /// </summary>
    private void Publish(string fullPath, WatcherChangeTypes changeType, Action<string, WatcherChangeTypes> onFileChanged)
    {
        var fileName = _fileSystem.Path.GetFileName(fullPath) ?? string.Empty;
        if (IsEditorTempFile(fileName))
        {
            _logger?.LogTrace("Suppressed (backup-file): {Path} {ChangeType}", fullPath, changeType);
            return;
        }

        var key = $"{fullPath}|{(int)changeType}";
        lock (_debounceLock)
        {
            if (_disposed)
            {
                return;
            }

            if (_pending.TryGetValue(key, out var existing))
            {
                _pending[key] = existing with { FullPath = fullPath, OnFileChanged = onFileChanged };
                existing.Timer.Change(DebounceWindow, Timeout.InfiniteTimeSpan);
            }
            else
            {
                var timer = _clock.CreateTimer(
                    static s =>
                    {
                        var (self, k) = ((FileWatcher, string))s!;
                        self.FireDebounced(k);
                    },
                    (this, key),
                    DebounceWindow,
                    Timeout.InfiniteTimeSpan);
                _pending[key] = new PendingChange(timer, fullPath, changeType, onFileChanged);
            }
        }
    }

    private void FireDebounced(string key)
    {
        PendingChange? pending;
        lock (_debounceLock)
        {
            if (_disposed || !_pending.Remove(key, out pending))
            {
                return;
            }
            pending.Timer.Dispose();
        }

        pending.OnFileChanged(pending.FullPath, pending.ChangeType);
        NotifySubscribers(pending.FullPath, pending.ChangeType);
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

        lock (_debounceLock)
        {
            foreach (var pending in _pending.Values)
            {
                pending.Timer.Dispose();
            }
            _pending.Clear();
        }

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
