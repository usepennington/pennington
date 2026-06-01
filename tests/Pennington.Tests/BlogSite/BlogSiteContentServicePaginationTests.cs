using Pennington.BlogSite;
using Pennington.BlogSite.Services;
using Pennington.FrontMatter;
using Pennington.Pipeline;

namespace Pennington.Tests.BlogSite;

public class BlogSiteContentServicePaginationTests : IDisposable
{
    private readonly string _root;

    public BlogSiteContentServicePaginationTests()
    {
        _root = Directory.CreateTempSubdirectory("blogsite-pagination-").FullName;
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { }
    }

    [Fact]
    public async Task EmitsArchivePagesForPostsBeyondPageSize()
    {
        WritePosts(count: 25, tags: ["dotnet"]);

        var routes = await DiscoverRoutesAsync(postsPerPage: 10);

        routes.ShouldContain("/archive/page/2/");
        routes.ShouldContain("/archive/page/3/");
        routes.ShouldNotContain("/archive/page/1/");
        routes.ShouldNotContain("/archive/page/4/");
    }

    [Fact]
    public async Task EmitsNoArchivePagesWhenPostsFitOnFirstPage()
    {
        WritePosts(count: 5, tags: ["dotnet"]);

        var routes = await DiscoverRoutesAsync(postsPerPage: 10);

        routes.ShouldNotContain(r => r.StartsWith("/archive/page/"));
    }

    [Fact]
    public async Task EmitsCanonicalTagRouteAlways()
    {
        WritePosts(count: 3, tags: ["dotnet"]);

        var routes = await DiscoverRoutesAsync(postsPerPage: 10);

        routes.ShouldContain("/tags/dotnet/");
    }

    [Fact]
    public async Task EmitsPerTagPagesOnlyForTagsExceedingPageSize()
    {
        WritePosts(count: 15, tags: ["popular"]);
        WritePosts(count: 3, tags: ["rare"]);

        var routes = await DiscoverRoutesAsync(postsPerPage: 10);

        routes.ShouldContain("/tags/popular/page/2/");
        routes.ShouldNotContain(r => r.StartsWith("/tags/rare/page/"));
    }

    [Fact]
    public async Task RespectsCustomTagsPageUrl()
    {
        WritePosts(count: 12, tags: ["dotnet"]);

        var routes = await DiscoverRoutesAsync(postsPerPage: 10, tagsPageUrl: "/topics");

        routes.ShouldContain("/topics/dotnet/");
        routes.ShouldContain("/topics/dotnet/page/2/");
    }

    [Fact]
    public async Task TreatsZeroPostsPerPageAsDisablingPagination()
    {
        WritePosts(count: 50, tags: ["dotnet"]);

        var routes = await DiscoverRoutesAsync(postsPerPage: 0);

        routes.ShouldNotContain(r => r.Contains("/page/"));
    }

    private async Task<List<string>> DiscoverRoutesAsync(
        int postsPerPage,
        string tagsPageUrl = "/tags")
    {
        var options = new BlogSiteOptions
        {
            SiteTitle = "Test",
            SiteDescription = "Test",
            ContentRootPath = _root,
            BlogContentPath = ".",
            PostsPerPage = postsPerPage,
            TagsPageUrl = tagsPageUrl,
        };
        var service = new BlogSiteContentService(options, new FrontMatterParser());

        var routes = new List<string>();
        await foreach (var item in service.DiscoverAsync())
        {
            routes.Add(item.Route.CanonicalPath.Value);
        }
        return routes;
    }

    private void WritePosts(int count, string[] tags)
    {
        var tagYaml = string.Join(", ", tags);
        var existing = Directory.GetFiles(_root, "*.md").Length;
        for (var i = 0; i < count; i++)
        {
            var idx = existing + i;
            var path = Path.Combine(_root, $"post-{tags[0]}-{idx:D4}.md");
            File.WriteAllText(path,
                $"""
                ---
                title: Post {idx}
                date: 2024-01-{(idx % 28) + 1:D2}
                tags: [{tagYaml}]
                ---

                Body for post {idx}.
                """);
        }
    }
}
