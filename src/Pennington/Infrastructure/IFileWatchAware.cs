namespace Pennington.Infrastructure;

/// <summary>
/// How a file-watched service wants <see cref="FileWatchDispatcher"/> to treat a change.
/// </summary>
public enum FileWatchResponse
{
    /// <summary>The change is irrelevant; nothing was done.</summary>
    Ignore,

    /// <summary>The service refreshed its own state in place; keep the instance.</summary>
    Refreshed,

    /// <summary>Drop the instance; a fresh one is built on the next resolve.</summary>
    Recreate,
}

/// <summary>
/// A directory and file pattern an <see cref="IFileWatchAware"/> needs OS-level watching for.
/// </summary>
/// <param name="Path">Absolute directory to watch.</param>
/// <param name="Pattern">File pattern within <paramref name="Path"/> (for example <c>*.yml</c>).</param>
/// <param name="IncludeSubdirectories">Whether changes in nested directories also count.</param>
public readonly record struct FileWatchScope(string Path, string Pattern, bool IncludeSubdirectories = false)
{
    /// <summary>Returns whether <paramref name="change"/> falls within this scope.</summary>
    public bool Matches(FileChangeNotification change)
    {
        // Compare with separators normalized so a watcher path ('\') and a scope built from a
        // forward-slash test path both match.
        var fullPath = change.FullPath.Replace('\\', '/');
        var lastSlash = fullPath.LastIndexOf('/');
        if (lastSlash < 0)
        {
            return false;
        }

        var changedDirectory = fullPath[..lastSlash];
        var scopeDirectory = Path.Replace('\\', '/').TrimEnd('/');

        var inScope = IncludeSubdirectories
            ? string.Equals(changedDirectory, scopeDirectory, StringComparison.OrdinalIgnoreCase)
                || changedDirectory.StartsWith(scopeDirectory + "/", StringComparison.OrdinalIgnoreCase)
            : string.Equals(changedDirectory, scopeDirectory, StringComparison.OrdinalIgnoreCase);
        if (!inScope)
        {
            return false;
        }

        var fileName = fullPath[(lastSlash + 1)..];
        return System.IO.Enumeration.FileSystemName.MatchesSimpleExpression(Pattern, fileName);
    }
}

/// <summary>
/// The single contract for anything that reacts to file-system changes. Implementers declare
/// the directories they need watched and how they respond to a change;
/// <see cref="FileWatchDispatcher"/> owns every <see cref="IFileWatcher"/> call.
/// </summary>
public interface IFileWatchAware
{
    /// <summary>
    /// Directories needing an OS-level watcher. Empty (the default) for aggregators that ride
    /// notifications other watchers already produce.
    /// </summary>
    IReadOnlyList<FileWatchScope> WatchScopes => [];

    /// <summary>
    /// Called on the file-watcher thread for every watched change. Must be quick and thread-safe.
    /// </summary>
    FileWatchResponse OnFileChanged(FileChangeNotification change);
}