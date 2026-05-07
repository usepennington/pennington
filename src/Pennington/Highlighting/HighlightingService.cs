namespace Pennington.Highlighting;

using System.Collections.Concurrent;
using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dispatches code highlighting to the highest-priority ICodeHighlighter
/// that supports the requested language. Falls back to PlainTextHighlighter
/// and emits an Info diagnostic once per unknown language per instance.
/// </summary>
/// <remarks>
/// Composing service registered as singleton. Holds two pieces of process-lifetime state:
/// the priority-sorted highlighter list (immutable once the container builds it) and a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> deduping unknown-language Info
/// diagnostics. Neither piece is file-derived, so file-watched lifetime is unnecessary.
/// </remarks>
public sealed class HighlightingService
{
    private readonly IReadOnlyList<ICodeHighlighter> _highlighters;
    private readonly PlainTextHighlighter _fallback = new();
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ConcurrentDictionary<string, byte> _seenUnknownLanguages =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes the service with the registered highlighters, ordered by descending <see cref="ICodeHighlighter.Priority"/>.
    /// </summary>
    public HighlightingService(IEnumerable<ICodeHighlighter> highlighters)
        : this(highlighters, httpContextAccessor: null)
    {
    }

    /// <summary>
    /// Initializes the service with the registered highlighters and an optional HTTP context accessor used to
    /// surface unknown-language Info diagnostics to the per-request <see cref="DiagnosticContext"/>.
    /// </summary>
    public HighlightingService(IEnumerable<ICodeHighlighter> highlighters, IHttpContextAccessor? httpContextAccessor)
    {
        // Sort by priority descending so we can pick the first match
        _highlighters = highlighters
            .OrderByDescending(h => h.Priority)
            .ToList();
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Highlight code using the best available highlighter for the language.
    /// Returns the highlighted HTML string.
    /// </summary>
    public string Highlight(string code, string language)
    {
        var highlighter = FindHighlighter(language, out var isFallback);
        if (isFallback && !string.IsNullOrWhiteSpace(language))
        {
            ReportUnknownLanguage(language);
        }

        return highlighter.Highlight(code, language);
    }

    /// <summary>
    /// Returns true if any registered highlighter (not the fallback) supports this language.
    /// </summary>
    public bool HasHighlighter(string language)
        => _highlighters.Any(h => h.SupportedLanguages.Contains(language));

    private ICodeHighlighter FindHighlighter(string language, out bool isFallback)
    {
        foreach (var h in _highlighters)
        {
            if (h.SupportedLanguages.Contains(language) || h.SupportedLanguages.Contains("*"))
            {
                isFallback = false;
                return h;
            }
        }

        isFallback = true;
        return _fallback;
    }

    private void ReportUnknownLanguage(string language)
    {
        if (!_seenUnknownLanguages.TryAdd(language, 0))
        {
            return;
        }

        var diagnostics = _httpContextAccessor?.HttpContext?.RequestServices.GetService<DiagnosticContext>();
        diagnostics?.AddInfo(
            $"Unknown code-fence language '{language}'; rendered as plain text.",
            source: nameof(HighlightingService));
    }
}
