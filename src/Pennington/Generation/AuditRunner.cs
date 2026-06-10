namespace Pennington.Generation;

using System.Collections.Immutable;
using Cli;
using Content;
using Infrastructure;
using Localization;
using Routing;
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
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<AuditRunner> _logger;
    private readonly bool _isHeadlessOneShot;
    private readonly Lock _runLock = new();
    private Task? _activeRun;

    /// <summary>Wires the runner to its dependencies.</summary>
    public AuditRunner(
        IServiceProvider services,
        AuditCache cache,
        IFileWatcher fileWatcher,
        LocalizationOptions localization,
        IHostApplicationLifetime lifetime,
        ILogger<AuditRunner> logger)
        : this(services, cache, fileWatcher, localization, lifetime, logger, PenningtonCli.Current.IsHeadlessOneShot)
    {
    }

    // Test seam: lets unit tests drive the headless-one-shot branch without depending on process args.
    internal AuditRunner(
        IServiceProvider services,
        AuditCache cache,
        IFileWatcher fileWatcher,
        LocalizationOptions localization,
        IHostApplicationLifetime lifetime,
        ILogger<AuditRunner> logger,
        bool isHeadlessOneShot)
    {
        _services = services;
        _cache = cache;
        _fileWatcher = fileWatcher;
        _localization = localization;
        _lifetime = lifetime;
        _logger = logger;
        _isHeadlessOneShot = isHeadlessOneShot;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Defer the initial pass until the application has fully started. A hosted service's
        // StartAsync runs while sibling hosted services — including the web server that backs
        // the in-process self-fetch — may not be up yet, so a build-mode pass that fetches
        // rendered HTML through the projection would race the server start and fail (the empty
        // result would then poison the projection's cache). ApplicationStarted fires only after
        // every hosted service, the server included, has started.
        _lifetime.ApplicationStarted.Register(RunInBackground);
        _fileWatcher.SubscribeToChanges(() => RunInBackground());
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Awaits the initial audit pass kicked off by <see cref="StartAsync"/> so callers (e.g. the
    /// <c>diag</c> CLI) read a fully-populated <see cref="AuditCache"/>. Completes immediately when
    /// no pass has been started.
    /// </summary>
    internal Task WaitForInitialPassAsync() => _activeRun ?? Task.CompletedTask;

    private void RunInBackground()
    {
        // Coalesce: if a run is already in flight, let it finish — the file watcher
        // will fire again only on the next change. Without this, a single batch of
        // file events (rare with multi-file edits) could fan out into multiple
        // overlapping audit passes that race on the cache.
        lock (_runLock)
        {
            if (_activeRun is { IsCompleted: false })
            {
                return;
            }

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
            // Rendered auditors issue one HTTP self-fetch per TOC page. In the long-lived serve
            // process this pass re-runs after every file edit and, on a large corpus (e.g.
            // ScaleStressExample's 5000 pages), floods the dev log — so serve skips them and
            // catches broken links per-request via PageLinkAuditProcessor instead. The headless
            // one-shot runs (build and diag) fire the pass once and exit, so resolve the rendered
            // auditors there: build gates its report on them and `diag warnings` reports them too.
            var renderedAuditors = _isHeadlessOneShot
                ? scope.ServiceProvider.GetServices<IRenderedAuditor>().ToList()
                : new List<IRenderedAuditor>();
            if (auditors.Count == 0 && renderedAuditors.Count == 0)
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

            var pagesSnapshot = pages.ToImmutable();
            var context = new BuildAuditContext(pagesSnapshot, _localization);
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

            if (renderedAuditors.Count > 0)
            {
                var projection = scope.ServiceProvider.GetService<Pipeline.ISiteProjection>();

                if (projection is null)
                {
                    _logger.LogDebug("Rendered auditors registered but no site projection available; skipping this pass.");
                }
                else
                {
                    // Rendered auditors (the broken-link crawl) must cover EVERY generated HTML
                    // page, not just navigation entries — a broken link on a Razor route like the
                    // homepage, which never enters the TOC, still has to surface. Start from full
                    // discovery, then union the TOC's locale-expanded routes so per-locale pages
                    // stay covered. Dedupe on canonical path.
                    var renderedRoutes = ImmutableList.CreateBuilder<ContentRoute>();
                    var seenRoutes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    await foreach (var item in contentServices.DiscoverAllAsync(cancellationToken))
                    {
                        if (item.Source is Pipeline.LlmsOnlySource)
                        {
                            continue;
                        }

                        if (seenRoutes.Add(item.Route.CanonicalPath.Value))
                        {
                            renderedRoutes.Add(item.Route);
                        }
                    }

                    foreach (var page in pagesSnapshot)
                    {
                        if (seenRoutes.Add(page.Route.CanonicalPath.Value))
                        {
                            renderedRoutes.Add(page.Route);
                        }
                    }

                    var renderedContext = new RenderedAuditContext(
                        renderedRoutes.ToImmutable(),
                        _localization,
                        async (route, ct) =>
                        {
                            var page = await projection.GetPageAsync(route.CanonicalPath, ct);
                            return page?.Html;
                        });

                    foreach (var auditor in renderedAuditors)
                    {
                        try
                        {
                            var produced = await auditor.AuditAsync(renderedContext, cancellationToken);
                            allDiagnostics.AddRange(produced);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Rendered auditor '{Code}' threw; skipping its diagnostics.", auditor.Code);
                        }
                    }
                }
            }

            var snapshot = allDiagnostics.ToImmutable();
            _cache.Set(snapshot);

            if (!_isHeadlessOneShot)
            {
                LogSummary(snapshot);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit pass failed; cache left unchanged.");
        }
    }

    private void LogSummary(ImmutableList<BuildDiagnostic> snapshot)
    {
        if (snapshot.Count == 0)
        {
            return;
        }

        var errors = snapshot.Count(d => d.Severity == Diagnostics.DiagnosticSeverity.Error);
        var warnings = snapshot.Count(d => d.Severity == Diagnostics.DiagnosticSeverity.Warning);
        var parts = new List<string>();
        if (errors > 0)
        {
            parts.Add($"{errors} error{(errors == 1 ? "" : "s")}");
        }

        if (warnings > 0)
        {
            parts.Add($"{warnings} warning{(warnings == 1 ? "" : "s")}");
        }

        if (parts.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Audit: {Summary}", string.Join(", ", parts));
    }
}