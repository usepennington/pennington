namespace Pennington.Taxonomy;

using System.Collections.Immutable;
using Content;
using FrontMatter;

/// <summary>
/// Resolves the registered <see cref="TaxonomyContentService{TFrontMatter, TKey}"/> for a given
/// <see cref="TaxonomyOptions{TFrontMatter, TKey}.BaseUrl"/> so a routed Razor <c>@page</c> can read a
/// taxonomy's terms directly — the alternative to mounting the bare-render <c>MapTaxonomy</c> endpoints
/// when the page wants the host's full layout, chrome, and search indexing.
/// </summary>
public sealed class TaxonomyAccessor
{
    private readonly IReadOnlyList<IContentService> _services;

    /// <summary>Creates the accessor over the registered content services.</summary>
    public TaxonomyAccessor(IEnumerable<IContentService> services) => _services = services.ToList();

    /// <summary>Returns the term list for the axis mounted at <paramref name="baseUrl"/>, or empty when none matches.</summary>
    /// <typeparam name="TFrontMatter">The axis front-matter type.</typeparam>
    /// <typeparam name="TKey">The axis key type.</typeparam>
    /// <param name="baseUrl">The axis <see cref="TaxonomyOptions{TFrontMatter, TKey}.BaseUrl"/>.</param>
    public Task<ImmutableList<TaxonomyTerm<TFrontMatter, TKey>>> GetTermsAsync<TFrontMatter, TKey>(string baseUrl)
        where TFrontMatter : IFrontMatter, new()
        where TKey : notnull
        => Find<TFrontMatter, TKey>(baseUrl)?.GetTermsAsync()
            ?? Task.FromResult(ImmutableList<TaxonomyTerm<TFrontMatter, TKey>>.Empty);

    /// <summary>Looks up a single term by slug on the axis mounted at <paramref name="baseUrl"/>; null when absent.</summary>
    /// <typeparam name="TFrontMatter">The axis front-matter type.</typeparam>
    /// <typeparam name="TKey">The axis key type.</typeparam>
    /// <param name="baseUrl">The axis <see cref="TaxonomyOptions{TFrontMatter, TKey}.BaseUrl"/>.</param>
    /// <param name="slug">The term slug to resolve.</param>
    public Task<TaxonomyTerm<TFrontMatter, TKey>?> TryGetTermAsync<TFrontMatter, TKey>(string baseUrl, string slug)
        where TFrontMatter : IFrontMatter, new()
        where TKey : notnull
        => Find<TFrontMatter, TKey>(baseUrl) is { } taxonomy
            ? taxonomy.TryGetTermAsync(slug)
            : Task.FromResult<TaxonomyTerm<TFrontMatter, TKey>?>(null);

    private TaxonomyContentService<TFrontMatter, TKey>? Find<TFrontMatter, TKey>(string baseUrl)
        where TFrontMatter : IFrontMatter, new()
        where TKey : notnull
        => _services.OfType<TaxonomyContentService<TFrontMatter, TKey>>()
            .FirstOrDefault(t => string.Equals(t.BaseUrl.Trim('/'), baseUrl.Trim('/'), StringComparison.OrdinalIgnoreCase));
}
