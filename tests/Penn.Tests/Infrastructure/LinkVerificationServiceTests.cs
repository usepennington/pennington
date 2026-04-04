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

    // --- 10. mailto: and tel: links are external ---

    [Fact]
    public void MailtoLink_ClassifiedAsExternal()
    {
        var service = new LinkVerificationService([]);
        var source = MakeRoute("/contact");
        var html = """<a href="mailto:user@example.com">Email us</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ExternalLink).ShouldBeTrue();
        var external = results[0] switch { ExternalLink e => e, _ => null };
        external.ShouldNotBeNull();
        external.Url.ShouldBe("mailto:user@example.com");
    }

    [Fact]
    public void TelLink_ClassifiedAsExternal()
    {
        var service = new LinkVerificationService([]);
        var source = MakeRoute("/contact");
        var html = """<a href="tel:+1234567890">Call us</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ExternalLink).ShouldBeTrue();
    }

    // --- 11. Protocol-relative links are external ---

    [Fact]
    public void ProtocolRelativeLink_ClassifiedAsExternal()
    {
        var service = new LinkVerificationService([]);
        var source = MakeRoute("/page");
        var html = """<a href="//cdn.example.com/lib.js">CDN</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ExternalLink).ShouldBeTrue();
    }

    // --- 12. index.html normalization ---

    [Fact]
    public void LinkToIndexHtml_MatchesKnownRoute()
    {
        var routes = new[] { MakeRoute("/docs/intro") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """<a href="/docs/intro/index.html">Intro</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
    }

    // --- 13. Case-insensitive path matching ---

    [Fact]
    public void CaseInsensitivePath_MatchesKnownRoute()
    {
        var routes = new[] { MakeRoute("/docs/Getting-Started") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """<a href="/docs/getting-started">Link</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
    }

    // --- 14. Empty HTML produces no results ---

    [Fact]
    public void EmptyHtml_ReturnsEmptyResults()
    {
        var service = new LinkVerificationService([MakeRoute("/page")]);
        var source = MakeRoute("/page");

        var results = service.VerifyLinks(source, "");

        results.ShouldBeEmpty();
    }

    [Fact]
    public void HtmlWithNoLinks_ReturnsEmptyResults()
    {
        var service = new LinkVerificationService([MakeRoute("/page")]);
        var source = MakeRoute("/page");
        var html = "<p>Just some text, no links here.</p>";

        var results = service.VerifyLinks(source, html);

        results.ShouldBeEmpty();
    }

    // --- 15. Link and image src in same page ---

    [Fact]
    public void PageWithLinksAndImages_BothChecked()
    {
        var routes = new[] { MakeRoute("/docs/intro") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """
            <a href="/docs/intro">Intro</a>
            <img src="/images/logo.png">
            <a href="/docs/missing">Missing</a>
            """;

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(3);

        // href attributes are extracted first, then src attributes
        // Result 0: /docs/intro (valid link)
        (results[0] is ValidLink).ShouldBeTrue();
        // Result 1: /docs/missing (broken internal link — href extracted before src)
        var brokenLink = results[1] switch { BrokenLinkResult b => b, _ => null };
        brokenLink.ShouldNotBeNull();
        brokenLink.Type.ShouldBe(LinkType.Internal);
        brokenLink.Url.ShouldBe("/docs/missing");
        // Result 2: /images/logo.png (broken image — src extracted after all href)
        var brokenImg = results[2] switch { BrokenLinkResult b => b, _ => null };
        brokenImg.ShouldNotBeNull();
        brokenImg.Type.ShouldBe(LinkType.Image);
        brokenImg.Url.ShouldBe("/images/logo.png");
    }

    // --- 16. Link with both query string and fragment ---

    [Fact]
    public void LinkWithQueryAndHash_StrippedBeforeChecking()
    {
        var routes = new[] { MakeRoute("/docs/page") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/index");
        var html = """<a href="/docs/page?tab=install#step-1">Page</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
    }

    // --- 17. Root path link ---

    [Fact]
    public void RootLink_ValidIfRootRouteExists()
    {
        var routes = new[] { MakeRoute("/") };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/page");
        var html = """<a href="/">Home</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
    }

    // --- 18. Multiple broken links all reported ---

    [Fact]
    public void MultipleBrokenLinks_AllReported()
    {
        var service = new LinkVerificationService([]);
        var source = MakeRoute("/page");
        var html = """
            <a href="/missing-1">One</a>
            <a href="/missing-2">Two</a>
            <a href="/missing-3">Three</a>
            """;

        var results = service.VerifyLinks(source, html);

        var brokenCount = results.Count(r => r is BrokenLinkResult);
        brokenCount.ShouldBe(3);
    }

    // --- 19. Large real-world page ---

    [Fact]
    public void RealisticDocPage_MixedLinkTypes()
    {
        var routes = new[]
        {
            MakeRoute("/docs/getting-started"),
            MakeRoute("/docs/configuration"),
            MakeRoute("/docs/api-reference"),
        };
        var service = new LinkVerificationService(routes);
        var source = MakeRoute("/docs/getting-started");
        var html = """
            <nav>
                <a href="/docs/configuration">Configuration</a>
                <a href="/docs/api-reference">API</a>
                <a href="/docs/deployment">Deploy</a>
            </nav>
            <article>
                <h2 id="intro">Introduction</h2>
                <p>See <a href="#intro">above</a> for context.</p>
                <p>Visit <a href="https://github.com/example">GitHub</a>.</p>
                <img src="/images/diagram.png" alt="Architecture">
                <p>Contact us at <a href="mailto:support@example.com">support</a>.</p>
            </article>
            """;

        var results = service.VerifyLinks(source, html);

        // 2 valid internal links (/docs/configuration, /docs/api-reference)
        // 1 broken internal link (/docs/deployment)
        // 1 valid anchor link (#intro)
        // 1 external link (https://github.com/example)
        // 1 broken image (/images/diagram.png)
        // 1 external mailto
        var valid = results.Count(r => r is ValidLink);
        var broken = results.Count(r => r is BrokenLinkResult);
        var external = results.Count(r => r is ExternalLink);

        valid.ShouldBe(3); // 2 internal + 1 anchor
        broken.ShouldBe(2); // 1 missing page + 1 missing image
        external.ShouldBe(2); // github + mailto
    }
}
