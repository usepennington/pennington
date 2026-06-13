using Pennington.Favicon;

namespace Pennington.Tests.Favicon;

/// <summary>Unit tests for <see cref="FaviconMimeTypes.InferFromHref"/>.</summary>
public class FaviconMimeTypesTests
{
    [Theory]
    [InlineData("/favicon.ico", "image/x-icon")]
    [InlineData("/icon.png", "image/png")]
    [InlineData("/icon.svg", "image/svg+xml")]
    [InlineData("/icon.gif", "image/gif")]
    [InlineData("/icon.webp", "image/webp")]
    public void InfersKnownExtensions(string href, string expected)
        => FaviconMimeTypes.InferFromHref(href).ShouldBe(expected);

    [Theory]
    [InlineData("/site.webmanifest")]
    [InlineData("/icon")]
    [InlineData("/path/no-extension/")]
    public void ReturnsNull_ForUnknownOrAbsentExtension(string href)
        => FaviconMimeTypes.InferFromHref(href).ShouldBeNull();

    [Fact]
    public void StripsQueryString()
        => FaviconMimeTypes.InferFromHref("/favicon.ico?v=2").ShouldBe("image/x-icon");

    [Fact]
    public void StripsFragment()
        => FaviconMimeTypes.InferFromHref("/icon.svg#frag").ShouldBe("image/svg+xml");

    [Fact]
    public void IsCaseInsensitive()
        => FaviconMimeTypes.InferFromHref("/ICON.PNG").ShouldBe("image/png");
}
