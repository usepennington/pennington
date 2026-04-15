namespace Pennington.Roslyn.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Extracts code text from a syntax node.
/// When <c>bodyOnly=false</c>, returns the full declaration text.
/// When <c>bodyOnly=true</c>, returns expression body or block body content for methods,
/// or content between braces for types.
/// </summary>
internal static class CodeFragmentExtractor
{
    /// <summary>
    /// Extracts a code fragment from the given syntax node.
    /// When <paramref name="includeLeadingTrivia"/> is false, leading comments/xmldoc
    /// attached to the node as leading trivia are stripped — useful when the caller
    /// renders parsed xmldoc separately and does not want a duplicate comment block.
    /// </summary>
    public static Task<string> ExtractCodeFragmentAsync(SyntaxNode node, string fullText, bool bodyOnly, bool includeLeadingTrivia = true)
    {
        if (!bodyOnly)
        {
            return Task.FromResult(includeLeadingTrivia ? node.ToFullString() : ToStringWithLineIndent(node, fullText));
        }

        var body = ExtractBody(node);
        return Task.FromResult(body ?? (includeLeadingTrivia ? node.ToFullString() : ToStringWithLineIndent(node, fullText)));
    }

    // When leading trivia is stripped, node.ToString() drops the first line's
    // leading whitespace too, leaving the first line at column 0 while subsequent
    // lines keep their source indent. Walk back over ' '/'\t' (stopping at a newline
    // or any non-whitespace so same-line xmldoc/comments stay excluded) and prepend
    // that whitespace so every line shares a baseline for downstream dedent.
    private static string ToStringWithLineIndent(SyntaxNode node, string fullText)
    {
        var spanStart = node.SpanStart;
        var lineStart = spanStart;
        while (lineStart > 0 && fullText[lineStart - 1] is ' ' or '\t')
        {
            lineStart--;
        }

        return fullText[lineStart..spanStart] + node.ToString();
    }

    private static string? ExtractBody(SyntaxNode node)
    {
        return node switch
        {
            MethodDeclarationSyntax method => ExtractMethodBody(method),
            ConstructorDeclarationSyntax constructor => ExtractBlockBody(constructor.Body),
            DestructorDeclarationSyntax destructor => ExtractBlockBody(destructor.Body),
            AccessorDeclarationSyntax accessor => ExtractBlockBody(accessor.Body),
            OperatorDeclarationSyntax op => ExtractMethodLikeBody(op.Body, op.ExpressionBody),
            ConversionOperatorDeclarationSyntax conv => ExtractMethodLikeBody(conv.Body, conv.ExpressionBody),
            PropertyDeclarationSyntax property => ExtractPropertyBody(property),
            TypeDeclarationSyntax typeDecl => ExtractBraceContent(typeDecl.OpenBraceToken, typeDecl.CloseBraceToken),
            EnumDeclarationSyntax enumDecl => ExtractBraceContent(enumDecl.OpenBraceToken, enumDecl.CloseBraceToken),
            NamespaceDeclarationSyntax nsDecl => ExtractBraceContent(nsDecl.OpenBraceToken, nsDecl.CloseBraceToken),
            _ => null,
        };
    }

    private static string? ExtractMethodBody(MethodDeclarationSyntax method)
    {
        if (method.ExpressionBody is { } expressionBody)
        {
            return expressionBody.Expression.ToFullString().TrimEnd();
        }

        return ExtractBlockBody(method.Body);
    }

    private static string? ExtractMethodLikeBody(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody)
    {
        if (expressionBody is not null)
        {
            return expressionBody.Expression.ToFullString().TrimEnd();
        }

        return ExtractBlockBody(body);
    }

    private static string? ExtractBlockBody(BlockSyntax? block)
    {
        if (block is null)
        {
            return null;
        }

        // Return everything between the braces, trimming outer whitespace
        var openBrace = block.OpenBraceToken;
        var closeBrace = block.CloseBraceToken;

        var start = openBrace.Span.End;
        var end = closeBrace.SpanStart;

        if (end <= start)
        {
            return string.Empty;
        }

        var fullText = block.SyntaxTree.GetText().ToString();
        return fullText[start..end].TrimEnd();
    }

    private static string? ExtractPropertyBody(PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody is { } expressionBody)
        {
            return expressionBody.Expression.ToFullString().TrimEnd();
        }

        if (property.AccessorList is not null)
        {
            return property.AccessorList.ToString();
        }

        return null;
    }

    private static string? ExtractBraceContent(SyntaxToken openBrace, SyntaxToken closeBrace)
    {
        if (openBrace.IsMissing || closeBrace.IsMissing)
        {
            return null;
        }

        var start = openBrace.Span.End;
        var end = closeBrace.SpanStart;

        if (end <= start)
        {
            return string.Empty;
        }

        var fullText = openBrace.SyntaxTree!.GetText().ToString();
        return fullText[start..end].TrimEnd();
    }
}