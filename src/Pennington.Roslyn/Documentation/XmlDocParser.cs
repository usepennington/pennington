namespace Pennington.Roslyn.Documentation;

using System.Collections.Immutable;
using System.Xml.Linq;

public sealed class XmlDocParser : IXmlDocParser
{
    public ParsedXmlDoc Parse(string? xmlDocumentation)
    {
        if (string.IsNullOrWhiteSpace(xmlDocumentation))
        {
            return ParsedXmlDoc.Empty;
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xmlDocumentation, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            return ParsedXmlDoc.Empty;
        }

        var root = doc.Root;
        if (root is null)
        {
            return ParsedXmlDoc.Empty;
        }

        var summary = ParseChildren(root.Element("summary"));
        var remarks = ParseChildren(root.Element("remarks"));
        var returns = ParseChildren(root.Element("returns"));
        var example = ParseChildren(root.Element("example"));

        var paramsBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<XmlDocNode>>();
        foreach (var p in root.Elements("param"))
        {
            var name = p.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
            {
                paramsBuilder[name] = ParseChildren(p);
            }
        }

        var typeParamsBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<XmlDocNode>>();
        foreach (var p in root.Elements("typeparam"))
        {
            var name = p.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
            {
                typeParamsBuilder[name] = ParseChildren(p);
            }
        }

        var seeAlsoBuilder = ImmutableArray.CreateBuilder<string>();
        foreach (var sa in root.Elements("seealso"))
        {
            var cref = sa.Attribute("cref")?.Value;
            if (!string.IsNullOrEmpty(cref))
            {
                seeAlsoBuilder.Add(cref);
            }
        }

        return new ParsedXmlDoc(
            Summary: summary,
            Remarks: remarks,
            Params: paramsBuilder.ToImmutable(),
            TypeParams: typeParamsBuilder.ToImmutable(),
            Returns: returns,
            Example: example,
            SeeAlso: seeAlsoBuilder.ToImmutable());
    }

    private static ImmutableArray<XmlDocNode> ParseChildren(XElement? element)
    {
        if (element is null)
        {
            return [];
        }

        var builder = ImmutableArray.CreateBuilder<XmlDocNode>();
        foreach (var node in element.Nodes())
        {
            var parsed = ParseNode(node);
            if (parsed is not null)
            {
                builder.Add(parsed.Value);
            }
        }

        return CollapseText(TrimBoundaryWhitespace(builder.ToImmutable()));
    }

    private static XmlDocNode? ParseNode(XNode node)
    {
        return node switch
        {
            XText text => new XmlDocNode(new TextNode(NormalizeWhitespace(text.Value))),
            XElement element => ParseElement(element),
            _ => null,
        };
    }

    private static XmlDocNode? ParseElement(XElement element)
    {
        return element.Name.LocalName switch
        {
            "c" => new XmlDocNode(new InlineCodeNode(element.Value)),
            "code" => new XmlDocNode(new CodeBlockNode(
                Language: element.Attribute("language")?.Value ?? "csharp",
                Text: TrimCodeBlock(element.Value))),
            "para" => new XmlDocNode(new ParaNode(ParseChildren(element))),
            "see" => ParseSeeOrCref(element),
            "paramref" => element.Attribute("name")?.Value is { Length: > 0 } pname
                ? new XmlDocNode(new ParamRefNode(pname))
                : null,
            "typeparamref" => element.Attribute("name")?.Value is { Length: > 0 } tname
                ? new XmlDocNode(new TypeParamRefNode(tname))
                : null,
            "list" => new XmlDocNode(new ListNode(
                Kind: element.Attribute("type")?.Value ?? "bullet",
                Items: ParseListItems(element))),
            _ => null,
        };
    }

