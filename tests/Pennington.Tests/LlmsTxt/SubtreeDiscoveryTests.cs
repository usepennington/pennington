using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.LlmsTxt;
using Pennington.Localization;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.LlmsTxt;

public class SubtreeDiscoveryTests
{
    private static readonly LocalizationOptions DefaultLocalization = new();

    private MarkdownContentService<DocFrontMatter> CreateService(MockFileSystem fs, UrlPath? basePageUrl = null)
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath("/content"),
            BasePageUrl = basePageUrl ?? new UrlPath("/docs"),
            SectionLabel = "Documentation",
        };
        return new MarkdownContentService<DocFrontMatter>(
            options, new FrontMatterParser(), fs, new FileWatcher(fs), DefaultLocalization);
    }

    private static MockFileSystem CreateFs(params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        foreach (var (path, content) in files)
        {
            var full = $"/content/{path}";
            var dir = fs.Path.GetDirectoryName(full);
            if (dir != null) fs.Directory.CreateDirectory(dir);
            fs.File.WriteAllText(full, content);
        }
        return fs;
    }

    [Fact]
    public async Task SidecarAtSubfolder_ProducesSubtreeWithCombinedPrefix()
    {
        var fs = CreateFs(
            ("reference/_llms.yaml", "title: API reference\ndescription: Type and member docs\n"),
            ("reference/foo.md", "---\ntitle: Foo\n---\n# Foo"));
        var service = CreateService(fs, new UrlPath("/docs"));

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.Count.ShouldBe(1);
        subtrees[0].RoutePrefix.ShouldBe("/docs/reference/");
        subtrees[0].Title.ShouldBe("API reference");
        subtrees[0].Description.ShouldBe("Type and member docs");
    }

    [Fact]
    public async Task SidecarAtContentRoot_UsesBasePageUrlAsPrefix()
    {
        var fs = CreateFs(
            ("_llms.yaml", "title: Docs\ndescription: All docs\n"));
        var service = CreateService(fs, new UrlPath("/docs"));

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.Count.ShouldBe(1);
        subtrees[0].RoutePrefix.ShouldBe("/docs/");
    }

    [Fact]
    public async Task SidecarWithRootBasePageUrl_ProducesPlainPrefix()
    {
        var fs = CreateFs(
            ("api/_llms.yaml", "title: API\n"));
        var service = CreateService(fs, new UrlPath("/"));

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.Count.ShouldBe(1);
        subtrees[0].RoutePrefix.ShouldBe("/api/");
    }

    [Fact]
    public async Task MultipleSidecars_ProduceOneSubtreeEach()
    {
        var fs = CreateFs(
            ("reference/_llms.yaml", "title: Reference\n"),
            ("recipes/_llms.yaml", "title: Recipes\n"));
        var service = CreateService(fs, new UrlPath("/docs"));

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.Count.ShouldBe(2);
        subtrees.Select(s => s.RoutePrefix).ShouldBe(
            new[] { "/docs/reference/", "/docs/recipes/" },
            ignoreOrder: true);
    }

    [Fact]
    public async Task SidecarMissingTitle_IsSkipped()
    {
        var fs = CreateFs(
            ("reference/_llms.yaml", "description: no title here\n"));
        var service = CreateService(fs);

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.ShouldBeEmpty();
    }

    [Fact]
    public async Task NoSidecars_ReturnsEmpty()
    {
        var fs = CreateFs(("reference/foo.md", "---\ntitle: Foo\n---\n"));
        var service = CreateService(fs);

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.ShouldBeEmpty();
    }

    [Fact]
    public async Task LlmsSubtree_NormalizesRoutePrefixToBracketedSlashForm()
    {
        new LlmsSubtree("api", "API", "").RoutePrefix.ShouldBe("/api/");
        new LlmsSubtree("/api", "API", "").RoutePrefix.ShouldBe("/api/");
        new LlmsSubtree("api/", "API", "").RoutePrefix.ShouldBe("/api/");
        new LlmsSubtree("/reference/api/", "API", "").RoutePrefix.ShouldBe("/reference/api/");
    }

    [Fact]
    public void LlmsSubtree_RejectsEmptyPrefixOrTitle()
    {
        Should.Throw<ArgumentException>(() => new LlmsSubtree("", "API", ""));
        Should.Throw<ArgumentException>(() => new LlmsSubtree("/api/", "", ""));
    }
}
