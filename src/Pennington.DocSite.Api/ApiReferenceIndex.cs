namespace Pennington.DocSite.Api;

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Pennington.Infrastructure;
using Pennington.Roslyn.Workspace;

/// <summary>
/// One auto-discovered public type, slugged for use as the
/// <c>{key}</c> segment of the <c>/reference/api/{key}</c> Razor route.
/// </summary>
/// <param name="Slug">URL slug used as the <c>{key}</c> route segment.</param>
/// <param name="XmlDocId">Roslyn documentation comment ID (<c>T:Namespace.TypeName</c>) of the underlying type.</param>
/// <param name="TypeName">Short type name without the namespace.</param>
/// <param name="Namespace">Fully-qualified containing namespace, used for the index page's grouping headings.</param>
/// <param name="Summary">First sentence of the type's xmldoc summary, or <c>null</c> if absent.</param>
public sealed record ApiReferenceEntry(
    string Slug,
    string XmlDocId,
    string TypeName,
    string Namespace,
    string? Summary)
{
    /// <summary>Fully-qualified type name (namespace + dot + type name).</summary>
    public string FullTypeName =>
        string.IsNullOrEmpty(Namespace) ? TypeName : $"{Namespace}.{TypeName}";
}

/// <summary>
/// Singleton that walks the Roslyn workspace once and publishes a
/// slug → entry map for the API-reference Razor page and content service.
/// </summary>
public sealed partial class ApiReferenceIndex
{
    private readonly ISolutionWorkspaceService _workspace;
    private readonly ApiReferenceOptions _options;
    private readonly ILogger<ApiReferenceIndex> _logger;
    private readonly AsyncLazy<ImmutableDictionary<string, ApiReferenceEntry>> _entries;

    /// <summary>Initializes the index.</summary>
    public ApiReferenceIndex(
        ISolutionWorkspaceService workspace,
        ApiReferenceOptions options,
        ILogger<ApiReferenceIndex> logger)
    {
        _workspace = workspace;
        _options = options;
        _logger = logger;
        _entries = new AsyncLazy<ImmutableDictionary<string, ApiReferenceEntry>>(BuildAsync);
    }

    /// <summary>Gets the slug → entry map, building it on first access.</summary>
    public Task<ImmutableDictionary<string, ApiReferenceEntry>> GetEntriesAsync() => _entries.Value;

    private async Task<ImmutableDictionary<string, ApiReferenceEntry>> BuildAsync()
    {
        var projects = await ApiReferenceWorkspace.GetFilteredProjectsAsync(_workspace, _options.ProjectFilter);

        var collected = new List<ApiReferenceEntry>();
        var seenXmlDocIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var project in projects)
        {
            var compilation = await _workspace.GetCompilationAsync(project);
            if (compilation is null) continue;

            foreach (var type in ApiReferenceWorkspace.EnumerateTypes(compilation.Assembly.GlobalNamespace, includeNested: true))
            {
                if (!ShouldInclude(type)) continue;
                if (_options.TypeFilter is { } extra && !extra(type)) continue;

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

        var slugCounts = collected
            .GroupBy(e => e.Slug, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        var builder = ImmutableDictionary.CreateBuilder<string, ApiReferenceEntry>(StringComparer.Ordinal);
        foreach (var entry in collected)
        {
            var slug = slugCounts[entry.Slug] == 1 ? entry.Slug : Disambiguate(entry);
            builder.Add(slug, entry with { Slug = slug });
        }

        _logger.LogInformation(
            "ApiReferenceIndex: published {Count} auto-discovered type pages",
            builder.Count);

        return builder.ToImmutable();
    }

    private static string Disambiguate(ApiReferenceEntry entry)
    {
        var nsTail = LastNamespaceSegment(entry.Namespace);
        return string.IsNullOrEmpty(nsTail) ? entry.Slug : $"{ToSlug(nsTail)}-{entry.Slug}";
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
