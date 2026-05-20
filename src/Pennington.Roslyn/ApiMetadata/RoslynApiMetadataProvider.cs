namespace Pennington.Roslyn.ApiMetadata;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pennington.ApiMetadata;
using Pennington.Highlighting;
using Pennington.Infrastructure;
using Pennington.Roslyn.Documentation;
using Pennington.Roslyn.Symbols;
using Pennington.Roslyn.Workspace;

/// <summary>Roslyn-backed <see cref="IApiMetadataProvider"/>. Walks the configured solution to produce <see cref="ApiTypeSummary"/>, <see cref="ApiTypeDetail"/>, and <see cref="ApiMember"/> instances with pre-formatted signatures.</summary>
public sealed class RoslynApiMetadataProvider : IApiMetadataProvider
{
    private static readonly SymbolDisplayFormat ParameterTypeFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

    private static readonly SymbolDisplayFormat SignatureFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

    private static readonly SymbolDisplayFormat MemberTypeFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private readonly ISolutionWorkspaceService _workspace;
    private readonly ISymbolExtractionService _symbolService;
    private readonly IXmlDocParser _xmlDocParser;
    private readonly ICodeHighlighter _highlighter;
    private readonly ApiReferenceOptions _options;

    private readonly AsyncLazy<ImmutableArray<ApiTypeSummary>> _types;
    private readonly AsyncLazy<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> _extensions;
    private readonly ConcurrentDictionary<string, ApiTypeDetail?> _detailCache = new(StringComparer.Ordinal);

    /// <summary>Initializes the provider.</summary>
    public RoslynApiMetadataProvider(
        ISolutionWorkspaceService workspace,
        ISymbolExtractionService symbolService,
        IXmlDocParser xmlDocParser,
        ICodeHighlighter highlighter,
        ApiReferenceOptions options)
    {
        _workspace = workspace;
        _symbolService = symbolService;
        _xmlDocParser = xmlDocParser;
        _highlighter = highlighter;
        _options = options;
        _types = new AsyncLazy<ImmutableArray<ApiTypeSummary>>(BuildTypesAsync);
        _extensions = new AsyncLazy<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>>(BuildExtensionsAsync);
    }

    /// <inheritdoc />
    public Task<ImmutableArray<ApiTypeSummary>> GetTypesAsync() => _types.Value;

    /// <inheritdoc />
    public async Task<ApiTypeDetail?> GetTypeAsync(string uid)
    {
        if (_detailCache.TryGetValue(uid, out var cached))
        {
            return cached;
        }

        var detail = await BuildDetailAsync(uid);
        _detailCache[uid] = detail;
        return detail;
    }

    /// <inheritdoc />
    public async Task<ImmutableArray<ApiMember>> GetMembersAsync(
        string typeUid, MemberKind kind, AccessFilter access, MemberOrder order)
    {
        var info = await _symbolService.FindSymbolAsync(typeUid);
        if (info?.Symbol is not INamedTypeSymbol type)
        {
            return [];
        }

        var matched = EnumerateMemberSymbols(type, access)
            .Where(m => MatchesKind(m.Symbol, kind))
            .ToList();

        var builder = ImmutableArray.CreateBuilder<ApiMember>();
        foreach (var (symbol, declaringType) in matched)
        {
            var apiMember = await BuildMemberAsync(symbol, declaringType);
            if (apiMember is not null)
            {
                builder.Add(apiMember);
            }
        }

        if (kind is MemberKind.UnionCases or MemberKind.All && IsUnion(type))
        {
            foreach (var caseMember in await BuildUnionCaseMembersAsync(type))
            {
                builder.Add(caseMember);
            }
        }

        return order switch
        {
            MemberOrder.Alphabetical => builder
                .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
                .ToImmutableArray(),
            _ => builder.ToImmutable(),
        };
    }

    private static IEnumerable<(ISymbol Symbol, INamedTypeSymbol? InheritedFrom)> EnumerateMemberSymbols(
        INamedTypeSymbol type, AccessFilter access)
    {
        // Walk the type itself, then any base interfaces it inherits from. The type-level
        // `GetMembers()` only returns directly-declared members, so a recently-split base
        // interface (e.g. IContentService -> IContentEmitter) would otherwise drop the
        // inherited surface from the API page.
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var member in type.GetMembers())
        {
            if (!IncludeSymbol(member, access))
            {
                continue;
            }

            var key = member.GetDocumentationCommentId() ?? $"{member.Kind}:{member.Name}";
            if (seen.Add(key))
            {
                yield return (member, null);
            }
        }

