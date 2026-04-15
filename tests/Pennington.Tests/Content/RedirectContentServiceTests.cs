using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Content;

public class RedirectContentServiceTests
{
    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
    };

    private static RedirectContentService CreateService(
        MockFileSystem fs,
        string contentRoot,
        params IContentService[] extraServices)
    {
        var services = new ServiceCollection();
        foreach (var svc in extraServices) services.AddSingleton(svc);
        var options = new PenningtonOptions { ContentRootPath = contentRoot };
        var provider = services.BuildServiceProvider();
        return new RedirectContentService(provider, options, fs);
    }

    [Fact]
    public async Task GetRedirectMappingsAsync_ReturnsEmpty_WhenNoYamlAndNoFrontMatterRedirects()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");

        var service = CreateService(fs, "/content");

        var map = await service.GetRedirectMappingsAsync();
        map.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRedirectMappingsAsync_LoadsYamlRedirects()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        fs.File.WriteAllText("/content/_redirects.yml", """
            redirects:
              /old-page: /new-page/
              /archived: https://example.com/archive
            """);

        var service = CreateService(fs, "/content");

        var map = await service.GetRedirectMappingsAsync();
        map.Count.ShouldBe(2);
        map["/old-page"].ShouldBe("/new-page/");
        map["/archived"].ShouldBe("https://example.com/archive");
    }

    [Fact]
    public async Task GetRedirectMappingsAsync_IncludesFrontMatterRedirectsFromOtherServices()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");

        var stub = new StubRedirectingService(
            (MakeRoute("/main/old"), "/main/new/"));

        var service = CreateService(fs, "/content", stub);

        var map = await service.GetRedirectMappingsAsync();
        map.Count.ShouldBe(1);
        map["/main/old"].ShouldBe("/main/new/");
    }

    [Fact]
    public async Task GetRedirectMappingsAsync_MergesYamlAndFrontMatter_FrontMatterWins()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        fs.File.WriteAllText("/content/_redirects.yml", """
            redirects:
              /shared/path: /yaml-target/
              /yaml-only: /yaml-only-target/
            """);

        var stub = new StubRedirectingService(
            (MakeRoute("/shared/path"), "/front-matter-target/"),
            (MakeRoute("/fm-only"), "/fm-only-target/"));

        var service = CreateService(fs, "/content", stub);

        var map = await service.GetRedirectMappingsAsync();
        map.Count.ShouldBe(3);
        map["/shared/path"].ShouldBe("/front-matter-target/");
        map["/yaml-only"].ShouldBe("/yaml-only-target/");
        map["/fm-only"].ShouldBe("/fm-only-target/");
    }

    [Fact]
    public async Task DiscoverAsync_YieldsOneItemPerYamlRedirect_SoBuildCrawlerHitsThem()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        fs.File.WriteAllText("/content/_redirects.yml", """
            redirects:
              /old-a: /new-a/
              /old-b: /new-b/
            """);

        var service = CreateService(fs, "/content");

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync()) items.Add(item);

        items.Count.ShouldBe(2);
        foreach (var item in items)
        {
            (item.Source is RedirectSource).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task GetRedirectMappingsAsync_SkipsRedirectsFromItself_ToAvoidInfiniteRecursion()
    {
        // The service lists itself as an IContentService; its own DiscoverAsync
        // emits the YAML entries. The scan must skip itself so YAML entries aren't
        // double-counted into the map.
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        fs.File.WriteAllText("/content/_redirects.yml", """
            redirects:
              /a: /b/
            """);

        var serviceCollection = new ServiceCollection();
        var options = new PenningtonOptions { ContentRootPath = "/content" };
        var service = new RedirectContentService(serviceCollection.BuildServiceProvider(), options, fs);
        serviceCollection.AddSingleton<IContentService>(service);

        var map = await service.GetRedirectMappingsAsync();
        map.Count.ShouldBe(1);
        map["/a"].ShouldBe("/b/");
    }

    private sealed class StubRedirectingService : IContentService
    {
        private readonly (ContentRoute Route, string Target)[] _redirects;

        public StubRedirectingService(params (ContentRoute Route, string Target)[] redirects)
        {
            _redirects = redirects;
        }

        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var (route, target) in _redirects)
            {
                yield return new DiscoveredItem(route, new RedirectSource(new UrlPath(target)));
            }
            await Task.CompletedTask;
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() =>
            Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() =>
            Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() =>
            Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() =>
            Task.FromResult(ImmutableList<CrossReference>.Empty);
    }
}