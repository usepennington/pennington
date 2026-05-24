using System.Collections.Generic;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Pipeline;

public class MetadataEnrichmentTests
{
    private static ParsedItem MakeItem(string body) =>
        new(
            new ContentRoute
            {
                CanonicalPath = new UrlPath("/page/"),
                OutputFile = new FilePath("page/index.html"),
            },
            new DocFrontMatter { Title = "Page" },
            body);

    [Fact]
    public async Task ReadingTime_RoundsUpToWholeMinutes()
    {
        var body = string.Join(' ', Enumerable.Repeat("word", 250));
        var result = await new ReadingTimeEnricher().EnrichAsync(MakeItem(body));

        result[ReadingTimeEnricher.Key].ShouldBe(2); // ceil(250 / 200)
    }

    [Fact]
    public async Task ReadingTime_ShortBodyIsAtLeastOneMinute()
    {
        var result = await new ReadingTimeEnricher().EnrichAsync(MakeItem("just a few words here"));

        result[ReadingTimeEnricher.Key].ShouldBe(1);
    }

    [Fact]
    public async Task ReadingTime_EmptyBodyContributesNothing()
    {
        var result = await new ReadingTimeEnricher().EnrichAsync(MakeItem("   "));

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnrichmentService_WithNoEnrichers_LeavesItemUnchanged()
    {
        var item = MakeItem("hello world");
        var result = await new MetadataEnrichmentService([]).EnrichAsync(item);

        result.ShouldBeSameAs(item);
        result.Derived.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnrichmentService_MergesContributionsIntoDerived()
    {
        var item = MakeItem(string.Join(' ', Enumerable.Repeat("word", 400)));
        var service = new MetadataEnrichmentService([new ReadingTimeEnricher()]);

        var result = await service.EnrichAsync(item);

        result.Derived[ReadingTimeEnricher.Key].ShouldBe(2); // ceil(400 / 200)
    }

    [Fact]
    public async Task EnrichmentService_LaterEnricherWinsOnKeyCollision()
    {
        var service = new MetadataEnrichmentService(
        [
            new FakeEnricher("k", "first"),
            new FakeEnricher("k", "second"),
        ]);

        var result = await service.EnrichAsync(MakeItem("body"));

        result.Derived["k"].ShouldBe("second");
    }

    private sealed class FakeEnricher(string key, object? value) : IMetadataEnricher
    {
        public Task<IReadOnlyDictionary<string, object?>> EnrichAsync(ParsedItem item) =>
            Task.FromResult<IReadOnlyDictionary<string, object?>>(
                new Dictionary<string, object?> { [key] = value });
    }
}
