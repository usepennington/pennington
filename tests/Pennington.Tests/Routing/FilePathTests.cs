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

    [Fact]
    public void ResolveAgainstRoot_RelativePath_CombinesWithRoot()
    {
        var result = FilePath.ResolveAgainstRoot("Content", "/site/root");
        result.ShouldBe(Path.Combine("/site/root", "Content"));
    }

    [Fact]
    public void ResolveAgainstRoot_RootedPath_ReturnedUnchanged()
    {
        var rooted = Path.IsPathRooted("/abs/content") ? "/abs/content" : Path.Combine(Path.GetTempPath(), "content");
        var result = FilePath.ResolveAgainstRoot(rooted, "/site/root");
        result.ShouldBe(rooted);
    }

    [Fact]
    public void ResolveAgainstRoot_NullRoot_ReturnsPathUnchanged()
    {
        var result = FilePath.ResolveAgainstRoot("Content", null);
        result.ShouldBe("Content");
    }

    [Fact]
    public void ResolveAgainstRoot_EmptyRoot_ReturnsPathUnchanged()
    {
        var result = FilePath.ResolveAgainstRoot("Content", "");
        result.ShouldBe("Content");
    }
}