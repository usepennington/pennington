namespace YogaStudioExample.Services;

using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;
using Models;

public sealed class ContentHelper
{
    private readonly IEnumerable<IContentService> _services;
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly LocalizationOptions _localization;
    private readonly IWebHostEnvironment _env;

    public ContentHelper(
        IEnumerable<IContentService> services,
        FrontMatterParser parser,
        IContentRenderer renderer,
        LocalizationOptions localization,
        IWebHostEnvironment env)
    {
        _services = services;
        _parser = parser;
        _renderer = renderer;
        _localization = localization;
        _env = env;
    }

    /// <summary>
    /// Load and render a static page markdown file (e.g. about.md, contact.md)
    /// directly from Content/pages/, honoring the locale embedded in the URL.
    /// Content/pages/ is intentionally NOT registered as a Pennington markdown
    /// source — see Program.cs — so this helper reads files off disk by
    /// convention: <c>/about/</c> → <c>Content/pages/about.md</c>, and
    /// <c>/gen-z/about/</c> → <c>Content/pages/gen-z/about.md</c>.
    /// </summary>
    public async Task<(YogaFrontMatter FrontMatter, string Html)?> GetStaticPageAsync(string url)
    {
        var trimmed = url.Trim('/');
        if (string.IsNullOrEmpty(trimmed)) return null;

        // Split off a leading locale segment if it matches a configured locale.
        var segments = trimmed.Split('/', 2);
        string slug;
        string? localeSegment = null;
        if (segments.Length == 2 && _localization.Locales.ContainsKey(segments[0]))
        {
            localeSegment = segments[0];
            slug = segments[1];
        }
        else
        {
            slug = trimmed;
        }

        // slug should now be a single path segment like "about" or "faq".
        if (slug.Contains('/')) return null;

        var contentRoot = _env.ContentRootPath;
        var candidates = new List<string>();
        if (localeSegment is not null)
            candidates.Add(Path.Combine(contentRoot, "Content", "pages", localeSegment, $"{slug}.md"));
        candidates.Add(Path.Combine(contentRoot, "Content", "pages", $"{slug}.md"));

        foreach (var candidate in candidates)
        {
            if (!File.Exists(candidate)) continue;

            var raw = await File.ReadAllTextAsync(candidate);
            var parsed = _parser.Parse<YogaFrontMatter>(raw);
            var fm = parsed.Metadata ?? new YogaFrontMatter();

            // Build a transient route for the file so the renderer can resolve
            // relative links. The URL segment doesn't need to be canonical —
            // MarkdownContentRenderer only uses the source file path.
            var route = new ContentRoute
            {
                CanonicalPath = new UrlPath("/" + trimmed + "/"),
                OutputFile = new FilePath($"{trimmed}/index.html"),
                SourceFile = new FilePath(candidate),
                Locale = localeSegment ?? _localization.DefaultLocale,
            };

            var parsedItem = new ParsedItem(route, fm, parsed.Body);
            var rendered = await _renderer.RenderAsync(parsedItem);
            if (rendered is RenderedItem renderedItem)
                return (fm, renderedItem.Content.Html);
        }

        return null;
    }

    public async Task<(YogaBlogFrontMatter FrontMatter, string Html)?> GetRenderedBlogPostAsync(string url)
    {
        url = "/" + url.Trim('/');

        foreach (var service in _services)
        {
            await foreach (var item in service.DiscoverAsync())
            {
                if (!item.Route.CanonicalPath.Matches(new UrlPath(url))) continue;
                if (item.Source is not MarkdownFileSource source) continue;

                var content = await File.ReadAllTextAsync(source.Path.Value);
                var parsed = _parser.Parse<YogaBlogFrontMatter>(content);
                var fm = parsed.Metadata ?? new YogaBlogFrontMatter();

                var parsedItem = new ParsedItem(item.Route, fm, parsed.Body);
                var rendered = await _renderer.RenderAsync(parsedItem);
                if (rendered is RenderedItem renderedItem)
                    return (fm, renderedItem.Content.Html);
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