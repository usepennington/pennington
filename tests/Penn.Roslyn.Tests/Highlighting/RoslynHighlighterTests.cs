namespace Penn.Roslyn.Tests.Highlighting;

using Penn.Roslyn.Highlighting;

public sealed class RoslynHighlighterTests : IDisposable
{
    private readonly RoslynHighlighter _highlighter = new();

    [Fact]
    public void Highlights_CSharp_Code_With_Spans()
    {
        var result = _highlighter.Highlight("var x = 42;", "csharp");

        result.ShouldContain("hljs-keyword");
        result.ShouldContain("var");
    }

    [Fact]
    public void Priority_Is_100()
    {
        _highlighter.Priority.ShouldBe(100);
    }

    [Theory]
    [InlineData("csharp")]
    [InlineData("cs")]
    [InlineData("c#")]
    [InlineData("vb")]
    [InlineData("vbnet")]
    public void SupportedLanguages_Includes_Expected_Aliases(string language)
    {
        _highlighter.SupportedLanguages.ShouldContain(language);
    }

    [Fact]
    public void Returns_Html_With_Pre_Code_Structure()
    {
        var result = _highlighter.Highlight("var x = 42;", "csharp");

        result.ShouldStartWith("<pre><code");
        result.ShouldContain("language-csharp");
        result.ShouldContain("highlighted");
        result.ShouldEndWith("</code></pre>");
    }

    [Fact]
    public void Handles_Empty_Code()
    {
        var result = _highlighter.Highlight("", "csharp");

        result.ShouldNotBeNull();
        result.ShouldStartWith("<pre><code");
    }

    [Fact]
    public void Handles_Multiline_Code()
    {
        var code = """
            var x = 42;
            var y = "hello";
            Console.WriteLine(x);
            """;

        var result = _highlighter.Highlight(code, "csharp");

        result.ShouldContain("<span");
        result.ShouldContain("hljs-keyword");
    }

    public void Dispose() => _highlighter.Dispose();
}
