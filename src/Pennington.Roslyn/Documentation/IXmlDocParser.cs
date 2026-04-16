namespace Pennington.Roslyn.Documentation;

/// <summary>Parses raw xmldoc XML (the string returned by <c>ISymbol.GetDocumentationCommentXml()</c>) into a structured <see cref="ParsedXmlDoc"/> tree.</summary>
public interface IXmlDocParser
{
    /// <summary>Parses the given xmldoc XML string and returns a structured tree, or <see cref="ParsedXmlDoc.Empty"/> when the input is null, empty, or malformed.</summary>
    ParsedXmlDoc Parse(string? xmlDocumentation);
}