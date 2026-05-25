namespace Pennington.TreeSitter.Preprocessing;

using System.Net;
using Fragments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Diagnostics;
using Pennington.Highlighting;
using Pennington.Markdown.Extensions;

/// <summary>
/// Preprocesses code blocks with a <c>:symbol</c> modifier (e.g. <c>python:symbol</c>) by extracting the
/// referenced source via tree-sitter and rendering it with the shared highlighting service. A <c>:symbol-diff</c>
/// variant emits a unified diff between two referenced fragments.
/// </summary>
public sealed class TreeSitterCodeBlockPreprocessor : ICodeBlockPreprocessor
{
    private readonly ISourceFragmentService _fragmentService;
    private readonly HighlightingService _highlightingService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Creates the preprocessor wired to the fragment service, shared highlighting service, and per-request diagnostics accessor.</summary>
    public TreeSitterCodeBlockPreprocessor(
        ISourceFragmentService fragmentService,
        HighlightingService highlightingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _fragmentService = fragmentService;
        _highlightingService = highlightingService;
        _httpContextAccessor = httpContextAccessor;
    }

    private DiagnosticContext? Diagnostics =>
        _httpContextAccessor.HttpContext?.RequestServices.GetService<DiagnosticContext>();

    /// <inheritdoc />
    public int Priority => 100;

    /// <inheritdoc />
    public CodeBlockPreprocessResult? TryProcess(string code, string languageId)
    {
        var (baseLanguage, modifier, options) = ParseLanguageId(languageId);
        return modifier switch
        {
            "symbol" => ProcessSymbol(baseLanguage, code, options),
            // Imports and outline elision make no sense across a diff; honor only the body-only flag.
            "symbol-diff" => ProcessSymbolDiff(baseLanguage, code, new FragmentOptions { BodyOnly = options.BodyOnly }),
            _ => null,
        };
    }

    private CodeBlockPreprocessResult ProcessSymbol(string baseLanguage, string code, FragmentOptions options)
    {
        try
        {
            var fragments = new List<string>();
            foreach (var line in code.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var (filePath, namePath) = ParseReference(line);
                var result = _fragmentService.GetFragment(baseLanguage, filePath, namePath, options);
                if (!result.Succeeded)
                {
                    Diagnostics?.AddWarning($"tree-sitter :symbol — {result.Error}");
                    fragments.Add($"""<span class="comment">// Error: {WebUtility.HtmlEncode(result.Error)}</span>""");
                    continue;
                }

                var highlighted = _highlightingService.Highlight(result.Text!, baseLanguage);
                fragments.Add(ExtractInnerCodeHtml(highlighted));
            }

            var inner = string.Join("\n\n", fragments);
            var wrappedHtml = $"""<pre><code class="language-{baseLanguage} highlighted">{inner}</code></pre>""";
            return new CodeBlockPreprocessResult(wrappedHtml, baseLanguage, SkipTransform: false);
        }
        catch (Exception ex)
        {
            Diagnostics?.AddError($"Error processing :symbol — {ex.Message}");
            var errorHtml = $"<pre><code><!-- Error processing :symbol: {WebUtility.HtmlEncode(ex.Message)} -->{WebUtility.HtmlEncode(code)}</code></pre>";
            return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
        }
    }

    private CodeBlockPreprocessResult ProcessSymbolDiff(string baseLanguage, string code, FragmentOptions options)
    {
        try
        {
            var references = code.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (references.Length != 2)
            {
                Diagnostics?.AddError($"tree-sitter :symbol-diff requires exactly 2 references, got {references.Length}");
                var errorHtml = $"<pre><code><!-- Error: :symbol-diff requires exactly 2 references, got {references.Length} -->{WebUtility.HtmlEncode(code)}</code></pre>";
                return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
            }

            var fragment1 = ResolveFragment(baseLanguage, references[0], options);
            var fragment2 = ResolveFragment(baseLanguage, references[1], options);

            if (!fragment1.Succeeded || !fragment2.Succeeded)
            {
                var errors = new List<string>();
                if (!fragment1.Succeeded)
                {
                    errors.Add(fragment1.Error!);
                }

                if (!fragment2.Succeeded)
                {
                    errors.Add(fragment2.Error!);
                }

                foreach (var error in errors)
                {
                    Diagnostics?.AddWarning($"tree-sitter :symbol-diff — {error}");
                }

                var errorHtml = string.Join("\n", errors.Select(e =>
                    $"""<span class="comment">// {WebUtility.HtmlEncode(e)}</span>"""));
                return new CodeBlockPreprocessResult($"<pre><code>{errorHtml}</code></pre>", baseLanguage, SkipTransform: true);
            }

            var html1 = ExtractInnerCodeHtml(_highlightingService.Highlight(fragment1.Text!, baseLanguage));
            var html2 = ExtractInnerCodeHtml(_highlightingService.Highlight(fragment2.Text!, baseLanguage));

            var diff = ComputeAndRenderDiff(html1, html2, fragment1.Text!, fragment2.Text!);

            var preClass = diff.HasDifferences ? " class=\"has-diff\"" : "";
            var wrappedHtml = $"<pre{preClass}><code>{diff.Html}</code></pre>";

            return new CodeBlockPreprocessResult(wrappedHtml, baseLanguage, SkipTransform: true);
        }
        catch (Exception ex)
        {
            Diagnostics?.AddError($"Error processing :symbol-diff — {ex.Message}");
            var errorHtml = $"<pre><code><!-- Error processing :symbol-diff: {WebUtility.HtmlEncode(ex.Message)} -->{WebUtility.HtmlEncode(code)}</code></pre>";
            return new CodeBlockPreprocessResult(errorHtml, baseLanguage, SkipTransform: true);
        }
    }

