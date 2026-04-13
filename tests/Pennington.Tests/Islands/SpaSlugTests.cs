using Pennington.Islands;

namespace Pennington.Tests.Islands;

public class SpaSlugTests
{
    [Fact]
    public void ToSlug_Root_ReturnsIndex()
    {
        SpaSlug.ToSlug("/").ShouldBe("index");
    }

    [Fact]
    public void ToSlug_Path_ReturnsStrippedPath()
    {
        SpaSlug.ToSlug("/docs/intro").ShouldBe("docs/intro");
    }

    [Fact]
    public void ToSlug_WithTrailingSlash_ReturnsStrippedPath()
    {
        SpaSlug.ToSlug("/docs/intro/").ShouldBe("docs/intro");
    }

    [Fact]
    public void ToUrl_Index_ReturnsRoot()
    {
        SpaSlug.ToUrl("index").ShouldBe("/");
    }

    [Fact]
    public void ToUrl_Path_ReturnsPrefixedPath()
    {
        SpaSlug.ToUrl("docs/intro").ShouldBe("/docs/intro");
    }

    [Fact]
    public void ToUrl_Empty_ReturnsRoot()
    {
        SpaSlug.ToUrl("").ShouldBe("/");
    }
}