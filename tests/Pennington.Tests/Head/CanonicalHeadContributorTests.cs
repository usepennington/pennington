using Microsoft.AspNetCore.Http;
using Pennington.Content;
using Pennington.Head;
using Pennington.Infrastructure;

namespace Pennington.Tests.Head;

/// <summary>
/// DOM-level tests for <see cref="CanonicalHeadContributor"/> driven through the real
/// <see cref="HeadCompositionHtmlRewriter"/> + <see cref="HtmlResponseRewritingProcessor"/>.
/// </summary>
public class CanonicalHeadContributorTests
{
    private static async Task<string> Render(
        string requestPath,
        string body = "<html><head></head><body></body></html>",
        string? canonicalBaseUrl = "https://example.com")
    {
        var options = new PenningtonOptions { CanonicalBaseUrl = canonicalBaseUrl };
        var registry = new ContentRecordRegistry(Array.Empty<ContentRecord>());
        var processor = new HtmlResponseRewritingProcessor(
            [new HeadCompositionHtmlRewriter([new CanonicalHeadContributor(options)], registry)]);
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = requestPath;
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return await processor.ProcessAsync(body, ctx);
    }

    [Fact]
    public async Task EmitsCanonical_FromBaseUrlAndPath()
    {
        var html = await Render("/guide/intro/");

        html.ShouldContain("rel=\"canonical\"");
        html.ShouldContain("https://example.com/guide/intro/");
    }

    [Fact]
    public async Task DoesNotRun_WhenCanonicalBaseUrlIsUnset()
    {
        var html = await Render("/guide/intro/", canonicalBaseUrl: null);

        html.ShouldNotContain("rel=\"canonical\"");
    }

    [Fact]
    public async Task DoesNotOverwrite_PageAuthoredCanonical()
    {
        var body = """<html><head><link rel="canonical" href="https://other.test/custom/"></head><body></body></html>""";

        var html = await Render("/guide/intro/", body);

        html.ShouldContain("https://other.test/custom/");
        html.ShouldNotContain("https://example.com/guide/intro/");
    }
}
