using System.Collections.Immutable;
using System.Net;
using System.Text;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class BuildHtmlCacheTests
{
    private static CachedResponse Ok(string body) =>
        new(HttpStatusCode.OK, Encoding.UTF8.GetBytes(body), [], []);

    private static ContentRoute Route(string canonical) => new()
    {
        CanonicalPath = new UrlPath(canonical),
        OutputFile = new FilePath(canonical.Trim('/')),
    };

    /// <summary>Stub IContentService that maps a single file path to a fixed impact.</summary>
    private sealed class StubContentService(string watchedPath, ContentChangeImpact impact) : IContentService
    {
        public ContentChangeImpact GetAffectedRoutes(FileChangeNotification change) =>
            string.Equals(change.FullPath, watchedPath, StringComparison.OrdinalIgnoreCase) ? impact : ContentChangeImpact.None;
        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() => AsyncEnumerable.Empty<DiscoveredItem>();
        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;
    }

    [Fact]
    public async Task GetOrAddAsync_InvokesFactoryOncePerKey()
    {
        var cache = new BuildHtmlCache([]);
        var calls = 0;

        await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("one")); });
        var second = await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("two")); });

        calls.ShouldBe(1);
        Encoding.UTF8.GetString(second.Body).ShouldBe("one");
    }

    [Fact]
    public async Task GetOrAddAsync_DistinctKeys_EachInvokeFactory()
    {
        var cache = new BuildHtmlCache([]);
        var calls = 0;

        await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("a")); });
        await cache.GetOrAddAsync("/b", () => { calls++; return Task.FromResult(Ok("b")); });

        calls.ShouldBe(2);
    }

    [Fact]
    public async Task OnFileChanged_RoutesImpact_EvictsOnlyAffectedKey()
    {
        var service = new StubContentService(
            "/content/a.md",
            ContentChangeImpact.Routes([Route("/a/")]));
        var cache = new BuildHtmlCache([service]);
        var aCalls = 0;
        var bCalls = 0;

        await cache.GetOrAddAsync("/a/", () => { aCalls++; return Task.FromResult(Ok("a-one")); });
        await cache.GetOrAddAsync("/b/", () => { bCalls++; return Task.FromResult(Ok("b-one")); });

        var response = cache.OnFileChanged(new FileChangeNotification("/content/a.md", WatcherChangeTypes.Changed));
        response.ShouldBe(FileWatchResponse.Refreshed);

        var freshA = await cache.GetOrAddAsync("/a/", () => { aCalls++; return Task.FromResult(Ok("a-two")); });
        var freshB = await cache.GetOrAddAsync("/b/", () => { bCalls++; return Task.FromResult(Ok("b-two")); });

        aCalls.ShouldBe(2);
        Encoding.UTF8.GetString(freshA.Body).ShouldBe("a-two");
        bCalls.ShouldBe(1);
        Encoding.UTF8.GetString(freshB.Body).ShouldBe("b-one");
    }

    [Fact]
    public async Task OnFileChanged_WildcardImpact_ClearsEverything()
    {
        var service = new StubContentService(
            "/content/_meta.yml",
            ContentChangeImpact.Wildcard);
        var cache = new BuildHtmlCache([service]);
        var calls = 0;

        await cache.GetOrAddAsync("/a/", () => { calls++; return Task.FromResult(Ok("a")); });
        await cache.GetOrAddAsync("/b/", () => { calls++; return Task.FromResult(Ok("b")); });

        cache.OnFileChanged(new FileChangeNotification("/content/_meta.yml", WatcherChangeTypes.Changed));

        await cache.GetOrAddAsync("/a/", () => { calls++; return Task.FromResult(Ok("a2")); });
        await cache.GetOrAddAsync("/b/", () => { calls++; return Task.FromResult(Ok("b2")); });

        calls.ShouldBe(4);
    }

    [Fact]
    public async Task OnFileChanged_NoneImpact_KeepsCache()
    {
        var service = new StubContentService(
            "/content/a.md",
            ContentChangeImpact.Routes([Route("/a/")]));
        var cache = new BuildHtmlCache([service]);
        var calls = 0;

        await cache.GetOrAddAsync("/a/", () => { calls++; return Task.FromResult(Ok("a")); });

        // Change reported as outside the service's scope (different file).
        cache.OnFileChanged(new FileChangeNotification("/content/unrelated.txt", WatcherChangeTypes.Changed));

        await cache.GetOrAddAsync("/a/", () => { calls++; return Task.FromResult(Ok("a2")); });

        calls.ShouldBe(1);
    }

    [Fact]
    public async Task GetOrAddAsync_FaultedFactory_IsEvictedAndRetried()
    {
        var cache = new BuildHtmlCache([]);
        var calls = 0;

        await Should.ThrowAsync<InvalidOperationException>(() =>
            cache.GetOrAddAsync("/a", () =>
            {
                calls++;
                return Task.FromException<CachedResponse>(new InvalidOperationException("render failed"));
            }));

        var recovered = await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("recovered")); });

        calls.ShouldBe(2);
        Encoding.UTF8.GetString(recovered.Body).ShouldBe("recovered");
    }

    [Fact]
    public async Task GetOrAddAsync_ConcurrentFirstHits_CoalesceToOneRender()
    {
        var cache = new BuildHtmlCache([]);
        var calls = 0;
        var gate = new TaskCompletionSource();

        var tasks = Enumerable.Range(0, 32)
            .Select(_ => cache.GetOrAddAsync("/a", async () =>
            {
                Interlocked.Increment(ref calls);
                await gate.Task;
                return Ok("body");
            }))
            .ToArray();

        gate.SetResult();
        await Task.WhenAll(tasks);

        calls.ShouldBe(1);
    }

    [Fact]
    public void CachedResponse_ToHttpResponseMessage_PreservesStatusBodyAndHeaders()
    {
        var captured = new CachedResponse(
            HttpStatusCode.MovedPermanently,
            Encoding.UTF8.GetBytes("<html></html>"),
            ResponseHeaders: [new("Location", ["/new"]), new("X-Pennington-Diagnostic", ["Warning|msg"])],
            ContentHeaders: [new("Content-Type", ["text/html"])]);

        using var message = captured.ToHttpResponseMessage();

        message.StatusCode.ShouldBe(HttpStatusCode.MovedPermanently);
        message.Headers.GetValues("Location").ShouldBe(["/new"]);
        message.Headers.GetValues("X-Pennington-Diagnostic").ShouldBe(["Warning|msg"]);
        message.Content.Headers.GetValues("Content-Type").ShouldBe(["text/html"]);
    }
}