    private static XmlDocNode? ParseSeeOrCref(XElement element)
    {
        var cref = element.Attribute("cref")?.Value;
        var href = element.Attribute("href")?.Value;
        var display = string.IsNullOrWhiteSpace(element.Value) ? null : element.Value.Trim();

        if (!string.IsNullOrEmpty(cref))
        {
            return new XmlDocNode(new CrefNode(cref, display));
        }

        if (!string.IsNullOrEmpty(href))
        {
            return new XmlDocNode(new CrefNode(href, display));
        }

        var langword = element.Attribute("langword")?.Value;
        if (!string.IsNullOrEmpty(langword))
        {
            return new XmlDocNode(new InlineCodeNode(langword));
        }

        return null;
    }

    private static ImmutableArray<XmlDocListItem> ParseListItems(XElement listElement)
    {
        var builder = ImmutableArray.CreateBuilder<XmlDocListItem>();
        foreach (var item in listElement.Elements("item"))
        {
            var term = ParseChildren(item.Element("term"));
            var description = item.Element("description") is { } descEl
                ? ParseChildren(descEl)
                : ParseChildren(item);
            builder.Add(new XmlDocListItem(term, description));
        }

        return builder.ToImmutable();
    }

    private static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Sample the original string's boundary whitespace before we strip indentation,
        // so mixed-content siblings like "foo <c>bar</c> baz" keep their word boundaries.
        // ParseChildren trims the leading/trailing edges of the whole block.
        var hasLeadingWs = char.IsWhiteSpace(text[0]);
        var hasTrailingWs = char.IsWhiteSpace(text[^1]);

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimStart();
        }

        var trimmed = string.Join(" ", lines).Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return (hasLeadingWs ? " " : string.Empty) + trimmed + (hasTrailingWs ? " " : string.Empty);
    }

    private static string TrimCodeBlock(string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n').ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        var minIndent = int.MaxValue;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var indent = 0;
            while (indent < line.Length && line[indent] == ' ')
            {
                indent++;
            }

            if (indent < minIndent)
            {
                minIndent = indent;
            }
        }

        if (minIndent is > 0 and < int.MaxValue)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                lines[i] = lines[i].Length >= minIndent ? lines[i][minIndent..] : lines[i];
            }
        }

        return string.Join('\n', lines);
    }

    private static ImmutableArray<XmlDocNode> CollapseText(ImmutableArray<XmlDocNode> nodes)
    {
        if (nodes.Length == 0)
        {
            return nodes;
        }

        var builder = ImmutableArray.CreateBuilder<XmlDocNode>(nodes.Length);
        string? pendingText = null;

        foreach (var node in nodes)
        {
            if (node is TextNode t)
            {
                // Concatenate directly — boundary whitespace is already carried on each
                // TextNode by NormalizeWhitespace, so we must not inject an extra space.
                pendingText = pendingText is null ? t.Text : pendingText + t.Text;
                continue;
            }

            if (pendingText is { Length: > 0 })
            {
                builder.Add(new XmlDocNode(new TextNode(pendingText)));
            }

            pendingText = null;
            builder.Add(node);
        }

        if (pendingText is { Length: > 0 })
        {
            builder.Add(new XmlDocNode(new TextNode(pendingText)));
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<XmlDocNode> TrimBoundaryWhitespace(ImmutableArray<XmlDocNode> nodes)
    {
        if (nodes.Length == 0)
        {
            return nodes;
        }

        var builder = nodes.ToBuilder();

        if (builder[0] is TextNode first && first.Text.Length > 0 && char.IsWhiteSpace(first.Text[0]))
        {
            var trimmed = first.Text.TrimStart();
            builder[0] = trimmed.Length > 0
                ? new XmlDocNode(new TextNode(trimmed))
                : new XmlDocNode(new TextNode(string.Empty));
        }

        if (builder[^1] is TextNode last && last.Text.Length > 0 && char.IsWhiteSpace(last.Text[^1]))
        {
            var trimmed = last.Text.TrimEnd();
            builder[^1] = trimmed.Length > 0
                ? new XmlDocNode(new TextNode(trimmed))
                : new XmlDocNode(new TextNode(string.Empty));
        }

        return builder.ToImmutable();
    }
}