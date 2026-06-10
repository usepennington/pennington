namespace Pennington.SocialCards;

using System.Collections.Immutable;
using Content;
using Microsoft.Extensions.DependencyInjection;
using Pipeline;
using Routing;

/// <summary>
/// Discovers one social-card image route per content page so the static build bakes a card for every
/// page: the crawler HTTP-fetches each discovered route and the sibling
/// <see cref="SocialCardEndpointExtensions.MapSocialCards"/> endpoint renders it on demand. Mirrors
/// <see cref="Taxonomy.TaxonomyContentService{TFrontMatter, TKey}"/> — it emits
/// <see cref="EndpointSource"/> routes served by an endpoint and projects no records of its own.
/// </summary>
public sealed class SocialCardContentService : IContentService, IMetaContentService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SocialCardOptions _options;

    /// <summary>Creates the service. Sibling content services are resolved on demand to avoid a DI cycle.</summary>
    public SocialCardContentService(IServiceProvider serviceProvider, SocialCardOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <inheritdoc/>
    public string DefaultSectionLabel => "";

    /// <inheritdoc/>
    public int SearchPriority => 0;

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        // Resolve siblings on demand (rather than via a ctor IEnumerable<IContentService>) to avoid a
        // DI cycle: this service is itself in that set. Exclude every meta-service (this instance
        // included) so the sibling record walk can't recurse back into derived-route discovery.
        var siblings = _serviceProvider.GetServices<IContentService>()
            .SourceServices()
            .ToList();

        await foreach (var record in siblings.GetAllRecordsAsync())
        {
            var cardPath = SocialCardUrl.RelativePath(record.Route.CanonicalPath, _options.BaseUrl);
            yield return new DiscoveredItem(
                new ContentRoute
                {
                    // A file route, not a page: keep the .png path verbatim (no trailing-slash /
                    // index.html shaping that ContentRouteFactory applies to HTML pages).
                    CanonicalPath = new UrlPath(cardPath),
                    OutputFile = new FilePath(cardPath.TrimStart('/')),
                    Locale = record.Route.Locale,
                },
                new EndpointSource());
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Cards are outputs, not content. Returning empty keeps the sibling walk in
    /// <see cref="DiscoverAsync"/> free of recursion (a card record would re-enter discovery via
    /// <see cref="Content.ContentRecordRegistry"/>) and keeps card routes out of taxonomy/search.
    /// </remarks>
    public IAsyncEnumerable<ContentRecord> GetRecordsAsync()
        => System.Linq.AsyncEnumerable.Empty<ContentRecord>();

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);
}
