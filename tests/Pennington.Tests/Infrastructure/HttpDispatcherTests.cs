using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class HttpDispatcherTests
{
    [Fact]
    public void CreateClient_UnstartedTestServer_ThrowsSelfFetchUnavailable()
    {
        // A TestServer whose host hasn't started has a null Application, so CreateHandler()
        // throws InvalidOperationException. The dispatcher must surface that as the dedicated
        // infrastructure failure (not a generic exception a per-page catch would swallow) so
        // the projection retries instead of caching an empty corpus. This is the exact
        // condition the Windows build-ordering bug hit when a startup hosted service raced
        // the server start.
        using var server = new TestServer(new ServiceCollection().BuildServiceProvider());
        var dispatcher = new HttpDispatcher(server, new BuildHtmlCache([]));

        Should.Throw<SelfFetchUnavailableException>(() => dispatcher.CreateClient());
    }

    [Fact]
    public void CreateClient_NonTestServerWithoutAddresses_ThrowsSelfFetchUnavailable()
    {
        // The Kestrel path with no bound addresses is the same "server isn't ready" condition.
        var dispatcher = new HttpDispatcher(new NoAddressServer(), new BuildHtmlCache([]));

        Should.Throw<SelfFetchUnavailableException>(() => dispatcher.CreateClient());
    }

    private sealed class NoAddressServer : IServer
    {
        public IFeatureCollection Features { get; } = new FeatureCollection();
        public void Dispose() { }
        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
            where TContext : notnull => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
