using Penn.Routing;

namespace Penn.Tests.Routing;

public class ContentRouteTests
{
    [Fact]
    public void WithBaseUrl_CombinesCorrectly()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/getting-started/"),
            OutputFile = new FilePath("docs/getting-started/index.html")
        };

        var result = route.WithBaseUrl(new UrlPath("/mysite"));
        result.Value.ShouldBe("/mysite/docs/getting-started/");
    }

    [Fact]
    public void AbsoluteUrl_CombinesCorrectly()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/getting-started/"),
            OutputFile = new FilePath("docs/getting-started/index.html")
        };

        var result = route.AbsoluteUrl(new UrlPath("https://example.com"));
        result.Value.ShouldBe("https://example.com/docs/getting-started/");
    }

    [Fact]
    public void IsDefaultLocale_EmptyLocale_ReturnsTrue()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/"),
            OutputFile = new FilePath("docs/index.html")
        };

        route.IsDefaultLocale.ShouldBeTrue();
    }

    [Fact]
    public void IsDefaultLocale_WithLocale_ReturnsFalse()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/fr/docs/"),
            OutputFile = new FilePath("fr/docs/index.html"),
            Locale = "fr"
        };

        route.IsDefaultLocale.ShouldBeFalse();
    }

}
