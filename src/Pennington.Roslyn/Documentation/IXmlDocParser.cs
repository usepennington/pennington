namespace Pennington.Roslyn.Documentation;

/// <summary>Parses raw xmldoc XML (the string returned by <c>ISymbol.GetDocumentationCommentXml()</c>) into a structured <see cref="ParsedXmlDoc"/> tree.</summary>
public interface IXmlDocParser
{
    ParsedXmlDoc Parse(string? xmlDocumentation);
}