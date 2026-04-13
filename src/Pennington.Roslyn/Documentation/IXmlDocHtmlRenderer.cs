namespace Pennington.Roslyn.Documentation;

using System.Collections.Generic;

/// <summary>Renders <see cref="ParsedXmlDoc"/> nodes to HTML. Inline form avoids block wrapping for use in table cells.</summary>
public interface IXmlDocHtmlRenderer
{
    /// <summary>Render as block HTML — wraps bare text in <c>&lt;p&gt;</c>, promotes <c>&lt;para&gt;</c> to paragraphs, etc.</summary>
    string RenderHtml(IEnumerable<XmlDocNode> nodes);

    /// <summary>Render as inline HTML — no paragraph wrapping. Use inside <c>&lt;td&gt;</c> or single-sentence descriptions.</summary>
    string RenderInlineHtml(IEnumerable<XmlDocNode> nodes);
}
