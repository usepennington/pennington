namespace Penn.Highlighting;

using System.Text;
using System.Text.RegularExpressions;
using Penn.Infrastructure;

/// <summary>
/// Provides syntax highlighting for shell/bash/batch commands.
/// Implements ICodeHighlighter with priority 75 (higher than TextMate for shell languages).
/// </summary>
public sealed partial class ShellHighlighter : ICodeHighlighter
{
    public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "bash", "shell", "sh" };

    public int Priority => 75;

    public string Highlight(string code, string language)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

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

            // Rest of the line
            var rest = line[index..];

            // Strings (in single or double quotes)
            rest = StringRegex().Replace(rest,
                m => $"<span class=\"hljs-string\">{System.Net.WebUtility.HtmlEncode(m.Value)}</span>");

            // Flags/options
            rest = FlagsRegex().Replace(rest,
                m => $"<span class=\"hljs-params\">{System.Net.WebUtility.HtmlEncode(m.Value)}</span>");

            sb.Append(rest);
            sb.Append('\n');
        }

        sb.Append("</code></pre>");
        return sb.ToString();
    }

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
