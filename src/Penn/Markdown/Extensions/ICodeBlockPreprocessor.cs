namespace Penn.Markdown.Extensions;

/// <summary>
/// Preprocesses fenced code blocks before normal highlighting.
/// Implementations can intercept blocks with specific language modifiers
/// (e.g., "csharp:xmldocid") and provide pre-highlighted HTML.
/// </summary>
public interface ICodeBlockPreprocessor
{
    /// <summary>Priority — higher runs first.</summary>
    int Priority { get; }

    /// <summary>
    /// Attempts to preprocess a code block. Returns a result if handled, or null to pass through.
    /// </summary>
    CodeBlockPreprocessResult? TryProcess(string code, string languageId);
}

/// <summary>Result from a code block preprocessor.</summary>
/// <param name="HighlightedHtml">Fully highlighted HTML (wrapped in pre/code tags).</param>
/// <param name="BaseLanguage">The base language for CSS class purposes.</param>
/// <param name="SkipTransform">If true, skip CodeTransformer on the output.</param>
public record CodeBlockPreprocessResult(
    string HighlightedHtml,
    string BaseLanguage,
    bool SkipTransform = false);
