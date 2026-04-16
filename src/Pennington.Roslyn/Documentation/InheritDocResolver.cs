namespace Pennington.Roslyn.Documentation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Resolves <c>&lt;inheritdoc/&gt;</c> tags in xmldoc XML by walking the symbol's
/// inheritance chain (base overrides first, then implemented interface members)
/// and merging the first resolved base's doc children into the child XML.
/// Child tags win over inherited tags.
/// </summary>
internal static class InheritDocResolver
{
    private const int MaxDepth = 8;

    /// <summary>
    /// Returns <paramref name="rawXml"/> with <c>&lt;inheritdoc/&gt;</c> elements replaced by
    /// doc children merged from the first base member with xmldoc. Returns the input
    /// unchanged if there is no inheritdoc tag, no base resolves, or parsing fails.
    /// <c>&lt;inheritdoc cref="..."/&gt;</c> is left in place (out of scope).
    /// </summary>
    public static string? Resolve(string? rawXml, ISymbol symbol)
        => ResolveCore(rawXml, symbol, depth: 0);

    private static string? ResolveCore(string? rawXml, ISymbol symbol, int depth)
    {
        if (depth >= MaxDepth
            || string.IsNullOrWhiteSpace(rawXml)
            || !rawXml.Contains("inheritdoc", StringComparison.Ordinal))
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

        var baseRoot = FindBaseDocRoot(symbol, depth);
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
                var p = child.Attribute("name")?.Value ?? string.Empty;
                if (!existingParams.Contains(p))
                {
                    toAdd.Add(new XElement(child));
                }
            }
            else if (name == "typeparam")
            {
                var p = child.Attribute("name")?.Value ?? string.Empty;
                if (!existingTypeParams.Contains(p))
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

    private static XElement? FindBaseDocRoot(ISymbol symbol, int depth)
    {
        foreach (var candidate in GetCandidates(symbol))
        {
            var xml = candidate.GetDocumentationCommentXml();
            if (string.IsNullOrWhiteSpace(xml))
            {
                continue;
            }

            var resolved = ResolveCore(xml, candidate, depth + 1);
            if (string.IsNullOrWhiteSpace(resolved))
            {
                continue;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(resolved, LoadOptions.PreserveWhitespace);
            }
            catch
            {
                continue;
            }

            if (doc.Root is { } root)
            {
                return root;
            }
        }

        return null;
    }

    private static IEnumerable<ISymbol> GetCandidates(ISymbol symbol)
    {
        ISymbol? overridden = symbol switch
        {
            IMethodSymbol m => m.OverriddenMethod,
            IPropertySymbol p => p.OverriddenProperty,
            IEventSymbol e => e.OverriddenEvent,
            _ => null,
        };

        if (overridden is not null)
        {
            yield return overridden;
        }

        var containingType = symbol.ContainingType;
        if (containingType is null)
        {
            yield break;
        }

        foreach (var iface in containingType.AllInterfaces)
        {
            foreach (var iMember in iface.GetMembers())
            {
                var impl = containingType.FindImplementationForInterfaceMember(iMember);
                if (impl is not null && SymbolEqualityComparer.Default.Equals(impl, symbol))
                {
                    yield return iMember;
                }
            }
        }
    }
}
