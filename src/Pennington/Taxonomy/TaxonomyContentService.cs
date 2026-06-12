namespace Pennington.Taxonomy;

using System.Collections.Immutable;
using Content;
using FrontMatter;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Pipeline;
using Routing;

/// <summary>
/// Walks every other registered <see cref="IContentService"/>'s <see cref="ContentRecord"/>s,
/// selects those whose <see cref="ContentRecord.Metadata"/> is a <typeparamref name="TFrontMatter"/>,
/// projects keys via <see cref="TaxonomyOptions{TFrontMatter, TKey}.SelectKey"/> or
/// <see cref="TaxonomyOptions{TFrontMatter, TKey}.SelectKeys"/>, and exposes the result as the
/// taxonomy's index plus one route per term. Because it reads records rather than re-parsing
/// markdown files, any content service — markdown or custom — participates as long as it projects
/// records of the taxonomy's front-matter type.
///
/// <para>
/// The service emits its routes with <see cref="EndpointSource"/> — the canonical HTML is
/// produced by the sibling <c>MapTaxonomy</c> endpoints, mirroring the pattern documented in
/// <c>how-to/content-services/custom-content-service.md</c>. As a consequence the routes do
/// not appear in <c>sitemap.xml</c>; they do appear in navigation, search, and cross-references
/// through <see cref="GetContentTocEntriesAsync"/> and <see cref="GetCrossReferencesAsync"/>.
/// </para>
///
/// <para>
/// The service caches its computed term list in an <see cref="AsyncLazy{T}"/> and subscribes to
/// <see cref="IFileWatcher"/> so any change anywhere in the watched content tree drops the cache
/// and the next request rebuilds it.
/// </para>
/// </summary>
public sealed class TaxonomyContentService<TFrontMatter, TKey> : IContentService, ITaxonomyContentService, IMetaContentService
    where TFrontMatter : IFrontMatter, new()
    where TKey : notnull
{
    private readonly TaxonomyOptions<TFrontMatter, TKey> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _clock;
    private readonly Lock _lock = new();
    private AsyncLazy<ImmutableList<TaxonomyTerm<TFrontMatter, TKey>>> _termsLazy;

    /// <summary>Creates the service and subscribes to file-change notifications for hot reload.</summary>
    public TaxonomyContentService(
        TaxonomyOptions<TFrontMatter, TKey> options,
        IServiceProvider serviceProvider,
        IFileWatcher fileWatcher,
        TimeProvider? clock = null)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _clock = clock ?? TimeProvider.System;
        _termsLazy = new AsyncLazy<ImmutableList<TaxonomyTerm<TFrontMatter, TKey>>>(LoadTermsAsync);

        fileWatcher.SubscribeToChanges(InvalidateCache);
    }

    /// <inheritdoc/>
    public string DefaultSectionLabel => _options.ResolvedSectionLabel;

    /// <inheritdoc/>
    public int SearchPriority => _options.SearchPriority;

    /// <summary>The configured base URL (e.g. <c>/cuisine</c>) — exposed so endpoint mapping can find it.</summary>
    public string BaseUrl => _options.BaseUrl;

    /// <summary>The fully-resolved index URL with leading and trailing slashes.</summary>
    public string IndexUrl => _options.IndexUrl;

    /// <summary>Builds the per-term URL for the given <paramref name="slug"/>.</summary>
    public string TermUrl(string slug) => _options.TermUrl(slug);

    /// <summary>The Razor component to render the index page.</summary>
    public Type IndexPage => _options.IndexPage!;

    /// <summary>The Razor component to render each per-term page.</summary>
    public Type TermPage => _options.TermPage!;

    /// <summary>
    /// Returns the cached term list, computing it on first access. Used both by the discovery
    /// pipeline and by the live HTTP endpoints.
    /// </summary>
    public Task<ImmutableList<TaxonomyTerm<TFrontMatter, TKey>>> GetTermsAsync() => _termsLazy.Task;

    /// <summary>Looks up a single term by its slug. Returns <c>null</c> when not found.</summary>
    public async Task<TaxonomyTerm<TFrontMatter, TKey>?> TryGetTermAsync(string slug)
    {
        var terms = await _termsLazy;
        return terms.FirstOrDefault(t => t.Slug == slug);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        yield return new DiscoveredItem(
            ContentRouteFactory.FromUrl(new UrlPath(_options.IndexUrl)),
            new EndpointSource());

        var terms = await _termsLazy;
        foreach (var term in terms)
        {
            yield return new DiscoveredItem(
                ContentRouteFactory.FromUrl(term.Url),
                new EndpointSource());
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// A taxonomy projects no records: its outputs are term index/listing pages, not content to be
    /// re-taxonomized, faceted, or structure-marked. Returning empty also keeps the sibling walk in
    /// <see cref="LoadTermsAsync"/> free of the recursion that reading a taxonomy's own records would cause.
    /// </remarks>
    public IAsyncEnumerable<ContentRecord> GetRecordsAsync()
        => System.Linq.AsyncEnumerable.Empty<ContentRecord>();

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc/>
    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var terms = await _termsLazy;
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();
        var section = _options.ResolvedSectionLabel;

        builder.Add(new ContentTocItem(
            Title: section,
            Route: ContentRouteFactory.FromUrl(new UrlPath(_options.IndexUrl)),
            Order: 100,
            HierarchyParts: [_options.BaseUrl.Trim('/')],
            SectionLabel: section,
            Locale: null));

        var order = 110;
        foreach (var term in terms)
        {
            builder.Add(new ContentTocItem(
                Title: term.Label,
                Route: ContentRouteFactory.FromUrl(term.Url),
                Order: order,
                HierarchyParts: [_options.BaseUrl.Trim('/'), term.Slug],
                SectionLabel: section,
                Locale: null));
            order += 10;
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        if (!_options.EmitCrossReferences)
        {
            return ImmutableList<CrossReference>.Empty;
        }

        var terms = await _termsLazy;
        var prefix = _options.BaseUrl.Trim('/');
        var builder = ImmutableList.CreateBuilder<CrossReference>();
        foreach (var term in terms)
        {
            builder.Add(new CrossReference(
                $"{prefix}-{term.Slug}",
                term.Label,
                ContentRouteFactory.FromUrl(term.Url)));
        }
        return builder.ToImmutable();
    }

    private void InvalidateCache()
    {
        lock (_lock)
        {
            _termsLazy = new AsyncLazy<ImmutableList<TaxonomyTerm<TFrontMatter, TKey>>>(LoadTermsAsync);
        }
    }

    private async Task<ImmutableList<TaxonomyTerm<TFrontMatter, TKey>>> LoadTermsAsync()
    {
        // Resolve siblings on demand (a ctor IEnumerable<IContentService> would be a DI cycle — this
        // service is itself in that set). Exclude every meta-service (this instance, other taxonomies,
        // listings, social cards): they derive from source content and project nothing to taxonomize,
        // so walking them is pointless and would recurse back into discovery.
        var siblings = _serviceProvider.GetServices<IContentService>()
            .SourceServices()
            .ToList();

        // (key, item) pairs keyed by the user's TKey using its default equality semantics.
        var groups = new Dictionary<TKey, List<TaxonomyItem<TFrontMatter>>>();

        await foreach (var record in siblings.GetAllRecordsAsync())
        {
            // A record participates only when its metadata is the taxonomy's front-matter type.
            // This is how taxonomy stays opt-in by type across every content service, markdown
            // or custom: records of other types (or none) contribute nothing.
            if (record.Metadata is not TFrontMatter metadata || metadata.IsHiddenFromBuild(_clock))
            {
                continue;
            }

            var item = new TaxonomyItem<TFrontMatter>(metadata, record.Route.CanonicalPath);

            foreach (var key in ProjectKeys(metadata))
            {
                if (!groups.TryGetValue(key, out var bucket))
                {
                    bucket = [];
                    groups[key] = bucket;
                }
                bucket.Add(item);
            }
        }

        var terms = ImmutableList.CreateBuilder<TaxonomyTerm<TFrontMatter, TKey>>();
        foreach (var (key, items) in groups.OrderBy(g => _options.LabelFor(g.Key), StringComparer.OrdinalIgnoreCase))
        {
            var slug = _options.SlugFor(key);
            terms.Add(new TaxonomyTerm<TFrontMatter, TKey>(
                Key: key,
                Label: _options.LabelFor(key),
                Slug: slug,
                Url: new UrlPath(_options.TermUrl(slug)),
                Items: items.ToImmutableList()));
        }
        return terms.ToImmutable();
    }

    private IEnumerable<TKey> ProjectKeys(TFrontMatter metadata)
    {
        if (_options.SelectKey is { } single)
        {
            var key = single(metadata);
            if (!IsEmptyKey(key))
            {
                yield return key!;
            }

            yield break;
        }

        if (_options.SelectKeys is { } multi)
        {
            foreach (var key in multi(metadata))
            {
                if (IsEmptyKey(key))
                {
                    continue;
                }

                yield return key;
            }
        }
    }

    private static bool IsEmptyKey(TKey? key)
    {
        if (key is null)
        {
            return true;
        }

        if (key is string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        return EqualityComparer<TKey>.Default.Equals(key, default!);
    }
}