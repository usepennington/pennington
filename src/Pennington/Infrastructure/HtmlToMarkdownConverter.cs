namespace Pennington.Infrastructure;

using System.Text;
using AngleSharp.Dom;

/// <summary>
/// Tiny, hand-rolled HTML → Markdown converter used for llms.txt output.
/// <para>
/// Scope is deliberately minimal: the output is consumed by LLMs and doesn't
/// need to round-trip to the original HTML. It covers headings, paragraphs,
/// lists, links, inline/fenced code, blockquotes, images, horizontal rules,
/// and basic inline formatting. Everything else recurses into text content.
/// </para>
/// <para>
/// If this grows past ~250 lines, switch to a real library (e.g. ReverseMarkdown
/// on NuGet) rather than expanding it further.
/// </para>
/// </summary>
internal static class HtmlToMarkdownConverter
{
    /// <summary>
    /// Converts <paramref name="root"/> to markdown.
    /// </summary>
    /// <param name="rewriteHref">
    /// Optional callback invoked for every <c>&lt;a href&gt;</c> value before it's
    /// emitted. Return the href unchanged to keep the link as-is, or a new string
    /// to replace it. Anchor-only links (<c>#section</c>) and empty hrefs are
    /// never passed to the callback — they're emitted as plain text regardless.
    /// </param>
    public static string Convert(IElement root, Func<string, string>? rewriteHref = null)
    {
        var sb = new StringBuilder();
        Walk(root, sb, listDepth: 0, orderedStack: [], rewriteHref);
        return Normalize(sb.ToString());
    }

    private static void Walk(INode node, StringBuilder sb, int listDepth, Stack<(bool Ordered, int Index)> orderedStack, Func<string, string>? rewriteHref)
    {
        foreach (var child in node.ChildNodes)
        {
            switch (child)
            {
                case IText text:
                    sb.Append(CollapseWhitespace(text.Data));
                    break;

                case IElement element:
                    RenderElement(element, sb, listDepth, orderedStack, rewriteHref);
                    break;
            }
        }
    }

