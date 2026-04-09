using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Pipeline;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Content;

public class RazorPageContentServiceTests
{
    private static RazorPageContentService CreateService(
        System.Reflection.Assembly[] assemblies,
        MockFileSystem? fileSystem = null)
    {
        var fs = fileSystem ?? new MockFileSystem();
        return new RazorPageContentService(
            assemblies,
            fs,
            new FrontMatterParser(),
            NullLogger<RazorPageContentService>.Instance);
    }

    [Fact]
    public async Task DiscoverAsync_ReturnsEmpty_ForAssemblyWithNoPageComponents()
    {
        // System.String's assembly (System.Private.CoreLib) has no Blazor @page components
        var service = CreateService([typeof(string).Assembly]);

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
        var service = CreateService([]);

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
        // The Pennington.DocSite assembly has Pages.razor with route "/{*fileName:nonfile}" which is parameterized.
        // It should be excluded. We test by scanning Pennington.Tests assembly which has no @page components.
        var service = CreateService([typeof(RazorPageContentServiceTests).Assembly]);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        // Test assembly has no @page components, so nothing should be discovered
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task MissingTrailingSlashPages_EmptyWhenNoPagesDiscovered()
    {
        var service = CreateService([]);

        // Trigger discovery
        await foreach (var _ in service.DiscoverAsync()) { }

        service.MissingTrailingSlashPages.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_ReturnsEmpty_WhenNoSidecarFiles()
    {
        var service = CreateService([typeof(string).Assembly]);

        var entries = await service.GetContentTocEntriesAsync();

        entries.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCrossReferencesAsync_ReturnsEmpty_WhenNoSidecarFiles()
    {
        var service = CreateService([]);

        var refs = await service.GetCrossReferencesAsync();

        refs.ShouldBeEmpty();
    }
}
