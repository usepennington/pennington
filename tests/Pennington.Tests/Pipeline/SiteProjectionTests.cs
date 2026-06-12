namespace Pennington.Tests.Pipeline;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Pennington.Content;
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
        only.Origin.ShouldNotBeNull().Value.ShouldBeOfType<EndpointOrigin>().DirectUrl.ShouldBe("/_llms/agent-context.md");

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

    [Fact]
    public async Task SelfFetchUnavailable_DuringSeed_FaultsRatherThanCachingEmptyCorpus()
    {
        // Regression for the Windows build-ordering bug: when the in-process server isn't
        // started yet, the self-fetch throws SelfFetchUnavailableException. That must NOT be
        // swallowed as a per-page skip (which would cache an empty corpus and silently ship a
        // zero-page search index / llms.txt). It must fault the seed so AsyncLazy evicts and
        // the next access — once the server is up — rebuilds the real corpus.
        var dispatcher = new FlakyDispatcher();
        var projection = CreateProjection(
            contentServices: [new SinglePageContentService()],
            dispatcher: dispatcher);
        var ct = TestContext.Current.CancellationToken;

        // First access: server not ready -> infrastructure failure propagates.
        await Should.ThrowAsync<SelfFetchUnavailableException>(async () =>
        {
            await foreach (var _ in projection.GetPagesAsync(ct))
            {
            }
        });

        // Second access after the server is up: the faulted seed was evicted, so the
        // projection rebuilds and yields the real page instead of an empty corpus.
        dispatcher.ServerReady = true;
        var pages = new List<RenderedPage>();
        await foreach (var page in projection.GetPagesAsync(ct))
        {
            pages.Add(page);
        }

        pages.Count.ShouldBe(1);
        pages[0].Route.CanonicalPath.Value.ShouldBe("/page/");
        dispatcher.CreateClientCalls.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetPagesAsync_InsideCorpusFetch_FailsFastInsteadOfDeadlocking()
    {
        // The b719d73 guard: a request the projection itself issued (marked by the
        // corpus-fetch scope) must never await the projection — that's a task-cycle
        // deadlock. The projection throws a descriptive exception instead of hanging.
        var projection = CreateProjection();
        using var scope = CorpusFetchScope.EnterCorpusFetch();

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in projection.GetPagesAsync(TestContext.Current.CancellationToken))
            {
            }
        });

        ex.Message.ShouldContain("b719d73");
    }

    [Fact]
    public async Task GetPageAsync_InsideMaterialization_FailsFastInsteadOfDeadlocking()
    {
        // Re-entering the projection from work its own materialization spawned would
        // await a single-flight task that is waiting on the caller.
        var projection = CreateProjection();
        using var scope = CorpusFetchScope.EnterMaterialization();

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => projection.GetPageAsync(new UrlPath("/page/"), TestContext.Current.CancellationToken));

        ex.Message.ShouldContain("self-deadlock");
    }

    [Fact]
    public async Task GetPagesAsync_AfterScopeDisposed_Succeeds()
    {
        var projection = CreateProjection();
        using (CorpusFetchScope.EnterCorpusFetch())
        {
        }

        var pages = new List<RenderedPage>();
        await foreach (var page in projection.GetPagesAsync(TestContext.Current.CancellationToken))
        {
            pages.Add(page);
        }

        pages.ShouldBeEmpty();
    }

    private static SiteProjection CreateProjection(
        EndpointDataSource? endpointDataSource = null,
        IEnumerable<IContentService>? contentServices = null,
        IInProcessHttpDispatcher? dispatcher = null)
    {
        return new SiteProjection(
            contentServices: contentServices ?? [],
            enrichment: new MetadataEnrichmentService([]),
            renderer: new StubRenderer(),
            xrefResolver: new XrefResolvingService(new XrefResolver([])),
            fetcher: new RenderedHtmlFetcher(dispatcher ?? new StubDispatcher(), NullLogger<RenderedHtmlFetcher>.Instance),
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

    // Throws the infrastructure failure until the "server" is marked ready, mirroring
    // HttpDispatcher.CreateClient against a TestServer whose Application is not yet set.
    private sealed class FlakyDispatcher : IInProcessHttpDispatcher
    {
        public bool ServerReady { get; set; }
        public int CreateClientCalls { get; private set; }

        public HttpClient CreateClient()
        {
            CreateClientCalls++;
            if (!ServerReady)
            {
                throw new SelfFetchUnavailableException("server not started (test)");
            }

            return new HttpClient(new OkHandler()) { BaseAddress = new Uri("http://localhost/") };
        }

        private sealed class OkHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "<html><body><main><h1>Page</h1><p>Body</p></main></body></html>",
                        System.Text.Encoding.UTF8,
                        "text/html"),
                });
        }
    }

    // Minimal content service yielding a single fetchable TOC entry (no LlmsOnlySource,
    // so the projection takes the HTTP self-fetch path through the dispatcher).
    private sealed class SinglePageContentService : IContentService
    {
        private static readonly ContentRoute Route = new()
        {
            CanonicalPath = new UrlPath("/page/"),
            OutputFile = new FilePath("page/index.html"),
        };

        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() => AsyncEnumerable.Empty<DiscoveredItem>();

        public Task<System.Collections.Immutable.ImmutableList<ContentToCopy>> GetContentToCopyAsync()
            => Task.FromResult(System.Collections.Immutable.ImmutableList<ContentToCopy>.Empty);

        public Task<System.Collections.Immutable.ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
            => Task.FromResult(System.Collections.Immutable.ImmutableList.Create(
                new ContentTocItem("Page", Route, 0, ["page"], null, null)));

        public Task<System.Collections.Immutable.ImmutableList<CrossReference>> GetCrossReferencesAsync()
            => Task.FromResult(System.Collections.Immutable.ImmutableList<CrossReference>.Empty);

        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;
    }
}
