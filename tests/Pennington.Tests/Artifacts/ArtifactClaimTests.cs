using Pennington.Artifacts;
using Pennington.Routing;

namespace Pennington.Tests.Artifacts;

public class ArtifactClaimTests
{
    private static ArtifactClaim Claim(ArtifactClaimShape shape) => new("test", shape, "test claim");

    [Theory]
    [InlineData("/search/en/index.json", true)]
    [InlineData("/search/en/segments/t-aa.json", true)]
    [InlineData("/search/en/", false)]           // suffix mismatch
    [InlineData("/search/", false)]              // bare prefix, no .json
    [InlineData("/searching/x.json", false)]     // prefix requires the trailing slash
    [InlineData("/SEARCH/EN/INDEX.JSON", true)]  // case-insensitive
    public void PrefixClaim_WithSuffix_MatchesOnlyInsideTheTerritory(string path, bool expected)
    {
        var claim = Claim(new PrefixClaim(new UrlPath("/search/"), ".json"));

        claim.Matches(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData("/book-preview/guides/", true)]
    [InlineData("/book-preview/anything.html", true)]
    [InlineData("/book-previews/x", false)]
    [InlineData("/pdf/guides.pdf", false)]
    public void PrefixClaim_WithoutSuffix_ClaimsTheWholePrefix(string path, bool expected)
    {
        var claim = Claim(new PrefixClaim(new UrlPath("/book-preview/")));

        claim.Matches(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData("/reference/llms.txt", true)]
    [InlineData("/reference/api/llms.txt", true)]   // any depth — the mid-path catch-all
    [InlineData("/llms.txt", false)]                // the bare root belongs to an ExactClaim
    [InlineData("/reference/llms.txt.bak", false)]
    public void SuffixClaim_MatchesAtAnyDepth_ButNeverTheBareSuffix(string path, bool expected)
    {
        var claim = Claim(new SuffixClaim("/llms.txt"));

        claim.Matches(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData("/llms.txt", true)]
    [InlineData("/LLMS.TXT", true)]
    [InlineData("/llms.txt/", false)]
    [InlineData("/docs/llms.txt", false)]
    public void ExactClaim_MatchesOnlyTheExactPath(string path, bool expected)
    {
        var claim = Claim(new ExactClaim(new UrlPath("/llms.txt")));

        claim.Matches(path).ShouldBe(expected);
    }

    [Fact]
    public void Pattern_RendersGlobStylePerShape()
    {
        Claim(new PrefixClaim(new UrlPath("/search/"), ".json")).Pattern.ShouldBe("/search/**.json");
        Claim(new PrefixClaim(new UrlPath("/book-preview/"))).Pattern.ShouldBe("/book-preview/**");
        Claim(new SuffixClaim("/llms.txt")).Pattern.ShouldBe("**/llms.txt");
        Claim(new ExactClaim(new UrlPath("/robots.txt"))).Pattern.ShouldBe("/robots.txt");
    }
}
