namespace Pennington.Tui;

using Microsoft.Extensions.Logging;

/// <summary>Configures the dev-time TUI dashboard.</summary>
public sealed class PenningtonTuiOptions
{
    /// <summary>
    /// Debounce window between a file change and the table-of-contents refresh. Kept
    /// separate from <c>LiveReloadServer</c>'s 300ms debounce so the two subscribers
    /// don't collide.
    /// </summary>
    public TimeSpan FileChangeDebounce { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Maximum entries retained in the Main tab's Requests panel ring buffer.</summary>
    public int RequestBufferSize { get; set; } = 200;

    /// <summary>Maximum lines retained in the Main tab's Logs panel ring buffer.</summary>
    public int LogBufferSize { get; set; } = 500;

    /// <summary>Minimum <see cref="LogLevel"/> to surface in the Logs panel.</summary>
    public LogLevel LogMinLevel { get; set; } = LogLevel.Information;
}