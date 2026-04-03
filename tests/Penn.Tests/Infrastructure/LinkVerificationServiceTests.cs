using Penn.Generation;
using Penn.Infrastructure;
using Penn.Routing;

namespace Penn.Tests.Infrastructure;

public class LinkVerificationServiceTests
{
    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    // --- 1. Valid internal link ---

    [Fact]
    public void ValidInternalLink_ReturnsValidLink()
    {
        var routes = new[] { MakeRoute("/docs/getting-started") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/index");
        var html = """<a href="/docs/getting-started">Getting Started</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
        var valid = results[0] switch { ValidLink v => v, _ => null };
        valid.ShouldNotBeNull();
        valid.Url.ShouldBe("/docs/getting-started");
        valid.SourcePage.ShouldBe(source);
    }

    // --- 2. Broken internal link ---

    [Fact]
    public void BrokenInternalLink_ReturnsBrokenLinkResult()
    {
        var routes = new[] { MakeRoute("/docs/getting-started") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/index");
        var html = """<a href="/docs/nonexistent">Missing Page</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is BrokenLinkResult).ShouldBeTrue();
        var broken = results[0] switch { BrokenLinkResult b => b, _ => null };
        broken.ShouldNotBeNull();
        broken.Url.ShouldBe("/docs/nonexistent");
        broken.Type.ShouldBe(LinkType.Internal);
        broken.Reason.ShouldBe("Page not found");
        broken.SourcePage.ShouldBe(source);
    }

    // --- 3. External link classified correctly ---

    [Fact]
    public void ExternalLink_ReturnsExternalLink()
    {
        var routes = Array.Empty<ContentRoute>();
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """<a href="https://example.com">Example</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ExternalLink).ShouldBeTrue();
        var external = results[0] switch { ExternalLink e => e, _ => null };
        external.ShouldNotBeNull();
        external.Url.ShouldBe("https://example.com");
        external.SourcePage.ShouldBe(source);
    }

    // --- 4. Anchor link valid ---

    [Fact]
    public void AnchorLink_ReturnsValidLink()
    {
        var routes = Array.Empty<ContentRoute>();
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """<a href="#section-1">Jump to section</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
        var valid = results[0] switch { ValidLink v => v, _ => null };
        valid.ShouldNotBeNull();
        valid.Url.ShouldBe("#section-1");
    }

    // --- 5. Image src checked ---

    [Fact]
    public void ImageSrc_MissingRoute_ReturnsBrokenLinkWithImageType()
    {
        var routes = Array.Empty<ContentRoute>();
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """<img src="/images/missing.png">""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is BrokenLinkResult).ShouldBeTrue();
        var broken = results[0] switch { BrokenLinkResult b => b, _ => null };
        broken.ShouldNotBeNull();
        broken.Url.ShouldBe("/images/missing.png");
        broken.Type.ShouldBe(LinkType.Image);
        broken.Reason.ShouldBe("Page not found");
    }

    // --- 6. Multiple links in one page ---

    [Fact]
    public void MultipleLinks_MixedResults()
    {
        var routes = new[] { MakeRoute("/docs/intro") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """
            <a href="/docs/intro">Intro</a>
            <a href="/docs/missing">Missing</a>
            <a href="https://github.com">GitHub</a>
            """;

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(3);

        // Valid internal link
        (results[0] is ValidLink).ShouldBeTrue();
        var valid = results[0] switch { ValidLink v => v, _ => null };
        valid.ShouldNotBeNull();
        valid.Url.ShouldBe("/docs/intro");

        // Broken internal link
        (results[1] is BrokenLinkResult).ShouldBeTrue();
        var broken = results[1] switch { BrokenLinkResult b => b, _ => null };
        broken.ShouldNotBeNull();
        broken.Url.ShouldBe("/docs/missing");

        // External link
        (results[2] is ExternalLink).ShouldBeTrue();
        var external = results[2] switch { ExternalLink e => e, _ => null };
        external.ShouldNotBeNull();
        external.Url.ShouldBe("https://github.com");
    }

    // --- 7. Link with query string ---

    [Fact]
    public void LinkWithQueryString_StrippedBeforeChecking()
    {
        var routes = new[] { MakeRoute("/docs/page") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/index");
        var html = """<a href="/docs/page?v=2">Page</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
        var valid = results[0] switch { ValidLink v => v, _ => null };
        valid.ShouldNotBeNull();
        valid.Url.ShouldBe("/docs/page?v=2");
    }

    // --- 8. Link with hash ---

    [Fact]
    public void LinkWithHash_StrippedBeforeChecking()
    {
        var routes = new[] { MakeRoute("/docs/page") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/index");
        var html = """<a href="/docs/page#section">Page Section</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
        var valid = results[0] switch { ValidLink v => v, _ => null };
        valid.ShouldNotBeNull();
        valid.Url.ShouldBe("/docs/page#section");
    }

    // --- 9. Trailing slash tolerance ---

    [Fact]
    public void TrailingSlashTolerance_MatchesKnownRoute()
    {
        var routes = new[] { MakeRoute("/docs/page") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/index");
        var html = """<a href="/docs/page/">Page</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
        var valid = results[0] switch { ValidLink v => v, _ => null };
        valid.ShouldNotBeNull();
        valid.Url.ShouldBe("/docs/page/");
    }
}
