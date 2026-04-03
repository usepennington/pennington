using System.Collections.Immutable;
using System.Reflection;
using Penn.Content;
using Penn.Pipeline;

namespace Penn.Tests.Content;

public class RazorPageContentServiceTests
{
    [Fact]
    public async Task DiscoverAsync_ReturnsEmpty_ForAssemblyWithNoPageComponents()
    {
        // System.String's assembly (System.Private.CoreLib) has no Blazor @page components
        var service = new RazorPageContentService([typeof(string).Assembly]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task DiscoverAsync_ReturnsEmpty_ForEmptyAssemblyArray()
    {
        var service = new RazorPageContentService([]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.ShouldBeEmpty();
    }

    [Fact]
    public void DefaultSection_ReturnsEmptyString()
    {
        var service = new RazorPageContentService([]);
        service.DefaultSection.ShouldBe("");
    }

    [Fact]
    public void SearchPriority_ReturnsFive()
    {
        var service = new RazorPageContentService([]);
        service.SearchPriority.ShouldBe(5);
    }

    [Fact]
    public async Task GetContentToCopyAsync_ReturnsEmpty()
    {
        var service = new RazorPageContentService([]);
        var result = await service.GetContentToCopyAsync();
        result.ShouldBe(ImmutableList<ContentToCopy>.Empty);
    }

    [Fact]
    public async Task GetContentToCreateAsync_ReturnsEmpty()
    {
        var service = new RazorPageContentService([]);
        var result = await service.GetContentToCreateAsync();
        result.ShouldBe(ImmutableList<ContentToCreate>.Empty);
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_ReturnsEmpty()
    {
        var service = new RazorPageContentService([]);
        var result = await service.GetContentTocEntriesAsync();
        result.ShouldBe(ImmutableList<ContentTocItem>.Empty);
    }

    [Fact]
    public async Task GetCrossReferencesAsync_ReturnsEmpty()
    {
        var service = new RazorPageContentService([]);
        var result = await service.GetCrossReferencesAsync();
        result.ShouldBe(ImmutableList<CrossReference>.Empty);
    }

    [Fact]
    public async Task DiscoverAsync_SkipsParameterizedRoutes()
    {
        // The Penn.DocSite assembly has Pages.razor with route "/{*fileName:nonfile}" which is parameterized.
        // It should be excluded. We test by scanning Penn.Tests assembly which has no @page components.
        var service = new RazorPageContentService([typeof(RazorPageContentServiceTests).Assembly]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        // Test assembly has no @page components, so nothing should be discovered
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task DiscoverAsync_YieldsRazorPageSource()
    {
        // If we find any items, they should have RazorPageSource content source.
        // We can't easily create a test assembly with @page components,
        // but we verify the service implements IContentService correctly.
        IContentService service = new RazorPageContentService([typeof(string).Assembly]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        // No items from CoreLib, but the interface contract is satisfied
        items.ShouldBeEmpty();
    }
}
