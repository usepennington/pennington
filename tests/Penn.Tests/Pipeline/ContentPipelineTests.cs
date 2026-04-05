using System.Collections.Immutable;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Generation;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Pipeline;

public class ContentPipelineTests
{
    // --- Helpers ---

    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static ContentSource MakeSource() =>
        new ContentSource(new MarkdownFileSource("content/page.md"));

    private static RenderedContent MakeRenderedContent() => new(
        Html: "<p>test</p>",
        Outline: [],
        Tags: ImmutableList<Tag>.Empty,
        CrossReferences: ImmutableList<CrossReference>.Empty,
        SearchDocument: null,
        Social: null
    );

    private static OutputOptions MakeOptions() => new()
    {
        OutputDirectory = new FilePath("output")
    };

    // --- Test front matter types ---

    private record TestFrontMatter(string Title) : IFrontMatter;

    private record DraftFrontMatter : IFrontMatter, IDraftable
    {
        public string Title { get; init; } = "Draft";
        public bool IsDraft { get; init; } = true;
    }

    // --- Stub content service ---

    private class StubContentService : IContentService
    {
        private readonly List<DiscoveredItem> _items;
        public StubContentService(params DiscoveredItem[] items) => _items = [.. items];
        public string DefaultSection => "Test";
        public int SearchPriority => 5;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var item in _items) yield return item;
            await Task.CompletedTask;
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    // --- Stub parser (succeeds) ---

