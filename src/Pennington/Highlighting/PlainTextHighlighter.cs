namespace Pennington.Highlighting;

using System.Net;

/// <summary>Fallback highlighter — HTML-encodes code, no syntax highlighting.</summary>
public sealed class PlainTextHighlighter : ICodeHighlighter
{
    private static readonly HashSet<string> _all = ["*"];

    public IReadOnlySet<string> SupportedLanguages => _all;

    public string Highlight(string code, string language)
        => WebUtility.HtmlEncode(code);

    public int Priority => 0;
}