using Pennington.Highlighting;

namespace Pennington.Tests.Highlighting;

public class HighlightingTests
{
    [Fact]
    public void PlainTextHighlighter_Highlight_HtmlEncodesCode()
    {
        var highlighter = new PlainTextHighlighter();

        var result = highlighter.Highlight("<div class=\"test\">Hello & world</div>", "html");

        result.ShouldBe("&lt;div class=&quot;test&quot;&gt;Hello &amp; world&lt;/div&gt;");
    }

}
