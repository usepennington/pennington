using Pennington.Content;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Content;

public class ContentRootAssetServiceTests
{
    private static MockFileSystem CreateFs(params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");
        foreach (var (path, content) in files)
        {
            var fullPath = $"/content/{path}";
            var dir = fs.Path.GetDirectoryName(fullPath);
            if (dir != null)
            {
                fs.Directory.CreateDirectory(dir);
            }

            fs.File.WriteAllText(fullPath, content);
        }

        return fs;
    }

    [Fact]
    public async Task CopiesAssetsOutsideAnySourceAtContentRootRelativePath()
    {
        // The bug: shared assets at the content root (outside any markdown source) were served
        // live but never copied by the build.
        var fs = CreateFs(("assets/table.svg", "<svg/>"));
        var service = new ContentRootAssetService("/content", fs);

        var toCopy = await service.GetContentToCopyAsync();

        toCopy.Count.ShouldBe(1);
        toCopy[0].OutputPath.Value.ShouldBe("assets/table.svg");
    }

    [Fact]
    public async Task ExcludesSourceFiles()
    {
        // Mirrors the runtime mount's ServeUnknownFileTypes = false: source files have no
        // registered content type, so they are not served — and must not be copied either.
        var fs = CreateFs(
            ("page.md", "# Markdown"),
            ("page.mdx", "# MDX"),
            ("page.razor", "<div/>"),
            ("_meta.yml", "title: X"),
            ("data.yaml", "k: v"),
            ("assets/logo.png", "png"),
            ("assets/app.js", "code"),
            ("assets/site.css", "css"));
        var service = new ContentRootAssetService("/content", fs);

        var outputs = (await service.GetContentToCopyAsync()).Select(c => c.OutputPath.Value).ToList();

        outputs.ShouldBe(["assets/app.js", "assets/logo.png", "assets/site.css"], ignoreOrder: true);
    }

    [Fact]
    public async Task ExcludesDotPrefixedSegments()
    {
        // The runtime mount (new PhysicalFileProvider) defaults to ExclusionFilters.Sensitive, which
        // 404s dot-prefixed paths — even ones with a known content type (.well-known/security.txt).
        var fs = CreateFs(
            (".well-known/security.txt", "contact: x"),
            (".git/config", "[core]"),
            ("assets/logo.png", "png"));
        var service = new ContentRootAssetService("/content", fs);

        var outputs = (await service.GetContentToCopyAsync()).Select(c => c.OutputPath.Value).ToList();

        outputs.ShouldBe(["assets/logo.png"]);
    }

    [Fact]
    public async Task ExcludesFilesWithNoKnownContentType()
    {
        // ServeUnknownFileTypes = false parity: an unmapped extension is 404 at runtime, so the
        // build must not copy it.
        var fs = CreateFs(("assets/data.weirdext", "blob"));
        var service = new ContentRootAssetService("/content", fs);

        (await service.GetContentToCopyAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task NonExistentRoot_ReturnsEmpty()
    {
        var fs = new MockFileSystem();
        var service = new ContentRootAssetService("/nonexistent", fs);

        (await service.GetContentToCopyAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task CarriesNoRoutesNavOrXrefs()
    {
        var fs = CreateFs(("assets/logo.png", "png"));
        var service = new ContentRootAssetService("/content", fs);

        var discovered = new List<Pennington.Pipeline.DiscoveredItem>();
        await foreach (var item in service.DiscoverAsync())
        {
            discovered.Add(item);
        }

        discovered.ShouldBeEmpty();
        (await service.GetContentTocEntriesAsync()).ShouldBeEmpty();
        (await service.GetCrossReferencesAsync()).ShouldBeEmpty();
        (await service.GetContentToCreateAsync()).ShouldBeEmpty();
    }
}
