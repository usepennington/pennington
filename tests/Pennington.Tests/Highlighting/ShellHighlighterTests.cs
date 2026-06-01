using Pennington.Highlighting;

namespace Pennington.Tests.Highlighting;

public class ShellHighlighterTests
{
    private readonly ShellHighlighter _highlighter = new();

    [Fact]
    public void Highlight_BashCommand_ReturnsHtmlWithSpans()
    {
        var code = "dotnet build --configuration Release";

        var result = _highlighter.Highlight(code, "bash");

        result.ShouldContain("<span");
        result.ShouldContain("hljs-built_in");
        result.ShouldStartWith("<pre><code>");
        result.ShouldEndWith("</code></pre>");
    }

    [Fact]
    public void Highlight_CommentLine_GetsCommentClass()
    {
        var code = "# this is a comment";

        var result = _highlighter.Highlight(code, "bash");

        result.ShouldContain("hljs-comment");
    }

    [Fact]
    public void Highlight_StringInQuotes_GetsStringClass()
    {
        var code = "echo \"hello world\"";

        var result = _highlighter.Highlight(code, "bash");

        result.ShouldContain("hljs-string");
    }

    [Fact]
    public void Highlight_Flags_GetsParamsClass()
    {
        var code = "ls -la --color=auto";

        var result = _highlighter.Highlight(code, "bash");

        result.ShouldContain("hljs-params");
    }

    [Fact]
    public void Highlight_EncodesShellMetacharacters()
    {
        // Redirections/pipes must be HTML-encoded — the output is re-parsed by
        // AngleSharp downstream, so a raw "< b" would be swallowed as a bogus tag.
        var code = "cat a < b > c";

        var result = _highlighter.Highlight(code, "bash");

        result.ShouldContain("&lt;");
        result.ShouldContain("&gt;");
        result.ShouldNotContain("a < b");
    }

}