namespace Pennington.Tui;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Tui.Views;

/// <summary>
/// Drives the TUI lifecycle. Starts a dedicated foreground thread for
/// <c>XenoAtom.Terminal.Terminal</c> once Kestrel has bound, subscribes
/// to the shared <see cref="IFileWatcher"/>, refreshes the TOC on file changes,
/// and no-ops entirely when the host is launched in build mode (first command-line
/// arg is <c>build</c>). Diagnostics are collected on demand per request by
/// <see cref="Infrastructure.TuiDiagnosticsCaptureMiddleware"/>, not by a background
/// crawl — the dashboard stays quiet until the user actually hits a page.
/// </summary>
internal sealed class PenningtonTuiHostedService(
    PenningtonTuiOptions options,
    IHostApplicationLifetime lifetime,
    IServer server,
    IFileWatcher fileWatcher,
    IEnumerable<IContentService> contentServices,
    PenningtonOptions penningtonOptions,
    BoundedSequenceLog<RequestEntry> requestLog,
    BoundedSequenceLog<LogEntry> logBuffer,
    FileChangeLog fileChangeLog,
    PageDiagnosticsCollector diagnostics,
    TuiState state,
    ILogger<PenningtonTuiHostedService> logger) : IHostedService
{
    private readonly CancellationTokenSource _cts = new();
    private Thread? _tuiThread;
    private Timer? _debounceTimer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsBuildMode(Environment.GetCommandLineArgs()))
        {
            logger.LogDebug("Pennington.Tui: build mode detected, TUI disabled");
            return Task.CompletedTask;
        }

        lifetime.ApplicationStarted.Register(OnApplicationStarted);
        lifetime.ApplicationStopping.Register(OnApplicationStopping);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _debounceTimer?.Dispose();
        return Task.CompletedTask;
    }

    // args[0] is the executable; user args start at index 1. Mirrors the gate
    // in PenningtonExtensions.RunOrBuildAsync and LiveReloadServer.
    internal static bool IsBuildMode(string[] commandLineArgs) =>
        commandLineArgs.Length > 1 && commandLineArgs[1].Equals("build", StringComparison.OrdinalIgnoreCase);

    // dotnet watch sets DOTNET_WATCH=1 in the child process. The TUI grabs the
    // terminal surface exclusively, which fights with dotnet watch's own output
    // and hot-reload prompts, so we step aside entirely in this mode.
    internal static bool IsDotnetWatchMode() =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"));

    // When stdout isn't a TTY (CI logs, container logs, `dotnet run > log.txt`)
    // the TUI's ANSI control sequences are unreadable. Step aside so the host's
    // default Console logging produces plain log lines the operator can grep.
    internal static bool IsConsoleRedirected() => Console.IsOutputRedirected;

    private void OnApplicationStarted()
    {
        try
        {
            state.AppUrl = ResolveAppUrl();
            state.Locales = [.. penningtonOptions.Localization.Locales.Keys];

            fileWatcher.SubscribeToChanges(OnFileChanged);

            _tuiThread = new Thread(RunTuiLoop)
            {
                IsBackground = false,
                Name = "Pennington.Tui",
            };
            _tuiThread.Start();

            _ = Task.Run(RefreshTocAsync);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pennington.Tui: failed to start");
        }
    }

    private void OnApplicationStopping()
    {
        _cts.Cancel();
        _debounceTimer?.Dispose();
    }

    private string? ResolveAppUrl()
    {
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
        return addresses?.FirstOrDefault();
    }

    private void OnFileChanged(FileChangeNotification notification)
    {
        fileChangeLog.Record(notification.FullPath, notification.ChangeType, DateTimeOffset.Now);

        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(
            _ => _ = Task.Run(RefreshTocAsync),
            state: null,
            dueTime: options.FileChangeDebounce,
            period: Timeout.InfiniteTimeSpan);
    }

    private async Task RefreshTocAsync()
    {
        try
        {
            var groups = new List<ContentGroup>();
            foreach (var svc in contentServices)
            {
                var items = await svc.GetContentTocEntriesAsync();
                groups.Add(new ContentGroup(FormatServiceLabel(svc.GetType()), svc.DefaultSectionLabel, items));
            }
            state.ContentGroups = groups;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Pennington.Tui: TOC refresh failed");
        }
    }

    // Pretty-prints generic types so "MarkdownContentService`1[DocFrontMatter]" reads as
    // "MarkdownContentService<DocFrontMatter>" in the Content tab's tree headers.
    private static string FormatServiceLabel(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var baseName = type.Name.Split('`')[0];
        var args = string.Join(", ", type.GetGenericArguments().Select(a => a.Name));
        return $"{baseName}<{args}>";
    }

    private void RunTuiLoop()
    {
        try
        {
            TuiApp.Run(state, requestLog, logBuffer, fileChangeLog, diagnostics, RequestShutdown, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pennington.Tui: loop terminated");
        }
        finally
        {
            // Whatever made Terminal.Run return — Ctrl+Q's XenoAtom default exit,
            // our Ctrl+C binding, or a surprise exception — tell the host to tear
            // everything down. Otherwise Kestrel keeps running, the TUI thread ends,
            // and because it was foreground the process hangs on shutdown.
            lifetime.StopApplication();
        }
    }

    private void RequestShutdown()
    {
        // Ctrl+C in the TUI tears down Kestrel too, not just the dashboard.
        // StopApplication triggers ApplicationStopping, which cancels _cts and
        // unwinds the loop; the host's normal shutdown then stops Kestrel.
        lifetime.StopApplication();
    }
}