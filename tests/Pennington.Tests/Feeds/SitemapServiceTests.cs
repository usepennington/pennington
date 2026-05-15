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

    private class StubProgrammaticGenerator : IProgrammaticContentGenerator
    {
        public Task<ProgrammaticContent> GenerateAsync(ContentRoute route)
            => Task.FromResult(new ProgrammaticContent(
                new TextProgrammaticContent(null, "<p>gen</p>", "text/html")));
    }

    private static SitemapService CreateService(
        IContentService contentService,
        string canonicalBase = "https://example.com")
    {
        return new SitemapService(
            contentServices: [contentService],
            localization: new LocalizationOptions(),
            builder: new SitemapBuilder(new UrlPath(canonicalBase)));
    }

    [Fact]
    public async Task GetSitemapXml_IncludesLastModified_WhenContentHasDate()
    {
        var date = new DateTime(2026, 3, 15);
        var route = MakeRoute("/blog/my-post");
        var source = new ContentSource(new MarkdownFileSource("content/post.md"));
        var metadata = new TestFrontMatter { Title = "My Post", Date = date };
        var discovered = new DiscoveredItem(route, source) { Metadata = metadata };

        var service = CreateService(new StubContentService(discovered));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("<lastmod>2026-03-15</lastmod>");
    }

    [Fact]
    public async Task GetSitemapXml_OmitsLastModified_WhenContentHasNoDate()
    {
        var route = MakeRoute("/about");
        var source = new ContentSource(new MarkdownFileSource("content/about.md"));
        var metadata = new TestFrontMatter { Title = "About" };
        var discovered = new DiscoveredItem(route, source) { Metadata = metadata };

        var service = CreateService(new StubContentService(discovered));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("https://example.com/about/");
        xml.ShouldNotContain("lastmod");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesDrafts()
    {
        var route = MakeRoute("/draft-post");
        var source = new ContentSource(new MarkdownFileSource("content/draft.md"));
        var metadata = new TestFrontMatter { Title = "Draft", IsDraft = true };
        var discovered = new DiscoveredItem(route, source) { Metadata = metadata };

        var service = CreateService(new StubContentService(discovered));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("draft-post");
    }

    [Fact]
    public async Task GetSitemapXml_IncludesProgrammaticSource()
    {
        // Programmatic sources carry no discovery-time metadata — their route is
        // emitted directly. Guards against the regression where search/llms
        // enumerate TOC but sitemap used the parse pipeline, leaving programmatic
        // content out of sitemap.xml.
        var route = MakeRoute("/generated/page");
        var source = new ContentSource(new ProgrammaticSource(new StubProgrammaticGenerator()));
        var discovered = new DiscoveredItem(route, source);

        var service = CreateService(new StubContentService(discovered));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("https://example.com/generated/page/");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesNonHtmlOutputs()
    {
        // Custom IContentService impls can emit non-HTML outputs (JSON feeds,
        // generated CSV, etc.). Those routes are transport, not canonical
        // pages; the sitemap must skip anything whose output file is not HTML.
        var jsonRoute = new ContentRoute
        {
            CanonicalPath = new UrlPath("/data/intro.json"),
            OutputFile = new FilePath("data/intro.json"),
        };
        var htmlRoute = MakeRoute("/docs/intro");
        var discovered = new[]
        {
            new DiscoveredItem(jsonRoute, new ContentSource(new ProgrammaticSource(new StubProgrammaticGenerator()))),
            new DiscoveredItem(htmlRoute, new ContentSource(new ProgrammaticSource(new StubProgrammaticGenerator()))),
        };

        var service = CreateService(new StubContentService(discovered));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("data/intro.json");
        xml.ShouldContain("https://example.com/docs/intro/");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesRedirectSources()
    {
        // Routes backed by RedirectSource — explicit redirects from
        // _redirects.yml or per-page redirectUrl front matter — should never
        // appear in the sitemap.
        var route = MakeRoute("/legacy-page");
        var source = new ContentSource(new RedirectSource(new UrlPath("/new-page/")));
        var discovered = new DiscoveredItem(route, source);

        var service = CreateService(new StubContentService(discovered));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldNotContain("legacy-page");
    }

    [Fact]
    public async Task GetSitemapXml_IncludesMarkdownPage_WhenMetadataMissing()
    {
        // A markdown file whose front matter failed to parse arrives with no
        // metadata, but it still renders and is served — so it belongs in the
        // sitemap, just without a <lastmod>. (Drafts and redirects are filtered
        // upstream by the content service, never reaching here as a bare
        // MarkdownFileSource.)
        var route = MakeRoute("/corrupt");
        var source = new ContentSource(new MarkdownFileSource("content/corrupt.md"));
        var discovered = new DiscoveredItem(route, source);

        var service = CreateService(new StubContentService(discovered));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("https://example.com/corrupt/");
        xml.ShouldNotContain("lastmod");
    }

    [Fact]
    public async Task GetSitemapXml_ExcludesLlmsOnlySource()
    {
        // Llms-only pages have no HTML at the canonical URL — they shouldn't
        // be advertised to crawlers or other sitemap consumers.
        var visibleRoute = MakeRoute("/visible");
        var visibleItem = new DiscoveredItem(visibleRoute,
            new ContentSource(new MarkdownFileSource("content/visible.md")));

        var llmsRoute = MakeRoute("/agent-context");
        var llmsItem = new DiscoveredItem(llmsRoute,
            new ContentSource(new LlmsOnlySource("content/agent-context.llms.md")));

        var service = CreateService(new StubContentService(visibleItem, llmsItem));

        var xml = await service.GetSitemapXmlAsync();

        xml.ShouldContain("/visible");
        xml.ShouldNotContain("/agent-context");
    }
}