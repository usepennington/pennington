using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Generation;
using Pennington.Infrastructure;

namespace Pennington.Tests.Generation;

public class AuditRunnerTests
{
    [Fact]
    public async Task StartAsync_RunsRenderedAuditor_AndCachesItsDiagnostics()
    {
        var services = new ServiceCollection();
        services.AddSingleton<AuditCache>();
        services.AddSingleton<IAuditCache>(sp => sp.GetRequiredService<AuditCache>());
        services.AddSingleton<LocalizationOptions>(new LocalizationOptions());
        services.AddSingleton<IFileWatcher, StubFileWatcher>();
        services.AddSingleton<IInProcessHttpDispatcher, StubDispatcher>();
        services.AddTransient<IRenderedAuditor, FakeRenderedAuditor>();

        using var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<AuditCache>();
        var runner = new AuditRunner(
            sp,
            cache,
            sp.GetRequiredService<IFileWatcher>(),
            sp.GetRequiredService<LocalizationOptions>(),
            NullLogger<AuditRunner>.Instance);

        await runner.StartAsync(TestContext.Current.CancellationToken);

        // RunAsync started in StartAsync; give the cache a moment to be populated.
        for (var i = 0; i < 50 && cache.Diagnostics.IsEmpty; i++)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        cache.Diagnostics.Count.ShouldBe(1);
        cache.Diagnostics[0].Message.ShouldBe("rendered audit ran");
        cache.Diagnostics[0].SourceFile.ShouldBe("test.rendered");
    }

    private sealed class FakeRenderedAuditor : IRenderedAuditor
    {
        public string Code => "test.rendered";
        public Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(RenderedAuditContext context, CancellationToken cancellationToken)
        {
            IReadOnlyList<BuildDiagnostic> diagnostics =
            [
                new BuildDiagnostic(DiagnosticSeverity.Warning, Route: null, Message: "rendered audit ran", SourceFile: Code),
            ];
            return Task.FromResult(diagnostics);
        }
    }

    private sealed class StubDispatcher : IInProcessHttpDispatcher
    {
        public HttpClient CreateClient() => new(new StubHandler()) { BaseAddress = new Uri("http://localhost/") };

        private sealed class StubHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }

    private sealed class StubFileWatcher : IFileWatcher
    {
        public void AddPathWatch(string path, string filePattern, Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true) { }
        public void SubscribeToChanges(Action onUpdate) { }
        public void SubscribeToChanges(Action<FileChangeNotification> onUpdate) { }
        public void Dispose() { }
    }
}