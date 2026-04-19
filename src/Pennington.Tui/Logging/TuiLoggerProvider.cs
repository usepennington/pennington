namespace Pennington.Tui.Logging;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILoggerProvider"/> that routes every log record into the shared
/// <c>BoundedSequenceLog&lt;LogEntry&gt;</c> so the TUI's Main tab can render them.
/// Replaces the console logger provider (which would otherwise corrupt XenoAtom's
/// terminal frames).
/// </summary>
public sealed class TuiLoggerProvider(BoundedSequenceLog<LogEntry> buffer, LogLevel minLevel) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, TuiLogger> _loggers = new();

    /// <summary>Returns the <see cref="TuiLogger"/> for the given category, creating one on first use.</summary>
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new TuiLogger(name, buffer, minLevel));

    /// <summary>Drops cached loggers; the underlying buffer is owned elsewhere.</summary>
    public void Dispose() => _loggers.Clear();
}
