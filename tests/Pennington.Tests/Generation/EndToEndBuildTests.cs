using System.Collections.Immutable;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Markdown;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

/// <summary>
/// End-to-end tests exercising the full content pipeline through to build report
/// and link verification — the same flow a real "dotnet run -- build" would take.
/// </summary>
public class EndToEndBuildTests
{
    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static ContentSource MakeSource() =>
        new ContentSource(new MarkdownFileSource("content/page.md"));

    private static OutputOptions MakeOptions() => new()
    {
        OutputDirectory = new FilePath("output")
    };

    // --- Front matter types ---

    private record TestFrontMatter(string Title) : IFrontMatter;

    private record DraftFrontMatter : IFrontMatter
    {
        public string Title { get; init; } = "Draft";
        public bool IsDraft { get; init; } = true;
    }

    private record BlogTestFrontMatter : IFrontMatter
    {
        public string Title { get; init; } = "";
        public bool IsDraft { get; init; }
        public DateTime? Date { get; init; }
        public string? Description { get; init; }
    }

    // --- Stub services ---

    private class StubContentService(params DiscoveredItem[] items) : IContentService
    {
        public string DefaultSection => "Test";
        public int SearchPriority => 5;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var item in items) yield return item;
            await Task.CompletedTask;
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private class MarkdownParserStub(Func<DiscoveredItem, ContentItem> parseFunc) : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(parseFunc(item));
    }

    private static RenderedContent MakeRenderedContent(string html) => new(
        Html: html,
        Outline: [],
        Tags: ImmutableList<Tag>.Empty,
        CrossReferences: ImmutableList<CrossReference>.Empty,
        SearchDocument: null,
        Social: null
    );

    private class RenderWithHtmlStub(Func<ParsedItem, string> htmlFunc) : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
        {
            var html = htmlFunc(item);
            return Task.FromResult(new ContentItem(
                new RenderedItem(item.Route, item.Metadata, MakeRenderedContent(html))));
        }
    }

    // --- Tests ---

    [Fact]
    public async Task FullBuild_AllPagesSucceed_NoErrors()
    {
        var items = new[]
        {
            new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource()),
            new DiscoveredItem(MakeRoute("/docs/config"), MakeSource()),
            new DiscoveredItem(MakeRoute("/about"), MakeSource()),
        };
        var service = new StubContentService(items);

        var parser = new MarkdownParserStub(item =>
            new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Page"), "# Content")));
        var renderer = new RenderWithHtmlStub(_ => "<p>Content</p>");

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(3);
        report.FailedPages.ShouldBeEmpty();
        report.SkippedPages.ShouldBeEmpty();
        report.HasErrors.ShouldBeFalse();
        report.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task FullBuild_DraftsSkipped_SuccessesGenerated_FailuresRecorded()
    {
        var goodItem = new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource());
        var draftItem = new DiscoveredItem(MakeRoute("/blog/wip"), MakeSource());
        var badItem = new DiscoveredItem(MakeRoute("/docs/broken"), MakeSource());
        var service = new StubContentService(goodItem, draftItem, badItem);

        var parser = new MarkdownParserStub(item =>
        {
            if (item.Route.CanonicalPath.Value.Contains("broken/"))
                return new ContentItem(new FailedItem(item.Route, new ContentError("YAML parse error at line 3")));
            if (item.Route.CanonicalPath.Value.Contains("wip/"))
                return new ContentItem(new ParsedItem(item.Route, new DraftFrontMatter(), "# WIP"));
            return new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Page"), "# Content"));
        });
        var renderer = new RenderWithHtmlStub(_ => "<p>Content</p>");

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(1);
        report.SkippedPages.Count.ShouldBe(1);
        report.FailedPages.Count.ShouldBe(1);
        report.TotalPages.ShouldBe(3);
        report.HasErrors.ShouldBeTrue();

        // Verify the error diagnostic has the right message
        var errors = report.Diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error).ToList();
        errors.Count.ShouldBe(1);
        errors[0].Message.ShouldContain("YAML parse error");
        errors[0].Route.CanonicalPath.Value.ShouldBe("/docs/broken/");
    }

    [Fact]
    public async Task FullBuild_ThenLinkVerification_BrokenLinksDetected()
    {
        var introItem = new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource());
        var configItem = new DiscoveredItem(MakeRoute("/docs/config"), MakeSource());
        var service = new StubContentService(introItem, configItem);

        var parser = new MarkdownParserStub(item =>
            new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Page"), "# Content")));

        // Renderer produces HTML with links — some valid, some broken
        var renderer = new RenderWithHtmlStub(item =>
        {
            if (item.Route.CanonicalPath.Value == "/docs/intro/")
                return """
                    <p>See <a href="/docs/config">Configuration</a> for setup.</p>
                    <p>Also check <a href="/docs/missing-page">this page</a>.</p>
                    <img src="/images/logo.png" alt="Logo">
                    """;
            return """<p>Back to <a href="/docs/intro">Intro</a>.</p>""";
        });

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        // All pages generated successfully
        report.GeneratedPages.Count.ShouldBe(2);
        report.HasErrors.ShouldBeFalse();

        // Now run link verification on the rendered output
        var knownRoutes = report.GeneratedPages;
        var linkService = new LinkVerificationService(knownRoutes);

        // Simulate verifying links on each rendered page
        var reportBuilder = new BuildReportBuilder();
        foreach (var page in report.GeneratedPages)
            reportBuilder.AddGeneratedPage(page);

        // We need the rendered HTML — collect it from a second pipeline run
        var discovered = pipeline.DiscoverAsync();
        var parsed = pipeline.ParseAsync(discovered);
        var rendered = pipeline.RenderAsync(parsed);
        await foreach (var item in rendered)
        {
            if (item is RenderedItem r)
            {
                var linkResults = linkService.VerifyLinks(r.Route, r.Content.Html);
                foreach (var linkResult in linkResults)
                {
                    if (linkResult is BrokenLinkResult broken)
                    {
                        reportBuilder.AddBrokenLink(new BrokenLink(
                            broken.SourcePage, broken.Url, broken.Type, broken.Reason));
                    }
                }
            }
        }

        var finalReport = reportBuilder.Build();

        // Should have broken links from /docs/intro
        finalReport.BrokenLinks.Count.ShouldBe(2); // /docs/missing-page + /images/logo.png
        finalReport.HasErrors.ShouldBeTrue(); // broken links → error

        var brokenUrls = finalReport.BrokenLinks.Select(b => b.Url).ToList();
        brokenUrls.ShouldContain("/docs/missing-page");
        brokenUrls.ShouldContain("/images/logo.png");

        // Verify source page tracking
        finalReport.BrokenLinks.All(b => b.SourcePage.CanonicalPath.Value == "/docs/intro/").ShouldBeTrue();
    }

    [Fact]
    public async Task FullBuild_MultipleContentServices_AllContributeToReport()
    {
        var docsService = new StubContentService(
            new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource()),
            new DiscoveredItem(MakeRoute("/docs/api"), MakeSource()));
        var blogService = new StubContentService(
            new DiscoveredItem(MakeRoute("/blog/hello"), MakeSource()),
            new DiscoveredItem(MakeRoute("/blog/draft"), MakeSource()));

        var parser = new MarkdownParserStub(item =>
        {
            if (item.Route.CanonicalPath.Value.Contains("draft"))
                return new ContentItem(new ParsedItem(item.Route, new DraftFrontMatter(), "# Draft"));
            return new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Page"), "# Content"));
        });
        var renderer = new RenderWithHtmlStub(_ => "<p>Content</p>");

        var pipeline = new ContentPipeline([docsService, blogService], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(3); // 2 docs + 1 blog
        report.SkippedPages.Count.ShouldBe(1); // 1 draft
        report.TotalPages.ShouldBe(4);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task FullBuild_WithCrossReferences_LinksResolveCorrectly()
    {
        var introItem = new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource());
        var apiItem = new DiscoveredItem(MakeRoute("/docs/api"), MakeSource());
        var service = new StubContentService(introItem, apiItem);

        var parser = new MarkdownParserStub(item =>
            new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Page"), "# Content")));

        // Intro links to API by path — should be valid
        var renderer = new RenderWithHtmlStub(item =>
        {
            if (item.Route.CanonicalPath.Value == "/docs/intro/")
                return """<p>See the <a href="/docs/api">API Reference</a> for details.</p>""";
            return """<p>Back to <a href="/docs/intro">Intro</a>.</p>""";
        });

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        // Verify links between the two pages
        var linkService = new LinkVerificationService(report.GeneratedPages);

        // Verify intro page links
        var introResults = linkService.VerifyLinks(MakeRoute("/docs/intro"),
            """<p>See the <a href="/docs/api">API Reference</a> for details.</p>""");
        introResults.Count.ShouldBe(1);
        (introResults[0] is ValidLink).ShouldBeTrue();

        // Verify api page links
        var apiResults = linkService.VerifyLinks(MakeRoute("/docs/api"),
            """<p>Back to <a href="/docs/intro">Intro</a>.</p>""");
        apiResults.Count.ShouldBe(1);
        (apiResults[0] is ValidLink).ShouldBeTrue();
    }

    [Fact]
    public async Task FullBuild_ReportOutput_FormatsCorrectly()
    {
        var goodItem = new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource());
        var badItem = new DiscoveredItem(MakeRoute("/docs/broken"), MakeSource());
        var service = new StubContentService(goodItem, badItem);

        var parser = new MarkdownParserStub(item =>
        {
            if (item.Route.CanonicalPath.Value.Contains("broken/"))
                return new ContentItem(new FailedItem(item.Route, new ContentError("Front matter parse failed: invalid YAML")));
            return new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Intro"), "# Intro"));
        });
        var renderer = new RenderWithHtmlStub(_ => "<p>Content</p>");

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        var output = report.ToFormattedString();

        output.ShouldContain("2 pages in");
        output.ShouldContain("1 pages generated");
        output.ShouldContain("1 pages failed");
        output.ShouldContain("ERRORS");
        output.ShouldContain("/docs/broken");
        output.ShouldContain("Front matter parse failed: invalid YAML");
    }

    [Fact]
    public async Task FullBuild_EmptySite_NoContentDiscovered()
    {
        var service = new StubContentService(); // no items
        var parser = new MarkdownParserStub(_ => throw new InvalidOperationException("Should not be called"));
        var renderer = new RenderWithHtmlStub(_ => throw new InvalidOperationException("Should not be called"));

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.ShouldBeEmpty();
        report.FailedPages.ShouldBeEmpty();
        report.SkippedPages.ShouldBeEmpty();
        report.TotalPages.ShouldBe(0);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task FullBuild_AllPagesFail_ReportShowsAllFailures()
    {
        var items = new[]
        {
            new DiscoveredItem(MakeRoute("/bad-1"), MakeSource()),
            new DiscoveredItem(MakeRoute("/bad-2"), MakeSource()),
            new DiscoveredItem(MakeRoute("/bad-3"), MakeSource()),
        };
        var service = new StubContentService(items);

        var parser = new MarkdownParserStub(item =>
            new ContentItem(new FailedItem(item.Route, new ContentError($"Error in {item.Route.CanonicalPath.Value}"))));
        var renderer = new RenderWithHtmlStub(_ => throw new InvalidOperationException("Should not be called"));

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.ShouldBeEmpty();
        report.FailedPages.Count.ShouldBe(3);
        report.HasErrors.ShouldBeTrue();

        var output = report.ToFormattedString();
        output.ShouldContain("3 pages failed");
        output.ShouldContain("0 pages generated");
        output.ShouldContain("/bad-1");
        output.ShouldContain("/bad-2");
        output.ShouldContain("/bad-3");
    }

    [Fact]
    public async Task LinkVerification_OnRenderedOutput_FullFlow()
    {
        // Simulate a doc site with navigation links — a realistic "build then verify" flow
        var pages = new[]
        {
            new DiscoveredItem(MakeRoute("/"), MakeSource()),
            new DiscoveredItem(MakeRoute("/docs/getting-started"), MakeSource()),
            new DiscoveredItem(MakeRoute("/docs/configuration"), MakeSource()),
            new DiscoveredItem(MakeRoute("/docs/api-reference"), MakeSource()),
        };
        var service = new StubContentService(pages);

        var parser = new MarkdownParserStub(item =>
            new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Page"), "# Content")));

        var navHtml = """
            <nav>
                <a href="/">Home</a>
                <a href="/docs/getting-started">Getting Started</a>
                <a href="/docs/configuration">Config</a>
                <a href="/docs/api-reference">API</a>
                <a href="/docs/deployment">Deploy</a>
            </nav>
            """;

        // Every page includes the same nav
        var renderer = new RenderWithHtmlStub(_ => navHtml);

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        var linkService = new LinkVerificationService(report.GeneratedPages);

        // Check one page — nav links should all be valid except /docs/deployment
        var results = linkService.VerifyLinks(MakeRoute("/"), navHtml);

        var validCount = results.Count(r => r is ValidLink);
        var brokenCount = results.Count(r => r is BrokenLinkResult);

        validCount.ShouldBe(4); // /, getting-started, configuration, api-reference
        brokenCount.ShouldBe(1); // /docs/deployment

        var broken = results.Where(r => r is BrokenLinkResult).ToList();
        var brokenLink = broken[0].ShouldBeCase<BrokenLinkResult>();
        brokenLink.Url.ShouldBe("/docs/deployment");
        brokenLink.Reason.ShouldBe("Page not found");
    }
}
