using Microsoft.Extensions.DependencyInjection;
using Pennington.BlogSite;
using Pennington.BlogSite.Services;
using Pennington.Content;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.BlogSite;

public class BlogSiteContentServiceTests
{
    [Fact]
    public async Task GetRecordsAsync_ProjectsHomePageRecord_WithSiteIdentity()
    {
        var options = new BlogSiteOptions
        {
            SiteTitle = "My Blog",
            SiteDescription = "Posts about things",
            ContentRootPath = ".",
        };
        var service = new BlogSiteContentService(options, new ServiceCollection().BuildServiceProvider());

        var records = new List<ContentRecord>();
        await foreach (var record in service.GetRecordsAsync())
        {
            records.Add(record);
        }

        var home = records.ShouldHaveSingleItem();
        home.Route.CanonicalPath.Value.ShouldBe("/");
        home.Metadata.Title.ShouldBe("My Blog");
        home.Metadata.Description.ShouldBe("Posts about things");
    }

    [Fact]
    public async Task DiscoverAsync_EmitsArchivePagesBeyondPageSize()
    {
        var service = new BlogSiteContentService(Options(postsPerPage: 10), ProviderWith(PostCount: 25));

        var routes = await DiscoverRoutesAsync(service);

        routes.ShouldContain("/archive/page/2/");
        routes.ShouldContain("/archive/page/3/");
        routes.ShouldNotContain("/archive/page/1/");
        routes.ShouldNotContain("/archive/page/4/");
    }

    [Fact]
    public async Task DiscoverAsync_EmitsNoArchivePagesWhenPostsFitOnFirstPage()
    {
        var service = new BlogSiteContentService(Options(postsPerPage: 10), ProviderWith(PostCount: 5));

        var routes = await DiscoverRoutesAsync(service);

        routes.ShouldNotContain(r => r.StartsWith("/archive/page/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DiscoverAsync_TreatsZeroPostsPerPageAsDisablingPagination()
    {
        var service = new BlogSiteContentService(Options(postsPerPage: 0), ProviderWith(PostCount: 50));

        var routes = await DiscoverRoutesAsync(service);

        routes.ShouldNotContain(r => r.Contains("/page/", StringComparison.Ordinal));
    }

    private static BlogSiteOptions Options(int postsPerPage) => new()
    {
        SiteTitle = "Test",
        SiteDescription = "Test",
        ContentRootPath = ".",
        BlogBaseUrl = "/blog",
        PostsPerPage = postsPerPage,
    };

    private static async Task<List<string>> DiscoverRoutesAsync(BlogSiteContentService service)
    {
        var routes = new List<string>();
        await foreach (var item in service.DiscoverAsync())
        {
            routes.Add(item.Route.CanonicalPath.Value);
        }
        return routes;
    }

    private static IServiceProvider ProviderWith(int PostCount)
    {
        var records = Enumerable.Range(1, PostCount)
            .Select(i => new ContentRecord(
                ContentRouteFactory.FromUrl(new UrlPath($"/blog/post-{i:D2}/")),
                new BlogSiteFrontMatter { Title = $"Post {i}", Date = new DateTime(2024, 1, 1).AddDays(i) }))
            .ToList();

        return new ServiceCollection()
            .AddSingleton(new ContentRecordRegistry(records))
            .AddSingleton<IPageResolver>(new NullPageResolver())
            .AddSingleton(TimeProvider.System)
            .AddTransient<BlogPostQuery>()
            .BuildServiceProvider();
    }

    private sealed class NullPageResolver : IPageResolver
    {
        public Task<RenderedItem?> ResolveAsync(UrlPath requested) => Task.FromResult<RenderedItem?>(null);
    }
}