    private static void RenderElement(IElement element, StringBuilder sb, int listDepth, Stack<(bool Ordered, int Index)> orderedStack, Func<string, string>? rewriteHref)
    {
        var tag = element.TagName;

        switch (tag)
        {
            case "H1" or "H2" or "H3" or "H4" or "H5" or "H6":
                {
                    var level = int.Parse(tag.AsSpan(1));
                    EnsureBlockStart(sb);
                    sb.Append(new string('#', level)).Append(' ');
                    AppendInline(element, sb, rewriteHref);
                    sb.Append("\n\n");
                    break;
                }

            case "P":
                EnsureBlockStart(sb);
                AppendInline(element, sb, rewriteHref);
                sb.Append("\n\n");
                break;

            case "BR":
                sb.Append("  \n");
                break;

            case "HR":
                EnsureBlockStart(sb);
                sb.Append("---\n\n");
                break;

            case "UL":
                {
                    EnsureBlockStart(sb);
                    orderedStack.Push((false, 0));
                    Walk(element, sb, listDepth + 1, orderedStack, rewriteHref);
                    orderedStack.Pop();
                    if (listDepth == 0) sb.Append('\n');
                    break;
                }

            case "OL":
                {
                    EnsureBlockStart(sb);
                    orderedStack.Push((true, 0));
                    Walk(element, sb, listDepth + 1, orderedStack, rewriteHref);
                    orderedStack.Pop();
                    if (listDepth == 0) sb.Append('\n');
                    break;
                }

            case "LI":
                {
                    var indent = new string(' ', Math.Max(0, listDepth - 1) * 2);
                    sb.Append(indent);
                    if (orderedStack.Count > 0 && orderedStack.Peek().Ordered)
                    {
                        var frame = orderedStack.Pop();
                        var next = (frame.Ordered, frame.Index + 1);
                        orderedStack.Push(next);
                        sb.Append(next.Item2).Append(". ");
                    }
                    else
                    {
                        sb.Append("- ");
                    }

                    var inner = new StringBuilder();
                    Walk(element, inner, listDepth, orderedStack, rewriteHref);
                    var line = inner.ToString().TrimEnd();
                    // Re-indent continuation lines under the bullet.
                    var continuation = indent + "  ";
                    sb.Append(line.Replace("\n", "\n" + continuation));
                    sb.Append('\n');
                    break;
                }

            case "PRE":
                {
                    EnsureBlockStart(sb);
                    var codeEl = element.QuerySelector(":scope > code") ?? element;
                    var lang = ExtractLanguage(codeEl);
                    sb.Append("```").Append(lang).Append('\n');
                    sb.Append(codeEl.TextContent.TrimEnd('\n'));
                    sb.Append("\n```\n\n");
                    break;
                }

            case "CODE":
                // Inline code (PRE > CODE handled above, so parents already consumed children)
                sb.Append('`').Append(element.TextContent).Append('`');
                break;

            case "STRONG" or "B":
                sb.Append("**");
                AppendInline(element, sb, rewriteHref);
                sb.Append("**");
                break;

            case "EM" or "I":
                sb.Append('*');
                AppendInline(element, sb, rewriteHref);
                sb.Append('*');
                break;

            case "A":
                {
                    var href = element.GetAttribute("href");
                    var text = element.TextContent.Trim();
                    if (string.IsNullOrEmpty(href) || href.StartsWith('#'))
                    {
                        sb.Append(text);
                    }
                    else
                    {
                        var finalHref = rewriteHref is not null ? rewriteHref(href) : href;
                        sb.Append('[').Append(text).Append("](").Append(finalHref).Append(')');
                    }
                    break;
                }

            case "IMG":
                {
                    var src = element.GetAttribute("src");
                    var alt = element.GetAttribute("alt") ?? "";
                    if (!string.IsNullOrEmpty(src))
                    {
                        sb.Append("![").Append(alt).Append("](").Append(src).Append(')');
                    }
                    break;
                }

            case "BLOCKQUOTE":
                {
                    EnsureBlockStart(sb);
                    var inner = new StringBuilder();
                    Walk(element, inner, listDepth, orderedStack, rewriteHref);
                    var text = inner.ToString().TrimEnd();
                    foreach (var line in text.Split('\n'))
                    {
                        sb.Append("> ").Append(line).Append('\n');
                    }
                    sb.Append('\n');
                    break;
                }

            case "SCRIPT" or "STYLE" or "NOSCRIPT" or "SVG" or "IFRAME":
                // Ignore non-content tags entirely.
                break;

            case "DIV" or "SECTION" or "ARTICLE" or "MAIN" or "HEADER" or "FOOTER" or "NAV" or "ASIDE":
                EnsureBlockStart(sb);
                Walk(element, sb, listDepth, orderedStack, rewriteHref);
                EnsureBlockStart(sb);
                break;

            default:
                // For unknown/inline tags, recurse into children.
                Walk(element, sb, listDepth, orderedStack, rewriteHref);
                break;
        }
    }

    private static void AppendInline(IElement element, StringBuilder sb, Func<string, string>? rewriteHref)
    {
        foreach (var child in element.ChildNodes)
        {
            switch (child)
            {
                case IText text:
                    sb.Append(CollapseWhitespace(text.Data));
                    break;
                case IElement el:
                    RenderElement(el, sb, listDepth: 0, orderedStack: new Stack<(bool, int)>(), rewriteHref);
                    break;
            }
        }
    }

    private static string ExtractLanguage(IElement codeEl)
    {
        // Class formats commonly used: "language-cs", "lang-cs", etc.
        var classAttr = codeEl.GetAttribute("class") ?? "";
        foreach (var cls in classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (cls.StartsWith("language-", StringComparison.OrdinalIgnoreCase))
                return cls["language-".Length..];
            if (cls.StartsWith("lang-", StringComparison.OrdinalIgnoreCase))
                return cls["lang-".Length..];
        }
        return "";
    }

    private static string CollapseWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var sb = new StringBuilder(text.Length);
        var prevSpace = false;
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!prevSpace)
                {
                    sb.Append(' ');
                    prevSpace = true;
                }
            }
            else
            {
                sb.Append(ch);
                prevSpace = false;
            }
        }
        return sb.ToString();
    }

    private static void EnsureBlockStart(StringBuilder sb)
    {
        if (sb.Length == 0) return;
        // Ensure we're at the start of a new line (and ideally separated by blank line).
        if (sb[^1] != '\n')
        {
            sb.Append('\n');
        }
    }

    private static string Normalize(string md)
    {
        // Collapse 3+ consecutive newlines into 2 (one blank line).
        var sb = new StringBuilder(md.Length);
        var newlineRun = 0;
        foreach (var ch in md)
        {
            if (ch == '\n')
            {
                newlineRun++;
                if (newlineRun <= 2) sb.Append(ch);
            }
            else
            {
                newlineRun = 0;
                sb.Append(ch);
            }
        }
        return sb.ToString().Trim() + "\n";
    }
}
