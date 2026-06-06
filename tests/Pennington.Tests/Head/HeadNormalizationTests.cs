using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Pennington.Content;
using Pennington.Head;
using Pennington.Infrastructure;

namespace Pennington.Tests.Head;

/// <summary>
/// Tests the head composition rewriter's <c>NormalizeExisting</c> pass: page-authored managed head
/// tags get a <c>data-head</c> stamp (for the generic SPA sweep) while assets and the theme
/// bootstrap script are left untouched (so the SPA sweep never removes/re-adds them).
/// </summary>
public class HeadNormalizationTests
{
    private static async Task<string> Render(string body)
    {
        // CanonicalBaseUrl unset, so the canonical contributor no-ops; the rewriter still runs (a
        // contributor is registered) and normalizes the page-authored head.
        var options = new PenningtonOptions();
        var registry = new ContentRecordRegistry(Array.Empty<ContentRecord>());
        var processor = new HtmlResponseRewritingProcessor(
            [new HeadCompositionHtmlRewriter([new CanonicalHeadContributor(options)], registry)]);
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/x/";
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return await processor.ProcessAsync(body, ctx);
    }

    [Fact]
    public async Task StampsDataHead_OnPageAuthoredDescription()
    {
        var html = await Render("""<html><head><meta name="description" content="Hello"></head><body></body></html>""");

        html.ShouldContain("data-head");
        html.ShouldContain("content=\"Hello\"");
    }

    [Fact]
    public async Task StampsDataHead_OnPageAuthoredOgAndHreflang()
    {
        var body = """<html><head><meta property="og:title" content="T"><link rel="alternate" hreflang="fr" href="/fr/x/"></head><body></body></html>""";

        var html = await Render(body);

        Regex.Matches(html, "data-head").Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task LeavesAssetsAndThemeScriptUnstamped()
    {
        var body = """<html><head><link rel="stylesheet" href="/styles.css"><script>console.log(1)</script></head><body></body></html>""";

        var html = await Render(body);

        html.ShouldNotContain("data-head");
    }
}
