using Penn.Routing;

namespace Penn.Tests.Routing;

public class ContentRouteTests
{
    [Fact]
    public void NavigationPath_AddsTrailingSlash()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/getting-started"),
            OutputFile = new FilePath("docs/getting-started/index.html")
        };

        route.NavigationPath.Value.ShouldBe("/docs/getting-started/");
    }

    [Fact]
    public void NavigationPath_AlreadyHasTrailingSlash_ReturnsSame()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/"),
            OutputFile = new FilePath("docs/index.html")
        };

        route.NavigationPath.Value.ShouldBe("/docs/");
    }

    [Fact]
    public void WithBaseUrl_CombinesCorrectly()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/getting-started"),
            OutputFile = new FilePath("docs/getting-started/index.html")
        };

        var result = route.WithBaseUrl(new UrlPath("/mysite"));
        result.Value.ShouldBe("/mysite/docs/getting-started");
    }

    [Fact]
    public void AbsoluteUrl_CombinesCorrectly()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/getting-started"),
            OutputFile = new FilePath("docs/getting-started/index.html")
        };

        var result = route.AbsoluteUrl(new UrlPath("https://example.com"));
        result.Value.ShouldBe("https://example.com/docs/getting-started");
    }

    [Fact]
    public void IsDefaultLocale_EmptyLocale_ReturnsTrue()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs"),
            OutputFile = new FilePath("docs/index.html")
        };

        route.IsDefaultLocale.ShouldBeTrue();
    }

    [Fact]
    public void IsDefaultLocale_WithLocale_ReturnsFalse()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/fr/docs"),
            OutputFile = new FilePath("fr/docs/index.html"),
            Locale = "fr"
        };

        route.IsDefaultLocale.ShouldBeFalse();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs"),
            OutputFile = new FilePath("docs/index.html"),
            Locale = "en"
        };

        var b = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs"),
            OutputFile = new FilePath("docs/index.html"),
            Locale = "en"
        };

        a.ShouldBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var a = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs"),
            OutputFile = new FilePath("docs/index.html")
        };

        var b = new ContentRoute
        {
            CanonicalPath = new UrlPath("/blog"),
            OutputFile = new FilePath("blog/index.html")
        };

        a.ShouldNotBe(b);
    }

    [Fact]
    public void SourceFile_WhenSet_ReturnsValue()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs"),
            OutputFile = new FilePath("docs/index.html"),
            SourceFile = new FilePath("Content/index.md")
        };

        route.SourceFile.ShouldNotBeNull();
        route.SourceFile.Value.Value.ShouldBe("Content/index.md");
    }

    [Fact]
    public void SourceFile_WhenNotSet_IsNull()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs"),
            OutputFile = new FilePath("docs/index.html")
        };

        route.SourceFile.ShouldBeNull();
    }
}
