using Pennington.Highlighting;
using Pennington.Markdown.Extensions;

namespace Pennington.Tests.Markdown.Extensions;

/// <summary>
/// Direct tests of <see cref="CodeBlockRenderingService.Render"/> — the API the
/// Razor <c>&lt;CodeBlock&gt;</c> component calls (CodeBlock.razor:61). The
/// markdown-side tests in <see cref="CodeBlockPreprocessorTests"/> cover the
/// Markdig pipeline path; these prove the same preprocessor composition runs
/// when the entry point is the Razor component.
/// </summary>
public class CodeBlockRenderingServiceTests
{
    [Fact]
    public void Render_MatchingPreprocessor_ReturnsPreprocessedHtml()
    {
        // Simulates a <CodeBlock> whose info string matches a registered code-fragment
        // preprocessor (which would resolve the fenced body).
        var preprocessor = new StubPreprocessor(
            matchLanguageId: "csharp:xmldocid,bodyonly",
            result: new CodeBlockPreprocessResult(
                HighlightedHtml: "<pre><code class=\"language-csharp highlighted\">return a * b;</code></pre>",
                BaseLanguage: "csharp",
                SkipTransform: true));

        var service = new CodeBlockRenderingService(
            new HighlightingService([]),
            [preprocessor]);

        var html = service.Render(
            code: "M:Foo.Bar",
            languageId: "csharp:xmldocid,bodyonly");

        html.ShouldContain("return a * b;");
        html.ShouldContain("language-csharp");
        // CodeBlockHtmlBuilder still wraps with the standard chrome.
        html.ShouldContain("data-language=\"csharp:xmldocid,bodyonly\"");
    }

    [Fact]
    public void Render_NoMatchingPreprocessor_FallsBackToHighlighter()
    {
        // No preprocessor matches "csharp"; should hit HighlightingService fallback.
        var preprocessor = new StubPreprocessor(
            matchLanguageId: "csharp:xmldocid",
            result: new CodeBlockPreprocessResult("<pre><code>should not appear</code></pre>", "csharp"));

        var service = new CodeBlockRenderingService(
            new HighlightingService([]),
            [preprocessor]);

        var html = service.Render(code: "var x = 1;", languageId: "csharp");

        html.ShouldContain("var x = 1;");
        html.ShouldNotContain("should not appear");
    }

    [Fact]
    public void Render_PreprocessorReturnsNull_FallsBackToHighlighter()
    {
        // Preprocessor returns null (e.g. a code-fragment preprocessor on a languageId it doesn't handle).
        var preprocessor = new StubPreprocessor(matchLanguageId: "csharp:xmldocid", result: null);

        var service = new CodeBlockRenderingService(
            new HighlightingService([]),
            [preprocessor]);

        var html = service.Render(code: "var x = 1;", languageId: "csharp:xmldocid");

        // Falls through to highlighter on baseLanguage "csharp".
        html.ShouldContain("var x = 1;");
    }

    [Fact]
    public void Render_PreprocessorPriority_HighestRunsFirst()
    {
        var low = new StubPreprocessor(
            matchLanguageId: "csharp:xmldocid",
            result: new CodeBlockPreprocessResult("<pre><code>low</code></pre>", "csharp", SkipTransform: true),
            priority: 10);

        var high = new StubPreprocessor(
            matchLanguageId: "csharp:xmldocid",
            result: new CodeBlockPreprocessResult("<pre><code>high</code></pre>", "csharp", SkipTransform: true),
            priority: 100);

        var service = new CodeBlockRenderingService(
            new HighlightingService([]),
            [low, high]);

        var html = service.Render(code: "M:Foo", languageId: "csharp:xmldocid");

        html.ShouldContain("high");
        html.ShouldNotContain(">low<");
    }

    [Fact]
    public void Render_SkipChrome_EmitsPreprocessorHtmlUnwrapped()
    {
        // A preprocessor whose output is not a code block (a rendered diagram) opts out
        // of the wrapper chrome entirely — the result is its HTML verbatim.
        var preprocessor = new StubPreprocessor(
            matchLanguageId: "beck",
            result: new CodeBlockPreprocessResult(
                HighlightedHtml: "<div class=\"beck-embed\"><svg></svg></div>",
                BaseLanguage: "beck",
                SkipTransform: true,
                SkipChrome: true));

        var service = new CodeBlockRenderingService(
            new HighlightingService([]),
            [preprocessor]);

        var html = service.Render(code: "nodes: []", languageId: "beck");

        html.ShouldBe("<div class=\"beck-embed\"><svg></svg></div>");
    }

    [Fact]
    public void Render_NoPreprocessors_FallsBackToHighlighter()
    {
        var service = new CodeBlockRenderingService(new HighlightingService([]), preprocessors: null);

        var html = service.Render(code: "var x = 1;", languageId: "csharp");

        html.ShouldContain("var x = 1;");
        html.ShouldContain("data-language=\"csharp\"");
    }

    [Fact]
    public void Render_LanguageWithModifier_ParsesBaseLanguageForFallback()
    {
        // When no preprocessor handles the modifier, the base language is what
        // the highlighter receives. Confirms ParseBaseLanguage strips the modifier.
        var service = new CodeBlockRenderingService(new HighlightingService([]), preprocessors: []);

        var html = service.Render(code: "var x = 1;", languageId: "csharp:xmldocid,bodyonly");

        // data-language carries the full languageId (so consumers can target the modifier).
        html.ShouldContain("data-language=\"csharp:xmldocid,bodyonly\"");
        // But the highlighter only saw the base — output is highlighted as plain csharp.
        html.ShouldContain("var x = 1;");
    }

    private sealed class StubPreprocessor(
        string matchLanguageId,
        CodeBlockPreprocessResult? result,
        int priority = 50) : ICodeBlockPreprocessor
    {
        public int Priority { get; } = priority;

        public CodeBlockPreprocessResult? TryProcess(string code, string languageId)
            => languageId == matchLanguageId ? result : null;
    }
}