namespace Pennington.Highlighting;

using System.Text;
using System.Text.RegularExpressions;
using Infrastructure;

/// <summary>
/// Provides syntax highlighting for shell/bash/batch commands.
/// Implements ICodeHighlighter with priority 75 (higher than TextMate for shell languages).
/// </summary>
public sealed partial class ShellHighlighter : ICodeHighlighter
{
    /// <inheritdoc/>
    public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "bash", "shell", "sh" };

    /// <inheritdoc/>
    public int Priority => 75;

    /// <inheritdoc/>
    public string Highlight(string code, string language)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("<pre><code>");

        var lines = code.SplitNewLines();

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');

            // Highlight comments first
            if (CommentRegex().IsMatch(line))
            {
                sb.Append($"<span class=\"hljs-comment\">{System.Net.WebUtility.HtmlEncode(line)}</span>\n");
                continue;
            }

            if (RemCommentRegex().IsMatch(line))
            {
                sb.Append($"<span class=\"hljs-comment\">{System.Net.WebUtility.HtmlEncode(line)}</span>\n");
                continue;
            }

            // Find the command (first word)
            var match = FirstCommandRegex().Match(line);
            var index = 0;

            if (match.Success)
            {
                // Leading whitespace
                sb.Append(System.Net.WebUtility.HtmlEncode(match.Groups[1].Value));
                // Command itself
                sb.Append($"<span class=\"hljs-built_in\">{System.Net.WebUtility.HtmlEncode(match.Groups[2].Value)}</span>");
                index = match.Length;
            }

            // Rest of the line: highlight strings, then flags inside the non-string
            // gaps. Every literal run is HTML-encoded so shell metacharacters
            // (<, >, &) can't leak as markup — the output is re-parsed downstream.
            sb.Append(HighlightRest(line[index..]));
            sb.Append('\n');
        }

        sb.Append("</code></pre>");
        return sb.ToString();
    }

    // Strings take precedence; flags are highlighted only in the gaps between
    // string spans. Literal text in every gap is HTML-encoded.
    private static string HighlightRest(string rest)
    {
        var sb = new StringBuilder();
        var pos = 0;
        foreach (Match m in StringRegex().Matches(rest))
        {
            AppendWithFlags(sb, rest[pos..m.Index]);
            sb.Append($"<span class=\"hljs-string\">{Encode(m.Value)}</span>");
            pos = m.Index + m.Length;
        }

        AppendWithFlags(sb, rest[pos..]);
        return sb.ToString();
    }

    private static void AppendWithFlags(StringBuilder sb, string text)
    {
        var pos = 0;
        foreach (Match m in FlagsRegex().Matches(text))
        {
            sb.Append(Encode(text[pos..m.Index]));
            sb.Append($"<span class=\"hljs-params\">{Encode(m.Value)}</span>");
            pos = m.Index + m.Length;
        }

        sb.Append(Encode(text[pos..]));
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);

    [GeneratedRegex(@"^\s*#")]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"^\s*(REM|rem)\b")]
    private static partial Regex RemCommentRegex();

    [GeneratedRegex(@"^(\s*)(\S+)")]
    private static partial Regex FirstCommandRegex();

    [GeneratedRegex(@"(['""])(.*?)(\1)")]
    private static partial Regex StringRegex();

    [GeneratedRegex(@"(?<=\s)([-/][\w-]+)")]
    private static partial Regex FlagsRegex();
}