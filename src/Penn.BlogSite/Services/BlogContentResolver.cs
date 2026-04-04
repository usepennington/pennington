namespace Penn.BlogSite.Services;

using System.Collections.Immutable;
using System.Web;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Resolves blog content pages by URL and provides query methods
/// for listing posts, filtering by tag, etc.
/// </summary>
public sealed class BlogContentResolver
{
    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly BlogSiteOptions _options;

    private List<BlogPostPage>? _cachedPosts;

    public BlogContentResolver(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer,
        BlogSiteOptions options)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
        _options = options;
    }

    /// <summary>
    /// Get all blog posts ordered by date descending.
    /// </summary>
    public async Task<List<BlogPostPage>> GetAllPostsAsync()
    {
        if (_cachedPosts != null) return _cachedPosts;

        var posts = new List<BlogPostPage>();
        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (item.Source is not MarkdownFileSource source) continue;

                try
                {
                    var content = await File.ReadAllTextAsync(source.Path.Value);
                    var parsed = _parser.Parse<BlogSiteFrontMatter>(content);
                    var fm = parsed.Metadata ?? new BlogSiteFrontMatter();

                    if (fm.IsDraft) continue;

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
        }

        _cachedPosts = posts.OrderByDescending(p => p.FrontMatter.Date).ToList();
        return _cachedPosts;
    }

    /// <summary>
    /// Get a rendered blog post by URL.
    /// </summary>
    public async Task<RenderedBlogPost?> GetPostByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');

        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (!item.Route.CanonicalPath.Matches(new UrlPath(url))) continue;
                if (item.Source is not MarkdownFileSource source) continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<BlogSiteFrontMatter>(content);
                var fm = parsed.Metadata ?? new BlogSiteFrontMatter();

                var tags = fm.Tags
                    .Select(t => new BlogTag(t, $"{_options.TagsPageUrl}/{HttpUtility.UrlEncode(t)}"))
                    .ToArray();

                var parsedItem = new ParsedItem(item.Route, fm, parsed.Body);
                var rendered = await _renderer.RenderAsync(parsedItem);
                if (rendered is RenderedItem renderedItem)
                {
                    var post = new BlogPostPage(fm, item.Route.CanonicalPath.Value, tags);
                    return new RenderedBlogPost(post, renderedItem.Content.Html);
                }
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

        if (matching.IsEmpty) return null;

        var tag = matching.First().Tags.First(t =>
            string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));

        return (tag, matching);
    }
}
