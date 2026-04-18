namespace Pennington.Tui.Infrastructure;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Logs a single warning on startup when the TUI was skipped because the host
/// is running under <c>dotnet watch</c>. Registered by
/// <see cref="PenningtonTuiExtensions.AddPenningtonTui"/> in place of the real
/// TUI hosted service so callers get a visible reason the dashboard didn't appear.
/// </summary>
internal sealed class DotnetWatchNoticeService(ILogger<DotnetWatchNoticeService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("Pennington.Tui: dotnet watch detected (DOTNET_WATCH set), TUI disabled");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
