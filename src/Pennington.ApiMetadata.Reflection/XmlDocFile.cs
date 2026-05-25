namespace Pennington.ApiMetadata.Reflection;

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

/// <summary>
/// Loads a compiler-emitted <c>.xml</c> xmldoc file into a uid → raw <c>&lt;member&gt;</c> map.
/// The text is kept unparsed because <c>&lt;inheritdoc/&gt;</c> resolution
/// (<see cref="ReflectionInheritDocResolver"/>) and the record-parameter fallback
/// (<see cref="ReflectionRecordParamFallback"/>) operate on the raw XML and span assemblies —
/// a single global map keyed by uid lets a member's doc resolve against a base declared in
/// another assembly before anything is parsed.
/// </summary>
internal static class XmlDocFile
{
    /// <summary>
    /// Adds every <c>&lt;member name="..."&gt;</c> entry from the xmldoc at <paramref name="path"/>
    /// into <paramref name="target"/>, keyed by uid. A missing or malformed file contributes nothing.
    /// </summary>
    public static void LoadInto(string path, IDictionary<string, string> target)
    {
        if (!File.Exists(path))
        {
            return;
        }

        XDocument doc;
        try
        {
            doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            return;
        }

        var members = doc.Root?.Element("members")?.Elements("member");
        if (members is null)
        {
            return;
        }

        foreach (var m in members)
        {
            var name = m.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            target[name] = m.ToString();
        }
    }
}
