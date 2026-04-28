using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Feeds;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Feeds;

public class SitemapServiceTests
{
    private static ContentRoute MakeRoute(string path, string locale = "") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        Locale = locale,
    };

    private record TestFrontMatter : IFrontMatter, IRedirectable
    {
        public string Title { get; init; } = "Test";
        public DateTime? Date { get; init; }
        public bool IsDraft { get; init; }
        public string? RedirectUrl { get; init; }
    }

    private class StubContentService(params DiscoveredItem[] items) : IContentService
    {
        public string DefaultSectionLabel => "Test";
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

    private class StubParser(IFrontMatter metadata) : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(
                new ParsedItem(item.Route, metadata, "# Test")));
    }

    /// <summary>
    /// Parser that returns a FailedItem for any source it doesn't understand.
    /// Real-world behavior for non-markdown sources going through the markdown parser.
    /// </summary>
    private class FailingParser : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(
                new FailedItem(item.Route, new ContentError("unsupported"))));
    }

    private class StubProgrammaticGenerator : IProgrammaticContentGenerator
    {
        public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
            => Task.FromResult(new ProgrammaticContent(
                new TextProgrammaticContent(null, "<p>gen</p>", "text/html")));
    }

    private static SitemapService CreateService(
        IContentService contentService,
        IContentParser parser,
        string canonicalBase = "https://example.com")
    {
        return new SitemapService(
            contentServices: [contentService],
            localization: new LocalizationOptions(),
            parsers: [parser],
            builder: new SitemapBuilder(new UrlPath(canonicalBase)));
    }

    [Fact]
    public async Task GetSitemapXml_IncludesLastModified_WhenContentHasDate()
    {
        var date = new DateTime(2026, 3, 15);
        var route = MakeRoute("/blog/my-post");
        var source = new ContentSource(new MarkdownFileSource("content/post.md"));
        var discovered = new DiscoveredItem(route, source);
        var metadata = new TestFrontMatter { Title = "My Post", Date = date };

        var service = CreateService(
            new StubContentService(discovered),
            new StubParser(metadata));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("<lastmod>2026-03-15</lastmod>");
    }

    [Fact]
    public async Task GetSitemapXml_OmitsLastModified_WhenContentHasNoDate()
    {
        var route = MakeRoute("/about");
        var source = new ContentSource(new MarkdownFileSource("content/about.md"));
        var discovered = new DiscoveredItem(route, source);
        var metadata = new TestFrontMatter { Title = "About" };

        var service = CreateService(
            new StubContentService(discovered),
            new StubParser(metadata));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("https://example.com/about/");
        xml.ShouldNotContain("lastmod");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesDrafts()
    {
        var route = MakeRoute("/draft-post");
        var source = new ContentSource(new MarkdownFileSource("content/draft.md"));
        var discovered = new DiscoveredItem(route, source);
        var metadata = new TestFrontMatter { Title = "Draft", IsDraft = true };

        var service = CreateService(
            new StubContentService(discovered),
            new StubParser(metadata));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("draft-post");
    }

    [Fact]
    public async Task GetSitemapXml_IncludesProgrammaticSource_EvenWhenParserRejectsIt()
    {
        // Programmatic sources never reach the markdown parser — the service
        // short-circuits and emits the route directly. This guards against
        // the regression where search/llms enumerate TOC but sitemap used the
        // parse pipeline, leaving programmatic content out of sitemap.xml.
        var route = MakeRoute("/generated/page");
        var source = new ContentSource(new ProgrammaticSource(new StubProgrammaticGenerator()));
        var discovered = new DiscoveredItem(route, source);

        var service = CreateService(
            new StubContentService(discovered),
            new FailingParser());

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("https://example.com/generated/page/");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesNonHtmlOutputs()
    {
        // SpaNavigationContentService discovers routes for /_spa-data/*.json
        // pages — they're transport for the SPA shell, not canonical URLs.
        // The sitemap must skip anything whose output file is not HTML.
        var jsonRoute = new ContentRoute
        {
            CanonicalPath = new UrlPath("/_spa-data/docs/intro.json"),
            OutputFile = new FilePath("_spa-data/docs/intro.json"),
        };
        var htmlRoute = MakeRoute("/docs/intro");
        var discovered = new[]
        {
            new DiscoveredItem(jsonRoute, new ContentSource(new ProgrammaticSource(new StubProgrammaticGenerator()))),
            new DiscoveredItem(htmlRoute, new ContentSource(new ProgrammaticSource(new StubProgrammaticGenerator()))),
        };

        var service = CreateService(
            new StubContentService(discovered),
            new FailingParser());

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("_spa-data");
        xml.ShouldContain("https://example.com/docs/intro/");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesRedirectSources()
    {
        // Routes backed by RedirectSource — whether explicit redirects or
        // framework-internal placeholders like SpaNavigationContentService's
        // /_spa-data pages — should never appear in the sitemap.
        var route = MakeRoute("/legacy-page");
        var source = new ContentSource(new RedirectSource(new UrlPath("/new-page/")));
        var discovered = new DiscoveredItem(route, source);

        var service = CreateService(
            new StubContentService(discovered),
            new FailingParser());

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("legacy-page");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesMarkdownPagesWithFailedParse()
    {
        // If the markdown parser fails for a MarkdownFileSource (corrupt
        // front matter etc.) we should skip the entry rather than emit an
        // unfiltered URL — we can't honour IDraftable without metadata.
        var route = MakeRoute("/corrupt");
        var source = new ContentSource(new MarkdownFileSource("content/corrupt.md"));
        var discovered = new DiscoveredItem(route, source);

        var service = CreateService(
            new StubContentService(discovered),
            new FailingParser());

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("/corrupt");
    }
}