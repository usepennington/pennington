using System.Net;
using System.Text;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class BuildHtmlCacheTests
{
    private static CachedResponse Ok(string body) =>
        new(HttpStatusCode.OK, Encoding.UTF8.GetBytes(body), [], []);

    [Fact]
    public async Task GetOrAddAsync_InvokesFactoryOncePerKey()
    {
        var cache = new BuildHtmlCache();
        var calls = 0;

        await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("one")); });
        var second = await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("two")); });

        calls.ShouldBe(1);
        Encoding.UTF8.GetString(second.Body).ShouldBe("one");
    }

    [Fact]
    public async Task GetOrAddAsync_DistinctKeys_EachInvokeFactory()
    {
        var cache = new BuildHtmlCache();
        var calls = 0;

        await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("a")); });
        await cache.GetOrAddAsync("/b", () => { calls++; return Task.FromResult(Ok("b")); });

        calls.ShouldBe(2);
    }

    [Fact]
    public async Task OnFileChanged_ClearsCache_NextCallReRenders()
    {
        var cache = new BuildHtmlCache();
        var calls = 0;

        await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("one")); });

        var response = cache.OnFileChanged(new FileChangeNotification("/content/a.md", WatcherChangeTypes.Changed));
        response.ShouldBe(FileWatchResponse.Refreshed);

        var fresh = await cache.GetOrAddAsync("/a", () => { calls++; return Task.FromResult(Ok("two")); });

        calls.ShouldBe(2);
        Encoding.UTF8.GetString(fresh.Body).ShouldBe("two");
    }

    [Fact]
    public async Task GetOrAddAsync_FaultedFactory_IsEvictedAndRetried()
    {
        var cache = new BuildHtmlCache();
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
        var cache = new BuildHtmlCache();
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
