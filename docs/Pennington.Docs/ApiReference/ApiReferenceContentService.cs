namespace Pennington.Docs.ApiReference;

using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Content;
using Pipeline;
using Routing;

/// <summary>
/// Publishes one <c>/reference/api/{slug}/</c> entry per public Pennington
/// type (discovered via <see cref="ApiReferenceIndex"/>), backed by the
/// parameterized <c>ApiReferencePage.razor</c> template.
/// <para>
/// TOC entries are deliberately empty — these pages do not appear in the
/// sidebar. Search and llms.txt indexing stay on via
/// <see cref="GetIndexableEntriesAsync"/>, and xref links resolve via
/// <see cref="GetCrossReferencesAsync"/>.
/// </para>
/// </summary>
internal sealed class ApiReferenceContentService : IContentService
{
    private readonly ApiReferenceIndex _index;
    private readonly ILogger<ApiReferenceContentService> _logger;

    public ApiReferenceContentService(
        ApiReferenceIndex index,
        ILogger<ApiReferenceContentService> logger)
    {
        _index = index;
        _logger = logger;
    }

    public string DefaultSectionLabel => "";

    public int SearchPriority => 3;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
         var entries = await _index.GetEntriesAsync();
        var componentType = typeof(Components.Reference.ApiReferencePage);
        var sourceId = componentType.AssemblyQualifiedName
            ?? componentType.FullName
            ?? componentType.Name;

        foreach (var entry in entries.Values)
        {
            var route = ContentRouteFactory.FromUrl(new UrlPath($"/reference/api/{entry.Slug}/"));
            yield return new DiscoveredItem(route, new RazorPageSource(sourceId));
        }
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public async Task<ImmutableList<ContentTocItem>> GetIndexableEntriesAsync()
    {
        var entries = await _index.GetEntriesAsync();
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var entry in entries.Values)
        {
            var route = ContentRouteFactory.FromUrl(new UrlPath($"/reference/api/{entry.Slug}/"));
            var hierarchyParts = route.CanonicalPath.Value
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            builder.Add(new ContentTocItem(
                Title: entry.TypeName,
                Route: route,
                Order: int.MaxValue,
                HierarchyParts: hierarchyParts,
                SectionLabel: DefaultSectionLabel,
                Locale: null)
            {
                Description = entry.Summary,
            });
        }

        return builder.ToImmutable();
    }

    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var entries = await _index.GetEntriesAsync();
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var entry in entries.Values)
        {
            var route = ContentRouteFactory.FromUrl(new UrlPath($"/reference/api/{entry.Slug}/"));
            builder.Add(new CrossReference($"reference.api.{entry.Slug}", entry.TypeName, route));
        }

        return builder.ToImmutable();
    }
}
