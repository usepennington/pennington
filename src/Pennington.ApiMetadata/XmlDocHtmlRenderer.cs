namespace Pennington.ApiMetadata;

using System.Collections.Generic;
using System.Net;
using System.Text;

/// <summary>Renders <see cref="XmlDocNode"/> trees into HTML for display in the DocSite.</summary>
public sealed class XmlDocHtmlRenderer : IXmlDocHtmlRenderer
{
    /// <summary>Renders the nodes as block-level HTML, wrapping loose inline content in <c>&lt;p&gt;</c> and emitting <c>&lt;pre&gt;</c>/<c>&lt;ul&gt;</c>/<c>&lt;ol&gt;</c> for code blocks and lists.</summary>
    public string RenderHtml(IEnumerable<XmlDocNode> nodes)
    {
        var materialised = nodes.ToList();
        if (materialised.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var openParagraph = false;

        foreach (var node in materialised)
        {
            switch (node.Value)
            {
                case ParaNode p:
                    if (openParagraph)
                    {
                        sb.Append("</p>");
                        openParagraph = false;
                    }

                    sb.Append("<p>").Append(RenderInlineHtml(p.Children)).Append("</p>");
                    break;
                case CodeBlockNode cb:
                    if (openParagraph)
                    {
                        sb.Append("</p>");
                        openParagraph = false;
                    }

                    sb.Append("<pre><code class=\"language-")
                        .Append(WebUtility.HtmlEncode(cb.Language))
                        .Append("\">")
                        .Append(WebUtility.HtmlEncode(cb.Text))
                        .Append("</code></pre>");
                    break;
                case ListNode list:
                    if (openParagraph)
                    {
                        sb.Append("</p>");
                        openParagraph = false;
                    }

                    sb.Append(RenderList(list));
                    break;
                default:
                    if (!openParagraph)
                    {
                        sb.Append("<p>");
                        openParagraph = true;
                    }

                    RenderInlineNode(node, sb);
                    break;
            }
        }

        if (openParagraph)
        {
            sb.Append("</p>");
        }

        return sb.ToString();
    }

    /// <summary>Renders the nodes as inline HTML without wrapping paragraphs, suitable for embedding inside an existing block element.</summary>
    public string RenderInlineHtml(IEnumerable<XmlDocNode> nodes)
    {
        var sb = new StringBuilder();
        foreach (var node in nodes)
        {
            RenderInlineNode(node, sb);
        }

        return sb.ToString();
    }

    private void RenderInlineNode(XmlDocNode node, StringBuilder sb)
    {
        switch (node.Value)
        {
            case TextNode t:
                sb.Append(WebUtility.HtmlEncode(t.Text));
                break;
            case InlineCodeNode c:
                sb.Append("<code>").Append(WebUtility.HtmlEncode(c.Text)).Append("</code>");
                break;
            case ParaNode p:
                foreach (var child in p.Children)
                {
                    RenderInlineNode(child, sb);
                }

                break;
            case CrefNode cref:
                sb.Append("<code>")
                    .Append(WebUtility.HtmlEncode(cref.DisplayText ?? UidDisplay.Shorten(cref.CrefId)))
                    .Append("</code>");
                break;
            case ParamRefNode pr:
                sb.Append("<code>").Append(WebUtility.HtmlEncode(pr.ParamName)).Append("</code>");
                break;
            case TypeParamRefNode tpr:
                sb.Append("<code>").Append(WebUtility.HtmlEncode(tpr.ParamName)).Append("</code>");
                break;
            case CodeBlockNode cb:
                sb.Append("<code>").Append(WebUtility.HtmlEncode(cb.Text)).Append("</code>");
                break;
            case ListNode list:
                sb.Append(RenderList(list));
                break;
        }
    }

    private string RenderList(ListNode list)
    {
        var tag = list.Kind switch
        {
            "number" => "ol",
            "table" => "ul",
            _ => "ul",
        };

        var sb = new StringBuilder();
        sb.Append('<').Append(tag).Append('>');
        foreach (var item in list.Items)
        {
            sb.Append("<li>");
            if (item.Term.Length > 0)
            {
                sb.Append("<strong>").Append(RenderInlineHtml(item.Term)).Append("</strong> ");
            }

            sb.Append(RenderInlineHtml(item.Description));
            sb.Append("</li>");
        }

        sb.Append("</").Append(tag).Append('>');
        return sb.ToString();
    }

}
