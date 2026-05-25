namespace Pennington.ApiMetadata.Reflection;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

/// <summary>
/// Replaces
/// <c>&lt;inheritdoc/&gt;</c> elements in a member's raw xmldoc with the doc children merged
/// from the first base member (override chain first, then implemented interfaces) that carries
/// documentation. Child tags win over inherited tags; <c>&lt;param&gt;</c>/<c>&lt;typeparam&gt;</c>
/// merge by name. Raw XML is looked up by uid across every loaded assembly, so a member can
/// inherit docs from a base declared elsewhere. <c>&lt;inheritdoc cref="..."/&gt;</c> is left in place.
/// </summary>
internal static class ReflectionInheritDocResolver
{
    private const int MaxDepth = 8;

    /// <summary>Returns the member's resolved xmldoc, or its raw xmldoc unchanged when nothing inherits or no base resolves. <see langword="null"/> when the member has no raw xmldoc at all.</summary>
    public static string? Resolve(MemberInfo member, IReadOnlyDictionary<string, string> rawByUid)
    {
        var raw = rawByUid.GetValueOrDefault(UidOf(member));
        return ResolveCore(raw, member, rawByUid, depth: 0);
    }

    private static string? ResolveCore(string? rawXml, MemberInfo member, IReadOnlyDictionary<string, string> rawByUid, int depth)
    {
        if (depth >= MaxDepth
            || string.IsNullOrWhiteSpace(rawXml)
            || !rawXml.Contains("inheritdoc", System.StringComparison.Ordinal))
        {
            return rawXml;
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(rawXml, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            return rawXml;
        }

        var root = doc.Root;
        if (root is null)
        {
            return rawXml;
        }

        var inheritElements = root.Elements("inheritdoc")
            .Where(e => e.Attribute("cref") is null)
            .ToList();
        if (inheritElements.Count == 0)
        {
            return rawXml;
        }

        var baseRoot = FindBaseDocRoot(member, rawByUid, depth);
        if (baseRoot is null)
        {
            return rawXml;
        }

        var existingTags = new HashSet<string>(root.Elements()
            .Where(e => e.Name.LocalName != "inheritdoc")
            .Select(e => e.Name.LocalName));
        var existingParams = new HashSet<string>(root.Elements("param")
            .Select(e => e.Attribute("name")?.Value ?? string.Empty));
        var existingTypeParams = new HashSet<string>(root.Elements("typeparam")
            .Select(e => e.Attribute("name")?.Value ?? string.Empty));

        var toAdd = new List<XElement>();
        foreach (var child in baseRoot.Elements())
        {
            var name = child.Name.LocalName;
            if (name == "inheritdoc")
            {
                continue;
            }

            if (name == "param")
            {
                if (!existingParams.Contains(child.Attribute("name")?.Value ?? string.Empty))
                {
                    toAdd.Add(new XElement(child));
                }
            }
            else if (name == "typeparam")
            {
                if (!existingTypeParams.Contains(child.Attribute("name")?.Value ?? string.Empty))
                {
                    toAdd.Add(new XElement(child));
                }
            }
            else if (!existingTags.Contains(name))
            {
                toAdd.Add(new XElement(child));
            }
        }

        foreach (var e in inheritElements)
        {
            e.Remove();
        }

        foreach (var e in toAdd)
        {
            root.Add(e);
        }

        return root.ToString();
    }

    private static XElement? FindBaseDocRoot(MemberInfo member, IReadOnlyDictionary<string, string> rawByUid, int depth)
    {
        foreach (var candidate in GetCandidates(member))
        {
            var raw = rawByUid.GetValueOrDefault(UidOf(candidate));
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var resolved = ResolveCore(raw, candidate, rawByUid, depth + 1);
            if (string.IsNullOrWhiteSpace(resolved))
            {
                continue;
            }

            try
            {
                if (XDocument.Parse(resolved, LoadOptions.PreserveWhitespace).Root is { } root)
                {
                    return root;
                }
            }
            catch
            {
                // Skip a malformed base doc and try the next candidate.
            }
        }

        return null;
    }

    private static IEnumerable<MemberInfo> GetCandidates(MemberInfo member)
    {
        if (member is Type type)
        {
            if (type.BaseType is { } bt && bt != typeof(object))
            {
                yield return bt;
            }

            foreach (var iface in type.GetInterfaces())
            {
                yield return iface;
            }

            yield break;
        }

        var declaring = member.DeclaringType;
        if (declaring is null)
        {
            yield break;
        }

        // Override chain: the same member shape declared further up the base-type chain.
        for (var baseType = declaring.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (FindMatch(baseType, member, declaredOnly: true) is { } baseMember)
            {
                yield return baseMember;
            }
        }

        // Implemented (or, for interfaces, inherited) interface members.
        foreach (var iface in declaring.GetInterfaces())
        {
            if (FindMatch(iface, member, declaredOnly: false) is { } ifaceMember)
            {
                yield return ifaceMember;
            }
        }
    }

    private static MemberInfo? FindMatch(Type container, MemberInfo target, bool declaredOnly)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        if (declaredOnly)
        {
            flags |= BindingFlags.DeclaredOnly;
        }

        foreach (var candidate in container.GetMembers(flags))
        {
            if (candidate.MemberType != target.MemberType
                || !string.Equals(candidate.Name, target.Name, System.StringComparison.Ordinal))
            {
                continue;
            }

            if (SignaturesMatch(candidate, target))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool SignaturesMatch(MemberInfo a, MemberInfo b) => (a, b) switch
    {
        (MethodInfo ma, MethodInfo mb) => ParametersMatch(ma.GetParameters(), mb.GetParameters()),
        (PropertyInfo pa, PropertyInfo pb) => ParametersMatch(pa.GetIndexParameters(), pb.GetIndexParameters()),
        _ => true,
    };

    private static bool ParametersMatch(ParameterInfo[] a, ParameterInfo[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (var i = 0; i < a.Length; i++)
        {
            // Compare by metadata name — the two parameters come from different assemblies'
            // MetadataLoadContext views, so reference equality won't hold.
            if (!string.Equals(a[i].ParameterType.FullName, b[i].ParameterType.FullName, System.StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string UidOf(MemberInfo member)
        => member is Type type ? XmlDocIdFormatter.ForType(type) : XmlDocIdFormatter.ForMember(member);
}
