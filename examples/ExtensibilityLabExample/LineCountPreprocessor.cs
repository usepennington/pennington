namespace ExtensibilityLabExample;

using System.Net;
using Pennington.Markdown.Extensions;

/// <summary>
/// Implements <see cref="ICodeBlockPreprocessor"/>. Intercepts fenced
/// code blocks tagged <c>linecount</c> and renders them inside a
/// <c>&lt;figure class="linecount"&gt;</c> wrapper that reports how many
/// lines the snippet spans. Returns <see langword="null"/> for any
/// other language so the default highlighter chain runs.
/// <para>
/// <see cref="CodeBlockPreprocessResult.SkipTransform"/> is <c>true</c>
/// because the output already contains the line count badge we want and
/// should not be touched by <c>CodeTransformer</c>'s <c>// [!code]</c>
/// annotation pass.
/// </para>
/// <para>
/// Backs how-to 2.3.20 <c>/how-to/extensibility/code-block-preprocessor</c>.
/// </para>
/// </summary>
public sealed class LineCountPreprocessor : ICodeBlockPreprocessor
{
    /// <summary>
    /// 500 — higher than the shipped Roslyn preprocessor (250) so
    /// <c>linecount</c> wins over any language-modifier preprocessor
    /// that might claim the same fence info string.
    /// </summary>
    public int Priority => 500;

    public CodeBlockPreprocessResult? TryProcess(string code, string languageId)
    {
        if (!string.Equals(languageId, "linecount", StringComparison.OrdinalIgnoreCase))
            return null;

        var lineCount = CountLines(code);
        var encoded = WebUtility.HtmlEncode(code);

        var html = $"""
            <figure class="linecount" data-extensibility-lab="line-count-preprocessor">
              <figcaption>Line count: <strong>{lineCount}</strong></figcaption>
              <pre><code>{encoded}</code></pre>
            </figure>
            """;

        return new CodeBlockPreprocessResult(
            HighlightedHtml: html,
            BaseLanguage: "linecount",
            SkipTransform: true);
    }

    private static int CountLines(string code)
    {
        if (string.IsNullOrEmpty(code)) return 0;
        var count = 1;
        foreach (var ch in code) if (ch == '\n') count++;
        // Trim trailing newline so "one\ntwo\n" counts as 2.
        if (code.EndsWith('\n')) count--;
        return count;
    }
}