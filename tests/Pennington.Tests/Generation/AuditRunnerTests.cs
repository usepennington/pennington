using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

public class AuditRunnerTests
{
    [Fact]
    public async Task StartAsync_InBuildMode_RunsRenderedAuditor_AndCachesItsDiagnostics()
    {
        var services = new ServiceCollection();
        services.AddSingleton<AuditCache>();
        services.AddSingleton<IAuditCache>(sp => sp.GetRequiredService<AuditCache>());
        services.AddSingleton(new LocalizationOptions());
        services.AddSingleton<IFileWatcher, StubFileWatcher>();
        services.AddSingleton<ISiteProjection, StubProjection>();
        services.AddTransient<IRenderedAuditor, FakeRenderedAuditor>();

        using var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<AuditCache>();
        var lifetime = new StubLifetime();
        var runner = new AuditRunner(
            sp,
            cache,
            sp.GetRequiredService<IFileWatcher>(),
            sp.GetRequiredService<LocalizationOptions>(),
            lifetime,
            NullLogger<AuditRunner>.Instance,
            isBuildMode: true);

        await runner.StartAsync(TestContext.Current.CancellationToken);

        // The initial pass is gated on ApplicationStarted so the server is up before any
        // self-fetch; nothing should have run yet.
        cache.Diagnostics.ShouldBeEmpty();
        lifetime.FireStarted();

        // RunAsync started on ApplicationStarted; give the cache a moment to be populated.
        for (var i = 0; i < 50 && cache.Diagnostics.IsEmpty; i++)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        cache.Diagnostics.Count.ShouldBe(1);
        cache.Diagnostics[0].Message.ShouldBe("rendered audit ran");
        cache.Diagnostics[0].SourceFile.ShouldBe("test.rendered");
    }

    [Fact]
    public async Task StartAsync_InDevMode_SkipsRenderedAuditors()
    {
        var services = new ServiceCollection();
        services.AddSingleton<AuditCache>();
        services.AddSingleton<IAuditCache>(sp => sp.GetRequiredService<AuditCache>());
        services.AddSingleton(new LocalizationOptions());
        services.AddSingleton<IFileWatcher, StubFileWatcher>();
        services.AddSingleton<ISiteProjection, StubProjection>();
        var renderedAuditor = new FakeRenderedAuditor();
        services.AddSingleton<IRenderedAuditor>(renderedAuditor);

        using var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<AuditCache>();
        var lifetime = new StubLifetime();
        var runner = new AuditRunner(
            sp,
            cache,
            sp.GetRequiredService<IFileWatcher>(),
            sp.GetRequiredService<LocalizationOptions>(),
            lifetime,
            NullLogger<AuditRunner>.Instance,
            isBuildMode: false);

        await runner.StartAsync(TestContext.Current.CancellationToken);
        lifetime.FireStarted();

        // Wait long enough for any background run to settle.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        renderedAuditor.AuditCalls.ShouldBe(0);
        cache.Diagnostics.ShouldBeEmpty();
    }

    private sealed class FakeRenderedAuditor : IRenderedAuditor
    {
        public string Code => "test.rendered";
        public int AuditCalls { get; private set; }
        public Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(RenderedAuditContext context, CancellationToken cancellationToken)
        {
            AuditCalls++;
            IReadOnlyList<BuildDiagnostic> diagnostics =
            [
                new BuildDiagnostic(DiagnosticSeverity.Warning, Route: null, Message: "rendered audit ran", SourceFile: Code),
            ];
            return Task.FromResult(diagnostics);
        }
    }

    private sealed class StubProjection : ISiteProjection
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators
        public async IAsyncEnumerable<RenderedPage> GetPagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }
#pragma warning restore CS1998

        public Task<RenderedPage?> GetPageAsync(UrlPath canonicalPath, CancellationToken cancellationToken = default)
            => Task.FromResult<RenderedPage?>(null);
    }

    private sealed class StubFileWatcher : IFileWatcher
    {
        public void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true) { }
        public void SubscribeToChanges(Action onUpdate) { }
        public void SubscribeToChanges(Action<FileChangeNotification> onUpdate) { }
        public void Dispose() { }
    }

    private sealed class StubLifetime : IHostApplicationLifetime
    {
        private readonly CancellationTokenSource _started = new();
        public CancellationToken ApplicationStarted => _started.Token;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;
        public void StopApplication() { }

        // Mirrors the host firing ApplicationStarted once every hosted service (the server
        // included) has started — the gate AuditRunner now waits on before its initial pass.
        public void FireStarted() => _started.Cancel();
    }
}