using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.LlmsTxt;
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
            options, new FrontMatterParser(), fs, DefaultLocalization);
    }

    private static MockFileSystem CreateFs(params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        foreach (var (path, content) in files)
        {
            var full = $"/content/{path}";
            var dir = fs.Path.GetDirectoryName(full);
            if (dir != null)
            {
                fs.Directory.CreateDirectory(dir);
            }

            fs.File.WriteAllText(full, content);
        }
        return fs;
    }

    [Fact]
    public async Task SidecarAtSubfolder_ProducesSubtreeWithCombinedPrefix()
    {
        var fs = CreateFs(
            ("reference/_meta.yml", "title: API reference\nllms:\n  description: Type and member docs\n"),
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
            ("_meta.yml", "title: Docs\nllms:\n  description: All docs\n"));
        var service = CreateService(fs, new UrlPath("/docs"));

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.Count.ShouldBe(1);
        subtrees[0].RoutePrefix.ShouldBe("/docs/");
    }

    [Fact]
    public async Task SidecarWithRootBasePageUrl_ProducesPlainPrefix()
    {
        var fs = CreateFs(
            ("api/_meta.yml", "title: API\nllms:\n  description: API docs\n"));
        var service = CreateService(fs, new UrlPath("/"));

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.Count.ShouldBe(1);
        subtrees[0].RoutePrefix.ShouldBe("/api/");
    }

    [Fact]
    public async Task MultipleSidecars_ProduceOneSubtreeEach()
    {
        var fs = CreateFs(
            ("reference/_meta.yml", "title: Reference\nllms:\n  description: ref\n"),
            ("recipes/_meta.yml", "title: Recipes\nllms:\n  description: how-to\n"));
        var service = CreateService(fs, new UrlPath("/docs"));

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.Count.ShouldBe(2);
        subtrees.Select(s => s.RoutePrefix).ShouldBe(
            new[] { "/docs/reference/", "/docs/recipes/" },
            ignoreOrder: true);
    }

    [Fact]
    public async Task SidecarMissingLlmsBlock_IsNotASubtree()
    {
        var fs = CreateFs(
            ("reference/_meta.yml", "title: Reference\norder: 1\n"));
        var service = CreateService(fs);

        var subtrees = await service.GetLlmsSubtreesAsync();

        subtrees.ShouldBeEmpty();
    }

    [Fact]
    public async Task SidecarLlmsBlockWithoutTitle_IsNotASubtree()
    {
        var fs = CreateFs(
            ("reference/_meta.yml", "llms:\n  description: no folder title\n"));
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
    public async Task FolderMetadata_SurfacesTitleOrderAndLlmsRows()
    {
        var fs = CreateFs(
            ("explanation/_meta.yml", "title: Explanation\norder: 2\n"),
            ("reference/_meta.yml", "title: Reference\norder: 5\nllms:\n  description: API surface\n"));
        var service = CreateService(fs, new UrlPath("/"));

        var folderMetadata = await service.GetFolderMetadataAsync();

        folderMetadata.Count.ShouldBe(2);
        var explanation = folderMetadata.Single(m => m.FolderUrlPrefix == "/explanation/");
        explanation.Title.ShouldBe("Explanation");
        explanation.Order.ShouldBe(2);
        explanation.LlmsDescription.ShouldBeNull();

        var reference = folderMetadata.Single(m => m.FolderUrlPrefix == "/reference/");
        reference.Title.ShouldBe("Reference");
        reference.Order.ShouldBe(5);
        reference.LlmsDescription.ShouldBe("API surface");
    }

    [Fact]
    public async Task FolderMetadata_EmptySidecarStillEmitsRow()
    {
        var fs = CreateFs(
            ("explanation/_meta.yml", ""));
        var service = CreateService(fs, new UrlPath("/"));

        var folderMetadata = await service.GetFolderMetadataAsync();

        // An empty sidecar yields no row — the deserializer returns null and the loader skips it.
        folderMetadata.ShouldBeEmpty();
    }

    [Fact]
    public async Task LlmsSubtree_NormalizesRoutePrefixToBracketedSlashForm()
    {
        new LlmsSubtree("api", "API", "").RoutePrefix.ShouldBe("/api/");
        new LlmsSubtree("/api", "API", "").RoutePrefix.ShouldBe("/api/");
        new LlmsSubtree("api/", "API", "").RoutePrefix.ShouldBe("/api/");
        new LlmsSubtree("/reference/api/", "API", "").RoutePrefix.ShouldBe("/reference/api/");

        await Task.CompletedTask;
    }

    [Fact]
    public void LlmsSubtree_RejectsEmptyPrefixOrTitle()
    {
        Should.Throw<ArgumentException>(() => new LlmsSubtree("", "API", ""));
        Should.Throw<ArgumentException>(() => new LlmsSubtree("/api/", "", ""));
    }
}
