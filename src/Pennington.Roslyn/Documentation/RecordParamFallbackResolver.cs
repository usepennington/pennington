namespace Pennington.Roslyn.Documentation;

using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// For positional record properties, the property's own xmldoc is empty — the
/// effective doc lives on the containing record's <c>&lt;param name="..."/&gt;</c>
/// tag. This resolver swaps the empty property xmldoc for a synthesized
/// <c>&lt;member&gt;&lt;summary&gt;...&lt;/summary&gt;&lt;/member&gt;</c> document
/// built from the matching param.
/// </summary>
internal static class RecordParamFallbackResolver
{
    /// <summary>
    /// Returns <paramref name="resolvedXml"/> unchanged unless the symbol is a
    /// positional record property with no <c>&lt;summary&gt;</c> of its own; in that
    /// case, returns a synthetic xmldoc carrying the containing record's
    /// <c>&lt;param&gt;</c> body as the property summary.
    /// </summary>
    public static string? Resolve(string? resolvedXml, ISymbol symbol)
    {
        if (symbol is not IPropertySymbol property)
        {
            return resolvedXml;
        }

        if (HasSummary(resolvedXml))
        {
            return resolvedXml;
        }

        if (!IsPositionalRecordProperty(property))
        {
            return resolvedXml;
        }

        var typeXml = property.ContainingType.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(typeXml))
        {
            return resolvedXml;
        }

        XDocument typeDoc;
        try
        {
            typeDoc = XDocument.Parse(typeXml, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            return resolvedXml;
        }

        var paramElement = typeDoc.Root?
            .Elements("param")
            .FirstOrDefault(p => string.Equals(p.Attribute("name")?.Value, property.Name, System.StringComparison.Ordinal));

        if (paramElement is null || !paramElement.Nodes().Any())
        {
            return resolvedXml;
        }

        var summary = new XElement("summary", paramElement.Nodes().Select(CloneNode));
        var member = new XElement("member", summary);
        return member.ToString();
    }

    private static bool HasSummary(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            return false;
        }

        var summary = doc.Root?.Element("summary");
        return summary is not null && summary.Nodes().Any();
    }

    private static bool IsPositionalRecordProperty(IPropertySymbol property)
    {
        if (property.ContainingType is not { IsRecord: true })
        {
            return false;
        }

        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ParameterSyntax)
            {
                return true;
            }
        }

        return false;
    }

    private static XNode CloneNode(XNode node)
        => node switch
        {
            XElement e => new XElement(e),
            XText t => new XText(t),
            _ => new XText(node.ToString()),
        };
}
