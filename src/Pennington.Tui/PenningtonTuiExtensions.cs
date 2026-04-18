namespace Pennington.Tui;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pennington.Tui.Infrastructure;
using Pennington.Tui.Logging;

/// <summary>Dependency injection extensions for the Pennington TUI dashboard.</summary>
public static class PenningtonTuiExtensions
{
    /// <summary>
    /// Register the dev-time TUI dashboard. When the host is launched in build mode
    /// (first command-line argument is <c>build</c>), the hosted service no-ops and
    /// the build runs exactly as without this package.
    /// </summary>
    public static IServiceCollection AddPenningtonTui(this IServiceCollection services, Action<PenningtonTuiOptions>? configure = null)
    {
        // dotnet watch owns the console and re-launches the child on file changes;
        // the TUI can't share that surface, so we bail out before touching logging
        // or the hosted service and let the host run as if the package weren't added.
        if (PenningtonTuiHostedService.IsDotnetWatchMode())
        {
            services.AddHostedService<DotnetWatchNoticeService>();
            return services;
        }

        var options = new PenningtonTuiOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddSingleton(new BoundedSequenceLog<RequestEntry>(options.RequestBufferSize));
        services.AddSingleton(new BoundedSequenceLog<LogEntry>(options.LogBufferSize));
        services.AddSingleton<FileChangeLog>();
        services.AddSingleton<PageDiagnosticsCollector>();
        services.AddSingleton<TuiState>();
        services.AddHostedService<PenningtonTuiHostedService>();

        // The TUI owns the terminal surface. Any other writer to stdout/stderr
        // (Kestrel startup banner, request logs, static-file load messages) will
        // paint over frames, so in dev mode we replace all log providers with the
        // TUI's own buffer. In build mode the TUI is inert and the BuildReport
        // console output needs to print normally, so we leave logging alone.
        if (!PenningtonTuiHostedService.IsBuildMode(Environment.GetCommandLineArgs()))
        {
            services.AddLogging(b =>
            {
                b.ClearProviders();
                b.Services.AddSingleton<ILoggerProvider>(sp =>
                    new TuiLoggerProvider(sp.GetRequiredService<BoundedSequenceLog<LogEntry>>(), options.LogMinLevel));
            });

            services.AddTransient<IStartupFilter, TuiStartupFilter>();
            services.AddTransient<TuiRequestTrackingMiddleware>();
            services.AddTransient<TuiDiagnosticsCaptureMiddleware>();

            // Shorten the host shutdown window. The default is 5s, and when Quit is
            // pressed Kestrel waits for that full window before closing active
            // connections — a noticeable hang after the TUI already unwound. 500ms
            // is plenty for a dev dashboard to drain; anything still in flight
            // gets cancelled.
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromMilliseconds(500));
        }

        return services;
    }
}
