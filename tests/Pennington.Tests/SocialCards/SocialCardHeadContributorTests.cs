using Microsoft.AspNetCore.Http;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Head;
using Pennington.Infrastructure;
using Pennington.Routing;
using Pennington.SocialCards;

namespace Pennington.Tests.SocialCards;

/// <summary>
/// DOM-level tests for <see cref="SocialCardHeadContributor"/>, driven through the real
/// <see cref="HeadCompositionHtmlRewriter"/> + <see cref="HtmlResponseRewritingProcessor"/> so the
/// card meta tags land in <c>&lt;head&gt;</c> exactly as in the response pipeline.
/// </summary>
public class SocialCardHeadContributorTests
{
    private sealed record Fm : IFrontMatter
    {
        public string Title { get; init; } = "";
    }

    private static ContentRecord Record(string url) =>
        new(ContentRouteFactory.FromUrl(new UrlPath(url)), new Fm { Title = url });

    private static async Task<string> Render(
        string requestPath,
        ContentRecordRegistry registry,
        string? canonicalBaseUrl = "https://example.com",
        string body = "<html><head></head><body></body></html>")
    {
        var cardOptions = new SocialCardOptions { Render = (_, _, _) => Task.FromResult<byte[]?>([1]) };
        var site = new PenningtonOptions { CanonicalBaseUrl = canonicalBaseUrl };
        var contributor = new SocialCardHeadContributor(cardOptions, site);
        var processor = new HtmlResponseRewritingProcessor(
            [new HeadCompositionHtmlRewriter([contributor], registry)]);
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = requestPath;
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return await processor.ProcessAsync(body, ctx);
    }

    [Fact]
    public async Task EmitsCardMetaTags_ForPageWithContentRecord()
    {
        var registry = new ContentRecordRegistry([Record("/guide/intro/")]);

        var html = await Render("/guide/intro/", registry);

        html.ShouldContain("""property="og:image" content="https://example.com/social-cards/guide/intro.png""");
        html.ShouldContain("""name="twitter:image" content="https://example.com/social-cards/guide/intro.png""");
        html.ShouldContain("""name="twitter:card" content="summary_large_image""");
    }

    [Fact]
    public async Task EmitsRootRelativeUrl_WithoutCanonicalBaseUrl()
    {
        var registry = new ContentRecordRegistry([Record("/guide/intro/")]);

        var html = await Render("/guide/intro/", registry, canonicalBaseUrl: null);

        html.ShouldContain("""property="og:image" content="/social-cards/guide/intro.png""");
    }

    [Fact]
    public async Task SkipsPages_WithoutContentRecord()
    {
        var registry = new ContentRecordRegistry(Array.Empty<ContentRecord>());

        var html = await Render("/no-record/", registry);

        html.ShouldNotContain("og:image");
    }

    [Fact]
    public async Task DoesNotOverwrite_PageAuthoredImage()
    {
        // Author-supplied image tags (e.g. BlogSite's SocialMediaImageUrlFactory via Blog.razor)
        // win reconciliation per key; dedup is per tag, so a page overriding the image authors both.
        var registry = new ContentRecordRegistry([Record("/guide/intro/")]);
        var body = """
            <html><head>
            <meta property="og:image" content="https://other.test/custom.png">
            <meta name="twitter:image" content="https://other.test/custom.png">
            </head><body></body></html>
            """;

        var html = await Render("/guide/intro/", registry, body: body);

        html.ShouldContain("https://other.test/custom.png");
        html.ShouldNotContain("/social-cards/guide/intro.png");
    }
}
