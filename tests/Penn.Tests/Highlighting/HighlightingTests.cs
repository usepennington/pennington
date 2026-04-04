using Penn.Highlighting;

namespace Penn.Tests.Highlighting;

public class HighlightingTests
{
    [Fact]
    public void PlainTextHighlighter_Highlight_HtmlEncodesCode()
    {
        var highlighter = new PlainTextHighlighter();

        var result = highlighter.Highlight("<div class=\"test\">Hello & world</div>", "html");

        result.ShouldBe("&lt;div class=&quot;test&quot;&gt;Hello &amp; world&lt;/div&gt;");
    }

    [Fact]
    public void ICodeHighlighter_StubHighlighter_VerifiesInterfaceContract()
    {
        ICodeHighlighter highlighter = new StubCSharpHighlighter();

        highlighter.SupportedLanguages.ShouldContain("csharp");
        highlighter.Priority.ShouldBe(100);
        highlighter.Highlight("var x = 1;", "csharp").ShouldBe("<span class=\"keyword\">var</span> x = 1;");
    }

    private sealed class StubCSharpHighlighter : ICodeHighlighter
    {
        private static readonly HashSet<string> _languages = ["csharp"];

        public IReadOnlySet<string> SupportedLanguages => _languages;

        public string Highlight(string code, string language)
            => code.Replace("var", "<span class=\"keyword\">var</span>");

        public int Priority => 100;
    }
}
