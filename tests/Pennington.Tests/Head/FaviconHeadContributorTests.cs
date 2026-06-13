using Microsoft.AspNetCore.Http;
using Pennington.Content;
using Pennington.Favicon;
using Pennington.Head;
using Pennington.Infrastructure;

namespace Pennington.Tests.Head;

/// <summary>
/// DOM-level tests for <see cref="FaviconHeadContributor"/> driven through the real
/// <see cref="HeadCompositionHtmlRewriter"/> + <see cref="HtmlResponseRewritingProcessor"/>.
/// </summary>
public class FaviconHeadContributorTests
{
    private static async Task<string> Render(
        FaviconOptions favicons,
        string body = "<html><head></head><body></body></html>")
    {
        var registry = new ContentRecordRegistry(Array.Empty<ContentRecord>());
        var processor = new HtmlResponseRewritingProcessor(
            [new HeadCompositionHtmlRewriter([new FaviconHeadContributor(favicons)], registry)]);
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/guide/intro/";
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return await processor.ProcessAsync(body, ctx);
    }

    [Fact]
    public async Task EmitsLink_PerConfiguredIcon()
    {
        var html = await Render(new FaviconOptions
        {
            Icons = [new("/favicon.ico"), new("/icon-32.png") { Sizes = "32x32" }],
        });

        // Two icons sharing rel="icon" both survive — proves AddRepeatable, not keyed dedup.
        html.ShouldContain("href=\"/favicon.ico\"");
        html.ShouldContain("href=\"/icon-32.png\"");
        html.ShouldContain("sizes=\"32x32\"");
    }

    [Fact]
    public async Task InfersType_FromExtension()
    {
        var html = await Render(new FaviconOptions
        {
            Icons = [new("/favicon.ico"), new("/icon.png"), new("/icon.svg")],
        });

        html.ShouldContain("type=\"image/x-icon\"");
        html.ShouldContain("type=\"image/png\"");
        html.ShouldContain("type=\"image/svg+xml\"");
    }

    [Fact]
    public async Task ExplicitType_OverridesInference()
    {
        var html = await Render(new FaviconOptions
        {
            Icons = [new("/icon.png") { Type = "image/custom" }],
        });

        html.ShouldContain("type=\"image/custom\"");
        html.ShouldNotContain("image/png");
    }

    [Fact]
    public async Task OmitsType_ForUnknownExtension()
    {
        var html = await Render(new FaviconOptions
        {
            Icons = [new("/site.webmanifest") { Rel = "manifest" }],
        });

        html.ShouldContain("rel=\"manifest\"");
        html.ShouldContain("href=\"/site.webmanifest\"");
        html.ShouldNotContain("type=");
    }

    [Fact]
    public async Task EmitsRel_Sizes_AndColorForMaskIcon()
    {
        var html = await Render(new FaviconOptions
        {
            Icons =
            [
                new("/apple-touch-icon.png") { Rel = "apple-touch-icon", Sizes = "180x180" },
                new("/mask.svg") { Rel = "mask-icon", Color = "#5bbad5" },
            ],
        });

        html.ShouldContain("rel=\"apple-touch-icon\"");
        html.ShouldContain("sizes=\"180x180\"");
        html.ShouldContain("rel=\"mask-icon\"");
        html.ShouldContain("color=\"#5bbad5\"");
    }

    [Fact]
    public async Task StampsDataHead_ForSpaPersistence()
    {
        var html = await Render(new FaviconOptions { Icons = [new("/favicon.ico")] });

        html.ShouldContain("data-head=\"link:rel:icon\"");
    }

    [Fact]
    public async Task DoesNotEmit_WhenNoIconsConfigured()
    {
        var html = await Render(new FaviconOptions());

        html.ShouldNotContain("rel=\"icon\"");
    }
}
