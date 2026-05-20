namespace Pennington.Roslyn.Symbols;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Computes the subset of file-local <c>using</c> directives that a code fragment actually depends on.
/// Goal: turn an extracted method body or type declaration into a copy-pasteable sample by
/// prepending only the imports the body's referenced symbols need — not the file's full using block.
/// </summary>
internal static class RequiredUsingsAnalyzer
{
    /// <summary>
    /// Returns the subset of <paramref name="fileRoot"/>'s file-local <c>using</c> directives
    /// (no <c>global using</c> directives, no implicit usings) that the symbols referenced inside
    /// <paramref name="fragmentNode"/> require, preserving the directives' original source order.
    /// </summary>
    public static ImmutableList<UsingDirectiveSyntax> Analyze(
        SyntaxNode fragmentNode,
        SemanticModel semanticModel,
        CompilationUnitSyntax fileRoot)
    {
        if (fragmentNode.SyntaxTree != fileRoot.SyntaxTree)
        {
            return ImmutableList<UsingDirectiveSyntax>.Empty;
        }

        var candidates = CollectFileLocalUsings(fileRoot);
        if (candidates.Count == 0)
        {
            return ImmutableList<UsingDirectiveSyntax>.Empty;
        }

        var ambientNamespaces = CollectAmbientNamespaces(fragmentNode);
        var referencedNamespaces = new HashSet<string>(StringComparer.Ordinal);
        var referencedStaticTypes = new HashSet<string>(StringComparer.Ordinal);
        var referencedAliases = new HashSet<string>(StringComparer.Ordinal);

        // Walk only SimpleNameSyntax nodes (IdentifierName + GenericName). These are the
        // smallest source positions where the author wrote a name, so:
        //   - PredefinedTypeSyntax (`string`, `int`) is naturally skipped, avoiding spurious
        //     `using System;` from `string` keyword usage.
        //   - The SimpleName is the right node for the "is this reference qualified?" check
        //     used to gate `using static`.
        //   - GetAliasInfo on an alias-qualified identifier returns the IAliasSymbol, which
        //     GetSymbolInfo would otherwise resolve straight to the underlying type.
        foreach (var name in fragmentNode.DescendantNodesAndSelf().OfType<SimpleNameSyntax>())
        {
            var alias = semanticModel.GetAliasInfo(name);
            if (alias is not null)
            {
                referencedAliases.Add(alias.Name);
                continue;
            }

            RecordSymbol(semanticModel.GetSymbolInfo(name).Symbol, name, ambientNamespaces, referencedNamespaces, referencedStaticTypes);
            RecordType(semanticModel.GetTypeInfo(name).Type, ambientNamespaces, referencedNamespaces);
        }

        var keep = ImmutableList.CreateBuilder<UsingDirectiveSyntax>();
        foreach (var directive in candidates)
        {
            if (IsRequired(directive, referencedNamespaces, referencedStaticTypes, referencedAliases))
            {
                keep.Add(directive);
            }
        }

        return keep.ToImmutable();
    }

