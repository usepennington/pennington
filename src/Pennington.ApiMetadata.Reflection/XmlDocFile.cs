namespace Pennington.ApiMetadata.Reflection;

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

/// <summary>
/// Indexes a Roslyn-emitted <c>.xml</c> xmldoc file by xmldocid, so reflection-side
/// consumers can pull parsed doc trees by the id they compute from <see cref="System.Type"/>
/// or <see cref="System.Reflection.MemberInfo"/>.
/// </summary>
internal sealed class XmlDocFile
{
    private readonly Dictionary<string, ParsedXmlDoc> _byId;

    private XmlDocFile(Dictionary<string, ParsedXmlDoc> byId)
    {
        _byId = byId;
    }

    /// <summary>An empty index — used when an assembly has no companion xmldoc file.</summary>
    public static XmlDocFile Empty { get; } = new(new Dictionary<string, ParsedXmlDoc>());

    /// <summary>Parses the xmldoc XML at <paramref name="path"/>. Returns <see cref="Empty"/> if the file is missing or malformed.</summary>
    public static XmlDocFile Load(string path, IXmlDocParser parser)
    {
        if (!File.Exists(path)) return Empty;

        XDocument doc;
        try
        {
            doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            return Empty;
        }

        var members = doc.Root?.Element("members")?.Elements("member");
        if (members is null) return Empty;

        var map = new Dictionary<string, ParsedXmlDoc>(System.StringComparer.Ordinal);
        foreach (var m in members)
        {
            var name = m.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(name)) continue;

            // Pass the <member> element as its own root — the parser reads summary/remarks/
            // param/etc. as direct children regardless of the root name. Re-wrapping via
            // `new XElement("doc", m.Nodes())` used to detach nodes from `m` mid-iteration,
            // which left subsequent members with no child content.
            map[name] = parser.Parse(m.ToString());
        }
        return new XmlDocFile(map);
    }

    /// <summary>Returns the parsed xmldoc for <paramref name="uid"/>, or <see cref="ParsedXmlDoc.Empty"/> when absent.</summary>
    public ParsedXmlDoc Get(string uid)
        => _byId.TryGetValue(uid, out var d) ? d : ParsedXmlDoc.Empty;
}
