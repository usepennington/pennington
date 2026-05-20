using Pennington.FrontMatter;
using YamlDotNet.Core;

namespace Pennington.Tests.FrontMatter;

public class FrontMatterParserDiagnosticsTests
{
    private const string MarkdownWithUnknownKey = """
        ---
        title: Hello
        fubar: nope
        ---
        body
        """;

    private const string MarkdownWithTwoUnknownKeys = """
        ---
        title: Hello
        fubar: nope
        bazquux: also-nope
        ---
        body
        """;

    private const string CleanMarkdown = """
        ---
        title: Hello
        description: World
        ---
        body
        """;

    private const string PlainMarkdown = "# Just a heading\n\nNo front matter here.";

    [Fact]
    public void Parse_WithUnknownKey_NonStrict_EmitsWarningAndDefaultsProperty()
    {
        var diagnostics = new DiagnosticContext();
        var parser = CreateParser(strict: false);

        var result = parser.Parse<DocFrontMatter>(MarkdownWithUnknownKey, "page.md", diagnostics);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("Hello");
        result.Body.Trim().ShouldBe("body");

        diagnostics.Diagnostics.Count.ShouldBe(1);
        var warning = diagnostics.Diagnostics[0];
        warning.Severity.ShouldBe(DiagnosticSeverity.Warning);
        warning.Source.ShouldBe("FrontMatterParser");
        warning.Message.ShouldContain("'fubar'");
        warning.Message.ShouldContain("page.md:3");
    }

    [Fact]
    public void Parse_WithUnknownKey_Strict_ThrowsAfterEmittingWarning()
    {
        var diagnostics = new DiagnosticContext();
        var parser = CreateParser(strict: true);

        Should.Throw<YamlException>(() =>
            parser.Parse<DocFrontMatter>(MarkdownWithUnknownKey, "page.md", diagnostics));

        diagnostics.Diagnostics.Count.ShouldBe(1);
        diagnostics.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostics.Diagnostics[0].Message.ShouldContain("'fubar'");
    }

    [Fact]
    public void Parse_WithKnownKeys_NoDiagnostics()
    {
        var diagnostics = new DiagnosticContext();
        var parser = CreateParser(strict: false);

        var result = parser.Parse<DocFrontMatter>(CleanMarkdown, "page.md", diagnostics);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("Hello");
        result.Metadata.Description.ShouldBe("World");
        diagnostics.HasAny.ShouldBeFalse();
    }

    [Fact]
    public void Parse_WithMultipleUnknownKeys_EmitsOnePerKey()
    {
        var diagnostics = new DiagnosticContext();
        var parser = CreateParser(strict: false);

        parser.Parse<DocFrontMatter>(MarkdownWithTwoUnknownKeys, "page.md", diagnostics);

        diagnostics.Diagnostics.Count.ShouldBe(2);
        diagnostics.Diagnostics[0].Message.ShouldContain("'fubar'");
        diagnostics.Diagnostics[0].Message.ShouldContain("page.md:3");
        diagnostics.Diagnostics[1].Message.ShouldContain("'bazquux'");
        diagnostics.Diagnostics[1].Message.ShouldContain("page.md:4");
    }

    [Fact]
    public void Parse_NoFrontMatter_NoScan()
    {
        var diagnostics = new DiagnosticContext();
        var parser = CreateParser(strict: false);

        var result = parser.Parse<DocFrontMatter>(PlainMarkdown, "page.md", diagnostics);

        result.Metadata.ShouldBeNull();
        result.Body.ShouldBe(PlainMarkdown);
        diagnostics.HasAny.ShouldBeFalse();
    }

    [Fact]
    public void DeserializeYaml_WithUnknownKey_ReportsLineWithoutFrontMatterOffset()
    {
        var diagnostics = new DiagnosticContext();
        var parser = CreateParser(strict: false);
        const string sidecar = "title: Hello\nfubar: nope";

        parser.DeserializeYaml<DocFrontMatter>(sidecar, "page.yaml", diagnostics);

        diagnostics.Diagnostics.Count.ShouldBe(1);
        diagnostics.Diagnostics[0].Message.ShouldContain("'fubar'");
        diagnostics.Diagnostics[0].Message.ShouldContain("page.yaml:2");
    }

    [Fact]
    public void Parse_WithoutSourcePath_FallsBackToUnknownLabel()
    {
        var diagnostics = new DiagnosticContext();
        var parser = CreateParser(strict: false);

        parser.Parse<DocFrontMatter>(MarkdownWithUnknownKey, sourcePath: null, diagnostics);

        diagnostics.Diagnostics.Count.ShouldBe(1);
        diagnostics.Diagnostics[0].Message.ShouldContain("<unknown>");
    }

    private static FrontMatterParser CreateParser(bool strict)
        => new(new FrontMatterParserOptions { StrictUnknownKeys = strict }, new NoopHttpContextAccessor());

    private sealed class NoopHttpContextAccessor : Microsoft.AspNetCore.Http.IHttpContextAccessor
    {
        public Microsoft.AspNetCore.Http.HttpContext? HttpContext { get => null; set { } }
    }
}