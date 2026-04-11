using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class LinkVerificationServiceTests
{
    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
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
        var valid = results[0].ShouldBeCase<ValidLink>();
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
        var broken = results[0].ShouldBeCase<BrokenLinkResult>();
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
        var external = results[0].ShouldBeCase<ExternalLink>();
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
        var valid = results[0].ShouldBeCase<ValidLink>();
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
        var broken = results[0].ShouldBeCase<BrokenLinkResult>();
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
        var valid = results[0].ShouldBeCase<ValidLink>();
        valid.Url.ShouldBe("/docs/intro");

        // Broken internal link
        var broken = results[1].ShouldBeCase<BrokenLinkResult>();
        broken.Url.ShouldBe("/docs/missing");

        // External link
        var external = results[2].ShouldBeCase<ExternalLink>();
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
        var valid = results[0].ShouldBeCase<ValidLink>();
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
        var valid = results[0].ShouldBeCase<ValidLink>();
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
        var valid = results[0].ShouldBeCase<ValidLink>();
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
        var external = results[0].ShouldBeCase<ExternalLink>();
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
        var brokenLink = results[1].ShouldBeCase<BrokenLinkResult>();
        brokenLink.Type.ShouldBe(LinkType.Internal);
        brokenLink.Url.ShouldBe("/docs/missing");
        // Result 2: /images/logo.png (broken image — src extracted after all href)
        var brokenImg = results[2].ShouldBeCase<BrokenLinkResult>();
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

    // --- 19. FindLinksWithoutTrailingSlash ---

    [Fact]
    public void FindLinksWithoutTrailingSlash_DetectsMissingSlash()
    {
        var html = """<a href="/docs/page">Page</a>""";
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.Count.ShouldBe(1);
        results[0].ShouldBe("/docs/page");
    }

    [Fact]
    public void FindLinksWithoutTrailingSlash_IgnoresCorrectLinks()
    {
        var html = """<a href="/docs/page/">Page</a>""";
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.ShouldBeEmpty();
    }

    [Fact]
    public void FindLinksWithoutTrailingSlash_IgnoresFileUrls()
    {
        var html = """<a href="/styles.css">CSS</a><img src="/logo.png">""";
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.ShouldBeEmpty();
    }

    [Fact]
    public void FindLinksWithoutTrailingSlash_IgnoresExternalLinks()
    {
        var html = """<a href="https://example.com/no-slash">External</a>""";
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.ShouldBeEmpty();
    }

    [Fact]
    public void FindLinksWithoutTrailingSlash_IgnoresAnchorLinks()
    {
        var html = """<a href="#section">Anchor</a>""";
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.ShouldBeEmpty();
    }

    [Fact]
    public void FindLinksWithoutTrailingSlash_IgnoresRootSlash()
    {
        var html = """<a href="/">Home</a>""";
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.ShouldBeEmpty();
    }

    [Fact]
    public void FindLinksWithoutTrailingSlash_MultipleBadLinks_AllReported()
    {
        var html = """
            <a href="/docs/intro">Intro</a>
            <a href="/docs/setup/">Setup</a>
            <a href="/about">About</a>
            """;
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.Count.ShouldBe(2);
        results.ShouldContain("/docs/intro");
        results.ShouldContain("/about");
    }

    [Fact]
    public void FindLinksWithoutTrailingSlash_LinkWithQueryAndHash_ChecksPathOnly()
    {
        var html = """<a href="/docs/page?v=2#section">Page</a>""";
        var results = LinkVerificationService.FindLinksWithoutTrailingSlash(html);
        results.Count.ShouldBe(1);
        results[0].ShouldBe("/docs/page?v=2#section");
    }

    // --- 19a. Base-URL stripping ---

    [Fact]
    public void BaseUrl_PrefixedInternalLink_ResolvesToCanonicalKnownRoute()
    {
        // Pass B scenario: site is rendered with base URL "/preview/" so all internal
        // hrefs get the prefix, but the known-routes set still holds unprefixed canonical paths.
        var routes = new[] { MakeRoute("/docs/intro") };
        var service = new LinkVerificationService(routes, "/preview/");
        var source = MakeRoute("/docs/intro");
        var html = """<a href="/preview/docs/intro/">Intro</a>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
    }

    [Fact]
    public void BaseUrl_PrefixedFrameworkAsset_IsStillRecognizedAsValid()
    {
        // Previously `/preview/_content/Pennington.UI/scripts.js?v=…` was misclassified as a
        // broken link because the `/_content/` prefix check didn't know about the base URL.
        var service = new LinkVerificationService([], "/preview/");
        var source = MakeRoute("/about");
        var html = """<script src="/preview/_content/Pennington.UI/scripts.js?v=639115119605337292"></script>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
    }

    [Fact]
    public void BaseUrl_UnprefixedFrameworkAssetStillValidInPassA()
    {
        // Pass A (default "/") must keep working as before.
        var service = new LinkVerificationService([], "/");
        var source = MakeRoute("/about");
        var html = """<script src="/_content/Pennington.UI/scripts.js"></script>""";

        var results = service.VerifyLinks(source, html);

        results.Count.ShouldBe(1);
        (results[0] is ValidLink).ShouldBeTrue();
    }

    // --- 20. Large real-world page ---

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
