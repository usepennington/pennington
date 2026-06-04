namespace Pennington.DocSite.Services;

using System.Collections.Immutable;
using System.Web;
using Content;
using FrontMatter;
using Pipeline;
using Routing;

/// <summary>
/// Resolves blog posts by URL and provides query methods for listings and tag filtering.
/// Parses <see cref="BlogPostFrontMatter"/> directly so post <c>Date</c>/<c>Author</c> fields
/// survive — the DocSite <see cref="ContentResolver"/> is bound to <see cref="DocSiteFrontMatter"/>
/// and would drop them.
/// </summary>
public sealed class BlogPostResolver
{
    /// <summary>URL prefix for browse-by-tag pages.</summary>
    public const string TagsBaseUrl = "/blog/tags";

    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly TimeProvider _clock;

    /// <summary>Creates a new resolver with the supplied content services, parser, renderer, and wall clock.</summary>
    public BlogPostResolver(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer,
        TimeProvider? clock = null)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
        _clock = clock ?? TimeProvider.System;
    }

    /// <summary>Gets all blog posts ordered by date descending (posts without a date sort last).</summary>
    public async Task<IReadOnlyList<BlogPostSummary>> GetAllPostsAsync()
    {
        var posts = new List<BlogPostSummary>();
        await foreach (var item in _services.DiscoverAllAsync())
        {
            if (item.Source.Value is not FileSource { Format: "markdown" } source)
            {
                continue;
            }

            if (!IsBlogPostRoute(item.Route))
            {
                continue;
            }

            try
            {
                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<BlogPostFrontMatter>(content, source.Path.Value);
                var fm = parsed.Metadata ?? new BlogPostFrontMatter();
                if (fm.IsHiddenFromBuild(_clock))
                {
                    continue;
                }

                posts.Add(new BlogPostSummary(fm, item.Route.CanonicalPath.Value));
            }
            catch
            {
                // Skip unparseable files.
            }
        }

        return posts
            .OrderByDescending(p => p.FrontMatter.Date ?? DateTime.MinValue)
            .ToList();
    }

    /// <summary>Gets a rendered blog post by URL, or null when no post matches.</summary>
    public async Task<RenderedBlogPost?> GetPostByUrlAsync(string url)
    {
        url = "/" + url.Trim('/');
        var target = new UrlPath(url);

        await foreach (var item in _services.DiscoverAllAsync())
        {
            if (item.Source.Value is not FileSource { Format: "markdown" } source)
            {
                continue;
            }

            if (!IsBlogPostRoute(item.Route))
            {
                continue;
            }

            if (!item.Route.CanonicalPath.Matches(target))
            {
                continue;
            }

            var content = await File.ReadAllTextAsync(source.Path.Value);
            var parsed = _parser.Parse<BlogPostFrontMatter>(content, source.Path.Value);
            var fm = parsed.Metadata ?? new BlogPostFrontMatter();

            var rendered = await _renderer.RenderAsync(new ParsedItem(item.Route, fm, parsed.Body));
            if (rendered.Value is RenderedItem renderedItem)
            {
                var summary = new BlogPostSummary(fm, item.Route.CanonicalPath.Value);
                return new RenderedBlogPost(summary, renderedItem.Content.Html);
            }
        }

        return null;
    }

    /// <summary>Gets all unique tags with their post counts, ordered by count then name.</summary>
    public async Task<ImmutableList<(BlogTag Tag, int Count)>> GetTagsWithCountsAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts
            .SelectMany(p => p.FrontMatter.Tags)
            .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Tag: MakeTag(g.Key), Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Tag.Name, StringComparer.OrdinalIgnoreCase)
            .ToImmutableList();
    }

    /// <summary>Gets the tag and matching posts for an encoded tag name, or null when none match.</summary>
    public async Task<(BlogTag Tag, ImmutableList<BlogPostSummary> Posts)?> GetPostsByTagAsync(string encodedTagName)
    {
        var tagName = HttpUtility.UrlDecode(encodedTagName);
        var posts = await GetAllPostsAsync();

        var matching = posts
            .Where(p => p.FrontMatter.Tags.Any(t =>
                string.Equals(t, tagName, StringComparison.OrdinalIgnoreCase)))
            .ToImmutableList();

        if (matching.IsEmpty)
        {
            return null;
        }

        return (MakeTag(tagName), matching);
    }

    private static BlogTag MakeTag(string name)
        => new(name, $"{TagsBaseUrl}/{HttpUtility.UrlEncode(name)}");

    private static bool IsBlogPostRoute(ContentRoute route)
        => route.CanonicalPath.Value.StartsWith("/blog/", StringComparison.OrdinalIgnoreCase);
}