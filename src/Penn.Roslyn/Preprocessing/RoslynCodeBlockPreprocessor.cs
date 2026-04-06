namespace Penn.Roslyn.Preprocessing;

using System.Net;
using Penn.Generation;
using Penn.Markdown.Extensions;
using Penn.Roslyn.Highlighting;
using Penn.Roslyn.Symbols;
using Penn.Roslyn.Utilities;

/// <summary>
/// Preprocesses code blocks with :xmldocid, :path, and :xmldocid-diff modifiers.
/// </summary>
public sealed class RoslynCodeBlockPreprocessor : ICodeBlockPreprocessor
{
    private readonly ISymbolExtractionService _symbolService;
    private readonly SyntaxHighlighter _highlighter;
    private readonly RoslynOptions _options;
    private readonly BuildDiagnosticsCollector _diagnostics;

    public RoslynCodeBlockPreprocessor(
        ISymbolExtractionService symbolService,
        SyntaxHighlighter highlighter,
        RoslynOptions options,
        BuildDiagnosticsCollector diagnostics)
    {
        _symbolService = symbolService;
        _highlighter = highlighter;
        _options = options;
        _diagnostics = diagnostics;
    }

    public int Priority => 100;

    public CodeBlockPreprocessResult? TryProcess(string code, string languageId)
    {
        var (baseLanguage, modifier) = ParseLanguageId(languageId);
        if (modifier is null)
        {
            return null;
        }

        return modifier switch
        {
            "xmldocid" => ProcessXmlDocId(baseLanguage, code, bodyOnly: false),
            "xmldocid,bodyonly" => ProcessXmlDocId(baseLanguage, code, bodyOnly: true),
            "xmldocid-diff" => ProcessXmlDocIdDiff(baseLanguage, code),
            "xmldocid-diff,bodyonly" => ProcessXmlDocIdDiff(baseLanguage, code, bodyOnly: true),
            "path" => ProcessPath(baseLanguage, code),
            _ => null
        };
    }

    internal static (string baseLanguage, string? modifier) ParseLanguageId(string languageId)
    {
        var trimmed = languageId.Trim();
        const string xmlDocIdDiffMarker = ":xmldocid-diff";
        const string xmlDocIdMarker = ":xmldocid";
        const string pathMarker = ":path";

        // Check for :xmldocid-diff first (before :xmldocid) to avoid false prefix match
        if (trimmed.Contains(xmlDocIdDiffMarker, StringComparison.OrdinalIgnoreCase))
        {
            var baseIndex = trimmed.IndexOf(xmlDocIdDiffMarker, StringComparison.OrdinalIgnoreCase);
            var baseLanguage = trimmed[..baseIndex];
            var modifierPart = trimmed.Contains(",bodyonly", StringComparison.OrdinalIgnoreCase)
                ? "xmldocid-diff,bodyonly"
                : "xmldocid-diff";
            return (baseLanguage, modifierPart);
        }

        if (trimmed.Contains(xmlDocIdMarker, StringComparison.OrdinalIgnoreCase))
        {
            var baseIndex = trimmed.IndexOf(xmlDocIdMarker, StringComparison.OrdinalIgnoreCase);
            var baseLanguage = trimmed[..baseIndex];
            var modifierPart = trimmed.Contains(",bodyonly", StringComparison.OrdinalIgnoreCase)
                ? "xmldocid,bodyonly"
                : "xmldocid";
            return (baseLanguage, modifierPart);
        }

        if (trimmed.Contains(pathMarker, StringComparison.OrdinalIgnoreCase))
        {
            var baseIndex = trimmed.IndexOf(pathMarker, StringComparison.OrdinalIgnoreCase);
            return (trimmed[..baseIndex], "path");
        }

        return (trimmed, null);
    }

    private CodeBlockPreprocessResult? ProcessXmlDocId(string baseLanguage, string code, bool bodyOnly)
    {
        try
        {
            var xmlDocIds = code.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var htmlFragments = new List<string>();

            foreach (var xmlDocId in xmlDocIds)
            {
                var fragment = AsyncHelpers.RunSync(() => _symbolService.ExtractCodeFragmentAsync(xmlDocId, bodyOnly));

                if (string.IsNullOrEmpty(fragment))
                {
                    _diagnostics.AddWarning(null, $"Unresolved xmldocid: {xmlDocId}");
                    var errorComment = $"""<span class="comment">// Error: Symbol not found for '{WebUtility.HtmlEncode(xmlDocId)}'</span>""";
                    htmlFragments.Add(errorComment);
                    continue;
                }

                var lang = DetectHighlighterLanguage(baseLanguage);
                var highlighted = _highlighter.Highlight(fragment, lang);
                var innerHtml = ExtractInnerCodeHtml(highlighted);
                htmlFragments.Add(innerHtml);
            }

            var combinedHtml = string.Join("\n\n", htmlFragments);
            var wrappedHtml = $"""<pre><code class="language-{baseLanguage} highlighted">{combinedHtml}</code></pre>""";

            return new CodeBlockPreprocessResult(wrappedHtml, baseLanguage, SkipTransform: false);
        }
        catch (Exception ex)
        {
            _diagnostics.AddError(null, $"Error processing xmldocid: {ex.Message}");
            var errorHtml = $"<pre><code><!-- Error processing xmldocid: {WebUtility.HtmlEncode(ex.Message)} -->{WebUtility.HtmlEncode(code)}</code></pre>";
            return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
        }
    }

