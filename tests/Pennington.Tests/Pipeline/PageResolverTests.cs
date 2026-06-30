using System.Collections.Immutable;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Pipeline;

public class PageResolverTests
{
    // --- Helpers ---

    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static ContentSource MakeSource() =>
        new ContentSource(new FileSource("content/page.md", "markdown"));

    private static RenderedContent MakeRenderedContent() => new(
        Html: "<p>test</p>",
        Outline: [],
        Tags: ImmutableList<Tag>.Empty,
        CrossReferences: ImmutableList<CrossReference>.Empty,
        Social: null
    );

    private record TestFrontMatter(string Title) : IFrontMatter;

    private record OtherFrontMatter(string Title) : IFrontMatter;

    // --- Stubs ---

    private class StubContentService(params DiscoveredItem[] items) : IContentService
    {
        private readonly List<DiscoveredItem> _items = [.. items];
        public string DefaultSectionLabel => "Test";
        public int SearchPriority => 5;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var item in _items)
            {
                yield return item;
            }

            await Task.CompletedTask;
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private class StubParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(new ParsedItem(item.Route, new TestFrontMatter("Test Page"), "# Hello")));
    }

    private class FailingParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(new FailedItem(item.Route, new ContentError("not parseable"))));
    }

    private class StubRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
            => Task.FromResult(new ContentItem(new RenderedItem(item.Route, item.Metadata, MakeRenderedContent())));
    }

    // --- Tests ---

    [Fact]
    public async Task ResolveAsync_MatchingRoute_ReturnsRenderedItem()
    {
        var item = new DiscoveredItem(MakeRoute("/page-1"), MakeSource());
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync(new UrlPath("/page-1"));

        rendered.ShouldNotBeNull();
        rendered.Route.CanonicalPath.Value.ShouldBe("/page-1/");
        rendered.Metadata.Title.ShouldBe("Test Page");
        rendered.Content.Html.ShouldBe("<p>test</p>");
    }

    [Fact]
    public async Task ResolveAsync_NoMatch_ReturnsNull()
    {
        var item = new DiscoveredItem(MakeRoute("/page-1"), MakeSource());
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync(new UrlPath("/does-not-exist"));

        rendered.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsync_MatchFailsToParse_ReturnsNull()
    {
        var item = new DiscoveredItem(MakeRoute("/redirect"), MakeSource());
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer(), new FailingParser());

        var rendered = await resolver.ResolveAsync(new UrlPath("/redirect"));

        rendered.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsync_FirstMatchAcrossServicesWins()
    {
        var docService = new StubContentService(new DiscoveredItem(MakeRoute("/docs/intro"), MakeSource()));
        var blogService = new StubContentService(new DiscoveredItem(MakeRoute("/blog/post"), MakeSource()));
        var resolver = new PageResolver([docService, blogService], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync(new UrlPath("/blog/post"));

        rendered.ShouldNotBeNull();
        rendered.Route.CanonicalPath.Value.ShouldBe("/blog/post/");
    }

    [Fact]
    public async Task ResolveAsync_LlmsOnlySource_ReturnsNull()
    {
        // *.llms.md routes feed llms.txt only — they have no HTML page, so an HTTP request for
        // one must 404 rather than render agent-only content for a human.
        var item = new DiscoveredItem(
            MakeRoute("/agent-context"),
            new ContentSource(new LlmsOnlySource("content/agent-context.llms.md", "markdown")));
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync(new UrlPath("/agent-context"));

        rendered.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsync_LlmsOnlyDoesNotShadowRealPageAtSameSlug()
    {
        // The resolver declines an llms-only match (continue) rather than returning, so a real
        // HTML page from another service at the same slug still wins.
        var llmsService = new StubContentService(new DiscoveredItem(
            MakeRoute("/overlap"),
            new ContentSource(new LlmsOnlySource("content/overlap.llms.md", "markdown"))));
        var pageService = new StubContentService(new DiscoveredItem(MakeRoute("/overlap"), MakeSource()));
        var resolver = new PageResolver([llmsService, pageService], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync(new UrlPath("/overlap"));

        rendered.ShouldNotBeNull();
        rendered.Content.Html.ShouldBe("<p>test</p>");
    }

    [Fact]
    public async Task ResolveAsync_NoParser_ReturnsNullInsteadOfThrowing()
    {
        // Bare host: no markdown source means no IContentParser. The resolver must construct with
        // the optional parser omitted and resolve nothing rather than fail (the bare-host bug).
        var item = new DiscoveredItem(MakeRoute("/page-1"), MakeSource());
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer());

        var rendered = await resolver.ResolveAsync(new UrlPath("/page-1"));

        rendered.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsyncGeneric_MatchingType_ReturnsTypedItem()
    {
        var item = new DiscoveredItem(MakeRoute("/page-1"), MakeSource());
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync<TestFrontMatter>(new UrlPath("/page-1"));

        rendered.ShouldNotBeNull();
        rendered.Metadata.Title.ShouldBe("Test Page");
        rendered.Content.Html.ShouldBe("<p>test</p>");
    }

    [Fact]
    public async Task ResolveAsyncGeneric_WrongType_ReturnsNull()
    {
        // The page parses as TestFrontMatter; asking for an unrelated type fails the narrow.
        var item = new DiscoveredItem(MakeRoute("/page-1"), MakeSource());
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync<OtherFrontMatter>(new UrlPath("/page-1"));

        rendered.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsyncGeneric_NoMatch_ReturnsNull()
    {
        var item = new DiscoveredItem(MakeRoute("/page-1"), MakeSource());
        var resolver = new PageResolver([new StubContentService(item)], new StubRenderer(), new StubParser());

        var rendered = await resolver.ResolveAsync<TestFrontMatter>(new UrlPath("/does-not-exist"));

        rendered.ShouldBeNull();
    }
}