        if (type.TypeKind != TypeKind.Interface)
        {
            yield break;
        }

        foreach (var baseInterface in type.AllInterfaces)
        {
            // Skip interfaces that come from a metadata reference rather than source — for
            // example, `IDisposable` from System.Runtime when the queried interface inherits
            // it transitively. Their members don't have source we can extract a signature
            // from, and they aren't part of the user's API surface that this page documents.
            if (baseInterface.Locations.All(l => l.IsInMetadata))
            {
                continue;
            }

            foreach (var member in baseInterface.GetMembers())
            {
                if (!IncludeSymbol(member, access))
                {
                    continue;
                }

                var key = member.GetDocumentationCommentId() ?? $"{member.Kind}:{member.Name}";
                if (seen.Add(key))
                {
                    yield return (member, baseInterface);
                }
            }
        }
    }

    private static bool IsUnion(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Struct)
        {
            return false;
        }

        // Both the C# 15 `union` keyword (net11.0+) and the polyfill shim (net10.0)
        // emit/declare `[Union]` on the struct and implement `IUnion`. Either signal
        // alone is enough — having both is the common case.
        var hasUnionAttr = type.GetAttributes()
            .Any(a => a.AttributeClass?.Name == "UnionAttribute"
                && a.AttributeClass.ContainingNamespace?.ToDisplayString() == "System.Runtime.CompilerServices");
        if (hasUnionAttr)
        {
            return true;
        }

        return type.AllInterfaces.Any(i => i.Name == "IUnion"
            && i.ContainingNamespace?.ToDisplayString() == "System.Runtime.CompilerServices");
    }

    private async Task<List<ApiMember>> BuildUnionCaseMembersAsync(INamedTypeSymbol type)
    {
        // Cases surface as the parameter type of single-arg constructors. The compiler
        // (and the polyfill) generate one ctor per case taking that case type as its
        // sole argument, so this enumeration is stable across both shapes.
        var cases = type.InstanceConstructors
            .Where(c => c.Parameters.Length == 1)
            .Select(c => c.Parameters[0].Type)
            .OfType<INamedTypeSymbol>()
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<INamedTypeSymbol>()
            .ToList();

        var built = new List<ApiMember>(cases.Count);
        foreach (var caseType in cases)
        {
            var docId = caseType.GetDocumentationCommentId();
            if (string.IsNullOrEmpty(docId))
            {
                continue;
            }

            var rawXml = caseType.GetDocumentationCommentXml();
            var hasInheritDoc = !string.IsNullOrWhiteSpace(rawXml)
                && rawXml.Contains("inheritdoc", StringComparison.Ordinal);
            var resolvedXml = InheritDocResolver.Resolve(rawXml, caseType);
            resolvedXml = RecordParamFallbackResolver.Resolve(resolvedXml, caseType);
            var parsedXml = _xmlDocParser.Parse(resolvedXml);

            string? signatureHtml = null;
            try
            {
                var decl = await _symbolService.ExtractDeclarationSignatureAsync(docId);
                if (!string.IsNullOrEmpty(decl))
                {
                    signatureHtml = _highlighter.Highlight(decl, "csharp");
                }
            }
            catch
            {
                // Best-effort signature; the case row is still useful without it.
            }

            built.Add(new ApiMember(
                Uid: docId,
                Name: caseType.Name,
                Kind: MemberKind.UnionCases,
                TypeDisplay: caseType.ToDisplayString(MemberTypeFormat),
                DefaultValue: null,
                IsRequired: false,
                HasInheritDocDirective: hasInheritDoc,
                Xmldoc: parsedXml,
                SignatureHtml: signatureHtml,
                Parameters: ImmutableArray<ApiParameter>.Empty,
                ReturnTypeDisplay: null));
        }

        return built;
    }

    /// <inheritdoc />
    public async Task<ImmutableArray<ExtensionMethodEntry>> GetExtensionMethodsForAsync(string receiverTypeName)
    {
        var index = await _extensions.Value;
        return index.TryGetValue(receiverTypeName, out var entries) ? entries : [];
    }

    /// <inheritdoc />
    public async Task<ParsedXmlDoc> GetXmldocAsync(string uid)
    {
        var info = await _symbolService.FindSymbolAsync(uid);
        if (info is null)
        {
            return ParsedXmlDoc.Empty;
        }

        var raw = info.Symbol.GetDocumentationCommentXml();
        var resolved = InheritDocResolver.Resolve(raw, info.Symbol);
        resolved = RecordParamFallbackResolver.Resolve(resolved, info.Symbol);
        return _xmlDocParser.Parse(resolved);
    }

    /// <inheritdoc />
    public async Task<ApiMember?> GetMemberAsync(string uid)
    {
        var info = await _symbolService.FindSymbolAsync(uid);
        if (info?.Symbol is not { } symbol || symbol is INamedTypeSymbol)
        {
            return null;
        }

        return await BuildMemberAsync(symbol, inheritedFrom: null);
    }

    private async Task<ApiTypeDetail?> BuildDetailAsync(string uid)
    {
        var info = await _symbolService.FindSymbolAsync(uid);
        if (info?.Symbol is not INamedTypeSymbol type)
        {
            return null;
        }

        var xmldoc = _xmlDocParser.Parse(type.GetDocumentationCommentXml());

        string? signatureHtml = null;
        try
        {
            var decl = await _symbolService.ExtractDeclarationSignatureAsync(uid);
            if (!string.IsNullOrEmpty(decl))
            {
                signatureHtml = _highlighter.Highlight(decl, "csharp");
            }
        }
        catch
        {
            // Declaration extraction is best-effort; leave signatureHtml null on failure.
        }

        var inheritance = ImmutableArray.CreateBuilder<string>();
        for (var b = type.BaseType; b is not null; b = b.BaseType)
        {
            if (b.GetDocumentationCommentId() is { Length: > 0 } id)
            {
                inheritance.Add(id);
            }
        }

        var implements = ImmutableArray.CreateBuilder<string>();
        foreach (var iface in type.Interfaces)
        {
            if (iface.GetDocumentationCommentId() is { Length: > 0 } id)
            {
                implements.Add(id);
            }
        }

        return new ApiTypeDetail(
            Summary: BuildSummary(type, xmldoc),
            Xmldoc: xmldoc,
            SignatureHtml: signatureHtml,
            Inheritance: inheritance.ToImmutable(),
            Implements: implements.ToImmutable(),
            Source: null);
    }

    private async Task<ApiMember?> BuildMemberAsync(ISymbol symbol, INamedTypeSymbol? inheritedFrom = null)
    {
        var kind = ClassifyMemberKind(symbol);
        if (kind is null)
        {
            return null;
        }

        var docId = symbol.GetDocumentationCommentId() ?? string.Empty;
        var rawXml = symbol.GetDocumentationCommentXml();
        var hasInheritDoc = !string.IsNullOrWhiteSpace(rawXml)
            && rawXml.Contains("inheritdoc", StringComparison.Ordinal);
        var resolvedXml = InheritDocResolver.Resolve(rawXml, symbol);
        resolvedXml = RecordParamFallbackResolver.Resolve(resolvedXml, symbol);
        var parsedXml = _xmlDocParser.Parse(resolvedXml);

        string name;
        string typeDisplay;
        string? defaultValue;
        bool isRequired;
        switch (symbol)
        {
            case IPropertySymbol p:
                name = p.Name;
                typeDisplay = p.Type.ToDisplayString(MemberTypeFormat);
                defaultValue = ExtractPropertyDefault(p);
                isRequired = p.IsRequired;
                break;
            case IFieldSymbol f:
                name = f.Name;
                typeDisplay = f.Type.ToDisplayString(MemberTypeFormat);
                defaultValue = ExtractFieldDefault(f);
                isRequired = f.IsRequired;
                break;
            case IMethodSymbol m:
                name = FormatMethodName(m);
                typeDisplay = FormatMethodTypeDisplay(m);
                defaultValue = null;
                isRequired = false;
                break;
            case IEventSymbol e:
                name = e.Name;
                typeDisplay = e.Type.ToDisplayString(MemberTypeFormat);
                defaultValue = null;
                isRequired = false;
                break;
            default:
                return null;
        }

        string? signatureHtml = null;
        try
        {
            var decl = await _symbolService.ExtractDeclarationSignatureAsync(docId);
            if (!string.IsNullOrEmpty(decl))
            {
                signatureHtml = _highlighter.Highlight(decl, "csharp");
            }
        }
        catch
        {
            // Signature extraction is best-effort.
        }

        var parameters = ImmutableArray<ApiParameter>.Empty;
        string? returnType = null;
        if (symbol is IMethodSymbol method)
        {
            if (method.Parameters.Length > 0)
            {
                var paramBuilder = ImmutableArray.CreateBuilder<ApiParameter>(method.Parameters.Length);
                foreach (var p in method.Parameters)
                {
                    var description = parsedXml.Params.TryGetValue(p.Name, out var nodes) ? nodes : [];
                    paramBuilder.Add(new ApiParameter(p.Name, FormatParameterType(p), description));
                }
                parameters = paramBuilder.ToImmutable();
            }
            if (kind == MemberKind.Methods && !method.ReturnsVoid)
            {
                returnType = method.ReturnType.ToDisplayString(ParameterTypeFormat);
            }
        }

        return new ApiMember(
            Uid: docId,
            Name: name,
            Kind: kind.Value,
            TypeDisplay: typeDisplay,
            DefaultValue: defaultValue,
            IsRequired: isRequired,
            HasInheritDocDirective: hasInheritDoc,
            Xmldoc: parsedXml,
            SignatureHtml: signatureHtml,
            Parameters: parameters,
            ReturnTypeDisplay: returnType,
            InheritedFromUid: inheritedFrom?.GetDocumentationCommentId(),
            InheritedFromName: inheritedFrom?.Name);
    }

    private async Task<ImmutableArray<ApiTypeSummary>> BuildTypesAsync()
    {
        var projects = await GetFilteredProjectsAsync();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var builder = ImmutableArray.CreateBuilder<ApiTypeSummary>();

        foreach (var project in projects)
        {
            var compilation = await _workspace.GetCompilationAsync(project);
            if (compilation is null)
            {
                continue;
            }

            foreach (var type in EnumerateTypes(compilation.Assembly.GlobalNamespace, includeNested: true))
            {
                if (!ShouldInclude(type))
                {
                    continue;
                }

                if (_options.TypeFilter is { } extra && !extra(type))
                {
                    continue;
                }

                var xmldoc = type.GetDocumentationCommentXml();
                if (string.IsNullOrWhiteSpace(xmldoc))
                {
                    continue;
                }

                var docId = type.GetDocumentationCommentId();
                if (string.IsNullOrEmpty(docId))
                {
                    continue;
                }

                if (!seen.Add(docId))
                {
                    continue;
                }

                builder.Add(new ApiTypeSummary(
                    Uid: docId,
                    Name: type.Name,
                    Namespace: type.ContainingNamespace.ToDisplayString(),
                    Assembly: project.AssemblyName ?? project.Name,
                    Kind: ClassifyKind(type),
                    Summary: ExtractSummarySentence(xmldoc)));
            }
        }

        return builder
            .OrderBy(t => t.FullTypeName, StringComparer.OrdinalIgnoreCase)
            .ToImmutableArray();
    }

    private async Task<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> BuildExtensionsAsync()
    {
        var projects = await GetFilteredProjectsAsync();
        var collected = new List<ExtensionMethodEntry>();

        foreach (var project in projects)
        {
            var compilation = await _workspace.GetCompilationAsync(project);
            if (compilation is null)
            {
                continue;
            }

            foreach (var type in EnumerateTypes(compilation.Assembly.GlobalNamespace, includeNested: false)
                .Where(t => t.IsStatic
                    && t.DeclaredAccessibility == Accessibility.Public
                    && t.Name.EndsWith("Extensions", StringComparison.Ordinal)))
            {
                foreach (var member in type.GetMembers())
                {
                    if (member is not IMethodSymbol { IsExtensionMethod: true } method)
                    {
                        continue;
                    }

                    if (method.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    if (method.Parameters.Length == 0)
                    {
                        continue;
                    }

                    var receiverName = method.Parameters[0].Type.Name;
                    if (string.IsNullOrEmpty(receiverName))
                    {
                        continue;
                    }

                    var docId = method.GetDocumentationCommentId();
                    if (string.IsNullOrEmpty(docId))
                    {
                        continue;
                    }

                    collected.Add(new ExtensionMethodEntry(
                        Name: FormatMethodName(method),
                        Signature: FormatExtensionSignature(method),
                        Package: project.AssemblyName ?? project.Name,
                        Uid: docId,
                        ReceiverTypeName: receiverName,
                        Xmldoc: _xmlDocParser.Parse(method.GetDocumentationCommentXml())));
                }
            }
        }

        return collected
            .GroupBy(e => e.ReceiverTypeName, StringComparer.Ordinal)
            .ToImmutableDictionary(
                g => g.Key,
                g => g.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                      .ThenBy(e => e.Signature.Length)
                      .ToImmutableArray(),
                StringComparer.Ordinal);
    }

    private Task<IEnumerable<Project>> GetFilteredProjectsAsync()
    {
        var effective = _options.ProjectFilter ?? ApiReferenceOptions.DefaultProjectFilter();
        return _workspace.GetProjectsAsync(p => effective(p));
    }

    private ApiTypeSummary BuildSummary(INamedTypeSymbol type, ParsedXmlDoc xmldoc)
    {
        var docId = type.GetDocumentationCommentId() ?? string.Empty;
        var assembly = type.ContainingAssembly?.Name ?? string.Empty;
        string? summary = null;
        if (xmldoc.HasSummary)
        {
            // Use a flattened plain-text form of the summary for list/header rendering.
            summary = ExtractSummarySentence(type.GetDocumentationCommentXml());
        }

        return new ApiTypeSummary(
            Uid: docId,
            Name: type.Name,
            Namespace: type.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Assembly: assembly,
            Kind: ClassifyKind(type),
            Summary: summary);
    }

    private static bool ShouldInclude(INamedTypeSymbol type)
    {
        if (type.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        if (type.IsImplicitlyDeclared)
        {
            return false;
        }

        if (type.TypeKind is TypeKind.Delegate or TypeKind.Error or TypeKind.Module)
        {
            return false;
        }

        if (IsTopLevelStatementsProgram(type))
        {
            return false;
        }

        if (InheritsFrom(type, "System.Attribute"))
        {
            return false;
        }

        if (InheritsFrom(type, "Microsoft.AspNetCore.Components.ComponentBase"))
        {
            return false;
        }

        return true;
    }

    private static bool IsTopLevelStatementsProgram(INamedTypeSymbol type)
        => type.Name == "Program" && type.ContainingNamespace.IsGlobalNamespace;

    private static bool InheritsFrom(INamedTypeSymbol type, string fullyQualifiedBase)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == fullyQualifiedBase)
            {
                return true;
            }
        }
        return false;
    }

    private static ApiTypeKind ClassifyKind(INamedTypeSymbol type)
    {
        if (type.IsRecord)
        {
            return ApiTypeKind.Record;
        }

        return type.TypeKind switch
        {
            TypeKind.Interface => ApiTypeKind.Interface,
            TypeKind.Struct => ApiTypeKind.Struct,
            TypeKind.Enum => ApiTypeKind.Enum,
            TypeKind.Delegate => ApiTypeKind.Delegate,
            _ => ApiTypeKind.Class,
        };
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol root, bool includeNested)
    {
        var queue = new Queue<INamespaceOrTypeSymbol>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            foreach (var member in queue.Dequeue().GetMembers())
            {
                if (member is INamespaceSymbol ns)
                {
                    queue.Enqueue(ns);
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                    if (includeNested)
                    {
                        foreach (var nested in type.GetTypeMembers())
                        {
                            yield return nested;
                        }
                    }
                }
            }
        }
    }

    private static string FormatParameterType(IParameterSymbol p)
    {
        var prefix = p.RefKind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            _ => string.Empty,
        };
        var typeText = p.Type.ToDisplayString(ParameterTypeFormat);
        var suffix = p.HasExplicitDefaultValue ? " (optional)" : string.Empty;
        return $"{prefix}{typeText}{suffix}";
    }

    private static string FormatMethodName(IMethodSymbol method) => method.TypeParameters.Length == 0
        ? method.Name
        : $"{method.Name}<{string.Join(", ", method.TypeParameters.Select(t => t.Name))}>";

    private static string FormatExtensionSignature(IMethodSymbol method)
    {
        var returnText = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString(SignatureFormat);
        var parameters = method.Parameters.Select((p, i) =>
        {
            var prefix = i == 0 ? "this " : string.Empty;
            var typeText = p.Type.ToDisplayString(SignatureFormat);
            var suffix = p.HasExplicitDefaultValue ? " = …" : string.Empty;
            return $"{prefix}{typeText}{suffix}";
        });
        return $"{returnText} {FormatMethodName(method)}({string.Join(", ", parameters)})";
    }

    private static string? ExtractSummarySentence(string? xmlDoc)
    {
        if (string.IsNullOrWhiteSpace(xmlDoc))
        {
            return null;
        }

        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlDoc);
            var summary = doc.Root?.Element("summary")?.Value;
            if (string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            var collapsed = System.Text.RegularExpressions.Regex.Replace(summary, @"\s+", " ").Trim();
            var period = collapsed.IndexOf('.');
            return period > 0 ? collapsed[..(period + 1)] : collapsed;
        }
        catch
        {
            return null;
        }
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

    private static bool MatchesKind(ISymbol symbol, MemberKind kind) => kind switch
    {
        MemberKind.Properties => symbol is IPropertySymbol,
        MemberKind.Fields => symbol is IFieldSymbol,
        MemberKind.Methods => symbol is IMethodSymbol m && m.MethodKind == MethodKind.Ordinary,
        MemberKind.Constructors => symbol is IMethodSymbol { MethodKind: MethodKind.Constructor },
        MemberKind.Events => symbol is IEventSymbol,
        MemberKind.All => true,
        _ => false,
    };

    private static MemberKind? ClassifyMemberKind(ISymbol symbol) => symbol switch
    {
        IPropertySymbol => MemberKind.Properties,
        IFieldSymbol => MemberKind.Fields,
        IMethodSymbol { MethodKind: MethodKind.Constructor } => MemberKind.Constructors,
        IMethodSymbol => MemberKind.Methods,
        IEventSymbol => MemberKind.Events,
        _ => null,
    };

    private static string FormatMethodTypeDisplay(IMethodSymbol method)
    {
        var paramsText = string.Join(", ", method.Parameters.Select(p =>
        {
            var typeText = p.Type.ToDisplayString(ParameterTypeFormat);
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

        var returnText = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString(ParameterTypeFormat);
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

                if (property.ContainingType.TypeKind == TypeKind.Interface)
                {
                    if (propertyDecl.ExpressionBody?.Expression is LiteralExpressionSyntax literal)
                    {
                        return literal.ToString();
                    }
                    return null;
                }

                if (propertyDecl.ExpressionBody is not null)
                {
                    return null;
                }

                return FallbackClrDefault(property);
            }

            if (syntax is ParameterSyntax parameterSyntax)
            {
                if (parameterSyntax.Default?.Value is { } paramDefault)
                {
                    return paramDefault.ToString();
                }
                return FallbackClrDefault(property);
            }
        }

        return null;
    }

    private static string? FallbackClrDefault(IPropertySymbol property)
    {
        if (property.IsRequired)
        {
            return null;
        }

        if (property.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return "null";
        }

        return property.Type.SpecialType switch
        {
            SpecialType.System_Boolean => "false",
            SpecialType.System_SByte
                or SpecialType.System_Byte
                or SpecialType.System_Int16
                or SpecialType.System_UInt16
                or SpecialType.System_Int32
                or SpecialType.System_UInt32
                or SpecialType.System_Int64
                or SpecialType.System_UInt64
                or SpecialType.System_Single
                or SpecialType.System_Double
                or SpecialType.System_Decimal => "0",
            _ => null,
        };
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

    private static string FormatConstant(object? value) => value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        bool b => b ? "true" : "false",
        char c => $"'{c}'",
        _ => value.ToString() ?? "null",
    };
}