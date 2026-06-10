using Pennington.Content;
using Pennington.DocSite;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions;

namespace Pennington.IntegrationTests;

public class DiagnosticTest
{
    private static string GetContentPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Pennington.slnx")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new DirectoryNotFoundException("Could not locate repository root (Pennington.slnx) from " + AppContext.BaseDirectory);
        }

        return Path.Combine(dir.FullName, "docs", "Pennington.Docs", "Content");
    }

    [Fact(Skip = "Docs content restructuring in progress")]
    public async Task DiscoverAsync_FindsContent()
    {
        var contentPath = GetContentPath();
        Directory.Exists(contentPath).ShouldBeTrue($"Content path not found: {contentPath}");

        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath(contentPath),
            BasePageUrl = new UrlPath("/"),
        };

        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), new RealFileSystem(), new LocalizationOptions());

        var items = new List<string>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item.Route.CanonicalPath.Value);
        }

        items.Count.ShouldBeGreaterThan(0);
        items.ShouldContain(i => i.Contains("creating-first-site"));
    }

    [Fact]
    public async Task TocEntries_WorkWithDocSiteFrontMatter()
    {
        var contentPath = GetContentPath();
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath(contentPath),
            BasePageUrl = new UrlPath("/"),
        };

        // Use DocSiteFrontMatter — same as the real site
        var service = new MarkdownContentService<DocSiteFrontMatter>(options, new FrontMatterParser(), new RealFileSystem(), new LocalizationOptions());
        var tocItems = await service.GetContentTocEntriesAsync();
        tocItems.Count.ShouldBeGreaterThan(0, "TOC entries should not be empty with DocSiteFrontMatter");
    }

    [Fact]
    public async Task TocEntries_WorkWithDocFrontMatter()
    {
        var contentPath = GetContentPath();
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath(contentPath),
            BasePageUrl = new UrlPath("/"),
        };

        // Use Pennington's DocFrontMatter
        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser(), new RealFileSystem(), new LocalizationOptions());
        var tocItems = await service.GetContentTocEntriesAsync();
        tocItems.Count.ShouldBeGreaterThan(0, "TOC entries should not be empty with DocFrontMatter");
    }

    [Fact]
    public async Task RazorPageContentService_DiscoversHomepage()
    {
        // The Pennington.Docs assembly contains Index.razor with @page "/"
        var docsAssembly = typeof(Docs.Components.Index).Assembly;
        var service = new RazorPageContentService(
            [docsAssembly],
            new RealFileSystem(),
            new FrontMatterParser(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<RazorPageContentService>.Instance);

        var items = new List<DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            items.Add(item);
        }

        items.ShouldContain(i => i.Route.CanonicalPath.Value == "/");
        var homeItem = items.First(i => i.Route.CanonicalPath.Value == "/");
        (homeItem.Source is RazorPageSource).ShouldBeTrue();
    }
}