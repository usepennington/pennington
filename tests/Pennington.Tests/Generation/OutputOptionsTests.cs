using Pennington.Generation;

namespace Pennington.Tests.Generation;

public class OutputOptionsTests
{
    [Fact]
    public void FromArgs_Empty_ReturnsDefaults()
    {
        var options = OutputOptions.FromArgs([]);

        options.BaseUrl.Value.ShouldBe("/");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_BuildVerbOnly_ReturnsDefaults()
    {
        var options = OutputOptions.FromArgs(["build"]);

        options.BaseUrl.Value.ShouldBe("/");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_BuildWithBaseUrl_ParsesBaseUrl()
    {
        var options = OutputOptions.FromArgs(["build", "/docs"]);

        options.BaseUrl.Value.ShouldBe("/docs");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_BuildWithBaseUrlAndOutputDir_ParsesBoth()
    {
        var options = OutputOptions.FromArgs(["build", "/docs", "dist"]);

        options.BaseUrl.Value.ShouldBe("/docs");
        options.OutputDirectory.Value.ShouldBe("dist");
    }

    [Fact]
    public void FromArgs_BuildVerbIsCaseInsensitive()
    {
        var options = OutputOptions.FromArgs(["BUILD", "/docs"]);

        options.BaseUrl.Value.ShouldBe("/docs");
    }

    [Fact]
    public void FromArgs_DevRunFlags_ReturnsDefaults()
    {
        // `dotnet run -- --urls=http://localhost:5000` shape must not leak a URL into BaseUrl.
        var options = OutputOptions.FromArgs(["--urls=http://localhost:5000"]);

        options.BaseUrl.Value.ShouldBe("/");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_XunitTestHostArgs_ReturnsDefaults()
    {
        // Regression: under `dotnet test`, the xunit v3 test host passes a temp
        // .dll path and runner flags. These previously landed in BaseUrl and
        // got prefixed onto every <a href> by BaseUrlRewritingProcessor.
        var options = OutputOptions.FromArgs(["/tmp/xunit-abc/test.dll", "--port", "54321"]);

        options.BaseUrl.Value.ShouldBe("/");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_UnknownVerb_ReturnsDefaults()
    {
        var options = OutputOptions.FromArgs(["serve", "/whatever"]);

        options.BaseUrl.Value.ShouldBe("/");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_BaseUrlFlag_EqualsForm_ParsesValue()
    {
        var options = OutputOptions.FromArgs(["build", "--base-url=/sub"]);

        options.BaseUrl.Value.ShouldBe("/sub");
        options.OutputDirectory.Value.ShouldBe("output");
    }

    [Fact]
    public void FromArgs_BaseUrlFlag_SpaceForm_ParsesValue()
    {
        var options = OutputOptions.FromArgs(["build", "--base-url", "/sub"]);

        options.BaseUrl.Value.ShouldBe("/sub");
    }

    [Fact]
    public void FromArgs_OutputFlag_EqualsForm_ParsesValue()
    {
        var options = OutputOptions.FromArgs(["build", "--output=dist"]);

        options.OutputDirectory.Value.ShouldBe("dist");
        options.BaseUrl.Value.ShouldBe("/");
    }

    [Fact]
    public void FromArgs_OutputFlag_SpaceForm_ParsesValue()
    {
        var options = OutputOptions.FromArgs(["build", "--output", "dist"]);

        options.OutputDirectory.Value.ShouldBe("dist");
    }

    [Fact]
    public void FromArgs_BothFlags_AnyOrder_ParsesBoth()
    {
        var options = OutputOptions.FromArgs(["build", "--output=dist", "--base-url=/sub"]);

        options.BaseUrl.Value.ShouldBe("/sub");
        options.OutputDirectory.Value.ShouldBe("dist");
    }

    [Fact]
    public void FromArgs_FlagsMixedWithPositional_FlagsWin()
    {
        // User passed positional baseUrl `/pos` and named flag `--base-url=/flag`.
        // The named flag should win — positional is the legacy fallback only.
        var options = OutputOptions.FromArgs(["build", "/pos", "--base-url=/flag"]);

        options.BaseUrl.Value.ShouldBe("/flag");
    }

    [Fact]
    public void FromArgs_FlagForBaseUrl_PositionalProvidesOutput()
    {
        // Covers the intended `build --base-url /sub dist` shape: flag for baseUrl,
        // positional for output directory.
        var options = OutputOptions.FromArgs(["build", "--base-url=/sub", "dist"]);

        options.BaseUrl.Value.ShouldBe("/sub");
        options.OutputDirectory.Value.ShouldBe("dist");
    }

    [Fact]
    public void FromArgs_BaseUrlFlagCaseInsensitive()
    {
        var options = OutputOptions.FromArgs(["build", "--BASE-URL=/sub"]);

        options.BaseUrl.Value.ShouldBe("/sub");
    }
}