namespace Pennington.Markdown.Extensions;

using System.Net;
using Highlighting;

/// <summary>
/// Shared rendering pipeline for fenced code blocks. Runs registered
/// <see cref="ICodeBlockPreprocessor"/>s first, falls back to
/// <see cref="HighlightingService"/> on the base language, applies
/// <see cref="CodeTransformer"/>, and wraps the result via
/// <see cref="CodeBlockHtmlBuilder"/>. Used by both the Markdig renderer
/// and the <c>&lt;CodeBlock&gt;</c> Razor component so markdown fences and
/// Razor usages produce identical HTML.
/// </summary>
public sealed class CodeBlockRenderingService
{
    private readonly HighlightingService _highlightingService;
    private readonly IReadOnlyList<ICodeBlockPreprocessor> _preprocessors;

    /// <summary>Creates the service with a highlighting dispatcher and the registered preprocessors (ordered by descending priority at construction).</summary>
    public CodeBlockRenderingService(
        HighlightingService highlightingService,
        IEnumerable<ICodeBlockPreprocessor>? preprocessors = null)
    {
        _highlightingService = highlightingService ?? throw new ArgumentNullException(nameof(highlightingService));
        _preprocessors = (preprocessors ?? [])
            .OrderByDescending(p => p.Priority)
            .ToList();
    }

    /// <summary>
    /// Renders <paramref name="code"/> with language tag <paramref name="languageId"/> through the full pipeline.
    /// </summary>
    public string Render(
        string code,
        string languageId,
        CodeHighlightRenderOptions? options = null,
        bool isInTabGroup = false)
    {
        var effectiveOptions = options ?? CodeHighlightRenderOptions.Default;

        foreach (var preprocessor in _preprocessors)
        {
            var result = preprocessor.TryProcess(code, languageId);
            if (result != null)
            {
                var html = result.SkipTransform
                    ? result.HighlightedHtml
                    : CodeTransformer.Transform(result.HighlightedHtml);
                return CodeBlockHtmlBuilder.BuildHtml(html, effectiveOptions, isInTabGroup, languageId);
            }
        }

        try
        {
            var baseLanguage = ParseBaseLanguage(languageId);
            var highlightedHtml = _highlightingService.Highlight(code, baseLanguage);

            if (baseLanguage.ToLowerInvariant() is not ("markdown" or "md"))
            {
                highlightedHtml = CodeTransformer.Transform(highlightedHtml);
            }

            return CodeBlockHtmlBuilder.BuildHtml(highlightedHtml, effectiveOptions, isInTabGroup, languageId);
        }
        catch
        {
            var fallbackHtml = $"<pre><code>{WebUtility.HtmlEncode(code)}</code></pre>";
            return CodeBlockHtmlBuilder.BuildHtml(fallbackHtml, effectiveOptions, isInTabGroup, languageId);
        }
    }

    private static string ParseBaseLanguage(string languageId)
    {
        var trimmed = languageId.Trim();
        var colonIndex = trimmed.IndexOf(':');
        return colonIndex >= 0 ? trimmed[..colonIndex] : trimmed;
    }
}
