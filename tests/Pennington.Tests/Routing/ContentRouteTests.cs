using Pennington.Routing;

namespace Pennington.Tests.Routing;

public class ContentRouteTests
{
    private static ContentRoute Route(string canonicalPath) => new()
    {
        CanonicalPath = new UrlPath(canonicalPath),
        OutputFile = new FilePath("_"),
    };

    // --- AbsoluteUrl with plain-path canonical base (/ or /preview/) ---
    // Path-mode composition. Covers the common dev / publish base-URL cases.

    [Fact]
    public void AbsoluteUrl_RootPathBase_WithNonRootRoute_ComposesPath()
    {
        var url = Route("/about/").AbsoluteUrl(new UrlPath("/"));
        url.Value.ShouldBe("/about/");
    }

    [Fact]
    public void AbsoluteUrl_PrefixedPathBase_WithNonRootRoute_ComposesPath()
    {
        var url = Route("/about/").AbsoluteUrl(new UrlPath("/preview/"));
        url.Value.ShouldBe("/preview/about/");
    }

    [Fact]
    public void AbsoluteUrl_PrefixedPathBase_WithRootRoute_ComposesPath()
    {
        var url = Route("/").AbsoluteUrl(new UrlPath("/preview/"));
        url.Value.ShouldBe("/preview");
    }

    // --- AbsoluteUrl with absolute URL canonical base (https://site.com) ---
    // URI-mode composition. This is the case that used to produce
    // `/https://site.com` for the root route and was the Phase 5 bug.

    [Fact]
    public void AbsoluteUrl_AbsoluteUrlBase_WithRootRoute_ComposesWellFormedUrl()
    {
        var url = Route("/").AbsoluteUrl(new UrlPath("https://beacon-docs.example.com"));
        url.Value.ShouldBe("https://beacon-docs.example.com/");
    }

    [Fact]
    public void AbsoluteUrl_AbsoluteUrlBase_WithTrailingSlash_WithRootRoute_ComposesWellFormedUrl()
    {
        var url = Route("/").AbsoluteUrl(new UrlPath("https://beacon-docs.example.com/"));
        url.Value.ShouldBe("https://beacon-docs.example.com/");
    }

    [Fact]
    public void AbsoluteUrl_AbsoluteUrlBase_WithNonRootRoute_ComposesWellFormedUrl()
    {
        var url = Route("/about/").AbsoluteUrl(new UrlPath("https://beacon-docs.example.com"));
        url.Value.ShouldBe("https://beacon-docs.example.com/about/");
    }

    [Fact]
    public void AbsoluteUrl_AbsoluteUrlBase_WithTrailingSlash_WithNonRootRoute_ComposesWellFormedUrl()
    {
        var url = Route("/about/").AbsoluteUrl(new UrlPath("https://beacon-docs.example.com/"));
        url.Value.ShouldBe("https://beacon-docs.example.com/about/");
    }

    [Fact]
    public void AbsoluteUrl_AbsoluteUrlBase_WithDeepRoute_ComposesWellFormedUrl()
    {
        var url = Route("/docs/getting-started/").AbsoluteUrl(new UrlPath("https://site.example.com"));
        url.Value.ShouldBe("https://site.example.com/docs/getting-started/");
    }

    [Fact]
    public void AbsoluteUrl_HttpScheme_Supported()
    {
        var url = Route("/").AbsoluteUrl(new UrlPath("http://site.example.com"));
        url.Value.ShouldBe("http://site.example.com/");
    }

    [Fact]
    public void AbsoluteUrl_UrlWithPort_Supported()
    {
        var url = Route("/about/").AbsoluteUrl(new UrlPath("https://site.example.com:8080"));
        url.Value.ShouldBe("https://site.example.com:8080/about/");
    }
}
