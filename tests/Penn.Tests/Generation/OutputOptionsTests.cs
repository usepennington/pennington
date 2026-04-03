using Penn.Generation;
using Penn.Routing;

namespace Penn.Tests.Generation;

public class OutputOptionsTests
{
    [Fact]
    public void DefaultBaseUrl_IsSlash()
    {
        var options = new OutputOptions
        {
            OutputDirectory = new FilePath("output")
        };

        options.BaseUrl.Value.ShouldBe("/");
    }

    [Fact]
    public void DefaultCleanOutput_IsTrue()
    {
        var options = new OutputOptions
        {
            OutputDirectory = new FilePath("output")
        };

        options.CleanOutput.ShouldBeTrue();
    }

    [Fact]
    public void FromArgs_WithNoExtraArgs()
    {
        var options = OutputOptions.FromArgs(["build"]);

        options.BaseUrl.Value.ShouldBe("/");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_WithBaseUrlArg()
    {
        var options = OutputOptions.FromArgs(["build", "/docs"]);

        options.BaseUrl.Value.ShouldBe("/docs");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_WithBaseUrlAndOutputDirArgs()
    {
        var options = OutputOptions.FromArgs(["build", "/blog", "dist"]);

        options.BaseUrl.Value.ShouldBe("/blog");
        options.OutputDirectory.Value.ShouldBe("dist");
    }

    [Fact]
    public void FromArgs_EmptyArgs_UsesDefaults()
    {
        var options = OutputOptions.FromArgs([]);

        options.BaseUrl.Value.ShouldBe("/");
        options.OutputDirectory.Value.ShouldBe("output");
    }
}
