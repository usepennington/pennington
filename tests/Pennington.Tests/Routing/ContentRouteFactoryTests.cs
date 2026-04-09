using Pennington.Routing;

namespace Pennington.Tests.Routing;

public class ContentRouteFactoryTests
{
    [Fact]
    public void FromMarkdownFile_BasicConversion()
    {
        var route = ContentRouteFactory.FromMarkdownFile(
            sourceFile: "Content/Docs/getting-started.md",
            contentRoot: "Content/Docs",
            basePageUrl: "/docs");

        route.CanonicalPath.Value.ShouldBe("/docs/getting-started/");
        route.OutputFile.Value.ShouldBe("docs/getting-started/index.html");
        route.SourceFile.ShouldNotBeNull();
        route.SourceFile.Value.Value.ShouldBe("Content/Docs/getting-started.md");
    }

    [Fact]
    public void FromMarkdownFile_IndexMd_BecomesBaseUrl()
    {
        var route = ContentRouteFactory.FromMarkdownFile(
            sourceFile: "Content/Docs/index.md",
            contentRoot: "Content/Docs",
            basePageUrl: "/docs");

        route.CanonicalPath.Value.ShouldBe("/docs/");
        route.OutputFile.Value.ShouldBe("docs/index.html");
    }

    [Fact]
    public void FromMarkdownFile_WithLocalePrefix()
    {
        var route = ContentRouteFactory.FromMarkdownFile(
            sourceFile: "Content/Docs/getting-started.md",
            contentRoot: "Content/Docs",
            basePageUrl: "/docs",
            locale: "fr");

        route.CanonicalPath.Value.ShouldBe("/fr/docs/getting-started/");
        route.OutputFile.Value.ShouldBe("fr/docs/getting-started/index.html");
        route.Locale.ShouldBe("fr");
    }

    [Fact]
    public void FromMarkdownFile_NestedPaths()
    {
        var route = ContentRouteFactory.FromMarkdownFile(
            sourceFile: "Content/Docs/guides/advanced/setup.md",
            contentRoot: "Content/Docs",
            basePageUrl: "/docs");

        route.CanonicalPath.Value.ShouldBe("/docs/guides/advanced/setup/");
        route.OutputFile.Value.ShouldBe("docs/guides/advanced/setup/index.html");
    }

    [Fact]
    public void FromMarkdownFile_IndexMd_WithLocale()
    {
        var route = ContentRouteFactory.FromMarkdownFile(
            sourceFile: "Content/Docs/index.md",
            contentRoot: "Content/Docs",
            basePageUrl: "/docs",
            locale: "de");

        route.CanonicalPath.Value.ShouldBe("/de/docs/");
        route.OutputFile.Value.ShouldBe("de/docs/index.html");
    }

    [Fact]
    public void FromRazorPage_BasicConversion()
    {
        var route = ContentRouteFactory.FromRazorPage("/about");

        route.CanonicalPath.Value.ShouldBe("/about/");
        route.OutputFile.Value.ShouldBe("about/index.html");
        route.SourceFile.ShouldBeNull();
    }

    [Fact]
    public void FromRazorPage_WithLocale()
    {
        var route = ContentRouteFactory.FromRazorPage("/about", locale: "es");

        route.CanonicalPath.Value.ShouldBe("/es/about/");
        route.OutputFile.Value.ShouldBe("es/about/index.html");
        route.Locale.ShouldBe("es");
    }

    [Fact]
    public void FromUrl_BasicConversion()
    {
        var route = ContentRouteFactory.FromUrl("/blog/my-post");

        route.CanonicalPath.Value.ShouldBe("/blog/my-post/");
        route.OutputFile.Value.ShouldBe("blog/my-post/index.html");
    }

    [Fact]
    public void FromUrl_WithLocale()
    {
        var route = ContentRouteFactory.FromUrl("/blog/my-post", locale: "ja");

        route.CanonicalPath.Value.ShouldBe("/ja/blog/my-post/");
        route.OutputFile.Value.ShouldBe("ja/blog/my-post/index.html");
        route.Locale.ShouldBe("ja");
    }

    [Fact]
    public void FromUrl_WithTrailingSlash_NormalizesToTrailingSlash()
    {
        var route = ContentRouteFactory.FromUrl("/blog/my-post/");

        route.CanonicalPath.Value.ShouldBe("/blog/my-post/");
        route.OutputFile.Value.ShouldBe("blog/my-post/index.html");
    }

    [Fact]
    public void FromCustom_WithSourceFile()
    {
        var route = ContentRouteFactory.FromCustom(
            url: "/api/data",
            sourceFile: new FilePath("data/source.json"));

        route.CanonicalPath.Value.ShouldBe("/api/data/");
        route.OutputFile.Value.ShouldBe("api/data/index.html");
        route.SourceFile.ShouldNotBeNull();
        route.SourceFile.Value.Value.ShouldBe("data/source.json");
    }

    [Fact]
    public void FromCustom_WithoutSourceFile()
    {
        var route = ContentRouteFactory.FromCustom(url: "/api/data");

        route.CanonicalPath.Value.ShouldBe("/api/data/");
        route.OutputFile.Value.ShouldBe("api/data/index.html");
        route.SourceFile.ShouldBeNull();
    }

    [Fact]
    public void FromCustom_WithLocale()
    {
        var route = ContentRouteFactory.FromCustom(
            url: "/api/data",
            locale: "pt");

        route.CanonicalPath.Value.ShouldBe("/pt/api/data/");
        route.OutputFile.Value.ShouldBe("pt/api/data/index.html");
        route.Locale.ShouldBe("pt");
    }

    [Fact]
    public void ForRedirect_BasicConversion()
    {
        var route = ContentRouteFactory.ForRedirect("/old-page");

        route.CanonicalPath.Value.ShouldBe("/old-page/");
        route.OutputFile.Value.ShouldBe("old-page/index.html");
        route.Locale.ShouldBe("");
        route.SourceFile.ShouldBeNull();
    }

    [Fact]
    public void ForRedirect_WithTrailingSlash_NormalizesToTrailingSlash()
    {
        var route = ContentRouteFactory.ForRedirect("/old-page/");

        route.CanonicalPath.Value.ShouldBe("/old-page/");
        route.OutputFile.Value.ShouldBe("old-page/index.html");
    }

    [Theory]
    [InlineData("../../etc/passwd", "Content/Docs")]
    [InlineData("Content/Docs/../../secret.md", "Content/Docs")]
    [InlineData("Content\\..\\..\\secret.md", "Content/Docs")]
    [InlineData("../outside/file.md", "Content/Docs")]
    public void FromMarkdownFile_PathTraversal_ThrowsArgumentException(string sourceFile, string contentRoot)
    {
        Should.Throw<ArgumentException>(() =>
            ContentRouteFactory.FromMarkdownFile(sourceFile, contentRoot, "/docs"));
    }

    [Fact]
    public void FromMarkdownFile_SiblingDirectoryOverlap_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            ContentRouteFactory.FromMarkdownFile(
                "Content/DocsExtra/file.md",
                "Content/Docs",
                "/docs"));
    }

    [Theory]
    [InlineData("Content/Docs/getting-started.md", "Content/Docs")]
    [InlineData("Content/Docs/guides/advanced/setup.md", "Content/Docs")]
    [InlineData("Content/Docs/index.md", "Content/Docs")]
    public void FromMarkdownFile_ValidPathWithinContentRoot_DoesNotThrow(string sourceFile, string contentRoot)
    {
        Should.NotThrow(() =>
            ContentRouteFactory.FromMarkdownFile(sourceFile, contentRoot, "/docs"));
    }
}