    private CodeBlockPreprocessResult? ProcessXmlDocIdDiff(string baseLanguage, string code, bool bodyOnly = false)
    {
        try
        {
            var xmlDocIds = code.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (xmlDocIds.Length != 2)
            {
                _diagnostics.AddError(null, $"xmldocid-diff requires exactly 2 XmlDocIds, got {xmlDocIds.Length}");
                var errorHtml = $"<pre><code><!-- Error: xmldocid-diff requires exactly 2 XmlDocIds, got {xmlDocIds.Length} -->{WebUtility.HtmlEncode(code)}</code></pre>";
                return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
            }

            var fragment1 = AsyncHelpers.RunSync(() => _symbolService.ExtractCodeFragmentAsync(xmlDocIds[0], bodyOnly));
            var fragment2 = AsyncHelpers.RunSync(() => _symbolService.ExtractCodeFragmentAsync(xmlDocIds[1], bodyOnly));

            // Handle errors
            var errors = new List<string>();
            if (string.IsNullOrEmpty(fragment1))
            {
                errors.Add($"Symbol not found: {xmlDocIds[0]}");
            }

            if (string.IsNullOrEmpty(fragment2))
            {
                errors.Add($"Symbol not found: {xmlDocIds[1]}");
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    _diagnostics.AddWarning(null, $"Unresolved xmldocid-diff: {error}");
                var errorHtml = string.Join("\n", errors.Select(e =>
                    $"""<span class="comment">// {WebUtility.HtmlEncode(e)}</span>"""));
                return new CodeBlockPreprocessResult(
                    $"<pre><code>{errorHtml}</code></pre>",
                    baseLanguage,
                    SkipTransform: true);
            }

            // Highlight both fragments
            var lang = DetectHighlighterLanguage(baseLanguage);
            var highlighted1 = _highlighter.Highlight(fragment1, lang);
            var highlighted2 = _highlighter.Highlight(fragment2, lang);

            var html1 = ExtractInnerCodeHtml(highlighted1);
            var html2 = ExtractInnerCodeHtml(highlighted2);

            var diffResult = ComputeAndRenderDiff(html1, html2, fragment1, fragment2);

            var preClass = diffResult.HasDifferences ? " class=\"has-diff\"" : "";
            var wrappedHtml = $"<pre{preClass}><code>{diffResult.Html}</code></pre>";

            return new CodeBlockPreprocessResult(wrappedHtml, baseLanguage, SkipTransform: true);
        }
        catch (Exception ex)
        {
            _diagnostics.AddError(null, $"Error processing xmldocid-diff: {ex.Message}");
            var errorHtml = $"<pre><code><!-- Error processing xmldocid-diff: {WebUtility.HtmlEncode(ex.Message)} -->{WebUtility.HtmlEncode(code)}</code></pre>";
            return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
        }
    }

    private CodeBlockPreprocessResult? ProcessPath(string baseLanguage, string code)
    {
        try
        {
            var relativePath = code.Trim();

            // Validate path to prevent directory traversal
            if (relativePath.Contains("..") || Path.IsPathRooted(relativePath))
            {
                _diagnostics.AddError(null, $"Invalid file path: {relativePath}");
                var errorHtml = $"<pre><code><!-- Error: Invalid file path -->{WebUtility.HtmlEncode(code)}</code></pre>";
                return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
            }

            if (string.IsNullOrEmpty(_options.SolutionPath))
            {
                _diagnostics.AddError(null, "Solution path not configured for :path code block");
                var errorHtml = $"<pre><code><!-- Error: Solution path not configured -->{WebUtility.HtmlEncode(code)}</code></pre>";
                return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
            }

            var solutionDir = Path.GetDirectoryName(_options.SolutionPath);
            if (string.IsNullOrEmpty(solutionDir))
            {
                _diagnostics.AddError(null, "Solution directory not found for :path code block");
                var errorHtml = $"<pre><code><!-- Error: Solution directory not found -->{WebUtility.HtmlEncode(code)}</code></pre>";
                return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
            }

            var fullPath = Path.Combine(solutionDir, relativePath);
            if (!File.Exists(fullPath))
            {
                _diagnostics.AddWarning(null, $"File not found for :path code block: {relativePath}");
                var errorHtml = $"<pre><code><!-- Error: File not found: {WebUtility.HtmlEncode(relativePath)} -->{WebUtility.HtmlEncode(code)}</code></pre>";
                return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
            }

            var content = File.ReadAllText(fullPath);
            var lang = DetectHighlighterLanguage(baseLanguage, Path.GetExtension(relativePath));
            var highlighted = _highlighter.Highlight(content, lang);

            return new CodeBlockPreprocessResult(highlighted, baseLanguage, SkipTransform: false);
        }
        catch (Exception ex)
        {
            _diagnostics.AddError(null, $"Error loading file for :path code block: {ex.Message}");
            var errorHtml = $"<pre><code><!-- Error loading file: {WebUtility.HtmlEncode(ex.Message)} -->{WebUtility.HtmlEncode(code)}</code></pre>";
            return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
        }
    }

