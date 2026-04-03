namespace Penn.Roslyn.Highlighting;

using Penn.Highlighting;

/// <summary>
/// Roslyn-based code highlighter for C# and VB. Priority 100 (beats TextMate at 50).
/// Uses AdhocWorkspace + Classifier API -- no solution workspace needed.
/// </summary>
public sealed class RoslynHighlighter : ICodeHighlighter, IDisposable
{
    private readonly SyntaxHighlighter _highlighter = new();

    public IReadOnlySet<string> SupportedLanguages { get; } =
        new HashSet<string> { "csharp", "cs", "c#", "vb", "vbnet" };

    public int Priority => 100;

    public string Highlight(string code, string language)
    {
        var lang = language.ToLowerInvariant() switch
        {
            "vb" or "vbnet" => SyntaxHighlighter.Language.VisualBasic,
            _ => SyntaxHighlighter.Language.CSharp
        };

        return _highlighter.Highlight(code, lang);
    }

    public void Dispose() => _highlighter.Dispose();
}
