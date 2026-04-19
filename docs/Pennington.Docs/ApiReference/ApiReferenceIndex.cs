namespace Pennington.Docs.ApiReference;

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Infrastructure;
using Roslyn.Workspace;

/// <summary>
/// One auto-discovered Pennington public type, slugged for use as the
/// <c>{key}</c> segment of the <c>/reference/api/{key}</c> Razor route.
/// </summary>
internal sealed record ApiReferenceEntry(
    string Slug,
    string XmlDocId,
    string TypeName,
    string Namespace,
    string? Summary)
{
    public string FullTypeName =>
        string.IsNullOrEmpty(Namespace) ? TypeName : $"{Namespace}.{TypeName}";
}

/// <summary>
/// Singleton that walks the Roslyn workspace once and publishes a
/// slug → entry map for the API-reference Razor page and content service.
/// </summary>
internal sealed partial class ApiReferenceIndex
{
    private readonly ISolutionWorkspaceService _workspace;
    private readonly ILogger<ApiReferenceIndex> _logger;
    private readonly AsyncLazy<ImmutableDictionary<string, ApiReferenceEntry>> _entries;

    public ApiReferenceIndex(
        ISolutionWorkspaceService workspace,
        ILogger<ApiReferenceIndex> logger)
    {
        _workspace = workspace;
        _logger = logger;
        _entries = new AsyncLazy<ImmutableDictionary<string, ApiReferenceEntry>>(BuildAsync);
    }

    public Task<ImmutableDictionary<string, ApiReferenceEntry>> GetEntriesAsync() => _entries.Value;

    private async Task<ImmutableDictionary<string, ApiReferenceEntry>> BuildAsync()
    {
        var projects = await _workspace.GetProjectsAsync(p =>
        {
            var name = StripTargetFrameworkSuffix(p.Name);
            return name.StartsWith("Pennington", StringComparison.Ordinal)
                && !name.EndsWith(".Tests", StringComparison.Ordinal)
                && !name.EndsWith(".IntegrationTests", StringComparison.Ordinal)
                && name != "Pennington.Docs";
        });

        var collected = new List<ApiReferenceEntry>();
        var seenXmlDocIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var project in projects)
        {
            var compilation = await _workspace.GetCompilationAsync(project);
            if (compilation is null) continue;

            foreach (var type in EnumerateTypes(compilation.Assembly.GlobalNamespace))
            {
                if (!ShouldInclude(type)) continue;

                var xmldoc = type.GetDocumentationCommentXml();
                if (string.IsNullOrWhiteSpace(xmldoc)) continue;

                var docId = type.GetDocumentationCommentId();
                if (string.IsNullOrEmpty(docId)) continue;
                if (!seenXmlDocIds.Add(docId)) continue;

                collected.Add(new ApiReferenceEntry(
                    Slug: ToSlug(type.Name),
                    XmlDocId: docId,
                    TypeName: type.Name,
                    Namespace: type.ContainingNamespace.ToDisplayString(),
                    Summary: ExtractSummarySentence(xmldoc)));
            }
        }

        var bySlug = collected
            .GroupBy(e => e.Slug, StringComparer.Ordinal)
            .ToList();

        var builder = ImmutableDictionary.CreateBuilder<string, ApiReferenceEntry>(StringComparer.Ordinal);
        foreach (var group in bySlug)
        {
            if (group.Count() == 1)
            {
                var entry = group.Single();
                builder.Add(entry.Slug, entry);
            }
            else
            {
                foreach (var candidate in group)
                {
                    var nsTail = LastNamespaceSegment(candidate.Namespace);
                    var disambiguated = string.IsNullOrEmpty(nsTail)
                        ? candidate.Slug
                        : $"{ToSlug(nsTail)}-{candidate.Slug}";
                    builder.Add(disambiguated, candidate with { Slug = disambiguated });
                }
            }
        }

        _logger.LogInformation(
            "ApiReferenceIndex: published {Count} auto-discovered type pages",
            builder.Count);

        return builder.ToImmutable();
    }

    private static bool ShouldInclude(INamedTypeSymbol type)
    {
        if (type.DeclaredAccessibility != Accessibility.Public) return false;
        if (type.IsImplicitlyDeclared) return false;
        if (type.TypeKind is TypeKind.Delegate or TypeKind.Error or TypeKind.Module) return false;
        if (InheritsFrom(type, "System.Attribute")) return false;
        if (InheritsFrom(type, "Microsoft.AspNetCore.Components.ComponentBase")) return false;
        return true;
    }

    private static bool InheritsFrom(INamedTypeSymbol type, string fullyQualifiedBase)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == fullyQualifiedBase) return true;
        }
        return false;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol root)
    {
        var queue = new Queue<INamespaceOrTypeSymbol>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var member in current.GetMembers())
            {
                if (member is INamespaceSymbol ns)
                {
                    queue.Enqueue(ns);
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                    foreach (var nested in type.GetTypeMembers())
                    {
                        yield return nested;
                    }
                }
            }
        }
    }

    internal static string ToSlug(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var sb = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0 && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
            {
                sb.Append('-');
            }
            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }

    private static string StripTargetFrameworkSuffix(string name)
    {
        var open = name.LastIndexOf('(');
        if (open < 0 || !name.EndsWith(')')) return name;
        return name[..open];
    }

    private static string LastNamespaceSegment(string ns)
    {
        if (string.IsNullOrEmpty(ns)) return string.Empty;
        var idx = ns.LastIndexOf('.');
        return idx < 0 ? ns : ns[(idx + 1)..];
    }

    private static string? ExtractSummarySentence(string xmlDoc)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlDoc);
            var summary = doc.Root?.Element("summary")?.Value;
            if (string.IsNullOrWhiteSpace(summary)) return null;

            var collapsed = SpaceReplaceRegex.Replace(summary, " ")
                .Trim();

            var period = collapsed.IndexOf('.');
            return period > 0 ? collapsed[..(period + 1)] : collapsed;
        }
        catch
        {
            return null;
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex SpaceReplaceRegex { get; }
}
