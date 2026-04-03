using Penn.Routing;

namespace Penn.Tests.Routing;

public class FilePathTests
{
    [Fact]
    public void ImplicitConversion_FromString_CreatesFilePath()
    {
        FilePath path = "some/file.txt";
        path.Value.ShouldBe("some/file.txt");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var path = new FilePath("content/page.md");
        path.ToString().ShouldBe("content/page.md");
    }

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
    public void Extension_ReturnsFileExtension()
    {
        var path = new FilePath("content/page.md");
        path.Extension.ShouldBe(".md");
    }

    [Fact]
    public void Extension_NoExtension_ReturnsEmpty()
    {
        var path = new FilePath("content/page");
        path.Extension.ShouldBe("");
    }

    [Fact]
    public void FileName_ReturnsFileName()
    {
        var path = new FilePath("content/docs/page.md");
        path.FileName.ShouldBe("page.md");
    }

    [Fact]
    public void FileNameWithoutExtension_ReturnsNameOnly()
    {
        var path = new FilePath("content/docs/page.md");
        path.FileNameWithoutExtension.ShouldBe("page");
    }

    [Fact]
    public void RecordEquality_SameValue_AreEqual()
    {
        var a = new FilePath("content/page.md");
        var b = new FilePath("content/page.md");
        a.ShouldBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentValue_AreNotEqual()
    {
        var a = new FilePath("content/page.md");
        var b = new FilePath("content/other.md");
        a.ShouldNotBe(b);
    }
}
