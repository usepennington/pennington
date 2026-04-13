namespace Pennington.Highlighting;

/// <summary>
/// Dispatches code highlighting to the highest-priority ICodeHighlighter
/// that supports the requested language. Falls back to PlainTextHighlighter.
/// </summary>
public sealed class HighlightingService
{
    private readonly IReadOnlyList<ICodeHighlighter> _highlighters;
    private readonly PlainTextHighlighter _fallback = new();

    public HighlightingService(IEnumerable<ICodeHighlighter> highlighters)
    {
        // Sort by priority descending so we can pick the first match
        _highlighters = highlighters
            .OrderByDescending(h => h.Priority)
            .ToList();
    }

    /// <summary>
    /// Highlight code using the best available highlighter for the language.
    /// Returns the highlighted HTML string.
    /// </summary>
    public string Highlight(string code, string language)
    {
        var highlighter = FindHighlighter(language);
        return highlighter.Highlight(code, language);
    }

    /// <summary>
    /// Returns true if any registered highlighter (not the fallback) supports this language.
    /// </summary>
    public bool HasHighlighter(string language)
        => _highlighters.Any(h => h.SupportedLanguages.Contains(language));

    private ICodeHighlighter FindHighlighter(string language)
    {
        foreach (var h in _highlighters)
        {
            if (h.SupportedLanguages.Contains(language) || h.SupportedLanguages.Contains("*"))
                return h;
        }

        return _fallback;
    }
}