namespace Pennington.DocSite.Api;

using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using Pennington.ApiMetadata;
using Pennington.Infrastructure;

/// <summary>
/// One auto-discovered public type, slugged for use as the
/// <c>{key}</c> segment of the <c>/reference/api/{key}</c> Razor route.
/// </summary>
/// <param name="Slug">URL slug used as the <c>{key}</c> route segment.</param>
/// <param name="Uid">Canonical xmldocid (<c>T:Namespace.TypeName</c>) of the underlying type.</param>
/// <param name="TypeName">Short type name without the namespace.</param>
/// <param name="Namespace">Fully-qualified containing namespace, used for the index page's grouping headings.</param>
/// <param name="Summary">First sentence of the type's xmldoc summary, or <c>null</c> if absent.</param>
public sealed record ApiReferenceEntry(
    string Slug,
    string Uid,
    string TypeName,
    string Namespace,
    string? Summary)
{
    /// <summary>Fully-qualified type name (namespace + dot + type name).</summary>
    public string FullTypeName =>
        string.IsNullOrEmpty(Namespace) ? TypeName : $"{Namespace}.{TypeName}";
}

/// <summary>
/// Singleton that asks the configured <see cref="IApiMetadataProvider"/> once
/// and publishes a slug → entry map for the API-reference Razor page and content service.
/// </summary>
public sealed class ApiReferenceIndex
{
    private readonly IApiMetadataProvider _provider;
    private readonly ILogger<ApiReferenceIndex> _logger;
    private readonly AsyncLazy<ImmutableDictionary<string, ApiReferenceEntry>> _entries;

    /// <summary>Name of the registration this index belongs to. Used by the content service to emit routes and cross-references.</summary>
    public string Name { get; }

    /// <summary>Initializes the index.</summary>
    public ApiReferenceIndex(IApiMetadataProvider provider, string name, ILogger<ApiReferenceIndex> logger)
    {
        _provider = provider;
        _logger = logger;
        Name = name;
        _entries = new AsyncLazy<ImmutableDictionary<string, ApiReferenceEntry>>(BuildAsync);
    }

    /// <summary>Gets the slug → entry map, building it on first access.</summary>
    public Task<ImmutableDictionary<string, ApiReferenceEntry>> GetEntriesAsync() => _entries.Task;

    private async Task<ImmutableDictionary<string, ApiReferenceEntry>> BuildAsync()
    {
        var types = await _provider.GetTypesAsync();

        var collected = new List<ApiReferenceEntry>(types.Length);
        foreach (var t in types)
        {
            collected.Add(new ApiReferenceEntry(
                Slug: ToSlug(t.Name),
                Uid: t.Uid,
                TypeName: t.Name,
                Namespace: t.Namespace,
                Summary: t.Summary));
        }

        // Each entry starts at depth 0 (just ToSlug(Name)). On a collision, every entry sharing
        // the current slug pulls in one more namespace segment from the tail. Two types sharing
        // both name *and* namespace would loop forever, so we cap each entry at its segment count.
        var depths = collected.ToDictionary(e => e.Uid, _ => 0, StringComparer.Ordinal);

        while (true)
        {
            var collisions = collected
                .GroupBy(e => BuildSlug(e, depths[e.Uid]), StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .ToList();

            if (collisions.Count == 0)
            {
                break;
            }

            var advanced = false;
            foreach (var group in collisions)
            {
                foreach (var entry in group)
                {
                    if (depths[entry.Uid] < SegmentCount(entry.Namespace))
                    {
                        depths[entry.Uid]++;
                        advanced = true;
                    }
                }
            }

            if (!advanced)
            {
                break;
            }
        }

        var builder = ImmutableDictionary.CreateBuilder<string, ApiReferenceEntry>(StringComparer.Ordinal);
        foreach (var entry in collected)
        {
            var slug = BuildSlug(entry, depths[entry.Uid]);
            builder.Add(slug, entry with { Slug = slug });
        }

        _logger.LogInformation(
            "ApiReferenceIndex({Name}): published {Count} auto-discovered type pages",
            Name, builder.Count);

        return builder.ToImmutable();
    }

    private static string BuildSlug(ApiReferenceEntry entry, int depth)
    {
        if (depth <= 0 || string.IsNullOrEmpty(entry.Namespace))
        {
            return entry.Slug;
        }

        var parts = entry.Namespace.Split('.');
        var start = Math.Max(0, parts.Length - depth);
        var sb = new StringBuilder();
        for (var i = start; i < parts.Length; i++)
        {
            sb.Append(ToSlug(parts[i]));
            sb.Append('-');
        }
        sb.Append(entry.Slug);
        return sb.ToString();
    }

    private static int SegmentCount(string ns) =>
        string.IsNullOrEmpty(ns) ? 0 : ns.AsSpan().Count('.') + 1;

    internal static string ToSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var sb = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            // Drop generic-arity markers and separators that would leak into URLs
            // or filesystem paths (Windows rejects `<`, `>`, `,` in filenames).
            if (c is '<' or '>' or ',' or ' ')
            {
                continue;
            }

            if (char.IsUpper(c) && i > 0 && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
            {
                sb.Append('-');
            }
            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString().Trim('-');
    }
}