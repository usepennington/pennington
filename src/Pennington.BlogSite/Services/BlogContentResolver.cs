namespace Pennington.BlogSite.Services;

using System.Collections.Immutable;
using System.Web;
using Content;
using FrontMatter;
using Infrastructure;
using Pipeline;
using Routing;

/// <summary>
/// Resolves blog content pages by URL and provides query methods
/// for listing posts, filtering by tag, etc.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — trusts IContentService for fresh data.
/// </summary>
public sealed class BlogContentResolver : IFileWatchAware
{
    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly BlogSiteOptions _options;
    private readonly TimeProvider _clock;
    private readonly AsyncLazy<List<BlogPostPage>> _postsLazy;

    /// <summary>Creates a new resolver with the supplied content services, parser, renderer, options, and wall clock.</summary>
    public BlogContentResolver(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer,
        BlogSiteOptions options,
        TimeProvider? clock = null)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
        _options = options;
        _clock = clock ?? TimeProvider.System;
        _postsLazy = new AsyncLazy<List<BlogPostPage>>(LoadAllPostsAsync);
    }

    /// <summary>
    /// Get all blog posts ordered by date descending.
    /// </summary>
    public Task<List<BlogPostPage>> GetAllPostsAsync() => _postsLazy.Value;

    private async Task<List<BlogPostPage>> LoadAllPostsAsync()
    {
        var posts = new List<BlogPostPage>();
        await foreach (var item in _services.DiscoverAllAsync())
        {
            if (item.Source is not MarkdownFileSource source)
            {
                continue;
            }

            try
            {
                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<BlogSiteFrontMatter>(content, source.Path.Value);
                var fm = parsed.Metadata ?? new BlogSiteFrontMatter();

                if (fm.IsHiddenFromBuild(_clock))
                {
                    continue;
                }

                var tags = fm.Tags
                    .Select(t => new BlogTag(t, $"{_options.TagsPageUrl}/{HttpUtility.UrlEncode(t)}"))
                    .ToArray();

                posts.Add(new BlogPostPage(fm, item.Route.CanonicalPath.Value, tags));
            }
            catch
            {
                // Skip unparseable files
            }
        }

        return posts.OrderByDescending(p => p.FrontMatter.Date).ToList();
    }

    /// <summary>
    /// Returns a single page of posts ordered by date descending. Returns null when
    /// <paramref name="page"/> is non-positive or beyond the last page.
    /// </summary>
    public async Task<PagedList<BlogPostPage>?> GetPagedPostsAsync(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return null;
        }

        var posts = await GetAllPostsAsync();
        if (posts.Count == 0)
        {
            return page == 1
                ? new PagedList<BlogPostPage>([], 1, pageSize, 0)
                : null;
        }

        var totalPages = (int)Math.Ceiling(posts.Count / (double)pageSize);
        if (page > totalPages)
        {
            return null;
        }

        var slice = posts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedList<BlogPostPage>(slice, page, pageSize, posts.Count);
    }

    /// <summary>
    /// Returns a single page of posts filtered by an encoded tag name. Returns null when the
    /// tag is unknown, <paramref name="page"/> is non-positive, or the page is beyond the last.
    /// </summary>
    public async Task<(BlogTag Tag, PagedList<BlogPostPage> Page)?> GetPagedPostsByTagAsync(
        string encodedTagName, int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return null;
        }

        var result = await GetPostsByTagAsync(encodedTagName);
        if (result is null)
        {
            return null;
        }

        var (tag, all) = result.Value;
        var totalPages = (int)Math.Ceiling(all.Count / (double)pageSize);
        if (page > totalPages)
        {
            return null;
        }

        var slice = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (tag, new PagedList<BlogPostPage>(slice, page, pageSize, all.Count));
    }

    /// <summary>
    /// Get a rendered blog post by URL.
    /// </summary>
    public async Task<RenderedBlogPost?> GetPostByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');
        var target = new UrlPath(url);

        await foreach (var item in _services.DiscoverAllAsync())
        {
            if (!item.Route.CanonicalPath.Matches(target))
            {
                continue;
            }

            if (item.Source is not MarkdownFileSource source)
            {
                continue;
            }

            var content = await File.ReadAllTextAsync(source.Path.Value);
            var parsed = _parser.Parse<BlogSiteFrontMatter>(content, source.Path.Value);
            var fm = parsed.Metadata ?? new BlogSiteFrontMatter();

            var tags = fm.Tags
                .Select(t => new BlogTag(t, $"{_options.TagsPageUrl}/{HttpUtility.UrlEncode(t)}"))
                .ToArray();

            var parsedItem = new ParsedItem(item.Route, fm, parsed.Body);
            var rendered = await _renderer.RenderAsync(parsedItem);
            if (rendered.Value is RenderedItem renderedItem)
            {
                var post = new BlogPostPage(fm, item.Route.CanonicalPath.Value, tags);
                return new RenderedBlogPost(post, renderedItem.Content.Html);
            }
        }

        return null;
    }

    /// <summary>
    /// Get all unique tags with their post counts.
    /// </summary>
    public async Task<ImmutableList<(BlogTag Tag, int Count)>> GetTagsWithCountsAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts
            .SelectMany(p => p.Tags)
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => (g.First(), g.Count()))
            .OrderByDescending(x => x.Item2)
            .ToImmutableList();
    }

    /// <summary>
    /// Get posts filtered by an encoded tag name.
    /// </summary>
    public async Task<(BlogTag Tag, ImmutableList<BlogPostPage> Posts)?> GetPostsByTagAsync(string encodedTagName)
    {
        var tagName = HttpUtility.UrlDecode(encodedTagName);
        var posts = await GetAllPostsAsync();

        var matching = posts
            .Where(p => p.Tags.Any(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase)))
            .ToImmutableList();

        if (matching.IsEmpty)
        {
            return null;
        }

        var tag = matching.First().Tags.First(t =>
            string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));

        return (tag, matching);
    }
}