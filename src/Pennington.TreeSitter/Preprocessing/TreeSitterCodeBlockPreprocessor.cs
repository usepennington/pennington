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
/// referenced source via tree-sitter and rendering it with the shared highlighting service.
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
        var (baseLanguage, modifier, bodyOnly) = ParseLanguageId(languageId);
        if (modifier is null)
        {
            return null;
        }

        try
        {
            var fragments = new List<string>();
            foreach (var line in code.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var (filePath, namePath) = ParseReference(line);
                var result = _fragmentService.GetFragment(baseLanguage, filePath, namePath, bodyOnly);
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

    /// <summary>
    /// Splits an info-string such as <c>python:symbol,bodyonly</c> into its base language, the <c>symbol</c>
    /// modifier (or null when absent), and whether the <c>bodyonly</c> flag is present.
    /// </summary>
    internal static (string baseLanguage, string? modifier, bool bodyOnly) ParseLanguageId(string languageId)
    {
        var trimmed = languageId.Trim();
        const string marker = ":symbol";

        var index = trimmed.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return (trimmed, null, false);
        }

        var baseLanguage = trimmed[..index];
        var afterMarker = trimmed[(index + marker.Length)..];
        var bodyOnly = afterMarker
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(flag => flag.Equals("bodyonly", StringComparison.OrdinalIgnoreCase));

        return (baseLanguage, "symbol", bodyOnly);
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
}
