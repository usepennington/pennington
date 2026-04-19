namespace Pennington.Roslyn.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>Information about an extracted symbol.</summary>
/// <param name="Symbol">The resolved Roslyn symbol.</param>
/// <param name="Document">Roslyn document that declares the symbol.</param>
/// <param name="SyntaxNode">Declaration syntax node for the symbol.</param>
/// <param name="SourceText">Full source text of the containing document.</param>
/// <param name="TextSpan">Span within <see cref="SourceText"/> covering the declaration.</param>
/// <param name="XmlDocumentation">Raw xmldoc XML for the symbol, if present.</param>
/// <param name="Project">Roslyn project the symbol belongs to.</param>
public record SymbolInfo(
    ISymbol Symbol,
    Document Document,
    SyntaxNode SyntaxNode,
    SourceText SourceText,
    TextSpan TextSpan,
    string? XmlDocumentation,
    Project Project
);