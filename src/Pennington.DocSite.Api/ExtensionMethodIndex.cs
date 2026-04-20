namespace Pennington.DocSite.Api;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Pennington.Infrastructure;
using Pennington.Roslyn.Workspace;

/// <summary>
/// One public static extension method discovered in a workspace assembly,
/// projected for reference-doc rendering.
/// </summary>
public sealed record ExtensionMethodEntry(
    string Name,
    string Signature,
    string Package,
    string XmlDocId,
    string ReceiverTypeName);

/// <summary>
/// Singleton that walks every <c>*Extensions</c> static class in the configured
/// projects and groups public extension methods by the unqualified short name
/// of their receiver type (<c>IServiceCollection</c>, <c>WebApplication</c>,
/// etc.) for the host-integration reference page.
/// </summary>
public sealed class ExtensionMethodIndex
{
    private readonly ISolutionWorkspaceService _workspace;
    private readonly ApiReferenceOptions _options;
    private readonly AsyncLazy<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> _entries;

    /// <summary>Initializes the index.</summary>
    public ExtensionMethodIndex(ISolutionWorkspaceService workspace, ApiReferenceOptions options)
    {
        _workspace = workspace;
        _options = options;
        _entries = new AsyncLazy<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>>(BuildAsync);
    }

    /// <summary>Gets the receiver-name → extension-method-entries map, building it on first access.</summary>
    public Task<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> GetEntriesAsync()
        => _entries.Value;

    private async Task<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> BuildAsync()
    {
        var projects = await ApiReferenceWorkspace.GetFilteredProjectsAsync(_workspace, _options.ProjectFilter);

        var collected = new List<ExtensionMethodEntry>();

        foreach (var project in projects)
        {
            var compilation = await _workspace.GetCompilationAsync(project);
            if (compilation is null) continue;

            foreach (var type in ApiReferenceWorkspace.EnumerateTypes(compilation.Assembly.GlobalNamespace)
                .Where(t => t.IsStatic
                    && t.DeclaredAccessibility == Accessibility.Public
                    && t.Name.EndsWith("Extensions", StringComparison.Ordinal)))
            {
                foreach (var member in type.GetMembers())
                {
                    if (member is not IMethodSymbol { IsExtensionMethod: true } method) continue;
                    if (method.DeclaredAccessibility != Accessibility.Public) continue;
                    if (method.Parameters.Length == 0) continue;

                    var receiver = method.Parameters[0].Type;
                    var receiverName = receiver.Name;
                    if (string.IsNullOrEmpty(receiverName)) continue;

                    var docId = method.GetDocumentationCommentId();
                    if (string.IsNullOrEmpty(docId)) continue;

                    collected.Add(new ExtensionMethodEntry(
                        Name: FormatName(method),
                        Signature: FormatSignature(method),
                        Package: project.AssemblyName ?? project.Name,
                        XmlDocId: docId,
                        ReceiverTypeName: receiverName));
                }
            }
        }

        return collected
            .GroupBy(e => e.ReceiverTypeName, StringComparer.Ordinal)
            .ToImmutableDictionary(
                g => g.Key,
                g => g
                    .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(e => e.Signature.Length)
                    .ToImmutableArray(),
                StringComparer.Ordinal);
    }

    private static string FormatName(IMethodSymbol method) => method.TypeParameters.Length == 0
        ? method.Name
        : $"{method.Name}<{string.Join(", ", method.TypeParameters.Select(t => t.Name))}>";

    private static readonly SymbolDisplayFormat SignatureFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

    private static string FormatSignature(IMethodSymbol method)
    {
        var returnText = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString(SignatureFormat);
        var parameters = method.Parameters.Select((p, i) =>
        {
            var prefix = i == 0 ? "this " : string.Empty;
            var typeText = p.Type.ToDisplayString(SignatureFormat);
            var suffix = p.HasExplicitDefaultValue ? " = …" : string.Empty;
            return $"{prefix}{typeText}{suffix}";
        });
        return $"{returnText} {FormatName(method)}({string.Join(", ", parameters)})";
    }
}
