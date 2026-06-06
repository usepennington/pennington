using Pennington.Routing;
using Pennington.SocialCards;

namespace Pennington.Tests.SocialCards;

public class SocialCardUrlTests
{
    [Fact]
    public void RelativePath_BuildsCardPath_FromCanonicalPath()
    {
        SocialCardUrl.RelativePath(new UrlPath("/blog/my-post/"), "/social-cards")
            .ShouldBe("/social-cards/blog/my-post.png");
    }

    [Fact]
    public void RelativePath_MapsHome_ToIndexSlug()
    {
        SocialCardUrl.RelativePath(new UrlPath("/"), "/social-cards")
            .ShouldBe("/social-cards/index.png");
    }

    [Fact]
    public void RelativePath_NormalizesBaseUrlTrailingSlash()
    {
        SocialCardUrl.RelativePath(new UrlPath("/guide/intro/"), "/cards/")
            .ShouldBe("/cards/guide/intro.png");
    }

    [Fact]
    public void For_PrependsCanonicalBaseUrl_WhenSet()
    {
        SocialCardUrl.For(new UrlPath("/blog/my-post/"), "/social-cards", "https://example.com")
            .ShouldBe("https://example.com/social-cards/blog/my-post.png");
    }

    [Fact]
    public void For_ReturnsRootRelative_WhenNoBaseUrl()
    {
        SocialCardUrl.For(new UrlPath("/blog/my-post/"), "/social-cards", null)
            .ShouldBe("/social-cards/blog/my-post.png");
    }

    [Theory]
    [InlineData("blog/my-post.png", "blog/my-post")]
    [InlineData("blog/my-post", "blog/my-post")]
    [InlineData("index.png", "")]
    [InlineData("index", "")]
    public void SlugToRecordKey_ReversesRelativePath(string slug, string expectedKey)
    {
        SocialCardUrl.SlugToRecordKey(slug).ShouldBe(expectedKey);
    }

    [Fact]
    public void RelativePath_And_SlugToRecordKey_RoundTrip()
    {
        // The card path the discovery service emits must reverse back to the registry key
        // the endpoint looks up — otherwise a baked card never matches its page.
        var canonical = new UrlPath("/reference/api/widgets/");
        var cardPath = SocialCardUrl.RelativePath(canonical, "/social-cards");
        var slug = cardPath["/social-cards/".Length..];

        SocialCardUrl.SlugToRecordKey(slug).ShouldBe("reference/api/widgets");
    }
}
