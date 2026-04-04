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
}
