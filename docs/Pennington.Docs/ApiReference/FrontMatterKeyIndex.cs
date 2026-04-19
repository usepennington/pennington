namespace Pennington.Docs.ApiReference;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Pennington.Infrastructure;
using Pennington.Roslyn.Workspace;

/// <summary>
/// One YAML front-matter key observed on one or more concrete <c>IFrontMatter</c> implementations.
/// </summary>
internal sealed record FrontMatterKeyEntry(
    string YamlKey,
    string ClrPropertyName,
    string TypeDisplay,
    string? DefaultValue,
    ImmutableArray<string> AppliesTo,
    string DeclaringSurface,
    string XmlDocId);

/// <summary>
/// Singleton that walks every public type implementing <c>Pennington.FrontMatter.IFrontMatter</c> in the solution
/// and projects their declared properties into a per-YAML-key catalog for the front-matter reference page.
/// </summary>
internal sealed class FrontMatterKeyIndex
{
    private static readonly string[] CapabilityInterfaces =
    [
        "IFrontMatter",
        "ITaggable",
        "IOrderable",
        "ISectionable",
        "IRedirectable",
    ];

    private readonly ISolutionWorkspaceService _workspace;
    private readonly AsyncLazy<ImmutableArray<FrontMatterKeyEntry>> _entries;

    public FrontMatterKeyIndex(ISolutionWorkspaceService workspace)
    {
        _workspace = workspace;
        _entries = new AsyncLazy<ImmutableArray<FrontMatterKeyEntry>>(BuildAsync);
    }

    public Task<ImmutableArray<FrontMatterKeyEntry>> GetEntriesAsync() => _entries.Value;

    private async Task<ImmutableArray<FrontMatterKeyEntry>> BuildAsync()
    {
        var projects = await _workspace.GetProjectsAsync(p =>
            p.Name.StartsWith("Pennington", StringComparison.Ordinal)
            && !p.Name.EndsWith(".Tests", StringComparison.Ordinal)
            && !p.Name.EndsWith(".IntegrationTests", StringComparison.Ordinal)
            && p.Name != "Pennington.Docs");

        var observations = new List<(string YamlKey, string Clr, string TypeDisplay, string? DefaultValue, string Record, string Surface, string XmlDocId)>();

        foreach (var project in projects)
        {
            var compilation = await _workspace.GetCompilationAsync(project);
            if (compilation is null) continue;

            foreach (var type in EnumerateTypes(compilation.Assembly.GlobalNamespace))
            {
                if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct)) continue;
                if (type.DeclaredAccessibility != Accessibility.Public) continue;
                if (type.IsAbstract || type.IsStatic) continue;
                if (!ImplementsFrontMatter(type)) continue;

                foreach (var member in type.GetMembers())
                {
                    if (member is not IPropertySymbol { DeclaredAccessibility: Accessibility.Public } property)
                        continue;
                    if (property.IsIndexer || property.IsStatic) continue;

                    var surface = ResolveDeclaringSurface(type, property.Name);
                    var defaultValue = ExtractDefault(property);
                    var typeDisplay = property.Type.ToDisplayString(TypeFormat);
                    var docId = property.GetDocumentationCommentId() ?? string.Empty;

                    observations.Add((
                        YamlKey: ToCamelCase(property.Name),
                        Clr: property.Name,
                        TypeDisplay: typeDisplay,
                        DefaultValue: defaultValue,
                        Record: type.Name,
                        Surface: surface,
                        XmlDocId: docId));
                }
            }
        }

        return observations
            .GroupBy(o => o.YamlKey, StringComparer.Ordinal)
            .Select(group =>
            {
                var primary = group.First();
                var records = group
                    .Select(o => o.Record)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(r => r, StringComparer.Ordinal)
                    .ToImmutableArray();

                var types = group
                    .Select(o => o.TypeDisplay)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var typeDisplay = types.Count == 1 ? types[0] : string.Join(" / ", types);

                var defaults = group
                    .Select(o => o.DefaultValue)
                    .Where(d => d is not null)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var defaultValue = defaults.Count switch
                {
                    0 => null,
                    1 => defaults[0],
                    _ => string.Join(" / ", defaults),
                };

                var xmlDocId = group
                    .FirstOrDefault(o => !string.IsNullOrEmpty(o.XmlDocId))
                    .XmlDocId ?? primary.XmlDocId;

                return new FrontMatterKeyEntry(
                    YamlKey: primary.YamlKey,
                    ClrPropertyName: primary.Clr,
                    TypeDisplay: typeDisplay,
                    DefaultValue: defaultValue,
                    AppliesTo: records,
                    DeclaringSurface: primary.Surface,
                    XmlDocId: xmlDocId);
            })
            .OrderBy(e => e.YamlKey, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static readonly SymbolDisplayFormat TypeFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

    private static string ResolveDeclaringSurface(INamedTypeSymbol record, string propertyName)
    {
        foreach (var iface in record.AllInterfaces)
        {
            if (!CapabilityInterfaces.Contains(iface.Name, StringComparer.Ordinal)) continue;
            foreach (var member in iface.GetMembers().OfType<IPropertySymbol>())
            {
                if (string.Equals(member.Name, propertyName, StringComparison.Ordinal))
                {
                    return iface.Name;
                }
            }
        }
        return "record-local";
    }

    private static string? ExtractDefault(IPropertySymbol property)
    {
        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();
            if (syntax is Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax decl
                && decl.Initializer?.Value is { } value)
            {
                return value.ToString();
            }
        }

        if (property.NullableAnnotation == NullableAnnotation.Annotated) return "null";

        return property.Type.SpecialType switch
        {
            SpecialType.System_Boolean => "false",
            SpecialType.System_String => "\"\"",
            _ => null,
        };
    }

    private static bool ImplementsFrontMatter(INamedTypeSymbol type) =>
        type.AllInterfaces.Any(i =>
            i.Name == "IFrontMatter"
            && i.ContainingNamespace.ToDisplayString() == "Pennington.FrontMatter");

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol root)
    {
        var queue = new Queue<INamespaceOrTypeSymbol>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var member in current.GetMembers())
            {
                switch (member)
                {
                    case INamespaceSymbol ns:
                        queue.Enqueue(ns);
                        break;
                    case INamedTypeSymbol type:
                        yield return type;
                        break;
                }
            }
        }
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (char.IsLower(name[0])) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