    private static List<UsingDirectiveSyntax> CollectFileLocalUsings(CompilationUnitSyntax fileRoot)
    {
        var list = new List<UsingDirectiveSyntax>();

        foreach (var directive in fileRoot.Usings)
        {
            if (!directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
            {
                list.Add(directive);
            }
        }

        // Usings can also live inside file-scoped or block-form namespaces.
        foreach (var ns in fileRoot.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
        {
            foreach (var directive in ns.Usings)
            {
                if (!directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                {
                    list.Add(directive);
                }
            }
        }

        return list;
    }

    private static HashSet<string> CollectAmbientNamespaces(SyntaxNode fragmentNode)
    {
        // Symbols declared in the same namespace as the fragment don't need a using.
        // Walk every BaseNamespaceDeclarationSyntax that contains the fragment and
        // record its full name plus any ancestor namespaces (a nested `namespace A.B { namespace C { ... } }`
        // makes both A.B and A.B.C ambient).
        var ambient = new HashSet<string>(StringComparer.Ordinal);
        for (var current = fragmentNode.Parent; current is not null; current = current.Parent)
        {
            if (current is BaseNamespaceDeclarationSyntax ns)
            {
                ambient.Add(ns.Name.ToString());
            }
        }

        return ambient;
    }

    private static void RecordSymbol(
        ISymbol? symbol,
        SimpleNameSyntax referenceNode,
        HashSet<string> ambientNamespaces,
        HashSet<string> referencedNamespaces,
        HashSet<string> referencedStaticTypes)
    {
        if (symbol is null)
        {
            return;
        }

        switch (symbol)
        {
            case INamedTypeSymbol type:
                RecordType(type, ambientNamespaces, referencedNamespaces);
                break;
            case IMethodSymbol method:
                if (method.IsExtensionMethod)
                {
                    var origin = method.ReducedFrom ?? method;
                    AddNamespace(origin.ContainingNamespace, ambientNamespaces, referencedNamespaces);
                }

                if (method.IsStatic && !method.IsExtensionMethod && IsUnqualifiedReference(referenceNode))
                {
                    AddStaticType(method.ContainingType, referencedStaticTypes);
                }

                foreach (var typeArg in method.TypeArguments)
                {
                    RecordType(typeArg, ambientNamespaces, referencedNamespaces);
                }

                break;
            case IPropertySymbol property when property.IsStatic && IsUnqualifiedReference(referenceNode):
                AddStaticType(property.ContainingType, referencedStaticTypes);
                break;
            case IFieldSymbol field when field.IsStatic && IsUnqualifiedReference(referenceNode):
                AddStaticType(field.ContainingType, referencedStaticTypes);
                break;
            case IEventSymbol ev when ev.IsStatic && IsUnqualifiedReference(referenceNode):
                AddStaticType(ev.ContainingType, referencedStaticTypes);
                break;
        }
    }

    private static void RecordType(ITypeSymbol? type, HashSet<string> ambientNamespaces, HashSet<string> referencedNamespaces)
    {
        if (type is null)
        {
            return;
        }

        switch (type)
        {
            case INamedTypeSymbol named:
                var outermost = OutermostContainingType(named);
                AddNamespace(outermost.ContainingNamespace, ambientNamespaces, referencedNamespaces);
                foreach (var typeArg in named.TypeArguments)
                {
                    RecordType(typeArg, ambientNamespaces, referencedNamespaces);
                }

                break;
            case IArrayTypeSymbol array:
                RecordType(array.ElementType, ambientNamespaces, referencedNamespaces);
                break;
            case IPointerTypeSymbol pointer:
                RecordType(pointer.PointedAtType, ambientNamespaces, referencedNamespaces);
                break;
        }
    }

    private static INamedTypeSymbol OutermostContainingType(INamedTypeSymbol type)
    {
        var current = type;
        while (current.ContainingType is { } parent)
        {
            current = parent;
        }

        return current;
    }

    private static void AddNamespace(INamespaceSymbol? ns, HashSet<string> ambientNamespaces, HashSet<string> referencedNamespaces)
    {
        if (ns is null || ns.IsGlobalNamespace)
        {
            return;
        }

        var name = ns.ToDisplayString();
        if (ambientNamespaces.Contains(name))
        {
            return;
        }

        referencedNamespaces.Add(name);
    }

    private static void AddStaticType(INamedTypeSymbol? type, HashSet<string> referencedStaticTypes)
    {
        if (type is null)
        {
            return;
        }

        referencedStaticTypes.Add(type.ToDisplayString());
    }

    private static bool IsUnqualifiedReference(SimpleNameSyntax referenceNode)
    {
        // A reference is "unqualified" — and therefore needs `using static` to bind — when the
        // simple name isn't the right-hand side of a `Type.Member` access. `Type.Method()` already
        // qualifies the member, so it doesn't drive a static-using requirement.
        return referenceNode.Parent is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name != referenceNode;
    }

    private static bool IsRequired(
        UsingDirectiveSyntax directive,
        HashSet<string> referencedNamespaces,
        HashSet<string> referencedStaticTypes,
        HashSet<string> referencedAliases)
    {
        var target = directive.Name?.ToString();
        if (string.IsNullOrEmpty(target))
        {
            return false;
        }

        if (directive.Alias is { Name: { } aliasName })
        {
            return referencedAliases.Contains(aliasName.Identifier.ValueText);
        }

        if (directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            return referencedStaticTypes.Contains(target);
        }

        // Plain `using X.Y;` — exact namespace match. C# does not flow `using X.Y;` to `X.Y.Sub`,
        // so we don't expand prefixes either.
        return referencedNamespaces.Contains(target);
    }
}