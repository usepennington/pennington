namespace Pennington.Roslyn.Highlighting;

using Pennington.Highlighting;

/// <summary>
/// Roslyn-based code highlighter for C# and VB. Priority 100 (beats TextMate at 50).
/// Uses AdhocWorkspace + Classifier API -- no solution workspace needed.
/// </summary>
public sealed class RoslynHighlighter : ICodeHighlighter
{
    private readonly SyntaxHighlighter _highlighter;

    /// <summary>Creates a new highlighter that delegates to the supplied <see cref="SyntaxHighlighter"/>.</summary>
    public RoslynHighlighter(SyntaxHighlighter highlighter)
    {
        _highlighter = highlighter;
    }

    /// <inheritdoc />
    public IReadOnlySet<string> SupportedLanguages { get; } =
        new HashSet<string> { "csharp", "cs", "c#", "vb", "vbnet" };

    /// <inheritdoc />
    public int Priority => 100;

    /// <inheritdoc />
    public string Highlight(string code, string language)
    {
        var lang = language.ToLowerInvariant() switch
        {
            "vb" or "vbnet" => SyntaxHighlighter.Language.VisualBasic,
            _ => SyntaxHighlighter.Language.CSharp
        };

        return _highlighter.Highlight(code, lang);
    }
}