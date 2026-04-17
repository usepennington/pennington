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

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new TuiLogger(name, buffer, minLevel));

    /// <inheritdoc/>
    public void Dispose() => _loggers.Clear();
}
