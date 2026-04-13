namespace Pennington.BlogSite.Services;

using System.Collections.Immutable;
using System.Web;
using System.Xml.Linq;
using Content;
using FrontMatter;
using Infrastructure;
using Pipeline;
using Routing;

/// <summary>
/// Yields content the stock <see cref="Pennington.Content.RazorPageContentService"/> cannot:
/// per-tag index routes (Tag.razor's <c>@page "/tags/{TagEncodedName}"</c>
/// template is parameterized and skipped at discovery) plus the
/// <c>/rss.xml</c> feed file. Without these the static build is missing
/// the tag listing pages and the RSS file the BlogSite header links to.
/// </summary>
public sealed class BlogSiteContentService : IContentService
{
    private readonly BlogSiteOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly AsyncLazy<ImmutableList<BlogPostDescriptor>> _posts;

    public BlogSiteContentService(
        BlogSiteOptions options,
        FrontMatterParser parser)
    {
        _options = options;
        _parser = parser;
        _posts = new AsyncLazy<ImmutableList<BlogPostDescriptor>>(LoadPostsAsync);
    }

    public string DefaultSection => "";
    public int SearchPriority => 0;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var posts = await _posts.Value;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var post in posts)
        {
            foreach (var tag in post.Tags)
            {
                if (!seen.Add(tag)) continue;

                var encoded = HttpUtility.UrlEncode(tag);
                var tagsPageSegment = _options.TagsPageUrl.Trim('/');
                var canonicalPath = new UrlPath($"/{tagsPageSegment}/{encoded}/");
                var outputFile = new FilePath($"{tagsPageSegment}/{encoded}/index.html");
                var route = new ContentRoute
                {
                    CanonicalPath = canonicalPath,
                    OutputFile = outputFile,
                };
                // Source value is informational — the static crawler issues
                // an HTTP GET which the Blazor router dispatches to Tag.razor
                // via its parameterized @page template.
                var componentType = typeof(Components.Pages.Tag);
                ContentSource source = new RazorPageSource(
                    componentType.AssemblyQualifiedName ?? componentType.FullName ?? componentType.Name);
                yield return new DiscoveredItem(route, source);
            }
        }
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    /// <summary>
    /// Build the RSS 2.0 XML document the <c>/rss.xml</c> endpoint returns.
    /// Exposed on the service so the minimal-API handler in
    /// <see cref="BlogSiteServiceExtensions.UseBlogSite"/> can share cached
    /// post metadata with <see cref="DiscoverAsync"/>.
    /// </summary>
    public async Task<string> GetRssXmlAsync()
    {
        var posts = await _posts.Value;
        var ordered = posts
            .Where(p => p.FrontMatter.Date.HasValue)
            .OrderByDescending(p => p.FrontMatter.Date!.Value)
            .ToList();

        var canonicalBase = _options.CanonicalBaseUrl?.TrimEnd('/') ?? string.Empty;

        XNamespace atom = "http://www.w3.org/2005/Atom";

        var channel = new XElement("channel",
            new XElement("title", _options.SiteTitle),
            new XElement("link", string.IsNullOrEmpty(canonicalBase) ? "/" : canonicalBase + "/"),
            new XElement("description", _options.Description));

        if (!string.IsNullOrEmpty(canonicalBase))
        {
            channel.Add(new XElement(atom + "link",
                new XAttribute("href", canonicalBase + "/rss.xml"),
                new XAttribute("rel", "self"),
                new XAttribute("type", "application/rss+xml")));
        }

        foreach (var post in ordered)
        {
            var url = string.IsNullOrEmpty(canonicalBase)
                ? post.Route.CanonicalPath.Value
                : canonicalBase + post.Route.CanonicalPath.Value;

            var entry = new XElement("item",
                new XElement("title", post.FrontMatter.Title),
                new XElement("link", url),
                new XElement("guid", new XAttribute("isPermaLink", "true"), url));

            if (!string.IsNullOrEmpty(post.FrontMatter.Description))
                entry.Add(new XElement("description", post.FrontMatter.Description));

            if (post.FrontMatter.Date.HasValue)
                entry.Add(new XElement("pubDate",
                    post.FrontMatter.Date.Value.ToUniversalTime().ToString("r")));

            if (!string.IsNullOrEmpty(post.FrontMatter.Author))
                entry.Add(new XElement("author", post.FrontMatter.Author));

            channel.Add(entry);
        }

        var rss = new XElement("rss",
            new XAttribute("version", "2.0"),
            new XAttribute(XNamespace.Xmlns + "atom", atom.NamespaceName),
            channel);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), rss);
        return doc.Declaration + Environment.NewLine + doc;
    }

    private async Task<ImmutableList<BlogPostDescriptor>> LoadPostsAsync()
    {
        var builder = ImmutableList.CreateBuilder<BlogPostDescriptor>();

        var contentRoot = Path.GetFullPath(
            Path.Combine(_options.ContentRootPath, _options.BlogContentPath));
        if (!Directory.Exists(contentRoot))
            return builder.ToImmutable();

        var contentRootPath = new FilePath(contentRoot);
        var baseUrl = new UrlPath(_options.BlogBaseUrl);

        var files = Directory.EnumerateFiles(contentRoot, "*.md", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string raw;
            try
            {
                raw = await File.ReadAllTextAsync(file);
            }
            catch
            {
                continue;
            }

            BlogSiteFrontMatter fm;
            try
            {
                var parsed = _parser.Parse<BlogSiteFrontMatter>(raw);
                fm = parsed.Metadata ?? new BlogSiteFrontMatter();
            }
            catch
            {
                continue;
            }

            if (fm.IsDraft) continue;

            var route = ContentRouteFactory.FromMarkdownFile(
                new FilePath(file), contentRootPath, baseUrl);

            builder.Add(new BlogPostDescriptor(route, fm, fm.Tags));
        }

        return builder.ToImmutable();
    }

    private sealed record BlogPostDescriptor(
        ContentRoute Route,
        BlogSiteFrontMatter FrontMatter,
        string[] Tags);
}