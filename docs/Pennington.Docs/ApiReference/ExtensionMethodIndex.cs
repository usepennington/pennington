namespace Pennington.Docs.ApiReference;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Pennington.Infrastructure;
using Pennington.Roslyn.Workspace;

/// <summary>
/// One public static extension method discovered in a Pennington assembly, projected for reference-doc rendering.
/// </summary>
internal sealed record ExtensionMethodEntry(
    string Name,
    string Signature,
    string Package,
    string XmlDocId,
    string ReceiverTypeName);

/// <summary>
/// Singleton that walks every <c>*Extensions</c> static class in the Pennington solution once and groups public
/// extension methods by the unqualified short name of their receiver type (<c>IServiceCollection</c>,
/// <c>WebApplication</c>, <c>IEndpointRouteBuilder</c>, etc.) for the host-integration reference page.
/// </summary>
internal sealed class ExtensionMethodIndex
{
    private readonly ISolutionWorkspaceService _workspace;
    private readonly AsyncLazy<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> _entries;

    public ExtensionMethodIndex(ISolutionWorkspaceService workspace)
    {
        _workspace = workspace;
        _entries = new AsyncLazy<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>>(BuildAsync);
    }

    public Task<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> GetEntriesAsync()
        => _entries.Value;

    private async Task<ImmutableDictionary<string, ImmutableArray<ExtensionMethodEntry>>> BuildAsync()
    {
        var projects = await _workspace.GetProjectsAsync(p =>
            p.Name.StartsWith("Pennington", StringComparison.Ordinal)
            && !p.Name.EndsWith(".Tests", StringComparison.Ordinal)
            && !p.Name.EndsWith(".IntegrationTests", StringComparison.Ordinal)
            && p.Name != "Pennington.Docs");

        var collected = new List<ExtensionMethodEntry>();

        foreach (var project in projects)
        {
            var compilation = await _workspace.GetCompilationAsync(project);
            if (compilation is null) continue;

            foreach (var type in EnumerateStaticExtensionTypes(compilation.Assembly.GlobalNamespace))
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

    private static IEnumerable<INamedTypeSymbol> EnumerateStaticExtensionTypes(INamespaceSymbol root)
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
                    case INamedTypeSymbol { IsStatic: true, DeclaredAccessibility: Accessibility.Public } type
                        when type.Name.EndsWith("Extensions", StringComparison.Ordinal):
                        yield return type;
                        break;
                }
            }
        }
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
