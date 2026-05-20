namespace Pennington.Tests.Markdown.Shortcodes;

using System.Reflection;
using Pennington.FrontMatter;
using Pennington.Markdown.Shortcodes;
using Pennington.Routing;

public class AssemblyVersionShortcodeTests
{
    private static ShortcodeContext MakeContext() => new(
        new ContentRoute
        {
            CanonicalPath = new UrlPath("/test/"),
            OutputFile = new FilePath("test/index.html"),
        },
        new DocFrontMatter { Title = "Test" });

    private static ShortcodeInvocation MakeInvocation(params (string Key, string Value)[] named) =>
        new(
            PositionalArgs: [],
            NamedArgs: named.ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase),
            Content: null);

    [Fact]
    public void Name_IsVersion()
    {
        new AssemblyVersionShortcode().Name.ShouldBe("Version");
    }

    [Fact]
    public async Task ExecuteAsync_DefaultFormat_ReturnsEntryAssemblyVersionString()
    {
        var shortcode = new AssemblyVersionShortcode();

        var result = await shortcode.ExecuteAsync(
            MakeInvocation(),
            MakeContext(),
            TestContext.Current.CancellationToken);

        // The test host has *some* entry assembly with a version — the resolver
        // falls back to "unknown" only when none is available. We can't predict
        // the exact value, but we can require a non-empty, dot-bearing string
        // (or the documented "unknown" sentinel).
        result.ShouldNotBeNullOrWhiteSpace();
        var entry = Assembly.GetEntryAssembly()?.GetName().Version;
        if (entry is not null)
        {
            result.ShouldBe(entry.ToString());
        }
    }

    [Fact]
    public async Task ExecuteAsync_FormatMajor_ReturnsLeadingComponent()
    {
        if (Assembly.GetEntryAssembly()?.GetName().Version is not { } version)
        {
            return;
        }

        var shortcode = new AssemblyVersionShortcode();

        var result = await shortcode.ExecuteAsync(
            MakeInvocation(("format", "major")),
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe(version.Major.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_FormatMinor_ReturnsMajorDotMinor()
    {
        if (Assembly.GetEntryAssembly()?.GetName().Version is not { } version)
        {
            return;
        }

        var shortcode = new AssemblyVersionShortcode();

        var result = await shortcode.ExecuteAsync(
            MakeInvocation(("format", "minor")),
            MakeContext(),
            TestContext.Current.CancellationToken);

        result.ShouldBe($"{version.Major}.{version.Minor}");
    }
}