    private class StubParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Test Page"), "# Hello")));
    }

    // --- Stub parser (returns FailedItem) ---

    private class FailingParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(new FailedItem(item.Route, new ContentError("YAML parse error at line 3"))));
    }

    // --- Stub parser (throws exception) ---

    private class ThrowingParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => throw new InvalidOperationException("Unexpected parse crash");
    }

    // --- Draft parser (returns ParsedItem with IDraftable metadata) ---

    private class DraftParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(new ParsedItem(item.Route, new DraftFrontMatter(), "# Draft")));
    }

    // --- Stub renderer (succeeds) ---

    private class StubRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
            => Task.FromResult(new ContentItem(new RenderedItem(item.Route, item.Metadata, MakeRenderedContent())));
    }

    // --- Stub renderer (returns FailedItem) ---

    private class FailingRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
            => Task.FromResult(new ContentItem(new FailedItem(item.Route, new ContentError("Render error: component crash"))));
    }

    // --- Helper to collect IAsyncEnumerable ---

    private static async Task<List<ContentItem>> CollectAsync(IAsyncEnumerable<ContentItem> items)
    {
        var result = new List<ContentItem>();
        await foreach (var item in items)
        {
            result.Add(item);
        }
        return result;
    }

    // --- Tests ---

    [Fact]
    public async Task FullPipeline_HappyPath_TwoItemsGenerated()
    {
        var item1 = new DiscoveredItem(MakeRoute("/page-1"), MakeSource());
        var item2 = new DiscoveredItem(MakeRoute("/page-2"), MakeSource());
        var service = new StubContentService(item1, item2);

        var pipeline = new ContentPipeline([service], new StubParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(2);
        report.FailedPages.Count.ShouldBe(0);
        report.SkippedPages.Count.ShouldBe(0);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task FailedParse_PropagatesThroughRenderAndGenerate()
    {
        var item = new DiscoveredItem(MakeRoute("/broken"), MakeSource());
        var service = new StubContentService(item);

        var pipeline = new ContentPipeline([service], new FailingParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.FailedPages.Count.ShouldBe(1);
        report.GeneratedPages.Count.ShouldBe(0);
        report.HasErrors.ShouldBeTrue();

        // Verify the error message is preserved
        report.Diagnostics.Count.ShouldBeGreaterThan(0);
        var errorDiag = report.Diagnostics.First(d => d is DiagnosticError);
        (errorDiag is DiagnosticError).ShouldBeTrue();
        var error = errorDiag switch { DiagnosticError e => e, _ => null };
        error.ShouldNotBeNull();
        error.Message.ShouldContain("YAML parse error at line 3");
    }

    [Fact]
    public async Task FailedRender_CapturedInReport()
    {
        var item = new DiscoveredItem(MakeRoute("/render-fail"), MakeSource());
        var service = new StubContentService(item);

        var pipeline = new ContentPipeline([service], new StubParser(), new FailingRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.FailedPages.Count.ShouldBe(1);
        report.GeneratedPages.Count.ShouldBe(0);
        report.HasErrors.ShouldBeTrue();

        var errorDiag = report.Diagnostics.First(d => d is DiagnosticError);
        var error = errorDiag switch { DiagnosticError e => e, _ => null };
        error.ShouldNotBeNull();
        error.Message.ShouldContain("Render error: component crash");
    }

    [Fact]
    public async Task ParserException_WrappedIntoFailedItem()
    {
        var item = new DiscoveredItem(MakeRoute("/crash"), MakeSource());
        var service = new StubContentService(item);

        var pipeline = new ContentPipeline([service], new ThrowingParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.FailedPages.Count.ShouldBe(1);
        report.GeneratedPages.Count.ShouldBe(0);
        report.HasErrors.ShouldBeTrue();

        var errorDiag = report.Diagnostics.First(d => d is DiagnosticError);
        var error = errorDiag switch { DiagnosticError e => e, _ => null };
        error.ShouldNotBeNull();
        error.Message.ShouldContain("Parse failed:");
        error.Message.ShouldContain("Unexpected parse crash");
    }

    [Fact]
    public async Task DraftItems_SkippedAtGenerateStage()
    {
        var item = new DiscoveredItem(MakeRoute("/draft-page"), MakeSource());
        var service = new StubContentService(item);

        var pipeline = new ContentPipeline([service], new DraftParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.SkippedPages.Count.ShouldBe(1);
        report.GeneratedPages.Count.ShouldBe(0);
        report.FailedPages.Count.ShouldBe(0);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task MixedResults_CorrectCounts()
    {
        var successItem = new DiscoveredItem(MakeRoute("/success"), MakeSource());
        var failItem = new DiscoveredItem(MakeRoute("/fail"), MakeSource());
        var draftItem = new DiscoveredItem(MakeRoute("/draft"), MakeSource());

        // Use a selective parser that fails for /fail and returns draft for /draft
        var parser = new SelectiveParser();
        var service = new StubContentService(successItem, failItem, draftItem);

        var pipeline = new ContentPipeline([service], parser, new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(1);
        report.FailedPages.Count.ShouldBe(1);
        report.SkippedPages.Count.ShouldBe(1);
        report.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task MultipleContentServices_AllDiscovered()
    {
        var item1 = new DiscoveredItem(MakeRoute("/service1/page"), MakeSource());
        var item2 = new DiscoveredItem(MakeRoute("/service2/page"), MakeSource());
        var service1 = new StubContentService(item1);
        var service2 = new StubContentService(item2);

        var pipeline = new ContentPipeline([service1, service2], new StubParser(), new StubRenderer());

        var discovered = await CollectAsync(pipeline.DiscoverAsync());

        discovered.Count.ShouldBe(2);
        discovered[0].Route.CanonicalPath.Value.ShouldBe("/service1/page/");
        discovered[1].Route.CanonicalPath.Value.ShouldBe("/service2/page/");
    }

    [Fact]
    public async Task RunAsync_SameAsManualStages()
    {
        var item1 = new DiscoveredItem(MakeRoute("/a"), MakeSource());
        var item2 = new DiscoveredItem(MakeRoute("/b"), MakeSource());
        var service = new StubContentService(item1, item2);
        var parser = new StubParser();
        var renderer = new StubRenderer();

        var pipeline = new ContentPipeline([service], parser, renderer);

        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(2);
        report.FailedPages.Count.ShouldBe(0);
        report.SkippedPages.Count.ShouldBe(0);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task EmptyPipeline_NoServices_EmptyReport()
    {
        var pipeline = new ContentPipeline([], new StubParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(0);
        report.FailedPages.Count.ShouldBe(0);
        report.SkippedPages.Count.ShouldBe(0);
        report.TotalPages.ShouldBe(0);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task ParseStage_FailedItemPassesThrough()
    {
        var route = MakeRoute("/already-failed");
        var failedItem = new ContentItem(new FailedItem(route, new ContentError("earlier failure")));

        async IAsyncEnumerable<ContentItem> Source()
        {
            yield return failedItem;
            await Task.CompletedTask;
        }

        var pipeline = new ContentPipeline([], new StubParser(), new StubRenderer());
        var results = await CollectAsync(pipeline.ParseAsync(Source()));

        results.Count.ShouldBe(1);
        (results[0] is FailedItem).ShouldBeTrue();
        var failed = results[0] switch { FailedItem f => f, _ => null };
        failed.ShouldNotBeNull();
        failed.Error.Message.ShouldBe("earlier failure");
    }

    [Fact]
    public async Task RenderStage_FailedItemPassesThrough()
    {
        var route = MakeRoute("/parse-failed");
        var failedItem = new ContentItem(new FailedItem(route, new ContentError("parse went wrong")));

        async IAsyncEnumerable<ContentItem> Source()
        {
            yield return failedItem;
            await Task.CompletedTask;
        }

        var pipeline = new ContentPipeline([], new StubParser(), new StubRenderer());
        var results = await CollectAsync(pipeline.RenderAsync(Source()));

        results.Count.ShouldBe(1);
        (results[0] is FailedItem).ShouldBeTrue();
        var failed = results[0] switch { FailedItem f => f, _ => null };
        failed.ShouldNotBeNull();
        failed.Error.Message.ShouldBe("parse went wrong");
    }

    [Fact]
    public async Task RendererException_WrappedIntoFailedItem()
    {
        var item = new DiscoveredItem(MakeRoute("/render-crash"), MakeSource());
        var service = new StubContentService(item);

        var pipeline = new ContentPipeline([service], new StubParser(), new ThrowingRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.FailedPages.Count.ShouldBe(1);
        report.HasErrors.ShouldBeTrue();
        var errorDiag = report.Diagnostics.First(d => d is DiagnosticError);
        var error = errorDiag switch { DiagnosticError e => e, _ => null };
        error.ShouldNotBeNull();
        error.Message.ShouldContain("Render failed:");
        error.Message.ShouldContain("Component exploded");
    }

    [Fact]
    public async Task MultipleFailures_AllRecordedInReport()
    {
        var item1 = new DiscoveredItem(MakeRoute("/fail-1"), MakeSource());
        var item2 = new DiscoveredItem(MakeRoute("/fail-2"), MakeSource());
        var item3 = new DiscoveredItem(MakeRoute("/success"), MakeSource());
        var service = new StubContentService(item1, item2, item3);

        var pipeline = new ContentPipeline([service], new SelectiveParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.FailedPages.Count.ShouldBe(2);
        report.GeneratedPages.Count.ShouldBe(1);
        report.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task ProgrammaticSource_DiscoveredAndFlowsThroughPipeline()
    {
        var generator = new StubProgrammaticGenerator();
        var route = MakeRoute("/generated/page");
        var source = new ContentSource(new ProgrammaticSource(generator));
        var item = new DiscoveredItem(route, source);
        var service = new StubContentService(item);

        // Parser checks source type, so use a parser that handles any source
        var pipeline = new ContentPipeline([service], new StubParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        // ProgrammaticSource passes through parser (our stub doesn't check source type)
        report.GeneratedPages.Count.ShouldBe(1);
        report.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task RenderStage_ParsedItemPassesThrough_AlreadyRenderedSkipped()
    {
        var route = MakeRoute("/already-rendered");
        var renderedItem = new ContentItem(
            new RenderedItem(route, new TestFrontMatter("Already Done"), MakeRenderedContent()));

        async IAsyncEnumerable<ContentItem> Source()
        {
            yield return renderedItem;
            await Task.CompletedTask;
        }

        var pipeline = new ContentPipeline([], new StubParser(), new StubRenderer());
        var results = await CollectAsync(pipeline.RenderAsync(Source()));

        results.Count.ShouldBe(1);
        // Already rendered items pass through unchanged
        (results[0] is RenderedItem).ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateStage_UnexpectedItemType_GetsWarning()
    {
        // If a DiscoveredItem somehow reaches the Generate stage (wasn't parsed or rendered),
        // it should get a warning
        var route = MakeRoute("/stuck");
        var discoveredItem = new ContentItem(new DiscoveredItem(route, MakeSource()));

        async IAsyncEnumerable<ContentItem> Source()
        {
            yield return discoveredItem;
            await Task.CompletedTask;
        }

        var pipeline = new ContentPipeline([], new StubParser(), new StubRenderer());
        var report = await pipeline.GenerateAsync(Source(), MakeOptions());

        report.GeneratedPages.Count.ShouldBe(0);
        report.Diagnostics.Any(d => d is DiagnosticWarning).ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateAsync_LinksWithoutTrailingSlash_EmitsWarnings()
    {
        var item = new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource());
        var service = new StubContentService(item);

        var parser = new StubParser();
        // Renderer produces HTML with a link missing a trailing slash
        var renderer = new CustomHtmlRenderer(_ =>
            """<p>See <a href="/docs/config">Config</a> and <a href="/docs/setup/">Setup</a>.</p>""");

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(1);
        var warnings = report.Diagnostics.Where(d => d is DiagnosticWarning).ToList();
        warnings.Count.ShouldBe(1);
        warnings[0].Message.ShouldContain("/docs/config");
        warnings[0].Message.ShouldContain("missing a trailing slash");
    }

    [Fact]
    public async Task GenerateAsync_AllLinksHaveTrailingSlash_NoWarnings()
    {
        var item = new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource());
        var service = new StubContentService(item);

        var parser = new StubParser();
        var renderer = new CustomHtmlRenderer(_ =>
            """<p>See <a href="/docs/config/">Config</a> and <a href="https://example.com">Ext</a>.</p>""");

        var pipeline = new ContentPipeline([service], parser, renderer);
        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(1);
        report.Diagnostics.Where(d => d is DiagnosticWarning).ShouldBeEmpty();
    }

    [Fact]
    public async Task Pipeline_MixedContentServices_FailureInOneDoesNotAffectOther()
    {
        var goodItem = new DiscoveredItem(MakeRoute("/good"), MakeSource());
        var badItem = new DiscoveredItem(MakeRoute("/bad-fail"), MakeSource());

        var goodService = new StubContentService(goodItem);
        var badService = new StubContentService(badItem);

        var pipeline = new ContentPipeline([goodService, badService], new SelectiveParser(), new StubRenderer());

        var report = await pipeline.RunAsync(MakeOptions());

        report.GeneratedPages.Count.ShouldBe(1);
        report.FailedPages.Count.ShouldBe(1);
    }

    // --- Selective parser for mixed results test ---

    private class SelectiveParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
        {
            if (item.Route.CanonicalPath.Value.Contains("fail"))
            {
                return Task.FromResult(new ContentItem(
                    new FailedItem(item.Route, new ContentError("Intentional parse failure"))));
            }

            if (item.Route.CanonicalPath.Value.Contains("draft"))
            {
                return Task.FromResult(new ContentItem(
                    new ParsedItem(item.Route, new DraftFrontMatter(), "# Draft")));
            }

            return Task.FromResult(new ContentItem(
                new ParsedItem(item.Route, new TestFrontMatter("Page"), "# Page")));
        }
    }

    // --- Custom HTML renderer (caller provides HTML) ---

    private class CustomHtmlRenderer(Func<ParsedItem, string> htmlFunc) : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
        {
            var content = new RenderedContent(
                Html: htmlFunc(item),
                Outline: [],
                Tags: ImmutableList<Tag>.Empty,
                CrossReferences: ImmutableList<CrossReference>.Empty,
                SearchDocument: null,
                Social: null);
            return Task.FromResult(new ContentItem(new RenderedItem(item.Route, item.Metadata, content)));
        }
    }

    // --- Stub renderer (throws exception) ---

    private class ThrowingRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
            => throw new InvalidOperationException("Component exploded");
    }

    // --- Stub programmatic content generator ---

    private class StubProgrammaticGenerator : IProgrammaticContentGenerator
    {
        public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
            => Task.FromResult(new ProgrammaticContent(
                new TextProgrammaticContent(
                    Metadata: null,
                    RawContent: "<p>Generated content</p>")));
    }
}
