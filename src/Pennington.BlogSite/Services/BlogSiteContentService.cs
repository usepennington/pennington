namespace Pennington.BlogSite.Services;

using System.Collections.Immutable;
using Content;
using FrontMatter;
using Microsoft.Extensions.DependencyInjection;
using Pipeline;
using Routing;

/// <summary>
/// Emits the paginated archive routes (<c>/archive/page/{n}</c>) the parameterized <c>@page</c> template
/// hides from automatic discovery, and projects the site-identity record for the template-owned home
/// page (so <c>/</c> participates in social-card generation). Post counts come from the shared
/// <see cref="BlogPostQuery"/> (cached records, no disk re-read); browse-by-tag is a registered taxonomy;
/// the canonical <c>/archive</c> route comes from <see cref="RazorPageContentService"/>.
/// </summary>
public sealed class BlogSiteContentService : IContentService
{
    private readonly BlogSiteOptions _options;
    private readonly IServiceProvider _services;

    /// <summary>
    /// Creates the service. The provider resolves <see cref="BlogPostQuery"/> on demand to avoid a
    /// construction cycle through the content-record registry.
    /// </summary>
    public BlogSiteContentService(BlogSiteOptions options, IServiceProvider services)
    {
        _options = options;
        _services = services;
    }

    /// <inheritdoc />
    public string DefaultSectionLabel => "";

    /// <inheritdoc />
    public int SearchPriority => 0;

    /// <inheritdoc />
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var pageSize = _options.PostsPerPage > 0 ? _options.PostsPerPage : int.MaxValue;
        var posts = await _services.GetRequiredService<BlogPostQuery>()
            .GetPostsAsync<BlogSiteFrontMatter>(_options.BlogBaseUrl);

        if (posts.Count <= pageSize)
        {
            yield break;
        }

        var totalPages = (int)Math.Ceiling(posts.Count / (double)pageSize);
        var archiveType = typeof(Components.Pages.Archive);
        ContentSource source = new RazorPageSource(
            archiveType.AssemblyQualifiedName ?? archiveType.FullName ?? archiveType.Name);

        for (var page = 2; page <= totalPages; page++)
        {
            yield return new DiscoveredItem(
                new ContentRoute
                {
                    CanonicalPath = new UrlPath($"/archive/page/{page}/"),
                    OutputFile = new FilePath($"archive/page/{page}/index.html"),
                },
                source);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// The home page is template-owned (<c>Home.razor</c>) — no markdown projects a record for it, so
    /// without this <c>/</c> sits out of record-joined features, most visibly social-card generation and
    /// its <c>og:image</c> tagging. <see cref="DiscoverAsync"/>'s items carry no metadata, so the default
    /// record bridge would yield nothing anyway.
    /// </remarks>
    public async IAsyncEnumerable<ContentRecord> GetRecordsAsync()
    {
        yield return new ContentRecord(
            ContentRouteFactory.FromUrl(new UrlPath("/")),
            new HomePageFrontMatter(_options.SiteTitle, _options.SiteDescription));
        await Task.CompletedTask;
    }

    /// <summary>Site-identity metadata for the template-owned home page record.</summary>
    private sealed record HomePageFrontMatter(string Title, string? Description) : IFrontMatter;

    /// <inheritdoc />
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc />
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <inheritdoc />
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc />
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);
}
