using Pennington.Routing;

namespace Pennington.Tests.Routing;

public class FilePathTests
{
    [Fact]
    public void DivisionOperator_CombinesTwoPaths()
    {
        var left = new FilePath("content");
        var right = new FilePath("page.md");
        var result = left / right;
        result.Value.ShouldBe("content/page.md");
    }

    [Fact]
    public void DivisionOperator_LeftHasTrailingSlash_CombinesCorrectly()
    {
        var left = new FilePath("content/");
        var right = new FilePath("page.md");
        var result = left / right;
        result.Value.ShouldBe("content/page.md");
    }

    [Fact]
    public void DivisionOperator_RightHasLeadingSlash_CombinesCorrectly()
    {
        var left = new FilePath("content");
        var right = new FilePath("/page.md");
        var result = left / right;
        result.Value.ShouldBe("content/page.md");
    }

    [Fact]
    public void DivisionOperator_BothHaveSlashes_CombinesCorrectly()
    {
        var left = new FilePath("content/");
        var right = new FilePath("/page.md");
        var result = left / right;
        result.Value.ShouldBe("content/page.md");
    }

    [Fact]
    public void DivisionOperator_EmptyLeft_ReturnsRight()
    {
        var left = new FilePath("");
        var right = new FilePath("page.md");
        var result = left / right;
        result.Value.ShouldBe("page.md");
    }

    [Fact]
    public void DivisionOperator_EmptyRight_ReturnsLeft()
    {
        var left = new FilePath("content");
        var right = new FilePath("");
        var result = left / right;
        result.Value.ShouldBe("content");
    }

    [Fact]
    public void DivisionOperator_BackslashesHandled()
    {
        var left = new FilePath("content\\docs");
        var right = new FilePath("\\page.md");
        var result = left / right;
        result.Value.ShouldBe("content\\docs/page.md");
    }

}