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
public static class HtmlToMarkdownConverter
{
    /// <summary>
    /// Converts <paramref name="root"/> to markdown.
    /// </summary>
    /// <param name="root">Root element whose descendants are converted.</param>
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
        // Content-visibility marker: `.humans-only` is for browser display only —
        // strip it (and its subtree) from the extracted markdown. The paired
        // `.robots-only` class is hidden from browsers via CSS but stays in the
        // markup, so it naturally flows through without a special case here.
        if (element.ClassList.Contains("humans-only"))
        {
            return;
        }

        // Pennington-aware: syntax-highlighted code blocks. `<div class="code-highlight-wrapper">`
        // wraps a `<pre><code>…<span class="line">…</span>…</pre>` tree. Strip the highlight
        // markup and emit a clean fenced code block with the original Markdig fence info-string
        // recovered from the `data-language` attribute.
        if (element.TagName == "DIV" && element.ClassList.Contains("code-highlight-wrapper"))
        {
            EmitCodeHighlightWrapper(element, sb);
            return;
        }

        // Pennington-aware: tabbed code blocks. `<div class="tab-container">` holds a tablist
        // of buttons and panels with the highlighted code per tab. Emit every tab as a labeled
        // H3 section so an LLM sees all language variants instead of just the first one.
        if (element.TagName == "DIV" && element.ClassList.Contains("tab-container"))
        {
            EmitTabbedCodeBlock(element, sb, listDepth, orderedStack, rewriteHref);
            return;
        }

        // Pennington-aware: GFM-style alerts. `<div class="markdown-alert markdown-alert-{type}">`
        // wraps a `markdown-alert-title` paragraph and the body. Emit as GFM admonition syntax
        // (`> [!NOTE]\n> body`) which both GitHub and Claude recognize.
        if (element.TagName == "DIV" && element.ClassList.Contains("markdown-alert"))
        {
            EmitMarkdownAlert(element, sb, listDepth, orderedStack, rewriteHref);
            return;
        }

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
                    if (listDepth == 0)
                    {
                        sb.Append('\n');
                    }

