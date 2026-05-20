namespace Pennington.Tests.LlmsTxt;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Pennington.Infrastructure;
using Pennington.LlmsTxt;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions.Testing;

public class MapGetEntryTests
{
    [Fact]
    public async Task EndpointWithLlmsTxtEntry_AppearsInIndex()
    {
        var endpoint = MakeRouteEndpoint(
            "/_llms/agent-context.md",
            new LlmsTxtEntryMetadata("Agent Context", "Build context for agents"));

        var service = CreateService(endpointDataSource: new StubEndpointDataSource(endpoint));

        var index = await service.GetLlmsTxtAsync();

        index.ShouldContain("Agent Context");
        index.ShouldContain("/_llms/agent-context.md");
        index.ShouldContain("Build context for agents");
    }

    [Fact]
    public async Task EndpointWithoutMetadata_IsIgnored()
    {
        var endpoint = MakeRouteEndpoint("/some/route", metadata: null);

        var service = CreateService(endpointDataSource: new StubEndpointDataSource(endpoint));

        var index = await service.GetLlmsTxtAsync();

        index.ShouldNotContain("/some/route");
    }

    [Fact]
    public async Task ParameterizedEndpoint_IsIgnored()
    {
        // Route patterns with parameters don't yield a concrete URL, so they
        // can't appear in the static index even with the metadata attached.
        var endpoint = MakeRouteEndpoint(
            "/items/{id}",
            new LlmsTxtEntryMetadata("Parameterized"));

        var service = CreateService(endpointDataSource: new StubEndpointDataSource(endpoint));

        var index = await service.GetLlmsTxtAsync();

        index.ShouldNotContain("Parameterized");
    }

    private static LlmsTxtService CreateService(EndpointDataSource endpointDataSource)
    {
        var fs = new MockFileSystem();
        var canonicalBase = new CanonicalBaseUrl(new UrlPath("https://example.test"));
        var dispatcher = new StubDispatcher();
        return new LlmsTxtService(
            contentServices: [],
            parser: new StubParser(),
            renderer: new StubRenderer(),
            xrefResolver: new XrefResolvingService(new XrefResolver([])),
            fetcher: new RenderedHtmlFetcher(dispatcher, NullLogger<RenderedHtmlFetcher>.Instance),
            subtrees: [],
            fileSystem: fs,
            hostingEnvironment: new StubHostEnvironment(),
            pennOptions: new PenningtonOptions { SiteTitle = "Test Site" },
            llmsTxtOptions: new LlmsTxtOptions(),
            canonicalBase: canonicalBase,
            navigationBuilder: new NavigationBuilder(),
            endpointDataSource: endpointDataSource,
            logger: NullLogger<LlmsTxtService>.Instance);
    }

    private static RouteEndpoint MakeRouteEndpoint(string pattern, object? metadata)
    {
        var collection = metadata is null
            ? new EndpointMetadataCollection(new HttpMethodMetadata(["GET"]))
            : new EndpointMetadataCollection(new HttpMethodMetadata(["GET"]), metadata);

        return new RouteEndpoint(
            requestDelegate: _ => Task.CompletedTask,
            routePattern: RoutePatternFactory.Parse(pattern),
            order: 0,
            metadata: collection,
            displayName: pattern);
    }

    private sealed class StubEndpointDataSource(params RouteEndpoint[] endpoints) : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints { get; } = endpoints;
        public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);
    }

    private sealed class StubParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item) =>
            Task.FromResult(new ContentItem(new FailedItem(item.Route, new ContentError("not used"))));
    }

    private sealed class StubRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item) =>
            Task.FromResult(new ContentItem(new FailedItem(item.Route, new ContentError("not used"))));
    }

    private sealed class StubDispatcher : IInProcessHttpDispatcher
    {
        public HttpClient CreateClient() => new();
    }

    private sealed class StubHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "Test";
        public string WebRootPath { get; set; } = "";
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = "/";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}