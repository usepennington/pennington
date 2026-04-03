using Penn.Highlighting;

namespace Penn.Tests.Highlighting;

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
    public void SupportedLanguages_ContainsBashShellSh()
    {
        _highlighter.SupportedLanguages.ShouldContain("bash");
        _highlighter.SupportedLanguages.ShouldContain("shell");
        _highlighter.SupportedLanguages.ShouldContain("sh");
    }

    [Fact]
    public void Priority_Is75()
    {
        _highlighter.Priority.ShouldBe(75);
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
    public void Highlight_EmptyCode_ReturnsEmptyString()
    {
        var result = _highlighter.Highlight("", "bash");

        result.ShouldBeEmpty();
    }
}
