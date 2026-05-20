namespace Pennington.Tui;

using System.Collections.Concurrent;
using System.IO;

/// <summary>
/// Tracks files changed this session, deduplicated by full path. Each entry keeps the
/// last-change timestamp and a count of changes — a save-on-type editor won't flood
/// the list with duplicates. The Main tab snapshots and renders ordered by
/// last-changed descending.
/// </summary>
public sealed class FileChangeLog
{
    private readonly ConcurrentDictionary<string, FileChangeEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Record a file change, upserting the entry for that path.</summary>
    public void Record(string fullPath, WatcherChangeTypes changeType, DateTimeOffset at)
    {
        _entries.AddOrUpdate(
            fullPath,
            _ => new FileChangeEntry(fullPath, changeType, at, 1),
            (_, existing) => existing with { LastChangeType = changeType, LastChanged = at, Count = existing.Count + 1 });
    }

    /// <summary>Snapshot of entries ordered by most recently changed.</summary>
    public IReadOnlyList<FileChangeEntry> Snapshot()
    {
        var items = _entries.Values.ToArray();
        Array.Sort(items, static (a, b) => b.LastChanged.CompareTo(a.LastChanged));
        return items;
    }
}

/// <summary>A single file in the change log.</summary>
/// <param name="FullPath">Absolute path to the file.</param>
/// <param name="LastChangeType">Type of the most recent change.</param>
/// <param name="LastChanged">Timestamp of the most recent change.</param>
/// <param name="Count">Total number of changes recorded this session.</param>
public readonly record struct FileChangeEntry(
    string FullPath,
    WatcherChangeTypes LastChangeType,
    DateTimeOffset LastChanged,
    int Count);