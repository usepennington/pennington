using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.Islands;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Islands;

public class SpaNavigationContentServiceTests
{
    private static SpaNavigationContentService CreateService(params IContentService[] contentServices)
    {
        var services = new ServiceCollection();
        foreach (var svc in contentServices)
            services.AddSingleton(svc);
        services.AddSingleton(new SpaNavigationOptions());

        var provider = services.BuildServiceProvider();
        return new SpaNavigationContentService(provider, new SpaNavigationOptions());
    }

    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath(
            string.IsNullOrEmpty(path.Trim('/')) ? "index.html" : $"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public async Task DiscoverAsync_SkipsRazorPageSource()
    {
        var inner = new StubContentService([
            new DiscoveredItem(MakeRoute("/"), new ContentSource(new RazorPageSource("MyApp.Index"))),
            new DiscoveredItem(MakeRoute("/about"), new ContentSource(new MarkdownFileSource(new FilePath("Content/about.md")))),
        ]);

        var service = CreateService(inner);
        var items = await CollectAsync(service.DiscoverAsync());

        items.ShouldHaveSingleItem();
        items[0].Route.CanonicalPath.Value.ShouldContain("about");
    }

    [Fact]
    public async Task DiscoverAsync_SkipsRedirectSource()
    {
        var inner = new StubContentService([
            new DiscoveredItem(MakeRoute("/old-page"), new ContentSource(new RedirectSource(new UrlPath("/new-page")))),
            new DiscoveredItem(MakeRoute("/docs"), new ContentSource(new MarkdownFileSource(new FilePath("Content/docs.md")))),
        ]);

        var service = CreateService(inner);
        var items = await CollectAsync(service.DiscoverAsync());

        items.ShouldHaveSingleItem();
        items[0].Route.CanonicalPath.Value.ShouldContain("docs");
    }

    [Fact]
    public async Task DiscoverAsync_IncludesMarkdownFileSource()
    {
        var inner = new StubContentService([
            new DiscoveredItem(MakeRoute("/getting-started"), new ContentSource(new MarkdownFileSource(new FilePath("Content/getting-started.md")))),
            new DiscoveredItem(MakeRoute("/guides/deploy"), new ContentSource(new MarkdownFileSource(new FilePath("Content/guides/deploy.md")))),
        ]);

        var service = CreateService(inner);
        var items = await CollectAsync(service.DiscoverAsync());

        items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task DiscoverAsync_SkipsNonHtmlOutputFiles()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/feed.xml"),
            OutputFile = new FilePath("feed.xml"),
        };
        var inner = new StubContentService([
            new DiscoveredItem(route, new ContentSource(new MarkdownFileSource(new FilePath("Content/feed.md")))),
        ]);

        var service = CreateService(inner);
        var items = await CollectAsync(service.DiscoverAsync());

        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task DiscoverAsync_SkipsItself()
    {
        // SpaNavigationContentService is registered as IContentService; it should skip itself
        var markdown = new StubContentService([
            new DiscoveredItem(MakeRoute("/page"), new ContentSource(new MarkdownFileSource(new FilePath("Content/page.md")))),
        ]);

        var services = new ServiceCollection();
        services.AddSingleton<IContentService>(markdown);
        services.AddSingleton(new SpaNavigationOptions());
        var provider = services.BuildServiceProvider();

        var spaService = new SpaNavigationContentService(provider, new SpaNavigationOptions());

        // Register itself as a content service too
        var services2 = new ServiceCollection();
        services2.AddSingleton<IContentService>(markdown);
        services2.AddSingleton<IContentService>(spaService);
        services2.AddSingleton(new SpaNavigationOptions());
        var provider2 = services2.BuildServiceProvider();

        var spaService2 = new SpaNavigationContentService(provider2, new SpaNavigationOptions());
        var items = await CollectAsync(spaService2.DiscoverAsync());

        // Should only have items from the markdown service, not recursive ones from itself
        items.ShouldHaveSingleItem();
    }

    private static async Task<List<DiscoveredItem>> CollectAsync(IAsyncEnumerable<DiscoveredItem> source)
    {
        var items = new List<DiscoveredItem>();
        await foreach (var item in source)
            items.Add(item);
        return items;
    }

    private class StubContentService(DiscoveredItem[] items) : IContentService
    {
        public string DefaultSection => "";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var item in items)
                yield return item;
            await Task.CompletedTask;
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
            => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
            => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
            => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
            => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }
}
