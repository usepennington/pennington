namespace ExtensibilityLabExample;

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Pennington.Highlighting;

/// <summary>
/// Implements <see cref="ICodeHighlighter"/> for a fictional <c>pipeline</c>
/// DSL — pipelines of the form
/// <c>source "name" -&gt; filter where=paid | transform total=sum | sink "name"</c>.
/// <para>
/// Keywords (<c>source</c>, <c>filter</c>, <c>transform</c>, <c>sink</c>)
/// and arrows (<c>-&gt;</c>, <c>|</c>) get wrapped in spans with CSS
/// classes so the stylesheet can theme them. Unrecognized tokens are
/// HTML-encoded and left alone.
/// </para>
/// <para>
/// Priority 100 — above <see cref="TextMateHighlighter"/>'s default (50)
/// and below <see cref="ShellHighlighter"/>'s 75 so this highlighter only
/// owns the <c>pipeline</c> language and nothing else.
/// </para>
/// <para>
/// Backs how-to 2.3.30 <c>/how-to/extensibility/custom-highlighter</c>.
/// </para>
/// </summary>
public sealed partial class PipelineHighlighter : ICodeHighlighter
{
    private static readonly HashSet<string> _keywords = new(StringComparer.OrdinalIgnoreCase)
        { "source", "filter", "transform", "sink", "where" };

    /// <summary>The languages this highlighter claims.</summary>
    public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "pipeline" };

    /// <summary>Priority for highlighter dispatch — higher wins.</summary>
    public int Priority => 100;

    /// <summary>Produce the highlighted HTML for one fence's body.</summary>
    public string Highlight(string code, string language)
    {
        if (string.IsNullOrEmpty(code)) return string.Empty;

        var sb = new StringBuilder();
        sb.Append("<pre><code data-extensibility-lab=\"pipeline-highlighter\">");

        foreach (var rawLine in code.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var position = 0;

            while (position < line.Length)
            {
                // Arrow `->`
                if (position + 1 < line.Length && line[position] == '-' && line[position + 1] == '>')
                {
                    sb.Append("<span class=\"pipeline-arrow\">-&gt;</span>");
                    position += 2;
                    continue;
                }

                // Pipe `|`
                if (line[position] == '|')
                {
                    sb.Append("<span class=\"pipeline-pipe\">|</span>");
                    position++;
                    continue;
                }

                // String literal "..."
                if (line[position] == '"')
                {
                    var end = line.IndexOf('"', position + 1);
                    if (end > 0)
                    {
                        var literal = line[position..(end + 1)];
                        sb.Append("<span class=\"pipeline-string\">");
                        sb.Append(WebUtility.HtmlEncode(literal));
                        sb.Append("</span>");
                        position = end + 1;
                        continue;
                    }
                }

                // Identifier / keyword
                var identMatch = IdentifierRegex().Match(line, position);
                if (identMatch.Success && identMatch.Index == position)
                {
                    var word = identMatch.Value;
                    if (_keywords.Contains(word))
                    {
                        sb.Append("<span class=\"pipeline-keyword\">");
                        sb.Append(WebUtility.HtmlEncode(word));
                        sb.Append("</span>");
                    }
                    else
                    {
                        sb.Append(WebUtility.HtmlEncode(word));
                    }
                    position += word.Length;
                    continue;
                }

                // Fallback: encode one character and continue.
                sb.Append(WebUtility.HtmlEncode(line[position].ToString()));
                position++;
            }

            sb.Append('\n');
        }

        sb.Append("</code></pre>");
        return sb.ToString();
    }

    [GeneratedRegex(@"[A-Za-z_][A-Za-z0-9_\-]*")]
    private static partial Regex IdentifierRegex();
}