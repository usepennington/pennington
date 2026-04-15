using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class XrefResolverTests
{
    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
    };

    [Fact]
    public async Task ResolveAsync_FirstRegisteredRouteWins_WhenDuplicateUidEmittedByFallback()
    {
        // Simulates MarkdownContentService.DiscoverRoutesWithFallbacks emitting
        // one entry for the default locale (EN) and one fallback entry for FR.
        // EN is discovered first; XrefResolver must preserve it.
        var defaultLocale = new StubContentService(
            new CrossReference("my-uid", "My Page", MakeRoute("/main/my-page")));
        var frenchFallback = new StubContentService(
            new CrossReference("my-uid", "My Page", MakeRoute("/fr/main/my-page")));

        var resolver = new XrefResolver([defaultLocale, frenchFallback]);

        var result = await resolver.ResolveAsync("my-uid");

        result.ShouldNotBeNull();
        result!.Route.CanonicalPath.Value.ShouldBe("/main/my-page/");
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_ForUnknownUid()
    {
        var resolver = new XrefResolver([new StubContentService()]);

        var result = await resolver.ResolveAsync("does-not-exist");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_ForEmptyUid()
    {
        var resolver = new XrefResolver([
            new StubContentService(new CrossReference("real", "Real", MakeRoute("/real"))),
        ]);

        var result = await resolver.ResolveAsync("");

        result.ShouldBeNull();
    }

    private sealed class StubContentService(params CrossReference[] refs) : IContentService
    {
        private readonly ImmutableList<CrossReference> _refs = refs.ToImmutableList();

        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() =>
            AsyncEnumerable.Empty<DiscoveredItem>();

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() =>
            Task.FromResult(ImmutableList<ContentToCopy>.Empty);

        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() =>
            Task.FromResult(ImmutableList<ContentToCreate>.Empty);

        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() =>
            Task.FromResult(ImmutableList<ContentTocItem>.Empty);

        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() =>
            Task.FromResult(_refs);

        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;
    }
}