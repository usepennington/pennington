using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class HttpDispatcherTests
{
    [Fact]
    public async Task CreateClient_KestrelPath_PoolsConnectionsAcrossClients()
    {
        // Regression: the dispatcher used to build a fresh HttpClientHandler per CreateClient(),
        // and both callers wrap the result in `using`. Every self-fetch therefore opened a new
        // loopback socket that then sat in TIME_WAIT — on a 22k-page corpus that exhausted the
        // ~16k Windows ephemeral port range partway through, and the remaining fetches failed
        // with SocketError.AddressAlreadyInUse. Those pages were silently dropped from the search
        // index, since SiteProjection treats a failed fetch as a per-page error. Sharing one
        // handler chain keeps the connections pooled, so a whole corpus costs a handful of ports.
        var clientPorts = new ConcurrentBag<int>();

        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();

        app.Map("/{**slug}", (HttpContext ctx) =>
        {
            clientPorts.Add(ctx.Connection.RemotePort);
            return Results.Text("<html><body>ok</body></html>", "text/html");
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var dispatcher = new HttpDispatcher(
            app.Services.GetRequiredService<IServer>(),
            new BuildHtmlCache([]));

        // Distinct paths on purpose: CachingHttpHandler replays a cached response per path, so
        // repeating one URL would never reach the socket and the test would pass vacuously.
        const int requests = 200;
        for (var i = 0; i < requests; i++)
        {
            using var client = dispatcher.CreateClient();
            var response = await client.GetAsync($"/page-{i}", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
        }

        clientPorts.Count.ShouldBe(requests);

        // Before the fix this was `requests` (one socket per fetch, none reused).
        clientPorts.Distinct().Count().ShouldBeLessThanOrEqualTo(Environment.ProcessorCount * 2);

        await app.StopAsync(TestContext.Current.CancellationToken);
    }

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
