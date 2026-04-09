using Pennington.Highlighting;

namespace Pennington.Tests.Highlighting;

public class HighlightingServiceTests
{
    [Fact]
    public void Highlight_NoRegisteredHighlighters_FallsBackToPlainText()
    {
        var service = new HighlightingService([]);

        var result = service.Highlight("var x = 1;", "csharp");

        result.ShouldBe("var x = 1;");
    }

    [Fact]
    public void Highlight_SingleHighlighterForLanguage_ReturnsThatHighlighterOutput()
    {
        var service = new HighlightingService([new CSharpHighlighter()]);

        var result = service.Highlight("var x = 1;", "csharp");

        result.ShouldBe("<span class=\"cs\">var x = 1;</span>");
    }

    [Fact]
    public void Highlight_MultipleHighlightersForSameLanguage_HighestPriorityWins()
    {
        var service = new HighlightingService([new CSharpHighlighter(), new PremiumCSharpHighlighter()]);

        var result = service.Highlight("var x = 1;", "csharp");

        result.ShouldBe("<span class=\"premium-cs\">var x = 1;</span>");
    }

    [Fact]
    public void Highlight_LanguageNotSupported_FallsBackToPlainText()
    {
        var service = new HighlightingService([new CSharpHighlighter()]);

        var result = service.Highlight("print('hello')", "python");

        result.ShouldBe("print(&#39;hello&#39;)");
    }

    [Fact]
    public void Highlight_WildcardHighlighterAtHigherPriority_BeatsPlainTextFallback()
    {
        var service = new HighlightingService([new HighPriorityWildcardHighlighter()]);

        var result = service.Highlight("anything", "rust");

        result.ShouldBe("<span class=\"wild\">anything</span>");
    }

    [Fact]
    public void HasHighlighter_SupportedLanguage_ReturnsTrue()
    {
        var service = new HighlightingService([new CSharpHighlighter(), new PythonHighlighter()]);

        service.HasHighlighter("csharp").ShouldBeTrue();
        service.HasHighlighter("python").ShouldBeTrue();
    }

    [Fact]
    public void HasHighlighter_UnsupportedLanguage_ReturnsFalse()
    {
        var service = new HighlightingService([new CSharpHighlighter()]);

        service.HasHighlighter("rust").ShouldBeFalse();
    }

    [Fact]
    public void Highlight_HtmlSpecialChars_AreEncodedByFallback()
    {
        var service = new HighlightingService([]);

        var result = service.Highlight("<div>", "html");

        result.ShouldBe("&lt;div&gt;");
    }

    // -- Stub highlighters ------------------------------------------------

    private class CSharpHighlighter : ICodeHighlighter
    {
        public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "csharp", "cs" };
        public int Priority => 50;
        public string Highlight(string code, string language) => $"<span class=\"cs\">{code}</span>";
    }

    private class PremiumCSharpHighlighter : ICodeHighlighter
    {
        public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "csharp", "cs" };
        public int Priority => 100;
        public string Highlight(string code, string language) => $"<span class=\"premium-cs\">{code}</span>";
    }

    private class PythonHighlighter : ICodeHighlighter
    {
        public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "python", "py" };
        public int Priority => 50;
        public string Highlight(string code, string language) => $"<span class=\"py\">{code}</span>";
    }

    private class HighPriorityWildcardHighlighter : ICodeHighlighter
    {
        public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "*" };
        public int Priority => 10;
        public string Highlight(string code, string language) => $"<span class=\"wild\">{code}</span>";
    }
}
