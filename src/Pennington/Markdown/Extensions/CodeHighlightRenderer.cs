namespace Pennington.Markdown.Extensions;

using System.Text;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Pennington.Highlighting;
using Pennington.Markdown.Extensions.Tabs;

/// <summary>
/// Custom Markdig renderer for fenced code blocks that:
/// 1. Extracts language from fenced code block info
/// 2. Calls HighlightingService.Highlight(code, language) to get highlighted HTML
/// 3. Calls CodeTransformer.Transform() to apply line annotations
/// 4. Calls CodeBlockHtmlBuilder.BuildHtml() to wrap in standard structure
/// </summary>
internal sealed class CodeHighlightRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly HighlightingService _highlightingService;
    private readonly Func<CodeHighlightRenderOptions> _optionsFactory;
    private readonly IReadOnlyList<ICodeBlockPreprocessor> _preprocessors;

    public CodeHighlightRenderer(
        HighlightingService highlightingService,
        Func<CodeHighlightRenderOptions>? optionsFactory = null,
        IEnumerable<ICodeBlockPreprocessor>? preprocessors = null)
    {
        _highlightingService = highlightingService ?? throw new ArgumentNullException(nameof(highlightingService));
        _optionsFactory = optionsFactory ?? (() => CodeHighlightRenderOptions.Default);
        _preprocessors = (preprocessors ?? [])
            .OrderByDescending(p => p.Priority)
            .ToList();
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock codeBlock)
    {
        if (!TryExtractFencedCodeBlock(codeBlock, out var languageId, out var code))
        {
            return;
        }

        var isInTabGroup = codeBlock.Parent is TabbedCodeBlock;
        var options = _optionsFactory();

        foreach (var preprocessor in _preprocessors)
        {
            var result = preprocessor.TryProcess(code, languageId);
            if (result != null)
            {
                var html = result.SkipTransform
                    ? result.HighlightedHtml
                    : CodeTransformer.Transform(result.HighlightedHtml);
                var wrappedHtml = CodeBlockHtmlBuilder.BuildHtml(html, options, isInTabGroup);
                renderer.Write(wrappedHtml);
                return;
            }
        }

        try
        {
            // Parse language (strip any modifiers like ":path")
            var baseLanguage = ParseBaseLanguage(languageId);

            // Step 1: Highlight code
            var highlightedHtml = _highlightingService.Highlight(code, baseLanguage);

            // Step 2: Apply transformations (line highlighting, focus, diff, etc.)
            if (baseLanguage.ToLowerInvariant() is not ("markdown" or "md"))
            {
                highlightedHtml = CodeTransformer.Transform(highlightedHtml);
            }

            // Step 3: Wrap in standard HTML structure
            var wrappedHtml = CodeBlockHtmlBuilder.BuildHtml(highlightedHtml, options, isInTabGroup);
            renderer.Write(wrappedHtml);
        }
        catch
        {
            // On error, return encoded plain text in standard structure
            var fallbackHtml = $"<pre><code>{System.Net.WebUtility.HtmlEncode(code)}</code></pre>";
            var wrappedFallback = CodeBlockHtmlBuilder.BuildHtml(fallbackHtml, options, isInTabGroup);
            renderer.Write(wrappedFallback);
        }
    }

    private static string ParseBaseLanguage(string languageId)
    {
        var trimmed = languageId.Trim();
        var colonIndex = trimmed.IndexOf(':');
        return colonIndex >= 0 ? trimmed[..colonIndex] : trimmed;
    }

    private static bool TryExtractFencedCodeBlock(CodeBlock codeBlock, out string languageId, out string code)
    {
        languageId = "";
        code = "";

        if (codeBlock is not FencedCodeBlock fencedCodeBlock ||
            codeBlock.Parser is not FencedCodeBlockParser fencedCodeBlockParser ||
            fencedCodeBlock.Info == null ||
            fencedCodeBlockParser.InfoPrefix == null)
        {
            return false;
        }

        languageId = fencedCodeBlock.Info.Replace(fencedCodeBlockParser.InfoPrefix, string.Empty);
        code = ExtractCode(codeBlock);
        return true;
    }

    private static string ExtractCode(LeafBlock leafBlock)
    {
        var code = new StringBuilder();

        var lines = leafBlock.Lines.Lines ?? [];
        var totalLines = lines.Length;

        for (var index = 0; index < totalLines; index++)
        {
            var line = lines[index];
            var slice = line.Slice;

            if (slice.Text == null)
            {
                continue;
            }

            var lineText = slice.Text.Substring(slice.Start, slice.Length);

            if (index > 0)
            {
                code.AppendLine();
            }

            code.Append(lineText);
        }

        return code.ToString().Trim();
    }
}
