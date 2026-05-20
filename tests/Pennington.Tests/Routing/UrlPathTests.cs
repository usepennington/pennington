using Pennington.Routing;

namespace Pennington.Tests.Routing;

public class UrlPathTests
{
    [Fact]
    public void DivisionOperator_CombinesTwoPaths()
    {
        var left = new UrlPath("/blog");
        var right = new UrlPath("post");
        var result = left / right;
        result.Value.ShouldBe("/blog/post");
    }

    [Fact]
    public void DivisionOperator_LeftHasTrailingSlash_CombinesCorrectly()
    {
        var left = new UrlPath("/blog/");
        var right = new UrlPath("post");
        var result = left / right;
        result.Value.ShouldBe("/blog/post");
    }

    [Fact]
    public void DivisionOperator_RightHasLeadingSlash_CombinesCorrectly()
    {
        var left = new UrlPath("/blog");
        var right = new UrlPath("/post");
        var result = left / right;
        result.Value.ShouldBe("/blog/post");
    }

    [Fact]
    public void DivisionOperator_BothHaveSlashes_CombinesCorrectly()
    {
        var left = new UrlPath("/blog/");
        var right = new UrlPath("/post");
        var result = left / right;
        result.Value.ShouldBe("/blog/post");
    }

    [Fact]
    public void DivisionOperator_EmptyLeft_ReturnsRightWithLeadingSlash()
    {
        var left = new UrlPath("");
        var right = new UrlPath("post");
        var result = left / right;
        result.Value.ShouldBe("/post");
    }

    [Fact]
    public void DivisionOperator_EmptyRight_ReturnsLeftWithLeadingSlash()
    {
        var left = new UrlPath("/blog");
        var right = new UrlPath("");
        var result = left / right;
        result.Value.ShouldBe("/blog");
    }

    [Fact]
    public void DivisionOperator_LeftWithoutLeadingSlash_AddsLeadingSlash()
    {
        var left = new UrlPath("blog");
        var right = new UrlPath("");
        var result = left / right;
        result.Value.ShouldBe("/blog");
    }

    [Fact]
    public void DivisionOperator_MultipleSegments()
    {
        var result = new UrlPath("/a") / new UrlPath("b") / new UrlPath("c");
        result.Value.ShouldBe("/a/b/c");
    }

    [Fact]
    public void RemoveTrailingSlash_RootSlash_PreservesIt()
    {
        var path = new UrlPath("/");
        path.RemoveTrailingSlash().Value.ShouldBe("/");
    }

    [Fact]
    public void Matches_SamePath_ReturnsTrue()
    {
        var a = new UrlPath("/blog/post");
        var b = new UrlPath("/blog/post");
        a.Matches(b).ShouldBeTrue();
    }

    [Fact]
    public void Matches_TrailingSlashTolerance_ReturnsTrue()
    {
        var a = new UrlPath("/blog/post");
        var b = new UrlPath("/blog/post/");
        a.Matches(b).ShouldBeTrue();
    }

    [Fact]
    public void Matches_IndexHtmlVariant_ReturnsTrue()
    {
        var a = new UrlPath("/blog/post");
        var b = new UrlPath("/blog/post/index.html");
        a.Matches(b).ShouldBeTrue();
    }

    [Fact]
    public void Matches_IndexHtmVariant_ReturnsTrue()
    {
        var a = new UrlPath("/blog/post");
        var b = new UrlPath("/blog/post/index.htm");
        a.Matches(b).ShouldBeTrue();
    }

    [Fact]
    public void Matches_CaseInsensitive_ReturnsTrue()
    {
        var a = new UrlPath("/Blog/Post");
        var b = new UrlPath("/blog/post");
        a.Matches(b).ShouldBeTrue();
    }

    [Fact]
    public void Matches_DifferentPaths_ReturnsFalse()
    {
        var a = new UrlPath("/blog/post");
        var b = new UrlPath("/blog/other");
        a.Matches(b).ShouldBeFalse();
    }

    [Fact]
    public void Matches_RootPaths_ReturnsTrue()
    {
        var a = new UrlPath("/");
        var b = new UrlPath("/index.html");
        a.Matches(b).ShouldBeTrue();
    }

    [Fact]
    public void Matches_TrailingSlashAndIndexHtml_BothMatch()
    {
        var a = new UrlPath("/blog/");
        var b = new UrlPath("/blog/index.html");
        a.Matches(b).ShouldBeTrue();
    }

    [Theory]
    [InlineData("/blog/post", "/BLOG/POST/")]
    [InlineData("/about", "/About/index.html")]
    [InlineData("/", "/index.htm")]
    public void Matches_VariousTolerantCombinations_ReturnsTrue(string pathA, string pathB)
    {
        var a = new UrlPath(pathA);
        var b = new UrlPath(pathB);
        a.Matches(b).ShouldBeTrue();
    }
}