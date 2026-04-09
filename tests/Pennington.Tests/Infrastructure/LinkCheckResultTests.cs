using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class LinkCheckResultTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void ValidLink_PreservesProperties()
    {
        var route = MakeRoute("/docs/intro");
        var valid = new ValidLink(route, "/docs/other");
        var result = new LinkCheckResult(valid);

        var recovered = result.ShouldBeCase<ValidLink>();
        recovered.SourcePage.ShouldBe(route);
        recovered.Url.ShouldBe("/docs/other");
    }

    [Fact]
    public void BrokenLinkResult_PreservesProperties()
    {
        var route = MakeRoute("/blog/post");
        var broken = new BrokenLinkResult(route, "/nowhere", LinkType.Internal, "Not found");
        var result = new LinkCheckResult(broken);

        var recovered = result.ShouldBeCase<BrokenLinkResult>();
        recovered.SourcePage.ShouldBe(route);
        recovered.Url.ShouldBe("/nowhere");
        recovered.Type.ShouldBe(LinkType.Internal);
        recovered.Reason.ShouldBe("Not found");
    }

    [Fact]
    public void ExternalLink_PreservesProperties()
    {
        var route = MakeRoute("/page");
        var external = new ExternalLink(route, "https://github.com");
        var result = new LinkCheckResult(external);

        var recovered = result.ShouldBeCase<ExternalLink>();
        recovered.SourcePage.ShouldBe(route);
        recovered.Url.ShouldBe("https://github.com");
    }

    // --- Exhaustive pattern matching ---

    [Fact]
    public void ExhaustivePatternMatch_AllThreeCases()
    {
        var route = MakeRoute("/test");
        LinkCheckResult valid = new LinkCheckResult(new ValidLink(route, "/ok"));
        LinkCheckResult broken = new LinkCheckResult(new BrokenLinkResult(route, "/bad", LinkType.Anchor, "Missing anchor"));
        LinkCheckResult external = new LinkCheckResult(new ExternalLink(route, "https://ext.com"));

        Describe(valid).ShouldBe("Valid: /ok");
        Describe(broken).ShouldBe("Broken: /bad (Anchor)");
        Describe(external).ShouldBe("External: https://ext.com");
    }

    private static string Describe(LinkCheckResult result) => result switch
    {
        ValidLink v => $"Valid: {v.Url}",
        BrokenLinkResult b => $"Broken: {b.Url} ({b.Type})",
        ExternalLink e => $"External: {e.Url}",
        _ => throw new InvalidOperationException("Unknown LinkCheckResult case")
    };

    // --- BrokenLinkResult uses LinkType from Pennington.Generation ---

    [Fact]
    public void BrokenLinkResult_SupportsAllLinkTypes()
    {
        var route = MakeRoute();

        var @internal = new BrokenLinkResult(route, "/a", LinkType.Internal, "not found");
        @internal.Type.ShouldBe(LinkType.Internal);

        var external = new BrokenLinkResult(route, "/b", LinkType.External, "timeout");
        external.Type.ShouldBe(LinkType.External);

        var anchor = new BrokenLinkResult(route, "/c#heading", LinkType.Anchor, "missing anchor");
        anchor.Type.ShouldBe(LinkType.Anchor);

        var image = new BrokenLinkResult(route, "/img.png", LinkType.Image, "404");
        image.Type.ShouldBe(LinkType.Image);
    }
}
