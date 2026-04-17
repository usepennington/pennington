namespace Pennington.Tui;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

/// <summary>
/// Thread-safe bounded queue of sequence-tagged entries. Producers run on arbitrary
/// threads (Kestrel workers, the ILogger pipeline); the TUI drains new rows each
/// tick on the terminal thread by tracking the last-seen sequence. Trims the oldest
/// when <paramref name="capacity"/> is exceeded; the minimum floor of 50 keeps the
/// dashboard from rendering empty on a busy page even if a caller passes 0.
/// </summary>
public sealed class BoundedSequenceLog<T>(int capacity)
{
    private readonly ConcurrentQueue<T> _entries = new();
    private readonly int _capacity = Math.Max(50, capacity);
    private long _sequence;

    /// <summary>Assign the next sequence id, hand it to <paramref name="factory"/>, and enqueue the result.</summary>
    public void Append(Func<long, T> factory)
    {
        var seq = Interlocked.Increment(ref _sequence);
        _entries.Enqueue(factory(seq));
        while (_entries.Count > _capacity && _entries.TryDequeue(out _)) { }
    }

    /// <summary>Snapshot of current entries, oldest first.</summary>
    public IReadOnlyList<T> Snapshot() => [.. _entries];
}

/// <summary>A single captured HTTP request, held in <c>BoundedSequenceLog&lt;RequestEntry&gt;</c>.</summary>
/// <param name="Sequence">Monotonically increasing id assigned at append time; used to stream new rows into the TUI.</param>
/// <param name="Timestamp">When the request completed.</param>
/// <param name="Method">HTTP method (GET, POST, …).</param>
/// <param name="Path">Request path without query string.</param>
/// <param name="QueryString">Query string including the leading <c>?</c>, or empty.</param>
/// <param name="Status">HTTP response status code.</param>
/// <param name="Duration">Wall time between request start and completion.</param>
public readonly record struct RequestEntry(
    long Sequence,
    DateTimeOffset Timestamp,
    string Method,
    string Path,
    string QueryString,
    int Status,
    TimeSpan Duration);

/// <summary>A single captured log record, held in <c>BoundedSequenceLog&lt;LogEntry&gt;</c>.</summary>
/// <param name="Sequence">Monotonically increasing id assigned at append time.</param>
/// <param name="Timestamp">When the record was written.</param>
/// <param name="Level">Severity level.</param>
/// <param name="Category">Logger category (typically the type name).</param>
/// <param name="Message">Formatted log message.</param>
/// <param name="Exception">Exception attached to the record, if any.</param>
public readonly record struct LogEntry(
    long Sequence,
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    Exception? Exception);
