using Microsoft.AspNetCore.Http;
using Pennington.Beck;

namespace Pennington.Tests.Beck;

/// <summary>
/// Tests for <see cref="BeckCodeBlockPreprocessor"/> — fence detection, inline and
/// <c>:symbol</c> rendering, flag parsing, and the loud-failure error box.
/// </summary>
public class BeckCodeBlockPreprocessorTests
{
    private const string ValidYaml = """
        type: architecture
        nodes:
          - { id: a, title: Alpha }
          - { id: b, title: Beta }
        edges:
          - { from: a, to: b }
        """;

    private static BeckCodeBlockPreprocessor CreatePreprocessor(Action<BeckOptions>? configure = null)
    {
        var options = new BeckOptions();
        configure?.Invoke(options);
        return new BeckCodeBlockPreprocessor(options, new HttpContextAccessor());
    }

    [Theory]
    [InlineData("csharp")]
    [InlineData("csharp:symbol")]
    [InlineData("yaml")]
    [InlineData("beckish")]
    public void TryProcess_NonBeckFence_ReturnsNull(string languageId)
    {
        var result = CreatePreprocessor().TryProcess(ValidYaml, languageId);

        result.ShouldBeNull();
    }

    [Fact]
    public void TryProcess_InlineFence_RendersSvgOwningTheWholeOutput()
    {
        var result = CreatePreprocessor().TryProcess(ValidYaml, "beck");

        result.ShouldNotBeNull();
        result.HighlightedHtml.ShouldContain("<svg");
        result.HighlightedHtml.ShouldContain("class=\"beck-embed\"");
        result.BaseLanguage.ShouldBe("beck");
        result.SkipTransform.ShouldBeTrue();
        result.SkipChrome.ShouldBeTrue();
    }

    [Fact]
    public void TryProcess_StaticFlag_ChangesTheRender()
    {
        var preprocessor = CreatePreprocessor();

        var full = preprocessor.TryProcess(ValidYaml, "beck")!.HighlightedHtml;
        var frozen = preprocessor.TryProcess(ValidYaml, "beck,static")!.HighlightedHtml;

        frozen.ShouldContain("<svg");
        frozen.ShouldNotBe(full);
    }

    [Fact]
    public void TryProcess_MalformedYaml_RendersErrorBoxWithEncodedBody()
    {
        var result = CreatePreprocessor().TryProcess("nodes: [ <oops", "beck");

        result.ShouldNotBeNull();
        result.HighlightedHtml.ShouldContain("beck-embed--error");
        result.HighlightedHtml.ShouldContain("&lt;oops");
        result.SkipChrome.ShouldBeTrue();
    }

    [Fact]
    public void TryProcess_SymbolFence_ReadsYamlFromContentRoot()
    {
        var root = Directory.CreateTempSubdirectory("beck-tests").FullName;
        try
        {
            File.WriteAllText(Path.Combine(root, "diagram.beck.yaml"), ValidYaml);
            var preprocessor = CreatePreprocessor(o => o.ContentRoot = root);

            var result = preprocessor.TryProcess("diagram.beck.yaml", "beck:symbol");

            result.ShouldNotBeNull();
            result.HighlightedHtml.ShouldContain("<svg");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TryProcess_SymbolFence_MissingFile_RendersErrorBox()
    {
        var preprocessor = CreatePreprocessor(o => o.ContentRoot = Path.GetTempPath());

        var result = preprocessor.TryProcess("does-not-exist.beck.yaml", "beck:symbol");

        result.ShouldNotBeNull();
        result.HighlightedHtml.ShouldContain("beck-embed--error");
    }

    [Fact]
    public void TryProcess_ZoomOnByDefault_EmitsZoomButtonAfterSvg()
    {
        var result = CreatePreprocessor().TryProcess(ValidYaml, "beck");

        result.ShouldNotBeNull();
        result.HighlightedHtml.ShouldContain("class=\"beck-zoom\"");
    }

    [Fact]
    public void TryProcess_ZoomDisabled_OmitsZoomButton()
    {
        var result = CreatePreprocessor(o => o.Zoom = false).TryProcess(ValidYaml, "beck");

        result.ShouldNotBeNull();
        result.HighlightedHtml.ShouldNotContain("beck-zoom");
    }

    [Fact]
    public void TryProcess_MalformedYaml_ErrorBoxHasNoZoomButton()
    {
        var result = CreatePreprocessor().TryProcess("nodes: [ <oops", "beck");

        result.ShouldNotBeNull();
        result.HighlightedHtml.ShouldNotContain("beck-zoom");
    }

    [Fact]
    public void ApplyStyle_InjectsMetaStyleOverride()
    {
        var styled = CreatePreprocessor().ApplyStyle(ValidYaml, "sketch", "beck,style=sketch");

        styled.ShouldContain("style: sketch");
    }

    [Fact]
    public void ApplyStyle_OverwritesTheDocumentsOwnStyle()
    {
        var yaml = "meta: { style: terminal }\n" + ValidYaml;

        var styled = CreatePreprocessor().ApplyStyle(yaml, "sketch", "beck,style=sketch");

        styled.ShouldContain("sketch");
        styled.ShouldNotContain("terminal");
    }

    [Fact]
    public void ApplyStyle_UnknownStyle_LeavesYamlUntouched()
    {
        var styled = CreatePreprocessor().ApplyStyle(ValidYaml, "no-such-style", "beck,style=no-such-style");

        styled.ShouldBe(ValidYaml);
    }
}
