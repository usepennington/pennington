using Pennington.StandardSite;

namespace Pennington.Tests.StandardSite;

/// <summary>Unit tests for <see cref="AtUri"/> build/parse.</summary>
public class AtUriTests
{
    [Fact]
    public void Build_ComposesThreePartUri()
        => AtUri.Build("did:plc:abc", "site.standard.document", "r1")
            .ShouldBe("at://did:plc:abc/site.standard.document/r1");

    [Fact]
    public void Parse_RoundTripsAValidUri()
    {
        var parsed = AtUri.Parse("at://did:plc:abc/site.standard.publication/3lk2").ShouldNotBeNull();
        parsed.Did.ShouldBe("did:plc:abc");
        parsed.Collection.ShouldBe("site.standard.publication");
        parsed.Rkey.ShouldBe("3lk2");
    }

    [Fact]
    public void Parse_ReturnsNull_ForNonAtUri()
        => AtUri.Parse("https://example.com").ShouldBeNull();
}