    private FragmentResult ResolveFragment(string baseLanguage, string reference, FragmentOptions options)
    {
        var (filePath, namePath) = ParseReference(reference);
        return _fragmentService.GetFragment(baseLanguage, filePath, namePath, options);
    }

    /// <summary>
    /// Splits an info-string such as <c>python:symbol,bodyonly</c> or <c>python:symbol-diff</c> into its base
    /// language, the modifier (<c>symbol</c>, <c>symbol-diff</c>, or null when absent), and the
    /// <see cref="FragmentOptions"/> parsed from the comma-separated flag tail.
    /// </summary>
    internal static (string baseLanguage, string? modifier, FragmentOptions options) ParseLanguageId(string languageId)
    {
        var trimmed = languageId.Trim();
        const string diffMarker = ":symbol-diff";
        const string marker = ":symbol";

        // Check :symbol-diff before :symbol so the shared :symbol prefix doesn't match first.
        var diffIndex = trimmed.IndexOf(diffMarker, StringComparison.OrdinalIgnoreCase);
        if (diffIndex >= 0)
        {
            return (trimmed[..diffIndex], "symbol-diff", ParseFlags(trimmed[(diffIndex + diffMarker.Length)..]));
        }

        var index = trimmed.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return (trimmed, null, FragmentOptions.Default);
        }

        return (trimmed[..index], "symbol", ParseFlags(trimmed[(index + marker.Length)..]));
    }

    /// <summary>Reads the comma-separated flag tail after a <c>:symbol</c> marker into a <see cref="FragmentOptions"/>; unknown flags are ignored.</summary>
    private static FragmentOptions ParseFlags(string afterMarker)
    {
        var flags = afterMarker.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new FragmentOptions
        {
            BodyOnly = flags.Any(flag => flag.Equals("bodyonly", StringComparison.OrdinalIgnoreCase)),
            IncludeImports = flags.Any(flag => flag.Equals("imports", StringComparison.OrdinalIgnoreCase)),
            SignaturesOnly = flags.Any(flag => flag.Equals("signatures", StringComparison.OrdinalIgnoreCase)),
        };
    }

    /// <summary>Splits a body line into a file path and member name path on the <c>" &gt; "</c> separator; a bare path yields an empty name path.</summary>
    internal static (string filePath, string namePath) ParseReference(string line)
    {
        const string separator = " > ";
        var index = line.IndexOf(separator, StringComparison.Ordinal);
        return index < 0
            ? (line.Trim(), string.Empty)
            : (line[..index].Trim(), line[(index + separator.Length)..].Trim());
    }

    /// <summary>
    /// Computes a line-level diff between two highlighted fragments and renders each line as a
    /// <c>&lt;span class="line"&gt;</c>, tagging removed lines (from the first fragment) <c>diff-remove</c> and
    /// inserted lines (from the second) <c>diff-add</c>. Whitespace-only changes are ignored.
    /// </summary>
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
            // Unchanged lines before this diff block.
            while (processedLinesA < diffBlock.DeleteStartA)
            {
                if (processedLinesA < htmlLines1.Length)
                {
                    outputLines.Add($"""<span class="line">{htmlLines1[processedLinesA]}</span>""");
                }

                processedLinesA++;
            }

            // Deleted lines (from the first fragment).
            for (var i = 0; i < diffBlock.DeleteCountA; i++)
            {
                var lineIndex = diffBlock.DeleteStartA + i;
                if (lineIndex < htmlLines1.Length)
                {
                    outputLines.Add($"""<span class="line diff-remove">{htmlLines1[lineIndex]}</span>""");
                }
            }

            // Inserted lines (from the second fragment).
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

        // Trailing unchanged lines.
        while (processedLinesA < htmlLines1.Length)
        {
            outputLines.Add($"""<span class="line">{htmlLines1[processedLinesA]}</span>""");
            processedLinesA++;
        }

        return new DiffRenderResult(string.Join("\n", outputLines), hasDifferences);
    }

    /// <summary>Extracts the inner HTML between the highlighter's <c>&lt;code&gt;</c> and <c>&lt;/code&gt;&lt;/pre&gt;</c> tags.</summary>
    private static string ExtractInnerCodeHtml(string html)
    {
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
        return endIndex == -1 ? html : html[contentStart..endIndex];
    }

    private static string[] SplitNewLines(string s) =>
        s.ReplaceLineEndings(Environment.NewLine).Split(Environment.NewLine);

    private sealed record DiffRenderResult(string Html, bool HasDifferences);
}