    private static DiffRenderResult ComputeAndRenderDiff(
        string highlightedHtml1,
        string highlightedHtml2,
        string plainText1,
        string plainText2)
    {
        var htmlLines1 = SplitNewLines(highlightedHtml1);
        var htmlLines2 = SplitNewLines(highlightedHtml2);

        var differ = new DiffPlex.Differ();
        var diffResult = differ.CreateLineDiffs(plainText1, plainText2, ignoreWhitespace: true);

        var outputLines = new List<string>();
        var processedLinesA = 0;
        var hasDifferences = diffResult.DiffBlocks.Count > 0;

        foreach (var diffBlock in diffResult.DiffBlocks)
        {
            // Add unchanged lines before this diff block
            while (processedLinesA < diffBlock.DeleteStartA)
            {
                if (processedLinesA < htmlLines1.Length)
                {
                    outputLines.Add($"""<span class="line">{htmlLines1[processedLinesA]}</span>""");
                }

                processedLinesA++;
            }

            // Add deleted lines (from first snippet)
            for (var i = 0; i < diffBlock.DeleteCountA; i++)
            {
                var lineIndex = diffBlock.DeleteStartA + i;
                if (lineIndex < htmlLines1.Length)
                {
                    outputLines.Add($"""<span class="line diff-remove">{htmlLines1[lineIndex]}</span>""");
                }
            }

            // Add inserted lines (from second snippet)
            for (var i = 0; i < diffBlock.InsertCountB; i++)
            {
                var lineIndex = diffBlock.InsertStartB + i;
                if (lineIndex < htmlLines2.Length)
                {
                    outputLines.Add($"""<span class="line diff-add">{htmlLines2[lineIndex]}</span>""");
                }
            }

            processedLinesA += diffBlock.DeleteCountA;
        }

        // Add any remaining unchanged lines from the end
        while (processedLinesA < htmlLines1.Length)
        {
            outputLines.Add($"""<span class="line">{htmlLines1[processedLinesA]}</span>""");
            processedLinesA++;
        }

        return new DiffRenderResult(string.Join("\n", outputLines), hasDifferences);
    }

    private static SyntaxHighlighter.Language DetectHighlighterLanguage(string baseLanguage, string? fileExtension = null)
    {
        // If we have a file extension, use it
        if (fileExtension is not null)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".vb" => SyntaxHighlighter.Language.VisualBasic,
                _ => SyntaxHighlighter.Language.CSharp
            };
        }

        return baseLanguage.ToLowerInvariant() switch
        {
            "vb" or "vbnet" => SyntaxHighlighter.Language.VisualBasic,
            _ => SyntaxHighlighter.Language.CSharp
        };
    }

    /// <summary>
    /// Extracts the inner HTML content from between pre/code tags.
    /// </summary>
    private static string ExtractInnerCodeHtml(string html)
    {
        // The SyntaxHighlighter produces: <pre><code class="language-csharp highlighted">...</code></pre>
        // We need to find the content between the opening code tag and the closing tags.
        const string closeTag = "</code></pre>";

        var codeTagStart = html.IndexOf("<code", StringComparison.Ordinal);
        if (codeTagStart == -1)
        {
            return html;
        }

        var contentStart = html.IndexOf('>', codeTagStart) + 1;
        if (contentStart == 0)
        {
            return html;
        }

        var endIndex = html.IndexOf(closeTag, contentStart, StringComparison.Ordinal);
        if (endIndex == -1)
        {
            return html;
        }

        return html[contentStart..endIndex];
    }

    private static string[] SplitNewLines(string s)
    {
        return s.ReplaceLineEndings(Environment.NewLine).Split(Environment.NewLine);
    }

    private sealed record DiffRenderResult(string Html, bool HasDifferences);
}
