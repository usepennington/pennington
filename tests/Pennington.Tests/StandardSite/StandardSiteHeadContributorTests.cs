using Microsoft.AspNetCore.Http;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Head;
using Pennington.Infrastructure;
using Pennington.Routing;
using Pennington.StandardSite;

namespace Pennington.Tests.StandardSite;

/// <summary>
/// DOM-level tests for <see cref="StandardSiteHeadContributor"/>, driven through the real
/// <see cref="HeadCompositionHtmlRewriter"/> + <see cref="HtmlResponseRewritingProcessor"/> so the
/// verification links land in <c>&lt;head&gt;</c> exactly as in the response pipeline.
/// </summary>
public class StandardSiteHeadContributorTests
{
    private sealed record DocFrontMatter : IFrontMatter, IStandardSiteDocument
    {
        public string Title { get; init; } = "";
        public string? AtprotoRkey { get; init; }
    }

    private static ContentRecord Record(string url, IFrontMatter fm) =>
        new(ContentRouteFactory.FromUrl(new UrlPath(url)), fm);

    private static async Task<string> Render(string requestPath, ContentRecordRegistry registry, StandardSiteOptions options)
    {
        var resolver = new StandardSiteUriResolver(options, registry);
        var contributor = new StandardSiteHeadContributor(options, resolver);
        var processor = new HtmlResponseRewritingProcessor([new HeadCompositionHtmlRewriter([contributor], registry)]);
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = requestPath;
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return await processor.ProcessAsync("<html><head></head><body></body></html>", ctx);
    }

    private static StandardSiteOptions Options() =>
        new() { Did = "did:plc:abc", PublicationRkey = "pub1" };

    [Fact]
    public async Task EmitsPublicationLink_OnEveryPage()
    {
        var registry = new ContentRecordRegistry([Record("/about/", new DocFrontMatter { Title = "About" })]);

        var html = await Render("/about/", registry, Options());

        html.ShouldContain("""rel="site.standard.publication""");
        html.ShouldContain("at://did:plc:abc/site.standard.publication/pub1");
    }

    [Fact]
    public async Task EmitsDocumentLink_WhenPageDeclaresRkey()
    {
        var registry = new ContentRecordRegistry([Record("/blog/post/", new DocFrontMatter { Title = "Post", AtprotoRkey = "doc9" })]);

        var html = await Render("/blog/post/", registry, Options());

        html.ShouldContain("""rel="site.standard.document""");
        html.ShouldContain("at://did:plc:abc/site.standard.document/doc9");
    }

    [Fact]
    public async Task OmitsDocumentLink_WhenPageHasNoRkey()
    {
        var registry = new ContentRecordRegistry([Record("/blog/post/", new DocFrontMatter { Title = "Post" })]);

        var html = await Render("/blog/post/", registry, Options());

        html.ShouldNotContain("site.standard.document");
    }

    [Fact]
    public async Task OmitsPublicationLink_WhenDisabled()
    {
        var registry = new ContentRecordRegistry([Record("/about/", new DocFrontMatter { Title = "About" })]);
        var options = Options() with { EmitPublicationLink = false };

        var html = await Render("/about/", registry, options);

        html.ShouldNotContain("site.standard.publication");
    }

    [Fact]
    public async Task StampsDataHead_ForSpaPersistence()
    {
        var registry = new ContentRecordRegistry([Record("/about/", new DocFrontMatter { Title = "About" })]);

        var html = await Render("/about/", registry, Options());

        html.ShouldContain("data-head");
    }
}
