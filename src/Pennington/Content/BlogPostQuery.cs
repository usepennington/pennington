namespace Pennington.Content;

using System.Collections.Immutable;
using Feeds;
using FrontMatter;
using Routing;

/// <summary>
/// Shared read model for blog-style content: lists posts, paginates them, renders a single post, and
/// builds the RSS feed. Everything reads the cached <see cref="ContentRecordRegistry"/> snapshot and
/// <see cref="IPageResolver"/>, so nothing re-reads or re-parses markdown per request. Generic over the
/// host's front-matter type; both the DocSite and BlogSite templates consume this one service.
/// </summary>
public sealed class BlogPostQuery
{
    private readonly ContentRecordRegistry _records;
    private readonly IPageResolver _resolver;
    private readonly TimeProvider _clock;

    /// <summary>Creates the query over the content-record registry, page resolver, and wall clock.</summary>
    public BlogPostQuery(ContentRecordRegistry records, IPageResolver resolver, TimeProvider? clock = null)
    {
        _records = records;
        _resolver = resolver;
        _clock = clock ?? TimeProvider.System;
    }

    /// <summary>
    /// Returns every published <typeparamref name="TFrontMatter"/> post whose route sits under
    /// <paramref name="basePrefix"/> (e.g. <c>/blog</c>), newest first. Drafts and future-dated posts are
    /// excluded. Reads the cached record snapshot — no file I/O.
    /// </summary>
    /// <typeparam name="TFrontMatter">The post front-matter type to collect.</typeparam>
    /// <param name="basePrefix">URL prefix the posts live under.</param>
    public async Task<ImmutableList<BlogPostRef<TFrontMatter>>> GetPostsAsync<TFrontMatter>(string basePrefix)
        where TFrontMatter : IFrontMatter
    {
        var prefix = "/" + basePrefix.Trim('/') + "/";
        var snapshot = await _records.GetSnapshotAsync();
        return snapshot.Values
            .Where(r => r.Metadata is TFrontMatter
                && r.Route.CanonicalPath.Value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && !r.Metadata.IsHiddenFromBuild(_clock))
            .Select(r => new BlogPostRef<TFrontMatter>((TFrontMatter)r.Metadata, r.Route.CanonicalPath))
            .OrderByDescending(p => p.FrontMatter.Date ?? DateTime.MinValue)
            .ToImmutableList();
    }

    /// <summary>
    /// Returns one page of posts under <paramref name="basePrefix"/>, newest first. Returns null when
    /// <paramref name="page"/>/<paramref name="pageSize"/> are non-positive or the page is past the end;
    /// page 1 of an empty set yields an empty page rather than null.
    /// </summary>
    /// <typeparam name="TFrontMatter">The post front-matter type to collect.</typeparam>
    /// <param name="basePrefix">URL prefix the posts live under.</param>
    /// <param name="page">1-based page index.</param>
    /// <param name="pageSize">Items per page.</param>
    public async Task<PagedList<BlogPostRef<TFrontMatter>>?> GetPageAsync<TFrontMatter>(
        string basePrefix, int page, int pageSize)
        where TFrontMatter : IFrontMatter
    {
        if (page < 1 || pageSize < 1)
        {
            return null;
        }

        var posts = await GetPostsAsync<TFrontMatter>(basePrefix);
        if (posts.Count == 0)
        {
            return page == 1 ? new PagedList<BlogPostRef<TFrontMatter>>([], 1, pageSize, 0) : null;
        }

        var totalPages = (int)Math.Ceiling(posts.Count / (double)pageSize);
        if (page > totalPages)
        {
            return null;
        }

        var slice = posts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<BlogPostRef<TFrontMatter>>(slice, page, pageSize, posts.Count);
    }

    /// <summary>
    /// Renders the single post at <paramref name="url"/> through the cached page resolver. Returns null
    /// when nothing matches or the matched page is not a <typeparamref name="TFrontMatter"/>.
    /// </summary>
    /// <typeparam name="TFrontMatter">The expected post front-matter type.</typeparam>
    /// <param name="url">Canonical URL of the post to render.</param>
    public async Task<RenderedBlogPost<TFrontMatter>?> GetRenderedPostAsync<TFrontMatter>(UrlPath url)
        where TFrontMatter : IFrontMatter
    {
        if (await _resolver.ResolveAsync(url) is not { } rendered || rendered.Metadata is not TFrontMatter fm)
        {
            return null;
        }

        return new RenderedBlogPost<TFrontMatter>(fm, rendered.Route.CanonicalPath, rendered.Content.Html);
    }

    /// <summary>
    /// Builds RSS 2.0 XML for the posts under <paramref name="basePrefix"/>, newest first.
    /// <paramref name="author"/> projects each post's author name (the field is template-specific, not
    /// part of <see cref="IFrontMatter"/>); pass null to omit authors.
    /// </summary>
    /// <typeparam name="TFrontMatter">The post front-matter type to feed the feed.</typeparam>
    /// <param name="siteTitle">Feed/site title.</param>
    /// <param name="siteDescription">Feed/site description.</param>
    /// <param name="canonicalBaseUrl">Absolute base URL used to compose item links; may be null.</param>
    /// <param name="basePrefix">URL prefix the posts live under.</param>
    /// <param name="author">Projects a post's author name, or null to omit.</param>
    public async Task<string> GetRssXmlAsync<TFrontMatter>(
        string siteTitle,
        string siteDescription,
        string? canonicalBaseUrl,
        string basePrefix,
        Func<TFrontMatter, string?>? author = null)
        where TFrontMatter : IFrontMatter
    {
        var posts = await GetPostsAsync<TFrontMatter>(basePrefix);
        var items = posts.Select(p => new RssFeedItem(
            p.FrontMatter.Title,
            p.FrontMatter.Description,
            p.Url,
            p.FrontMatter.Date,
            author?.Invoke(p.FrontMatter)));
        return RssFeedWriter.WriteXml(siteTitle, siteDescription, canonicalBaseUrl, items);
    }
}
