using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;
using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="HtmlResponseRewritingProcessor"/> —
/// the single HTML rewriting processor. The tests use stub rewriters to
/// verify orchestration: Order, pre-parse-before-DOM sequencing, and
/// short-circuit when no rewriter applies.
/// </summary>
public class HtmlResponseRewritingProcessorTests
{
    private sealed class StubRewriter(int order, bool shouldApply, string tag) : IHtmlResponseRewriter
    {
        public int Order => order;
        public bool ShouldApply(HttpContext context) => shouldApply;

        public readonly List<string> CallLog = [];

        public Task<string> PreParseAsync(string html, HttpContext context)
        {
            CallLog.Add($"{tag}:pre");
            return Task.FromResult(html);
        }

        public Task ApplyAsync(IDocument document, HttpContext context)
        {
            CallLog.Add($"{tag}:dom");
            document.Body?.SetAttribute($"data-{tag}", "1");
            return Task.CompletedTask;
        }
    }

    private static DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return ctx;
    }

    [Fact]
    public void ShouldProcess_SkipsNonHtml()
    {
        var processor = new HtmlResponseRewritingProcessor([new StubRewriter(10, true, "a")]);
        var ctx = CreateContext();
        ctx.Response.ContentType = "application/json";

        processor.ShouldProcess(ctx).ShouldBeFalse();
    }

    [Fact]
    public void ShouldProcess_SkipsWhenNoRewriterApplies()
    {
        var processor = new HtmlResponseRewritingProcessor(
        [
            new StubRewriter(10, false, "a"),
            new StubRewriter(20, false, "b"),
        ]);

        processor.ShouldProcess(CreateContext()).ShouldBeFalse();
    }

    [Fact]
    public void ShouldProcess_AcceptsWhenAnyRewriterApplies()
    {
        var processor = new HtmlResponseRewritingProcessor(
        [
            new StubRewriter(10, false, "a"),
            new StubRewriter(20, true, "b"),
        ]);

        processor.ShouldProcess(CreateContext()).ShouldBeTrue();
    }

    [Fact]
    public async Task ProcessAsync_RunsAllPreParseBeforeAnyDomPass()
    {
        var a = new StubRewriter(10, true, "a");
        var b = new StubRewriter(20, true, "b");
        var processor = new HtmlResponseRewritingProcessor([a, b]);

        await processor.ProcessAsync(
            "<html><body><p>hi</p></body></html>",
            CreateContext());

        // Pre-parse phase runs for all rewriters, then DOM phase runs for all.
        // Within each phase rewriters run in ascending Order.
        a.CallLog.ShouldBe(["a:pre", "a:dom"]);
        b.CallLog.ShouldBe(["b:pre", "b:dom"]);

        // Interleaved across both rewriters, ordering must be pre→pre→dom→dom.
        var combined = a.CallLog.Zip(b.CallLog).SelectMany(p => new[] { p.First, p.Second }).ToArray();
        combined.ShouldBe(["a:pre", "b:pre", "a:dom", "b:dom"]);
    }

    [Fact]
    public async Task ProcessAsync_RewritersRunInAscendingOrder()
    {
        // Register out of order intentionally.
        var b = new StubRewriter(20, true, "b");
        var a = new StubRewriter(10, true, "a");
        var processor = new HtmlResponseRewritingProcessor([b, a]);

        var result = await processor.ProcessAsync(
            "<html><body></body></html>",
            CreateContext());

        result.ShouldContain("data-a=\"1\"");
        result.ShouldContain("data-b=\"1\"");
    }

    [Fact]
    public async Task ProcessAsync_SkipsNonApplicableRewriters()
    {
        var applied = new StubRewriter(10, true, "a");
        var skipped = new StubRewriter(20, false, "b");
        var processor = new HtmlResponseRewritingProcessor([applied, skipped]);

        await processor.ProcessAsync(
            "<html><body></body></html>",
            CreateContext());

        applied.CallLog.ShouldContain("a:dom");
        skipped.CallLog.ShouldBeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_ReturnsBodyUnchangedWhenNoRewriterApplies()
    {
        var processor = new HtmlResponseRewritingProcessor(
        [
            new StubRewriter(10, false, "a"),
        ]);

        const string body = "<html><body>original</body></html>";
        var result = await processor.ProcessAsync(body, CreateContext());

        // No parse happened — the exact input is returned.
        result.ShouldBe(body);
    }
}
