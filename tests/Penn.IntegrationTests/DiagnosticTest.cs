using System.Reflection;
using Penn.Content;
using Penn.DocSite;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.IntegrationTests;

public class DiagnosticTest
{
    private static string GetContentPath() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "Penn.Docs", "Content"));

    [Fact]
    public async Task DiscoverAsync_FindsContent()
    {
        var contentPath = GetContentPath();
        Directory.Exists(contentPath).ShouldBeTrue($"Content path not found: {contentPath}");

        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath(contentPath),
            BasePageUrl = new UrlPath("/"),
        };

        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser());

        var items = new List<string>();
        await foreach (var item in service.DiscoverAsync())
            items.Add(item.Route.CanonicalPath.Value);

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
        var service = new MarkdownContentService<DocSiteFrontMatter>(options, new FrontMatterParser());
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

        // Use Penn's DocFrontMatter
        var service = new MarkdownContentService<DocFrontMatter>(options, new FrontMatterParser());
        var tocItems = await service.GetContentTocEntriesAsync();
        tocItems.Count.ShouldBeGreaterThan(0, "TOC entries should not be empty with DocFrontMatter");
    }

    [Fact]
    public async Task RazorPageContentService_DiscoversHomepage()
    {
        // The Penn.Docs assembly contains Index.razor with @page "/"
        var docsAssembly = typeof(Penn.Docs.Components.Index).Assembly;
        var service = new RazorPageContentService([docsAssembly]);

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
