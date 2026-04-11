using Pennington.Highlighting;

namespace Pennington.Tests.Highlighting;

public class TextMateHighlighterTests
{
    private readonly TextMateHighlighter _highlighter;

    public TextMateHighlighterTests()
    {
        var registry = new TextMateLanguageRegistry();
        _highlighter = new TextMateHighlighter(registry);
    }

    [Fact]
    public void Highlight_UnknownLanguage_ReturnsHtmlEncodedFallback()
    {
        var code = "<div>hello</div>";

        var result = _highlighter.Highlight(code, "nonexistentlanguage12345");

        result.ShouldContain("&lt;div&gt;");
    }

}
