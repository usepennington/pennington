using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class RenderedHtmlFetcherTests
{
    private sealed class StubHandler(string html) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html"),
            });
    }

    private sealed class StubDispatcher(string html) : IInProcessHttpDispatcher
    {
        public HttpClient CreateClient() =>
            new(new StubHandler(html)) { BaseAddress = new Uri("http://localhost/") };
    }

    [Fact]
    public async Task FetchContentAsync_ConcurrentCalls_DoNotRace()
    {
        // Regression guard for the per-call browsing context: AngleSharp's IBrowsingContext
        // is not safe for concurrent OpenAsync, and SearchArtifactService now fetches in parallel.
        const string html = "<html><body><main id=\"main\"><h1>Title</h1><p>Body</p></main></body></html>";
        var fetcher = new RenderedHtmlFetcher(new StubDispatcher(html), NullLogger<RenderedHtmlFetcher>.Instance);
        var ct = TestContext.Current.CancellationToken;

        var tasks = Enumerable.Range(0, 50)
            .Select(_ => fetcher.FetchContentAsync("/page", "#main", ct))
            .ToArray();

        var elements = await Task.WhenAll(tasks);

        elements.ShouldAllBe(e => e != null && e.QuerySelector("h1")!.TextContent == "Title");
    }
}