                    break;
                }

            case "OL":
                {
                    EnsureBlockStart(sb);
                    orderedStack.Push((true, 0));
                    Walk(element, sb, listDepth + 1, orderedStack, rewriteHref);
                    orderedStack.Pop();
                    if (listDepth == 0)
                    {
                        sb.Append('\n');
                    }

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
                    EmitFencedCode(sb, lang, codeEl.TextContent);
                    break;
                }

            case "CODE":
                // Inline code (PRE > CODE handled above, so parents already consumed children)
                EmitInlineCode(sb, element.TextContent);
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
            {
                return cls["language-".Length..];
            }

            if (cls.StartsWith("lang-", StringComparison.OrdinalIgnoreCase))
            {
                return cls["lang-".Length..];
            }
        }
        return "";
    }

    /// <summary>
    /// Emits the markdown form of a Pennington syntax-highlighted code block. Reads the
    /// original Markdig fence info-string from <c>data-language</c> on the wrapper, then
    /// extracts the inner <c>&lt;pre&gt;</c>'s text content (which strips all the highlight
    /// span markup automatically).
    /// </summary>
    private static void EmitCodeHighlightWrapper(IElement wrapper, StringBuilder sb)
    {
        EnsureBlockStart(sb);

        var languageId = wrapper.GetAttribute("data-language") ?? "";
        var preElement = wrapper.QuerySelector("pre");
        var codeText = preElement?.TextContent ?? "";

        EmitFencedCode(sb, languageId, codeText);
    }

    /// <summary>
    /// Emits a fenced code block, scaling the fence length to one more than the
    /// longest backtick run found in the content. Without this, content containing
    /// <c>```</c> would close the fence prematurely and produce malformed markdown.
    /// </summary>
    private static void EmitFencedCode(StringBuilder sb, string language, string content)
    {
        var fenceLen = Math.Max(3, MaxConsecutiveChars(content, '`') + 1);
        sb.Append('`', fenceLen).Append(language).Append('\n');
        sb.Append(content.TrimEnd('\n'));
        sb.Append('\n').Append('`', fenceLen).Append("\n\n");
    }

    /// <summary>
    /// Emits inline code. The wrapping backtick count is one more than the longest
    /// backtick run inside the content; if the content starts or ends with a backtick,
    /// a single space pad is added on both sides so the parser doesn't merge the
    /// inner backtick with the delimiter.
    /// </summary>
    private static void EmitInlineCode(StringBuilder sb, string content)
    {
        if (content.Length == 0)
        {
            return;
        }

        var tickLen = MaxConsecutiveChars(content, '`') + 1;
        var needsPad = content[0] == '`' || content[^1] == '`';

        sb.Append('`', tickLen);
        if (needsPad)
        {
            sb.Append(' ');
        }

        sb.Append(content);
        if (needsPad)
        {
            sb.Append(' ');
        }

        sb.Append('`', tickLen);
    }

    private static int MaxConsecutiveChars(string s, char c)
    {
        var max = 0;
        var run = 0;
        foreach (var ch in s)
        {
            run = ch == c ? run + 1 : 0;
            if (run > max)
            {
                max = run;
            }
        }
        return max;
    }

    /// <summary>
    /// Emits a Pennington alert block as GFM admonition syntax. Alert type is read from
    /// the <c>markdown-alert-{type}</c> class; the <c>markdown-alert-title</c> paragraph
    /// is dropped (the type name appears in the GFM marker line) and the body is rendered
    /// as quoted markdown.
    /// </summary>
    private static void EmitMarkdownAlert(IElement element, StringBuilder sb, int listDepth, Stack<(bool Ordered, int Index)> orderedStack, Func<string, string>? rewriteHref)
    {
        EnsureBlockStart(sb);

        var alertType = "NOTE";
        foreach (var cls in element.ClassList)
        {
            if (cls.StartsWith("markdown-alert-", StringComparison.OrdinalIgnoreCase))
            {
                alertType = cls["markdown-alert-".Length..].ToUpperInvariant();
                break;
            }
        }

        var inner = new StringBuilder();
        foreach (var child in element.Children)
        {
            // Skip the title paragraph — its content is the alert type, which is already
            // encoded in the `[!TYPE]` marker line.
            if (child.ClassList.Contains("markdown-alert-title"))
            {
                continue;
            }

            RenderElement(child, inner, listDepth, orderedStack, rewriteHref);
        }

        var bodyText = inner.ToString().TrimEnd();
        sb.Append("> [!").Append(alertType).Append("]\n");
        foreach (var line in bodyText.Split('\n'))
        {
            sb.Append("> ").Append(line).Append('\n');
        }
        sb.Append('\n');
    }

    /// <summary>
    /// Emits a Pennington tabbed code block as labeled H3 sections — one per tab. LLMs
    /// see every language variant rather than just the first, which is the correct read
    /// when the same operation is shown across (e.g.) C#, F#, and VB.
    /// </summary>
    private static void EmitTabbedCodeBlock(IElement container, StringBuilder sb, int listDepth, Stack<(bool Ordered, int Index)> orderedStack, Func<string, string>? rewriteHref)
    {
        EnsureBlockStart(sb);

        var buttons = container.QuerySelectorAll("[role='tab'], button.tab-button").ToList();
        var titleById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var button in buttons)
        {
            var id = button.GetAttribute("id");
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            titleById[id] = button.TextContent.Trim();
        }

        foreach (var panel in container.QuerySelectorAll("[aria-labelledby], .tab-panel"))
        {
            var labelledBy = panel.GetAttribute("aria-labelledby");
            var title = labelledBy is not null && titleById.TryGetValue(labelledBy, out var t)
                ? t
                : "";

            if (!string.IsNullOrEmpty(title))
            {
                sb.Append("### ").Append(title).Append("\n\n");
            }

            Walk(panel, sb, listDepth, orderedStack, rewriteHref);
            EnsureBlockStart(sb);
        }
    }

    private static string CollapseWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

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
        if (sb.Length == 0)
        {
            return;
        }
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
                if (newlineRun <= 2)
                {
                    sb.Append(ch);
                }
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