namespace Pennington.ApiMetadata;

/// <summary>Parses raw xmldoc XML (the string returned by <c>ISymbol.GetDocumentationCommentXml()</c> or the <c>summary</c>/<c>remarks</c> text in DocFx ManagedReference YAML) into a structured <see cref="ParsedXmlDoc"/> tree.</summary>
public interface IXmlDocParser
{
    /// <summary>Parses the given xmldoc XML string and returns a structured tree, or <see cref="ParsedXmlDoc.Empty"/> when the input is null, empty, or malformed.</summary>
    ParsedXmlDoc Parse(string? xmlDocumentation);
}