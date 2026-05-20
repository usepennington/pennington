namespace Pennington.Markdown;

using System.Text;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Pipeline;

/// <summary>
/// Extracts heading outline from a parsed Markdown document.
/// </summary>
public static class MarkdownOutlineGenerator
{
    /// <summary>Produces outline entries for every heading in the document that has an id attribute.</summary>
    public static OutlineEntry[] GenerateOutline(MarkdownDocument document)
    {
        var entries = new List<OutlineEntry>();

        foreach (var node in document.Descendants())
        {
            if (node is not HeadingBlock heading || heading.Inline == null)
            {
                continue;
            }

            var text = GetPlainText(heading.Inline);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var id = heading.TryGetAttributes()?.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            entries.Add(new OutlineEntry(id, text, heading.Level));
        }

        return entries.ToArray();
    }

    private static string GetPlainText(ContainerInline container)
    {
        var sb = new StringBuilder();
        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    sb.Append(literal.Content.ToString());
                    break;
                case CodeInline code:
                    sb.Append(code.Content);
                    break;
                case ContainerInline nested:
                    sb.Append(GetPlainText(nested));
                    break;
            }
        }
        return sb.ToString();
    }
}