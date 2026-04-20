namespace Pennington.DocSite.Api;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Publishes one <c>/reference/api/{slug}/</c> entry per discovered public
/// type (via <see cref="ApiReferenceIndex"/>), backed by the parameterized
/// <c>ApiReferencePage.razor</c> template.
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

    public ApiReferenceContentService(ApiReferenceIndex index)
    {
        _index = index;
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
            yield return new DiscoveredItem(RouteFor(entry), new RazorPageSource(sourceId));
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
            var route = RouteFor(entry);
            var hierarchyParts = route.CanonicalPath.Value
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            builder.Add(new ContentTocItem(
                Title: entry.FullTypeName,
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
            builder.Add(new CrossReference($"reference.api.{entry.Slug}", entry.FullTypeName, RouteFor(entry)));
        }

        return builder.ToImmutable();
    }

    private static ContentRoute RouteFor(ApiReferenceEntry entry)
        => ContentRouteFactory.FromUrl(new UrlPath($"/reference/api/{entry.Slug}/"));
}
