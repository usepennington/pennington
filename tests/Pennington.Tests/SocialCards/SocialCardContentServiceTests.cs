using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;
using Pennington.SocialCards;

namespace Pennington.Tests.SocialCards;

public class SocialCardContentServiceTests
{
    private sealed record Fm : IFrontMatter
    {
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public DateTime? Date { get; init; }
    }

    /// <summary>
    /// Projects records like the real <see cref="MarkdownContentService{T}"/> — attaching parsed
    /// front matter as <see cref="DiscoveredItem.Metadata"/> so the default
    /// <see cref="IContentService.GetRecordsAsync"/> bridge surfaces them.
    /// </summary>
    private sealed class FakeRecordContentService : IContentService
    {
        private readonly List<string> _urls;

        public FakeRecordContentService(params string[] urls) => _urls = urls.ToList();

        public string DefaultSectionLabel => "fake";
        public int SearchPriority => 1;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var url in _urls)
            {
                yield return new DiscoveredItem(
                    ContentRouteFactory.FromUrl(new UrlPath(url)),
                    new FileSource(new FilePath($"/content{url}index.md"), "markdown"))
                {
                    Metadata = new Fm { Title = url },
                };
            }
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

    private static SocialCardContentService BuildService(params string[] urls)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IContentService>(new FakeRecordContentService(urls));
        var options = new SocialCardOptions { Render = (_, _, _) => Task.FromResult<byte[]?>([1]) };
        services.AddSingleton(options);
        // Register the service itself so DiscoverAsync resolves it as a sibling and must exclude it.
        services.AddSingleton<IContentService>(sp => new SocialCardContentService(sp, options));

        var sp = services.BuildServiceProvider();
        return sp.GetServices<IContentService>().OfType<SocialCardContentService>().Single();
    }

    private static async Task<List<DiscoveredItem>> CollectAsync(IAsyncEnumerable<DiscoveredItem> source)
    {
        var list = new List<DiscoveredItem>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    [Fact]
    public async Task DiscoverAsync_YieldsOneEndpointCardRoute_PerSiblingRecord()
    {
        var service = BuildService("/blog/my-post/", "/blog/another/");

        var items = await CollectAsync(service.DiscoverAsync());

        items.Count.ShouldBe(2);
        items.ShouldAllBe(i => i.Source.Value is EndpointSource);
        items.Select(i => i.Route.CanonicalPath.Value)
            .ShouldBe(["/social-cards/blog/my-post.png", "/social-cards/blog/another.png"], ignoreOrder: true);
    }

    [Fact]
    public async Task DiscoverAsync_KeepsPngOutputFile_WithoutIndexHtmlShaping()
    {
        var service = BuildService("/blog/my-post/");

        var item = (await CollectAsync(service.DiscoverAsync())).Single();

        // A .png route must not be rewritten to a directory index page.
        item.Route.OutputFile.Value.ShouldBe("social-cards/blog/my-post.png");
        item.Route.CanonicalPath.Value.ShouldBe("/social-cards/blog/my-post.png");
    }

    [Fact]
    public async Task DiscoverAsync_MapsHome_ToIndexCard()
    {
        var service = BuildService("/");

        var item = (await CollectAsync(service.DiscoverAsync())).Single();

        item.Route.CanonicalPath.Value.ShouldBe("/social-cards/index.png");
    }

    [Fact]
    public async Task GetRecordsAsync_IsEmpty_SoCardsDoNotRecurseOrIndex()
    {
        var service = BuildService("/blog/my-post/");

        var records = new List<ContentRecord>();
        await foreach (var record in service.GetRecordsAsync())
        {
            records.Add(record);
        }

        records.ShouldBeEmpty();
    }

    [Fact]
    public async Task DiscoverAsync_ExcludesSelf_NoCardForCardRoutes()
    {
        // Two sibling pages -> exactly two cards, none of them a card-of-a-card.
        var service = BuildService("/a/", "/b/");

        var items = await CollectAsync(service.DiscoverAsync());

        items.Count.ShouldBe(2);
        items.ShouldNotContain(i => i.Route.CanonicalPath.Value.Contains("social-cards/social-cards"));
    }
}
