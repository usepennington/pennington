namespace Pennington.Roslyn.Symbols;

using System.Text;

/// <summary>
/// Normalizes XML documentation IDs by stripping namespace prefixes from parameter types.
/// E.g., <c>M:Type.Method(System.String,System.Int32)</c> becomes <c>M:Type.Method(String,Int32)</c>.
/// Handles generic parameters, nested delimiters, arrays, ref params, etc.
/// </summary>
internal static class XmlDocIdNormalizer
{
    /// <summary>
    /// Normalizes an XML documentation ID by stripping namespace prefixes from parameter types.
    /// Only modifies method-like IDs (those with parenthesized parameter lists).
    /// </summary>
    public static string Normalize(string xmlDocId)
    {
        if (string.IsNullOrEmpty(xmlDocId))
        {
            return xmlDocId;
        }

        // Only process IDs that have parameter lists (methods, constructors, operators, etc.)
        var parenIndex = xmlDocId.IndexOf('(');
        if (parenIndex < 0)
        {
            return xmlDocId;
        }

        var prefix = xmlDocId[..parenIndex];
        var paramsPart = xmlDocId[parenIndex..];

        var normalized = NormalizeParameters(paramsPart);
        return prefix + normalized;
    }

    private static string NormalizeParameters(string paramsPart)
    {
        var sb = new StringBuilder(paramsPart.Length);
        var i = 0;

        while (i < paramsPart.Length)
        {
            var ch = paramsPart[i];

            switch (ch)
            {
                case '(' or ')' or ',' or '[' or ']' or '@' or '*':
                    sb.Append(ch);
                    i++;
                    break;

                case '{':
                    sb.Append('{');
                    i++;
                    break;

                case '}':
                    sb.Append('}');
                    i++;
                    break;

                case '`':
                    // Generic parameter reference like `0, `1, ``0, ``1
                    sb.Append(ch);
                    i++;
                    // Consume additional backticks
                    while (i < paramsPart.Length && paramsPart[i] == '`')
                    {
                        sb.Append(paramsPart[i]);
                        i++;
                    }
                    // Consume digits
                    while (i < paramsPart.Length && char.IsDigit(paramsPart[i]))
                    {
                        sb.Append(paramsPart[i]);
                        i++;
                    }
                    break;

                default:
                    // This is a type name — read the full qualified name and strip the namespace
                    var (typeName, newIndex) = ReadTypeName(paramsPart, i);
                    sb.Append(StripNamespace(typeName));
                    i = newIndex;
                    break;
            }
        }

        return sb.ToString();
    }

    private static (string TypeName, int NewIndex) ReadTypeName(string text, int start)
    {
        var i = start;
        var depth = 0;

        while (i < text.Length)
        {
            var ch = text[i];

            if (ch == '{')
            {
                depth++;
                i++;
            }
            else if (ch == '}')
            {
                depth--;
                i++;
            }
            else if (depth == 0 && (ch is ',' or ')' or '[' or ']' or '@' or '*'))
            {
                break;
            }
            else
            {
                i++;
            }
        }

        return (text[start..i], i);
    }

    private static string StripNamespace(string qualifiedType)
    {
        // Handle generic types with curly braces: System.Collections.Generic.List{System.String}
        var braceIndex = qualifiedType.IndexOf('{');
        if (braceIndex >= 0)
        {
            var outerType = qualifiedType[..braceIndex];
            var innerPart = qualifiedType[braceIndex..];
            return StripNamespaceFromSimpleType(outerType) + NormalizeParameters(innerPart);
        }

        return StripNamespaceFromSimpleType(qualifiedType);
    }

    private static string StripNamespaceFromSimpleType(string typeName)
    {
        // Don't strip generic parameter references like `0
        if (typeName.Length > 0 && typeName[0] == '`')
        {
            return typeName;
        }

        var lastDot = typeName.LastIndexOf('.');
        return lastDot >= 0 ? typeName[(lastDot + 1)..] : typeName;
    }
}