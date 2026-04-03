namespace Penn.Roslyn.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>Information about an extracted symbol.</summary>
public record SymbolInfo(
    ISymbol Symbol,
    Document Document,
    SyntaxNode SyntaxNode,
    SourceText SourceText,
    TextSpan TextSpan,
    string? XmlDocumentation,
    Project Project
);
