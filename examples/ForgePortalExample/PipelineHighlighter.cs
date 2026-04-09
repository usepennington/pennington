namespace ForgePortalExample;

using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Penn.Highlighting;

public partial class PipelineHighlighter : ICodeHighlighter
{
    public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "pipeline", "pipe" };
    public int Priority => 60;

    public string Highlight(string code, string language)
    {
        var sb = new StringBuilder();
        foreach (var line in code.Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith('#'))
            {
                sb.AppendLine($"<span class=\"hljs-comment\">{HttpUtility.HtmlEncode(line)}</span>");
            }
            else
            {
                var highlighted = KeywordRegex().Replace(HttpUtility.HtmlEncode(line),
                    m => $"<span class=\"hljs-keyword\">{m.Value}</span>");
                highlighted = StringRegex().Replace(highlighted,
                    m => $"<span class=\"hljs-string\">{m.Value}</span>");
                sb.AppendLine(highlighted);
            }
        }
        return sb.ToString().TrimEnd();
    }

    [GeneratedRegex(@"\b(source|transform|sink|when|output|filter|map|select)\b")]
    private static partial Regex KeywordRegex();

    [GeneratedRegex(@"&quot;[^&]*&quot;")]
    private static partial Regex StringRegex();
}
