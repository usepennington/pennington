namespace Pennington.Tests.Markdown.Shortcodes;

using Pennington.FrontMatter;
using Pennington.Markdown.Shortcodes;
using Pennington.Routing;

public class PackageVersionShortcodeTests
{
    private static ShortcodeContext MakeContext() => new(
        new ContentRoute
        {
            CanonicalPath = new UrlPath("/test/"),
            OutputFile = new FilePath("test/index.html"),
        },
        new DocFrontMatter { Title = "Test" });

    private static ShortcodeInvocation MakeInvocation() =>
        new(PositionalArgs: [], NamedArgs: new Dictionary<string, string>(), Content: null);

    [Fact]
    public void Name_IsPackageVersion()
    {
        new PackageVersionShortcode().Name.ShouldBe("PackageVersion");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTrimmedPackageVersion()
    {
        var result = await new PackageVersionShortcode().ExecuteAsync(
            MakeInvocation(),
            MakeContext(),
            TestContext.Current.CancellationToken);

        // Mirrors the shared resolver: the published NuGet version with MinVer's
        // "+<sha>" build metadata trimmed off.
        result.ShouldBe(PenningtonVersion.Value);
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldNotContain("+");
    }
}
