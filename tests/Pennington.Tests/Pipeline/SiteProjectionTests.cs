namespace Pennington.Tests.Pipeline;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Pennington.Infrastructure;
using Pennington.LlmsTxt;
using Pennington.Pipeline;
using Pennington.Routing;
using Pennington.Search;

/// <summary>
/// Focused unit tests for <see cref="SiteProjection"/>. End-to-end behavior through
/// the live request pipeline is covered by <c>LlmsTxtAndSearchEndpointTests</c> in
/// the integration suite; these tests pin the structural guarantees the projection
/// must give consumers in isolation.
/// </summary>
public class SiteProjectionTests
{
    [Fact]
    public async Task EmptyCorpus_NoServicesNoEndpoints_YieldsNothing()
    {
        var projection = CreateProjection();

        var pages = new List<RenderedPage>();
        await foreach (var page in projection.GetPagesAsync(TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        pages.ShouldBeEmpty();
    }

    [Fact]
    public async Task EndpointEntry_YieldsEndpointOriginWithoutFetch()
    {
        var endpoint = MakeRouteEndpoint(
            "/_llms/agent-context.md",
            new LlmsTxtEntryMetadata("Agent Context", "Build context for agents"));
        var projection = CreateProjection(endpointDataSource: new StubEndpointDataSource(endpoint));

        var pages = new List<RenderedPage>();
        await foreach (var page in projection.GetPagesAsync(TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        pages.Count.ShouldBe(1);
        var only = pages[0];
        only.Toc.Title.ShouldBe("Agent Context");
        only.Toc.Description.ShouldBe("Build context for agents");
        only.Html.ShouldBe("");
        only.Content.ShouldBeNull();

        // Endpoint entries route to the user-defined URL — no fetch happens, so the
        // origin carries the DirectUrl the projection's downstream consumers point at.
        only.Origin.Value.ShouldBeOfType<EndpointOrigin>().DirectUrl.ShouldBe("/_llms/agent-context.md");

        // Lazy sections for endpoint entries materialize to an empty list rather than
        // throwing on null content.
        only.Sections.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ParameterizedEndpoint_IsSkipped()
    {
        // Route patterns with parameters yield no concrete URL, so they can't appear
        // in the projection — same constraint as the rest of the build crawl.
        var endpoint = MakeRouteEndpoint("/items/{id}", new LlmsTxtEntryMetadata("Parameterized"));
        var projection = CreateProjection(endpointDataSource: new StubEndpointDataSource(endpoint));

        var pages = new List<RenderedPage>();
        await foreach (var page in projection.GetPagesAsync(TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        pages.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetPageAsync_KnownPath_ReturnsMatchingEntry()
    {
        var endpoint = MakeRouteEndpoint(
            "/_llms/agent-context.md",
            new LlmsTxtEntryMetadata("Agent Context"));
        var projection = CreateProjection(endpointDataSource: new StubEndpointDataSource(endpoint));

        var page = await projection.GetPageAsync(new UrlPath("/_llms/agent-context.md"), TestContext.Current.CancellationToken);

        page.ShouldNotBeNull();
        page.Toc.Title.ShouldBe("Agent Context");
    }

    [Fact]
    public async Task GetPageAsync_UnknownPath_ReturnsNull()
    {
        var projection = CreateProjection();

        var page = await projection.GetPageAsync(new UrlPath("/not/found"), TestContext.Current.CancellationToken);

        page.ShouldBeNull();
    }

    private static SiteProjection CreateProjection(EndpointDataSource? endpointDataSource = null)
    {
        var dispatcher = new StubDispatcher();
        return new SiteProjection(
            contentServices: [],
            enrichment: new MetadataEnrichmentService([]),
            renderer: new StubRenderer(),
            xrefResolver: new XrefResolvingService(new XrefResolver([])),
            fetcher: new RenderedHtmlFetcher(dispatcher, NullLogger<RenderedHtmlFetcher>.Instance),
            extractor: new HeadingSectionExtractor(),
            options: new SiteProjectionOptions(),
            endpointDataSource: endpointDataSource ?? new StubEndpointDataSource(),
            logger: NullLogger<SiteProjection>.Instance);
    }

    private static RouteEndpoint MakeRouteEndpoint(string pattern, object? metadata)
    {
        var collection = metadata is null
            ? new EndpointMetadataCollection()
            : new EndpointMetadataCollection(metadata);

        return new RouteEndpoint(
            requestDelegate: _ => Task.CompletedTask,
            routePattern: RoutePatternFactory.Parse(pattern),
            order: 0,
            metadata: collection,
            displayName: pattern);
    }

    private sealed class StubEndpointDataSource(params RouteEndpoint[] endpoints) : EndpointDataSource
    {
        public override IReadOnlyList<Microsoft.AspNetCore.Http.Endpoint> Endpoints { get; } = endpoints;
        public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);
    }

    private sealed class StubRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item) =>
            Task.FromResult(new ContentItem(new FailedItem(item.Route, new ContentError("not used"))));
    }

    private sealed class StubDispatcher : IInProcessHttpDispatcher
    {
        public HttpClient CreateClient() => new(new StubHandler());

        private sealed class StubHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}
