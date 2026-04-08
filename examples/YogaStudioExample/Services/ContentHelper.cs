namespace YogaStudioExample.Services;

using Penn.Content;
using Penn.FrontMatter;
using Penn.Pipeline;
using Penn.Routing;
using YogaStudioExample.Models;

public sealed class ContentHelper
{
    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;

    public ContentHelper(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
    }

    public async Task<(T FrontMatter, string Html)?> GetRenderedPageAsync<T>(string url)
        where T : IFrontMatter, new()
    {
        url = "/" + url.Trim('/');

        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (!item.Route.CanonicalPath.Matches(new UrlPath(url))) continue;
                if (item.Source is not MarkdownFileSource source) continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<T>(content);
                var fm = parsed.Metadata ?? new T();

                var parsedItem = new ParsedItem(item.Route, fm, parsed.Body);
                var rendered = await _renderer.RenderAsync(parsedItem);
                if (rendered is RenderedItem renderedItem)
                {
                    return (fm, renderedItem.Content.Html);
                }
            }
        }

        return null;
    }

    public async Task<List<(ContentRoute Route, YogaBlogFrontMatter FrontMatter)>> GetAllBlogPostsAsync()
    {
        var posts = new List<(ContentRoute, YogaBlogFrontMatter)>();
        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (item.Source is not MarkdownFileSource source) continue;
                if (!item.Route.CanonicalPath.Value.StartsWith("/blog/", StringComparison.OrdinalIgnoreCase))
                    continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<YogaBlogFrontMatter>(content);
                var fm = parsed.Metadata;
                if (fm is null || fm.IsDraft) continue;

                posts.Add((item.Route, fm));
            }
        }

        return posts
            .OrderByDescending(p => p.Item2.Date)
            .ToList();
    }

    public async Task<List<(ContentRoute Route, YogaBlogFrontMatter FrontMatter)>> GetBlogPostsByTagAsync(string tag)
    {
        var all = await GetAllBlogPostsAsync();
        return all.Where(p => p.FrontMatter.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<string>> GetAllBlogTagsAsync()
    {
        var all = await GetAllBlogPostsAsync();
        return all
            .SelectMany(p => p.FrontMatter.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }
}
