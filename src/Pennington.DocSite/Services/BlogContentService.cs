namespace Pennington.DocSite.Services;

using System.Collections.Immutable;
using System.Web;
using Content;
using Feeds;
using FrontMatter;
using Infrastructure;
using LlmsTxt;
using Pipeline;
using Routing;

/// <summary>
/// Surfaces the blog routes the static build cannot otherwise discover — the <c>/blog</c>
/// index, the <c>/blog/tags</c> index, and one parameterized <c>/blog/tags/{tag}</c> page per
/// tag — and builds the RSS 2.0 feed served at <c>/rss.xml</c>. Reads blog markdown directly
/// from disk so it takes no dependency on the still-initializing content-service set.
/// </summary>
/// <remarks>
/// Registered via <c>AddFileWatched&lt;BlogContentService&gt;()</c> and aliased as
/// <see cref="IContentService"/> through a transient indirection, so each resolution sees the
/// current factory-managed instance and the <see cref="AsyncLazy{T}"/> post cache is dropped
/// when content files change. Blog post pages themselves are discovered by the markdown
/// content source registered for <see cref="BlogPostFrontMatter"/>.
/// </remarks>
public sealed class BlogContentService : IContentService, ILlmsSubtreeProvider, IFileWatchAware
{
    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    private readonly DocSiteOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly TimeProvider _clock;
    private readonly AsyncLazy<ImmutableList<BlogPostDescriptor>> _posts;

    /// <summary>Creates a new service bound to the supplied options, front-matter parser, and wall clock.</summary>
    public BlogContentService(DocSiteOptions options, FrontMatterParser parser, TimeProvider? clock = null)
    {
        _options = options;
        _parser = parser;
        _clock = clock ?? TimeProvider.System;
        _posts = new AsyncLazy<ImmutableList<BlogPostDescriptor>>(LoadPostsAsync);
    }

    /// <inheritdoc />
    public string DefaultSectionLabel => "";

    /// <inheritdoc />
    public int SearchPriority => 0;

    /// <inheritdoc />
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var posts = await _posts;

        yield return RazorRoute("/blog", "blog/index.html", typeof(Components.Pages.Blog));
        yield return RazorRoute("/blog/tags", "blog/tags/index.html", typeof(Components.Pages.BlogTags));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var post in posts)
        {
            foreach (var tag in post.FrontMatter.Tags)
            {
                if (!seen.Add(tag))
                {
                    continue;
                }

                var encoded = HttpUtility.UrlEncode(tag);
                yield return RazorRoute(
                    $"/blog/tags/{encoded}/",
                    $"blog/tags/{encoded}/index.html",
                    typeof(Components.Pages.BlogTagPage));
            }
        }
    }

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

    /// <summary>
    /// Declares <c>/blog/</c> as an llms.txt subtree so posts split out of the front-door
    /// <c>llms.txt</c> into a dedicated <c>/blog/llms.txt</c>. Returns nothing when the blog
    /// has no published posts.
    /// </summary>
    public async Task<ImmutableList<LlmsSubtree>> GetLlmsSubtreesAsync()
    {
        var posts = await _posts;
        if (posts.Count == 0)
        {
            return ImmutableList<LlmsSubtree>.Empty;
        }

        return ImmutableList.Create(new LlmsSubtree(
            routePrefix: "/blog/",
            title: "Blog",
            description: "Posts and announcements from the site blog."));
    }

    /// <summary>Builds the RSS 2.0 XML document returned by the <c>/rss.xml</c> endpoint.</summary>
    public async Task<string> GetRssXmlAsync()
    {
        var posts = await _posts;
        var items = posts.Select(p => new RssFeedItem(
            p.FrontMatter.Title,
            p.FrontMatter.Description,
            p.Route.CanonicalPath,
            p.FrontMatter.Date,
            p.FrontMatter.Author));

        return RssFeedWriter.WriteXml(
            _options.SiteTitle, _options.SiteDescription, _options.CanonicalBaseUrl, items);
    }

    private static DiscoveredItem RazorRoute(string canonicalPath, string outputFile, Type component)
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath(canonicalPath),
            OutputFile = new FilePath(outputFile),
        };
        // The source value is informational — the static crawler issues an HTTP GET
        // that the Blazor router dispatches to the page via its @page template.
        ContentSource source = new RazorPageSource(
            component.AssemblyQualifiedName ?? component.FullName ?? component.Name);
        return new DiscoveredItem(route, source);
    }

    private async Task<ImmutableList<BlogPostDescriptor>> LoadPostsAsync()
    {
        var builder = ImmutableList.CreateBuilder<BlogPostDescriptor>();

        var contentRoot = Path.GetFullPath(
            Path.Combine(_options.ContentRootPath.Value, "blog"));
        if (!Directory.Exists(contentRoot))
        {
            return builder.ToImmutable();
        }

        var contentRootPath = new FilePath(contentRoot);
        var baseUrl = new UrlPath("/blog");

        foreach (var file in Directory.EnumerateFiles(contentRoot, "*.md", SearchOption.AllDirectories))
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

            BlogPostFrontMatter fm;
            try
            {
                var parsed = _parser.Parse<BlogPostFrontMatter>(raw, file);
                fm = parsed.Metadata ?? new BlogPostFrontMatter();
            }
            catch
            {
                continue;
            }

            if (fm.IsHiddenFromBuild(_clock))
            {
                continue;
            }

            var route = ContentRouteFactory.FromMarkdownFile(
                new FilePath(file), contentRootPath, baseUrl);

            builder.Add(new BlogPostDescriptor(route, fm));
        }

        return builder.ToImmutable();
    }

    private sealed record BlogPostDescriptor(ContentRoute Route, BlogPostFrontMatter FrontMatter);
}