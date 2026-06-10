namespace Pennington.BlogSite.Services;

using Content;
using FrontMatter;
using Pipeline;
using Routing;

/// <summary>
/// Resolves the site's not-found body from a content-root <c>404.md</c>, rendered through the markdown
/// pipeline. Post listings, single-post rendering, browse-by-tag, and RSS are served by the shared
/// <see cref="BlogPostQuery"/> and the registered taxonomy axis.
/// </summary>
public sealed class BlogContentResolver
{
    private readonly FrontMatterParser _parser;
    private readonly IContentRenderer _renderer;
    private readonly BlogSiteOptions _options;

    /// <summary>Creates a new resolver with the supplied front-matter parser, renderer, and options.</summary>
    public BlogContentResolver(FrontMatterParser parser, IContentRenderer renderer, BlogSiteOptions options)
    {
        _parser = parser;
        _renderer = renderer;
        _options = options;
    }

    /// <summary>
    /// Resolves the site's not-found body from a content-root <c>404.md</c>, rendered through the
    /// markdown pipeline. Returns null when no <c>404.md</c> exists — the catch-all then tries a
    /// <c>NotFound</c> component, then the built-in message. The file is reserved out of discovery
    /// (<see cref="MarkdownContentServiceOptions.ReserveNotFoundPage"/>), so it is never a post route.
    /// </summary>
    public async Task<RenderedNotFound?> GetNotFoundContentAsync()
    {
        var root = Path.GetFullPath(_options.ContentRootPath.Value);
        var path = Path.Combine(root, MarkdownContentService<BlogSiteFrontMatter>.NotFoundPageFileName);
        if (!File.Exists(path))
        {
            return null;
        }

        var content = await File.ReadAllTextAsync(path);
        var parsed = _parser.Parse<BlogSiteFrontMatter>(content, path);
        var fm = parsed.Metadata ?? new BlogSiteFrontMatter();

        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/404"),
            OutputFile = new FilePath("404.html"),
        };
        // Single blog markdown source => index 0 => MarkdownFormat.Key.
        var parsedItem = new ParsedItem(route, fm, parsed.Body) { Format = MarkdownFormat.Key };
        var rendered = await _renderer.RenderAsync(parsedItem);
        if (rendered.Value is RenderedItem renderedItem)
        {
            var title = string.IsNullOrWhiteSpace(fm.Title) ? "Not Found" : fm.Title;
            return new RenderedNotFound(title, renderedItem.Content.Html);
        }

        return null;
    }
}
