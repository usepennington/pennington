namespace Pennington.Highlighting;

public interface ICodeHighlighter
{
    /// <summary>Languages this highlighter handles (e.g., "csharp", "python").</summary>
    IReadOnlySet<string> SupportedLanguages { get; }

    /// <summary>Highlight code. Returns HTML with spans.</summary>
    string Highlight(string code, string language);

    /// <summary>Priority — higher wins when multiple highlighters support a language.</summary>
    int Priority { get; }
}
