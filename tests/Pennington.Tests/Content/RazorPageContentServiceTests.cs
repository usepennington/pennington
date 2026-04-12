using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Content;

public class RazorPageContentServiceTests
{
    // --- AutoTitle tests ---

    [Theory]
    [InlineData("About", "About")]
    [InlineData("ClassDetail", "Class Detail")]
    [InlineData("GettingStarted", "Getting Started")]
    [InlineData("FAQPage", "FAQ Page")]
    [InlineData("HTMLParser", "HTML Parser")]
    [InlineData("Home", "Home")]
    public void AutoTitle_SplitsPascalCase(string input, string expected)
    {
        RazorPageContentService.AutoTitle(input).ShouldBe(expected);
    }

    // --- GetIndexableEntriesAsync vs GetContentTocEntriesAsync ---

    [Route("/about/")]
    private class AboutPage : ComponentBase { }

    [Route("/schedule/")]
    private class SchedulePage : ComponentBase { }

    private static RazorPageContentService CreateService(MockFileSystem? fs = null)
    {
        fs ??= new MockFileSystem();
        return new RazorPageContentService(
            [typeof(AboutPage).Assembly],
            fs,
            new FrontMatterParser(),
            NullLogger<RazorPageContentService>.Instance);
    }

    [Fact]
    public async Task GetIndexableEntriesAsync_IncludesPagesWithoutSidecar()
    {
        var service = CreateService();

        var entries = await service.GetIndexableEntriesAsync();

        // Should find at least our two test components (and potentially others in the assembly)
        entries.ShouldContain(e => e.Route.CanonicalPath.Value == "/about/");
        entries.ShouldContain(e => e.Route.CanonicalPath.Value == "/schedule/");
    }

    [Fact]
    public async Task GetContentTocEntriesAsync_SkipsPagesWithoutSidecar()
    {
        var service = CreateService();

        var entries = await service.GetContentTocEntriesAsync();

        // Pages without sidecar metadata should NOT appear in navigation TOC
        entries.ShouldNotContain(e => e.Route.CanonicalPath.Value == "/about/");
        entries.ShouldNotContain(e => e.Route.CanonicalPath.Value == "/schedule/");
    }

    [Fact]
    public async Task GetIndexableEntriesAsync_AutoTitlesFromComponentName()
    {
        var service = CreateService();

        var entries = await service.GetIndexableEntriesAsync();

        var about = entries.First(e => e.Route.CanonicalPath.Value == "/about/");
        about.Title.ShouldBe("About Page");

        var schedule = entries.First(e => e.Route.CanonicalPath.Value == "/schedule/");
        schedule.Title.ShouldBe("Schedule Page");
    }

    [Fact]
    public async Task GetIndexableEntriesAsync_DefaultsSearchFlagsToFalse()
    {
        var service = CreateService();

        var entries = await service.GetIndexableEntriesAsync();

        var about = entries.First(e => e.Route.CanonicalPath.Value == "/about/");
        about.ExcludeFromSearch.ShouldBeFalse();
        about.ExcludeFromLlms.ShouldBeFalse();
    }
}
