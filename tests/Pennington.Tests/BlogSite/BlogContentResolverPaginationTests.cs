using System.Collections.Immutable;
using Pennington.BlogSite;
using Pennington.BlogSite.Services;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.BlogSite;

public class BlogContentResolverPaginationTests : IDisposable
{
    private readonly string _root;

    public BlogContentResolverPaginationTests()
    {
        _root = Directory.CreateTempSubdirectory("blog-resolver-pagination-").FullName;
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { }
    }

    [Fact]
    public async Task PagedPostsReturnsCorrectSlice()
    {
        WritePosts(count: 25);
        var resolver = BuildResolver();

        var page2 = await resolver.GetPagedPostsAsync(page: 2, pageSize: 10);

        page2.ShouldNotBeNull();
        page2.Items.Count.ShouldBe(10);
        page2.Page.ShouldBe(2);
        page2.TotalItems.ShouldBe(25);
        page2.TotalPages.ShouldBe(3);
        page2.HasPrevious.ShouldBeTrue();
        page2.HasNext.ShouldBeTrue();
    }

    [Fact]
    public async Task PagedPostsLastPageHasNoNext()
    {
        WritePosts(count: 25);
        var resolver = BuildResolver();

        var page3 = await resolver.GetPagedPostsAsync(page: 3, pageSize: 10);

        page3.ShouldNotBeNull();
        page3.Items.Count.ShouldBe(5);
        page3.HasNext.ShouldBeFalse();
    }

    [Fact]
    public async Task PagedPostsReturnsNullForOutOfRangePage()
    {
        WritePosts(count: 5);
        var resolver = BuildResolver();

        (await resolver.GetPagedPostsAsync(page: 2, pageSize: 10)).ShouldBeNull();
        (await resolver.GetPagedPostsAsync(page: 0, pageSize: 10)).ShouldBeNull();
        (await resolver.GetPagedPostsAsync(page: -1, pageSize: 10)).ShouldBeNull();
    }

    [Fact]
    public async Task PagedPostsByTagFiltersAndSlices()
    {
        WritePosts(count: 15, tag: "popular");
        WritePosts(count: 3, tag: "rare");
        var resolver = BuildResolver();

        var result = await resolver.GetPagedPostsByTagAsync("popular", page: 2, pageSize: 10);

        result.ShouldNotBeNull();
        var (tag, page) = result.Value;
        tag.Name.ShouldBe("popular");
        page.Items.Count.ShouldBe(5);
        page.TotalItems.ShouldBe(15);
        page.TotalPages.ShouldBe(2);
    }

    [Fact]
    public async Task PagedPostsByTagReturnsNullForUnknownTag()
    {
        WritePosts(count: 5, tag: "dotnet");
        var resolver = BuildResolver();

        (await resolver.GetPagedPostsByTagAsync("unknown", 1, 10)).ShouldBeNull();
    }

    private BlogContentResolver BuildResolver()
    {
        var options = new BlogSiteOptions
        {
            SiteTitle = "Test",
            Description = "Test",
            ContentRootPath = _root,
            BlogContentPath = ".",
            TagsPageUrl = "/tags",
        };
        var service = new FakeMarkdownContentService(Directory.GetFiles(_root, "*.md"));
        return new BlogContentResolver(
            [service],
            new FrontMatterParser(),
            new NullRenderer(),
            options);
    }

    private void WritePosts(int count, string tag = "dotnet")
    {
        var existing = Directory.GetFiles(_root, "*.md").Length;
        for (var i = 0; i < count; i++)
        {
            var idx = existing + i;
            var path = Path.Combine(_root, $"post-{tag}-{idx:D4}.md");
            File.WriteAllText(path,
                $"""
                ---
                title: Post {idx}
                date: 2024-01-{(idx % 28) + 1:D2}
                tags: [{tag}]
                ---

                Body for post {idx}.
                """);
        }
    }

    private sealed class FakeMarkdownContentService(IEnumerable<string> filePaths) : IContentService
    {
        private readonly List<string> _files = filePaths.ToList();

        public string DefaultSectionLabel => "blog";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var file in _files)
            {
                var route = ContentRouteFactory.FromUrl(new UrlPath($"/blog/{Path.GetFileNameWithoutExtension(file)}/"));
                yield return new DiscoveredItem(route, new MarkdownFileSource(new FilePath(file)));
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

    private sealed class NullRenderer : IContentRenderer
    {
        public Task<ContentItem> RenderAsync(ParsedItem item)
            => throw new NotSupportedException("Paged-list tests should not invoke the renderer.");
    }
}
