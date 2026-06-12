namespace Pennington.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Infrastructure;
using Pennington.Pipeline;

/// <summary>
/// Regression for the b719d73 deadlock class: a response processor that awaits
/// <see cref="ISiteProjection"/> used to hang the site forever on first request (the
/// projection's self-fetches re-entered the processor, which awaited the blocked
/// materialization — a task-level circular wait). The corpus-fetch guard now fails fast,
/// so the request COMPLETES: the misbehaving processor's inner consumption throws on
/// every projection-issued page fetch instead of parking on it.
/// </summary>
[Collection(DocsTestServerCollection.Name)]
public class CorpusFetchGuardTests
{
    private readonly DocsWebApplicationFactory _factory;

    public CorpusFetchGuardTests(DocsWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task ProjectionConsumingResponseProcessor_FailsFast_InsteadOfDeadlocking()
    {
        using var poisoned = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddSingleton<IResponseProcessor, ProjectionConsumingProcessor>()));
        using var client = poisoned.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(3);

        // Pre-guard, this request never returned: the outer processor started the
        // projection materialization and every self-fetched page's processor awaited the
        // same blocked task. Any completed response — success or error — proves the
        // cycle is broken; the client timeout is the hang detector.
        var poisonedResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);
        _ = poisonedResponse.StatusCode; // completed at all = no deadlock
    }

    /// <summary>
    /// Deliberately violates the ISiteProjection lifecycle invariant — runs on every HTML
    /// response and awaits the projection. The guard turns the resulting deadlock into a
    /// descriptive InvalidOperationException on projection-issued fetches.
    /// </summary>
    private sealed class ProjectionConsumingProcessor : IResponseProcessor
    {
        public int Order => 1000;

        public bool ShouldProcess(HttpContext context)
            => context.Response.ContentType?.Contains("text/html") == true;

        public async Task<string> ProcessAsync(string responseBody, HttpContext context)
        {
            var projection = context.RequestServices.GetRequiredService<ISiteProjection>();
            await foreach (var _ in projection.GetPagesAsync(context.RequestAborted))
            {
                break;
            }

            return responseBody;
        }
    }
}
