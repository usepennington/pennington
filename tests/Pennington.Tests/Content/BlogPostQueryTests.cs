using System.Collections.Immutable;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Content;

public class BlogPostQueryTests
{
    [Fact]
    public async Task GetPostsAsync_OrdersByDateDescending_AndExcludesDrafts()
    {
        var query = Build(
            Post("/blog/old/", "Old", new DateTime(2024, 1, 1)),
            Post("/blog/new/", "New", new DateTime(2024, 6, 1)),
            Post("/blog/draft/", "Draft", new DateTime(2024, 12, 1), isDraft: true));

        var posts = await query.GetPostsAsync<PostFm>("/blog");

        posts.Select(p => p.FrontMatter.Title).ShouldBe(["New", "Old"]);
    }

    [Fact]
    public async Task GetPostsAsync_FiltersByBasePrefix()
    {
        var query = Build(
            Post("/blog/in/", "In", new DateTime(2024, 1, 1)),
            Post("/notes/out/", "Out", new DateTime(2024, 1, 1)));

        var posts = await query.GetPostsAsync<PostFm>("/blog");

        posts.ShouldHaveSingleItem().FrontMatter.Title.ShouldBe("In");
    }

    [Fact]
    public async Task GetPageAsync_ReturnsCorrectSlice()
    {
        var query = Build(Enumerable.Range(1, 25)
            .Select(i => Post($"/blog/post-{i:D2}/", $"Post {i:D2}", new DateTime(2024, 1, 1).AddDays(i)))
            .ToArray());

        var page2 = await query.GetPageAsync<PostFm>("/blog", page: 2, pageSize: 10);

        page2.ShouldNotBeNull();
        page2.Items.Count.ShouldBe(10);
        page2.Page.ShouldBe(2);
        page2.TotalItems.ShouldBe(25);
        page2.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task GetPageAsync_ReturnsNullForOutOfRangePage()
    {
        var query = Build(Post("/blog/only/", "Only", new DateTime(2024, 1, 1)));

        (await query.GetPageAsync<PostFm>("/blog", page: 2, pageSize: 10)).ShouldBeNull();
        (await query.GetPageAsync<PostFm>("/blog", page: 0, pageSize: 10)).ShouldBeNull();
    }

    [Fact]
    public async Task GetPageAsync_ReturnsEmptyFirstPageWhenNoPosts()
    {
        var query = Build();

        var page1 = await query.GetPageAsync<PostFm>("/blog", page: 1, pageSize: 10);

        page1.ShouldNotBeNull();
        page1.Items.ShouldBeEmpty();
        page1.TotalItems.ShouldBe(0);
    }

    private static BlogPostQuery Build(params ContentRecord[] records)
        => new(new ContentRecordRegistry(records), new NullPageResolver(), TimeProvider.System);

    private static ContentRecord Post(string url, string title, DateTime date, bool isDraft = false)
        => new(ContentRouteFactory.FromUrl(new UrlPath(url)), new PostFm { Title = title, Date = date, IsDraft = isDraft });

    private sealed record PostFm : IFrontMatter
    {
        public string Title { get; init; } = "";
        public DateTime? Date { get; init; }
        public bool IsDraft { get; init; }
    }

    private sealed class NullPageResolver : IPageResolver
    {
        public Task<RenderedItem?> ResolveAsync(UrlPath requested)
            => throw new NotSupportedException("Listing tests should not resolve a single page.");
    }
}
