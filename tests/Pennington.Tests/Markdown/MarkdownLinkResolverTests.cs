using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Markdown;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Markdown;

public class MarkdownLinkResolverTests
{
    private static readonly LocalizationOptions DefaultLocalization = new();

    private static MockFileSystem CreateFs(string rootPath, params (string Path, string Content)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory(rootPath);
        foreach (var (path, content) in files)
        {
            var fullPath = $"{rootPath}/{path}";
            var dir = fs.Path.GetDirectoryName(fullPath);
            if (dir != null)
            {
                fs.Directory.CreateDirectory(dir);
            }

            fs.File.WriteAllText(fullPath, content);
        }
        return fs;
    }

    private static MarkdownContentService<DocFrontMatter> CreateService(
        MockFileSystem fs, string contentPath = "/content", string basePageUrl = "/")
    {
        var options = new MarkdownContentServiceOptions
        {
            ContentPath = new FilePath(contentPath),
            BasePageUrl = new UrlPath(basePageUrl),
        };
        return new MarkdownContentService<DocFrontMatter>(
            options, new FrontMatterParser(), fs, DefaultLocalization);
    }

    [Fact]
    public async Task ResolveAsync_RewritesDotMdSuffixedLink()
    {
        var fs = CreateFs("/content",
            ("tutorials/interactive-prompt-and-dashboard-tutorial.md", "---\ntitle: Interactive\n---\nbody"),
            ("how-to/organizing-layout.md", "---\ntitle: Organizing\n---\nbody"));
        var service = CreateService(fs, "/content", "/console");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/tutorials/interactive-prompt-and-dashboard-tutorial.md");

        var result = await resolver.ResolveAsync(source, "../how-to/organizing-layout.md");

        result.ShouldBe("/console/how-to/organizing-layout/");
    }

    [Fact]
    public async Task ResolveAsync_RewritesBareSiblingReference()
    {
        // Reproduces MinimalExample: sub-folder/page-one.md linking to "sample-post"
        // (no extension, bare sibling) should resolve to /sub-folder/sample-post/.
        var fs = CreateFs("/content",
            ("sub-folder/page-one.md", "---\ntitle: Page One\n---\nbody"),
            ("sub-folder/sample-post.md", "---\ntitle: Sample\n---\nbody"));
        var service = CreateService(fs, "/content", "/");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/sub-folder/page-one.md");

        var result = await resolver.ResolveAsync(source, "sample-post");

        result.ShouldBe("/sub-folder/sample-post/");
    }

    [Fact]
    public async Task ResolveAsync_RewritesDotDotParentLink()
    {
        // Reproduces MinimalExample: sub-folder/page-two.md linking to "../"
        // (home page) should resolve to "/".
        var fs = CreateFs("/content",
            ("index.md", "---\ntitle: Home\n---\nbody"),
            ("sub-folder/page-two.md", "---\ntitle: Page Two\n---\nbody"));
        var service = CreateService(fs, "/content", "/");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/sub-folder/page-two.md");

        var result = await resolver.ResolveAsync(source, "../");

        result.ShouldBe("/");
    }

    [Fact]
    public async Task ResolveAsync_RewritesRelativeAssetReference()
    {
        // Reproduces BeaconDocsExample: `./beacon-arch.png` from getting-started/index.md
        // should resolve to `/getting-started/beacon-arch.png`, not
        // `/getting-started/index/beacon-arch.png`.
        var fs = CreateFs("/content",
            ("getting-started/index.md", "---\ntitle: GS\n---\nbody"));
        var service = CreateService(fs, "/content", "/");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/getting-started/index.md");

        var result = await resolver.ResolveAsync(source, "./beacon-arch.png");

        result.ShouldBe("/getting-started/beacon-arch.png");
    }

    [Fact]
    public async Task ResolveAsync_PreservesFragment()
    {
        var fs = CreateFs("/content",
            ("foo.md", "---\ntitle: Foo\n---\nbody"),
            ("bar.md", "---\ntitle: Bar\n---\nbody"));
        var service = CreateService(fs, "/content", "/");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/foo.md");

        var result = await resolver.ResolveAsync(source, "bar.md#section");

        result.ShouldBe("/bar/#section");
    }

    [Fact]
    public async Task ResolveAsync_PreservesQueryString()
    {
        var fs = CreateFs("/content",
            ("foo.md", "---\ntitle: Foo\n---\nbody"),
            ("bar.md", "---\ntitle: Bar\n---\nbody"));
        var service = CreateService(fs, "/content", "/");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/foo.md");

        var result = await resolver.ResolveAsync(source, "bar.md?x=1");

        result.ShouldBe("/bar/?x=1");
    }

    [Fact]
    public async Task ResolveAsync_LeavesAbsoluteUrlsUntouched()
    {
        var fs = CreateFs("/content",
            ("foo.md", "---\ntitle: Foo\n---\nbody"));
        var service = CreateService(fs);
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/foo.md");

        (await resolver.ResolveAsync(source, "/absolute/path")).ShouldBeNull();
        (await resolver.ResolveAsync(source, "https://external.example.com/page")).ShouldBeNull();
        (await resolver.ResolveAsync(source, "mailto:user@example.com")).ShouldBeNull();
        (await resolver.ResolveAsync(source, "#fragment")).ShouldBeNull();
        (await resolver.ResolveAsync(source, "tel:+15551234")).ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsync_LeavesXrefLinksUntouched()
    {
        var fs = CreateFs("/content",
            ("foo.md", "---\ntitle: Foo\n---\nbody"));
        var service = CreateService(fs);
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/foo.md");

        var result = await resolver.ResolveAsync(source, "xref:some.uid");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNullForUnknownSourceFile()
    {
        var fs = CreateFs("/content",
            ("foo.md", "---\ntitle: Foo\n---\nbody"));
        var service = CreateService(fs, "/content", "/docs");
        var resolver = new MarkdownLinkResolver([service]);

        // Source outside any registered content root — asset fallback can't help.
        var source = new FilePath("/outside/bar.md");

        var result = await resolver.ResolveAsync(source, "foo");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveAsync_HonoursBasePageUrlForAssets()
    {
        // With a non-root BasePageUrl, asset resolution should include the prefix.
        var fs = CreateFs("/content",
            ("getting-started/index.md", "---\ntitle: GS\n---\nbody"));
        var service = CreateService(fs, "/content", "/docs");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/getting-started/index.md");

        var result = await resolver.ResolveAsync(source, "./beacon-arch.png");

        result.ShouldBe("/docs/getting-started/beacon-arch.png");
    }

    [Fact]
    public async Task ResolveAsync_ResolvesSiblingWithDotSlashPrefix()
    {
        var fs = CreateFs("/content",
            ("guide/intro.md", "---\ntitle: Intro\n---\nbody"),
            ("guide/next.md", "---\ntitle: Next\n---\nbody"));
        var service = CreateService(fs, "/content", "/");
        var resolver = new MarkdownLinkResolver([service]);

        var source = new FilePath("/content/guide/intro.md");

        var result = await resolver.ResolveAsync(source, "./next.md");

        result.ShouldBe("/guide/next/");
    }
}