namespace Pennington.ApiMetadata.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

/// <summary>
/// Supplies xmldoc for positional record properties that carry none of their own. A positional
/// record property has no xmldoc of its own — its effective doc is the containing record's
/// matching <c>&lt;param name="..."/&gt;</c>. When a property has no summary and is positional,
/// this returns a synthetic <c>&lt;member&gt;&lt;summary&gt;…&lt;/summary&gt;&lt;/member&gt;</c>
/// built from that param.
/// </summary>
internal static class ReflectionRecordParamFallback
{
    /// <summary>Returns <paramref name="resolvedXml"/> unchanged unless <paramref name="member"/> is a positional record property with no summary, in which case it returns synthetic xmldoc carrying the record's matching <c>&lt;param&gt;</c> body.</summary>
    public static string? Resolve(string? resolvedXml, MemberInfo member, IReadOnlyDictionary<string, string> rawByUid)
    {
        if (member is not PropertyInfo property || property.DeclaringType is not { } declaring)
        {
            return resolvedXml;
        }

        if (HasSummary(resolvedXml) || !IsPositionalRecordProperty(property, declaring))
        {
            return resolvedXml;
        }

        var typeXml = rawByUid.GetValueOrDefault(XmlDocIdFormatter.ForType(declaring));
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
            .FirstOrDefault(p => string.Equals(p.Attribute("name")?.Value, property.Name, StringComparison.Ordinal));
        if (paramElement is null || !paramElement.Nodes().Any())
        {
            return resolvedXml;
        }

        var summary = new XElement("summary", paramElement.Nodes().Select(CloneNode));
        return new XElement("member", summary).ToString();
    }

    private static bool HasSummary(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        try
        {
            var summary = XDocument.Parse(xml, LoadOptions.PreserveWhitespace).Root?.Element("summary");
            return summary is not null && summary.Nodes().Any();
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPositionalRecordProperty(PropertyInfo property, Type declaring)
    {
        // A record exposes a synthesized `<Clone>$` method; positional members appear as a
        // constructor parameter sharing the property's exact name (and type).
        if (!declaring.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Any(m => m.Name == "<Clone>$"))
        {
            return false;
        }

        foreach (var ctor in declaring.GetConstructors())
        {
            if (ctor.GetParameters().Any(p =>
                    string.Equals(p.Name, property.Name, StringComparison.Ordinal)
                    && string.Equals(p.ParameterType.FullName, property.PropertyType.FullName, StringComparison.Ordinal)))
            {
                return true;
            }
        }

        return false;
    }

    private static XNode CloneNode(XNode node) => node switch
    {
        XElement e => new XElement(e),
        XText t => new XText(t),
        _ => new XText(node.ToString()),
    };
}
