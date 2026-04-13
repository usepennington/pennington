namespace Pennington.Roslyn.Documentation;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pennington.Roslyn.Symbols;

internal sealed class MemberEnumerator : IMemberEnumerator
{
    private static readonly SymbolDisplayFormat TypeDisplayFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private static readonly SymbolDisplayFormat ShortTypeDisplayFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

    private readonly ISymbolExtractionService _symbolService;
    private readonly IXmlDocParser _xmlDocParser;

    public MemberEnumerator(ISymbolExtractionService symbolService, IXmlDocParser xmlDocParser)
    {
        _symbolService = symbolService;
        _xmlDocParser = xmlDocParser;
    }

    public async Task<IReadOnlyList<MemberDescriptor>> EnumerateAsync(
        string typeXmlDocId,
        MemberKind kind,
        AccessFilter access,
        MemberOrder order)
    {
        var symbolInfo = await _symbolService.FindSymbolAsync(typeXmlDocId);
        if (symbolInfo?.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return [];
        }

        var members = typeSymbol.GetMembers()
            .Where(m => IncludeSymbol(m, access) && MatchesKind(m, kind))
            .ToList();

        var descriptors = members
            .Select(m => BuildDescriptor(m, kind))
            .Where(d => d is not null)
            .Select(d => d!)
            .ToList();

        return order switch
        {
            MemberOrder.Alphabetical => descriptors
                .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            _ => descriptors,
        };
    }

    private MemberDescriptor? BuildDescriptor(ISymbol symbol, MemberKind requestedKind)
    {
        var memberKind = ClassifyKind(symbol);
        if (memberKind is null)
        {
            return null;
        }

        var docId = symbol.GetDocumentationCommentId() ?? string.Empty;
        var parsedXml = _xmlDocParser.Parse(symbol.GetDocumentationCommentXml());

        return symbol switch
        {
            IPropertySymbol p => new MemberDescriptor(
                Name: p.Name,
                XmlDocId: docId,
                TypeDisplay: p.Type.ToDisplayString(TypeDisplayFormat),
                DefaultValue: ExtractPropertyDefault(p),
                IsRequired: p.IsRequired,
                Xmldoc: parsedXml,
                Kind: memberKind.Value),
            IFieldSymbol f => new MemberDescriptor(
                Name: f.Name,
                XmlDocId: docId,
                TypeDisplay: f.Type.ToDisplayString(TypeDisplayFormat),
                DefaultValue: ExtractFieldDefault(f),
                IsRequired: f.IsRequired,
                Xmldoc: parsedXml,
                Kind: memberKind.Value),
            IMethodSymbol m => new MemberDescriptor(
                Name: FormatMethodName(m),
                XmlDocId: docId,
                TypeDisplay: FormatMethodSignature(m),
                DefaultValue: null,
                IsRequired: false,
                Xmldoc: parsedXml,
                Kind: memberKind.Value),
            IEventSymbol e => new MemberDescriptor(
                Name: e.Name,
                XmlDocId: docId,
                TypeDisplay: e.Type.ToDisplayString(TypeDisplayFormat),
                DefaultValue: null,
                IsRequired: false,
                Xmldoc: parsedXml,
                Kind: memberKind.Value),
            _ => null,
        };
    }

    private static bool IncludeSymbol(ISymbol symbol, AccessFilter access)
    {
        if (symbol.IsImplicitlyDeclared)
        {
            return false;
        }

        if (symbol is IMethodSymbol method)
        {
            switch (method.MethodKind)
            {
                case MethodKind.PropertyGet:
                case MethodKind.PropertySet:
                case MethodKind.EventAdd:
                case MethodKind.EventRemove:
                case MethodKind.EventRaise:
                case MethodKind.StaticConstructor:
                case MethodKind.Destructor:
                    return false;
            }
        }

        return access switch
        {
            AccessFilter.Public => symbol.DeclaredAccessibility == Accessibility.Public,
            AccessFilter.Protected => symbol.DeclaredAccessibility is Accessibility.Protected
                or Accessibility.ProtectedOrInternal,
            AccessFilter.PublicAndProtected => symbol.DeclaredAccessibility is Accessibility.Public
                or Accessibility.Protected
                or Accessibility.ProtectedOrInternal,
            _ => true,
        };
    }

    private static bool MatchesKind(ISymbol symbol, MemberKind kind)
    {
        return kind switch
        {
            MemberKind.Properties => symbol is IPropertySymbol,
            MemberKind.Fields => symbol is IFieldSymbol,
            MemberKind.Methods => symbol is IMethodSymbol m && m.MethodKind == MethodKind.Ordinary,
            MemberKind.Constructors => symbol is IMethodSymbol { MethodKind: MethodKind.Constructor },
            MemberKind.Events => symbol is IEventSymbol,
            MemberKind.All => true,
            _ => false,
        };
    }

    private static MemberKind? ClassifyKind(ISymbol symbol)
    {
        return symbol switch
        {
            IPropertySymbol => MemberKind.Properties,
            IFieldSymbol => MemberKind.Fields,
            IMethodSymbol { MethodKind: MethodKind.Constructor } => MemberKind.Constructors,
            IMethodSymbol => MemberKind.Methods,
            IEventSymbol => MemberKind.Events,
            _ => null,
        };
    }

    private static string FormatMethodName(IMethodSymbol method)
    {
        if (method.MethodKind == MethodKind.Constructor)
        {
            return method.ContainingType.Name;
        }

        return method.TypeParameters.Length == 0
            ? method.Name
            : $"{method.Name}<{string.Join(", ", method.TypeParameters.Select(tp => tp.Name))}>";
    }

    private static string FormatMethodSignature(IMethodSymbol method)
    {
        var paramsText = string.Join(", ", method.Parameters.Select(p =>
        {
            var typeText = p.Type.ToDisplayString(ShortTypeDisplayFormat);
            var prefix = p.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty,
            };
            var suffix = p.HasExplicitDefaultValue ? $" = {FormatConstant(p.ExplicitDefaultValue)}" : string.Empty;
            return $"{prefix}{typeText} {p.Name}{suffix}";
        }));

        if (method.MethodKind == MethodKind.Constructor)
        {
            return $"{method.ContainingType.Name}({paramsText})";
        }

        var returnText = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString(ShortTypeDisplayFormat);
        return $"{returnText} {FormatMethodName(method)}({paramsText})";
    }

    private static string? ExtractPropertyDefault(IPropertySymbol property)
    {
        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();

            if (syntax is PropertyDeclarationSyntax propertyDecl)
            {
                if (propertyDecl.Initializer?.Value is { } propInit)
                {
                    return propInit.ToString();
                }

                return null;
            }

            if (syntax is ParameterSyntax parameterSyntax)
            {
                return parameterSyntax.Default?.Value.ToString();
            }
        }

        return null;
    }

    private static string? ExtractFieldDefault(IFieldSymbol field)
    {
        foreach (var reference in field.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is VariableDeclaratorSyntax declarator
                && declarator.Initializer?.Value is { } init)
            {
                return init.ToString();
            }
        }

        if (field.HasConstantValue)
        {
            return FormatConstant(field.ConstantValue);
        }

        return null;
    }

    private static string FormatConstant(object? value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            bool b => b ? "true" : "false",
            char c => $"'{c}'",
            _ => value.ToString() ?? "null",
        };
    }
}
