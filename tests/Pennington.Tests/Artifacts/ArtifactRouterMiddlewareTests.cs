using System.Collections.Immutable;
using System.Text;
using Microsoft.AspNetCore.Http;
using Pennington.Artifacts;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Artifacts;

public class ArtifactRouterMiddlewareTests
{
    [Fact]
    public async Task ClaimedPath_ResolverHit_ServesBytesAndContentType()
    {
        var service = new FakeArtifactService(
            new ArtifactClaim("fake", new PrefixClaim(new UrlPath("/search/"), ".json"), "test"),
            resolved: new ArtifactContent("{}"u8.ToArray(), "application/json; charset=utf-8"));
        var (context, nextCalled) = await InvokeAsync("/search/en/index.json", service);

        nextCalled().ShouldBeFalse();
        context.Response.ContentType.ShouldBe("application/json; charset=utf-8");
        BodyOf(context).ShouldBe("{}");
        service.ResolvedPaths.ShouldBe(["search/en/index.json"]);
    }

    [Fact]
    public async Task ClaimedPath_ResolverDeclines_FallsThrough()
    {
        // Claim-and-decline: a miss inside the territory must reach content routing,
        // so a real page under a claimed prefix keeps working.
        var service = new FakeArtifactService(
            new ArtifactClaim("fake", new PrefixClaim(new UrlPath("/search/"), ".json"), "test"),
            resolved: null);
        var (_, nextCalled) = await InvokeAsync("/search/en/missing.json", service);

        nextCalled().ShouldBeTrue();
    }

    [Fact]
    public async Task UnclaimedPath_NeverConsultsTheResolver()
    {
        var service = new FakeArtifactService(
            new ArtifactClaim("fake", new PrefixClaim(new UrlPath("/search/"), ".json"), "test"),
            resolved: new ArtifactContent([1], "application/json"));
        var (_, nextCalled) = await InvokeAsync("/docs/page/", service);

        nextCalled().ShouldBeTrue();
        service.ResolvedPaths.ShouldBeEmpty();
    }

    [Fact]
    public async Task NonGetMethod_FallsThroughWithoutMatching()
    {
        var service = new FakeArtifactService(
            new ArtifactClaim("fake", new ExactClaim(new UrlPath("/llms.txt")), "test"),
            resolved: new ArtifactContent([1], "text/plain"));
        var (_, nextCalled) = await InvokeAsync("/llms.txt", service, method: "POST");

        nextCalled().ShouldBeTrue();
        service.ResolvedPaths.ShouldBeEmpty();
    }

    [Fact]
    public async Task HeadRequest_SetsHeadersWithoutBody()
    {
        var service = new FakeArtifactService(
            new ArtifactClaim("fake", new ExactClaim(new UrlPath("/llms.txt")), "test"),
            resolved: new ArtifactContent("hello"u8.ToArray(), "text/plain; charset=utf-8"));
        var (context, nextCalled) = await InvokeAsync("/llms.txt", service, method: "HEAD");

        nextCalled().ShouldBeFalse();
        context.Response.ContentType.ShouldBe("text/plain; charset=utf-8");
        context.Response.ContentLength.ShouldBe(5);
        BodyOf(context).ShouldBeEmpty();
    }

    [Fact]
    public async Task FirstResolvingServiceWins_LaterServicesNotConsulted()
    {
        var first = new FakeArtifactService(
            new ArtifactClaim("first", new ExactClaim(new UrlPath("/file.txt")), "test"),
            resolved: new ArtifactContent("first"u8.ToArray(), "text/plain"));
        var second = new FakeArtifactService(
            new ArtifactClaim("second", new ExactClaim(new UrlPath("/file.txt")), "test"),
            resolved: new ArtifactContent("second"u8.ToArray(), "text/plain"));
        var (context, _) = await InvokeAsync("/file.txt", first, second);

        BodyOf(context).ShouldBe("first");
        second.ResolvedPaths.ShouldBeEmpty();
    }

    private static Task<(DefaultHttpContext Context, Func<bool> NextCalled)> InvokeAsync(
        string path,
        params FakeArtifactService[] services)
        => InvokeAsync(path, services, method: "GET");

    private static Task<(DefaultHttpContext Context, Func<bool> NextCalled)> InvokeAsync(
        string path,
        FakeArtifactService service,
        string method)
        => InvokeAsync(path, [service], method);

    private static async Task<(DefaultHttpContext Context, Func<bool> NextCalled)> InvokeAsync(
        string path,
        FakeArtifactService[] services,
        string method)
    {
        var nextCalled = false;
        var middleware = new ArtifactRouterMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, services);
        return (context, () => nextCalled);
    }

    private static string BodyOf(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEnd();
    }

    private sealed class FakeArtifactService(ArtifactClaim claim, ArtifactContent? resolved) : IArtifactContentService
    {
        public List<string> ResolvedPaths { get; } = [];

        public ImmutableList<ArtifactClaim> Claims { get; } = [claim];

        public Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken)
        {
            ResolvedPaths.Add(relativePath);
            return Task.FromResult(resolved);
        }

        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() => AsyncEnumerable.Empty<DiscoveredItem>();
    }
}
