using System.Collections.Immutable;
using Penn.Content;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Content;

public class IContentServiceTests
{
    [Fact]
    public async Task Stub_DiscoversItemsViaIAsyncEnumerable()
    {
        IContentService service = new StubContentService();

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(1);
        items[0].Route.CanonicalPath.Value.ShouldBe("/test/");
        items[0].Route.OutputFile.Value.ShouldBe("test/index.html");
        (items[0].Source is MarkdownFileSource).ShouldBeTrue();
    }

    [Fact]
    public async Task Stub_ReturnsEmptyListForContentToCopy()
    {
        IContentService service = new StubContentService();
        var result = await service.GetContentToCopyAsync();
        result.ShouldBe(ImmutableList<ContentToCopy>.Empty);
    }

    [Fact]
    public async Task Stub_ReturnsEmptyListForContentToCreate()
    {
        IContentService service = new StubContentService();
        var result = await service.GetContentToCreateAsync();
        result.ShouldBe(ImmutableList<ContentToCreate>.Empty);
    }

    [Fact]
    public async Task Stub_ReturnsEmptyListForContentTocEntries()
    {
        IContentService service = new StubContentService();
        var result = await service.GetContentTocEntriesAsync();
        result.ShouldBe(ImmutableList<ContentTocItem>.Empty);
    }

    [Fact]
    public async Task Stub_ReturnsEmptyListForCrossReferences()
    {
        IContentService service = new StubContentService();
        var result = await service.GetCrossReferencesAsync();
        result.ShouldBe(ImmutableList<CrossReference>.Empty);
    }

    private class StubContentService : IContentService
    {
        public string DefaultSection => "Test";
        public int SearchPriority => 5;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath("/test/"),
                OutputFile = new FilePath("test/index.html")
            };
            yield return new DiscoveredItem(route, new ContentSource(new MarkdownFileSource("test.md")));
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
