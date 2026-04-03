using Penn.Highlighting;

namespace Penn.Tests.Highlighting;

public class TextMateHighlighterTests
{
    private readonly TextMateHighlighter _highlighter;

    public TextMateHighlighterTests()
    {
        var registry = new TextMateLanguageRegistry();
        _highlighter = new TextMateHighlighter(registry);
    }

    [Fact]
    public void Highlight_CSharpCode_ReturnsHtmlWithSpans()
    {
        var code = "var x = 1;";

        var result = _highlighter.Highlight(code, "csharp");

        result.ShouldContain("<span");
        result.ShouldContain("</span>");
        result.ShouldStartWith("<pre><code>");
        result.ShouldEndWith("</code></pre>");
    }

    [Fact]
    public void Highlight_CSharpCode_ReturnsHtmlWithHljsClasses()
    {
        var code = "public class Foo { }";

        var result = _highlighter.Highlight(code, "csharp");

        result.ShouldContain("hljs-");
    }

    [Fact]
    public void SupportedLanguages_ContainsWildcard()
    {
        _highlighter.SupportedLanguages.ShouldContain("*");
    }

    [Fact]
    public void Priority_Is50()
    {
        _highlighter.Priority.ShouldBe(50);
    }

    [Fact]
    public void Highlight_EmptyCode_ReturnsEmptyString()
    {
        var result = _highlighter.Highlight("", "csharp");

        result.ShouldBeEmpty();
    }

    [Fact]
    public void Highlight_UnknownLanguage_ReturnsHtmlEncodedFallback()
    {
        var code = "<div>hello</div>";

        var result = _highlighter.Highlight(code, "nonexistentlanguage12345");

        result.ShouldContain("&lt;div&gt;");
    }

    [Fact]
    public void Highlight_JavaScriptCode_ProducesOutput()
    {
        var code = "function hello() { return 42; }";

        var result = _highlighter.Highlight(code, "javascript");

        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldContain("<pre><code>");
    }
}
