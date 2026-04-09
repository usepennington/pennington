using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.Feeds;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Feeds;

public class SitemapServiceTests
{
    private static ContentRoute MakeRoute(string path, string locale = "") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        Locale = locale,
    };

    private record TestFrontMatter : IFrontMatter, IDateable, IDraftable
    {
        public string Title { get; init; } = "Test";
        public DateTime? Date { get; init; }
        public bool IsDraft { get; init; }
    }

    private static RenderedContent MakeRenderedContent() => new(
        Html: "<p>Hello</p>",
        Outline: [],
        Tags: ImmutableList<Tag>.Empty,
        CrossReferences: ImmutableList<CrossReference>.Empty,
        SearchDocument: null,
        Social: null
    );

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

    private class StubParser(IFrontMatter metadata) : IContentParser
    {
        public Task<ContentItem> ParseAsync(DiscoveredItem item)
            => Task.FromResult(new ContentItem(
                new ParsedItem(item.Route, metadata, "# Test")));
    }

    private class StubRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
            => Task.FromResult(new ContentItem(
                new RenderedItem(item.Route, item.Metadata, MakeRenderedContent())));
    }

    private static SitemapService CreateService(
        IContentService contentService,
        IContentParser parser,
        IContentRenderer renderer,
        string canonicalBase = "https://example.com")
    {
        var services = new ServiceCollection();
        services.AddSingleton<IContentService>(contentService);
        services.AddSingleton(parser);
        services.AddSingleton(renderer);
        services.AddSingleton(new LocalizationOptions());
        var sp = services.BuildServiceProvider();

        var builder = new SitemapBuilder(new UrlPath(canonicalBase));
        return new SitemapService(sp, builder);
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
            new StubParser(metadata),
            new StubRenderer());

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
            new StubParser(metadata),
            new StubRenderer());

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
            new StubParser(metadata),
            new StubRenderer());

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("draft-post");
    }
}
