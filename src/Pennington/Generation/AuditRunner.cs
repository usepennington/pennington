namespace Pennington.Generation;

using System.Collections.Immutable;
using Content;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Hosted service that runs every registered <see cref="IBuildAuditor"/> at startup
/// and again whenever <see cref="IFileWatcher"/> reports a content change. Writes
/// the aggregated diagnostics into the shared <see cref="AuditCache"/>. In dev mode,
/// emits a one-line summary via <see cref="ILogger"/> after each run.
/// </summary>
public sealed class AuditRunner : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly AuditCache _cache;
    private readonly IFileWatcher _fileWatcher;
    private readonly LocalizationOptions _localization;
    private readonly ILogger<AuditRunner> _logger;
    private readonly bool _isBuildMode = PenningtonBuildMode.IsBuildMode();
    private readonly Lock _runLock = new();
    private Task? _activeRun;

    /// <summary>Wires the runner to its dependencies.</summary>
    public AuditRunner(
        IServiceProvider services,
        AuditCache cache,
        IFileWatcher fileWatcher,
        LocalizationOptions localization,
        ILogger<AuditRunner> logger)
    {
        _services = services;
        _cache = cache;
        _fileWatcher = fileWatcher;
        _localization = localization;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Prime the cache with an initial pass so the first request after startup
        // already sees current diagnostics. Subsequent file changes invalidate via
        // the IFileWatcher subscription below.
        _activeRun = RunAsync(cancellationToken);
        _fileWatcher.SubscribeToChanges(() => RunInBackground());
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void RunInBackground()
    {
        // Coalesce: if a run is already in flight, let it finish — the file watcher
        // will fire again only on the next change. Without this, a single batch of
        // file events (rare with multi-file edits) could fan out into multiple
        // overlapping audit passes that race on the cache.
        lock (_runLock)
        {
            if (_activeRun is { IsCompleted: false }) return;
            _activeRun = RunAsync(CancellationToken.None);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Resolve auditors per-run because they are registered transient — that
            // keeps anything they capture (e.g. file-watched content services) fresh.
            using var scope = _services.CreateScope();
            var auditors = scope.ServiceProvider.GetServices<IBuildAuditor>().ToList();
            if (auditors.Count == 0)
            {
                _cache.Set(ImmutableList<BuildDiagnostic>.Empty);
                return;
            }

            var contentServices = scope.ServiceProvider.GetServices<IContentService>();
            var pages = ImmutableList.CreateBuilder<ContentTocItem>();
            foreach (var service in contentServices)
            {
                pages.AddRange(await service.GetContentTocEntriesAsync());
            }

            var context = new BuildAuditContext(pages.ToImmutable(), _localization);
            var allDiagnostics = ImmutableList.CreateBuilder<BuildDiagnostic>();
            foreach (var auditor in auditors)
            {
                try
                {
                    var produced = await auditor.AuditAsync(context, cancellationToken);
                    allDiagnostics.AddRange(produced);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Auditor '{Code}' threw; skipping its diagnostics.", auditor.Code);
                }
            }

            var snapshot = allDiagnostics.ToImmutable();
            _cache.Set(snapshot);

            if (!_isBuildMode)
                LogSummary(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit pass failed; cache left unchanged.");
        }
    }

    private void LogSummary(ImmutableList<BuildDiagnostic> snapshot)
    {
        if (snapshot.Count == 0) return;

        var errors = snapshot.Count(d => d.Severity == Diagnostics.DiagnosticSeverity.Error);
        var warnings = snapshot.Count(d => d.Severity == Diagnostics.DiagnosticSeverity.Warning);
        var parts = new List<string>();
        if (errors > 0) parts.Add($"{errors} error{(errors == 1 ? "" : "s")}");
        if (warnings > 0) parts.Add($"{warnings} warning{(warnings == 1 ? "" : "s")}");
        if (parts.Count == 0) return;

        _logger.LogInformation("Audit: {Summary}", string.Join(", ", parts));
    }
}
