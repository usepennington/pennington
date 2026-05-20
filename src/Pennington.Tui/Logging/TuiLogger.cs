namespace Pennington.Tui.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILogger"/> implementation that writes formatted records into the shared
/// <c>BoundedSequenceLog&lt;LogEntry&gt;</c>. One instance per category. The TUI surface
/// drains the buffer on each render tick.
/// </summary>
internal sealed class TuiLogger(string category, BoundedSequenceLog<LogEntry> buffer, LogLevel minLevel) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minLevel && logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception is null)
        {
            return;
        }

        var at = DateTimeOffset.Now;
        buffer.Append(seq => new LogEntry(seq, at, logLevel, category, message, exception));
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}