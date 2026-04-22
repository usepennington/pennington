using Pennington.Routing;

namespace Pennington.Tests.Routing;

public class UrlComposerTests
{
    [Fact]
    public void Combine_RootPathBase_WithAbsolutePath_ReturnsPathUnchanged()
    {
        var url = UrlComposer.Combine(new UrlPath("/"), new UrlPath("/_llms/page.md"));
        url.Value.ShouldBe("/_llms/page.md");
    }

    [Fact]
    public void Combine_PrefixedPathBase_ComposesWithPrefix()
    {
        var url = UrlComposer.Combine(new UrlPath("/sub/"), new UrlPath("/_llms/page.md"));
        url.Value.ShouldBe("/sub/_llms/page.md");
    }

    [Fact]
    public void Combine_AbsoluteHttpsBase_ProducesFullyQualifiedUrl()
    {
        var url = UrlComposer.Combine(new UrlPath("https://docs.example.com"), new UrlPath("/_llms/page.md"));
        url.Value.ShouldBe("https://docs.example.com/_llms/page.md");
    }

    [Fact]
    public void Combine_AbsoluteHttpsBase_WithTrailingSlash_ProducesFullyQualifiedUrl()
    {
        var url = UrlComposer.Combine(new UrlPath("https://docs.example.com/"), new UrlPath("/_llms/page.md"));
        url.Value.ShouldBe("https://docs.example.com/_llms/page.md");
    }

    [Fact]
    public void Combine_AbsoluteHttpsBase_WithRootPath_LeavesRootSlash()
    {
        var url = UrlComposer.Combine(new UrlPath("https://docs.example.com"), new UrlPath("/"));
        url.Value.ShouldBe("https://docs.example.com/");
    }

    [Fact]
    public void Combine_HttpScheme_Supported()
    {
        var url = UrlComposer.Combine(new UrlPath("http://localhost:5000"), new UrlPath("/_llms/page.md"));
        url.Value.ShouldBe("http://localhost:5000/_llms/page.md");
    }

    [Fact]
    public void CanonicalBaseUrl_Combine_DelegatesToUrlComposer()
    {
        var baseUrl = new CanonicalBaseUrl(new UrlPath("https://site.com"));
        baseUrl.Combine(new UrlPath("/_llms/a/b.md")).Value.ShouldBe("https://site.com/_llms/a/b.md");
    }

    [Fact]
    public void CanonicalBaseUrl_PathOnlyBase_FallsBackToRootRelative()
    {
        var baseUrl = new CanonicalBaseUrl(new UrlPath("/"));
        baseUrl.Combine(new UrlPath("/_llms/a/b.md")).Value.ShouldBe("/_llms/a/b.md");
    }
}
