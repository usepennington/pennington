namespace Pennington.DocSite.Api;

using System.Collections.Immutable;
using Pennington.Content;
using Pennington.LlmsTxt;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Publishes one <c>{RoutePrefix}{slug}/</c> entry per discovered public type,
/// backed by the shared <see cref="Components.Reference.ApiReferencePage"/>
/// template. One instance per <see cref="ApiReferenceRegistration"/>.
/// <para>
/// The TOC surfaces one sidebar entry pointing at the registration's index page,
/// titled via <see cref="ApiReferenceRegistrationOptions.TocTitle"/>. Per-type
/// pages are kept out of the sidebar — the index page lists them. Search and
/// llms.txt indexing stay on via <see cref="GetIndexableEntriesAsync"/>, and
/// xref links resolve via <see cref="GetCrossReferencesAsync"/>. Subtree
/// declaration is surfaced via <see cref="GetLlmsSubtreesAsync"/> so the
/// registration's prefix gets its own <c>{prefix}llms.txt</c> split file.
/// </para>
/// </summary>
internal sealed class ApiReferenceContentService : IContentService, ILlmsSubtreeProvider
{
    private readonly ApiReferenceIndex _index;
    private readonly ApiReferenceRegistration _registration;

    public ApiReferenceContentService(ApiReferenceIndex index, ApiReferenceRegistration registration)
    {
        _index = index;
        _registration = registration;
    }

    /// <inheritdoc/>
    public Task<ImmutableList<LlmsSubtree>> GetLlmsSubtreesAsync()
        => Task.FromResult(ImmutableList.Create(new LlmsSubtree(
            routePrefix: _registration.RoutePrefix,
            title: _registration.TocTitle ?? "API reference",
            description: "Type and member reference for this library.")));

    public string DefaultSectionLabel => "";

    public int SearchPriority => 3;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var entries = await _index.GetEntriesAsync();

        var pageType = typeof(Components.Reference.ApiReferencePage);
        var pageSourceId = pageType.AssemblyQualifiedName ?? pageType.FullName ?? pageType.Name;

        var indexPageType = typeof(Components.Reference.ApiReferenceIndexPage);
        var indexSourceId = indexPageType.AssemblyQualifiedName ?? indexPageType.FullName ?? indexPageType.Name;

        // Root index page for this registration: /<prefix>/.
        yield return new DiscoveredItem(
            ContentRouteFactory.FromUrl(new UrlPath(_registration.RoutePrefix)),
            new RazorPageSource(indexSourceId));

        foreach (var entry in entries.Values)
        {
            yield return new DiscoveredItem(RouteFor(entry), new RazorPageSource(pageSourceId));
        }
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        if (_registration.TocTitle is null)
        {
            return Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        }

        var route = ContentRouteFactory.FromUrl(new UrlPath(_registration.RoutePrefix));
        var hierarchyParts = route.CanonicalPath.Value
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        return Task.FromResult(ImmutableList.Create(new ContentTocItem(
            Title: _registration.TocTitle,
            Route: route,
            Order: int.MaxValue,
            HierarchyParts: hierarchyParts,
            SectionLabel: _registration.TocSectionLabel ?? DefaultSectionLabel,
            Locale: null)));
    }

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
        var isDefault = string.Equals(_registration.Name, "default", StringComparison.Ordinal);

        foreach (var entry in entries.Values)
        {
            // Back-compat: the unnamed/default registration keeps the flat
            // `reference.api.{slug}` xref uid. Named registrations get
            // `reference.api.{name}.{slug}` so two trees can't collide.
            var uid = isDefault
                ? $"reference.api.{entry.Slug}"
                : $"reference.api.{_registration.Name}.{entry.Slug}";
            builder.Add(new CrossReference(uid, entry.FullTypeName, RouteFor(entry)));
        }

        return builder.ToImmutable();
    }

    private ContentRoute RouteFor(ApiReferenceEntry entry)
        => ContentRouteFactory.FromUrl(new UrlPath($"{_registration.RoutePrefix}{entry.Slug}/"));
}