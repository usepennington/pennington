namespace Pennington.BlogSite.Services;

using System.Collections.Immutable;
using System.Web;
using Content;
using Feeds;
using FrontMatter;
using Infrastructure;
using Pipeline;
using Routing;

/// <summary>
/// Yields content the stock <see cref="RazorPageContentService"/> cannot:
/// per-tag index routes (Tag.razor's <c>@page "/tags/{TagEncodedName}"</c>
/// template is parameterized and skipped at discovery) plus the
/// <c>/rss.xml</c> feed file. Without these the static build is missing
/// the tag listing pages and the RSS file the BlogSite header links to.
/// </summary>
/// <remarks>
/// File-reading service registered via <c>AddFileWatched&lt;BlogSiteContentService&gt;()</c>
/// and aliased as <see cref="IContentService"/> through a transient indirection so each
/// resolution sees the current factory-managed instance. The internal <see cref="AsyncLazy{T}"/>
/// post cache is dropped when the factory rebuilds on file-change events.
/// </remarks>
public sealed class BlogSiteContentService : IContentService, IFileWatchAware
{
    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    private readonly BlogSiteOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly TimeProvider _clock;
    private readonly AsyncLazy<ImmutableList<BlogPostDescriptor>> _posts;

    /// <summary>Creates a new service bound to the supplied options, front-matter parser, and wall clock.</summary>
    public BlogSiteContentService(
        BlogSiteOptions options,
        FrontMatterParser parser,
        TimeProvider? clock = null)
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

        var pageSize = _options.PostsPerPage > 0 ? _options.PostsPerPage : int.MaxValue;
        var tagComponentType = typeof(Components.Pages.Tag);
        var archiveComponentType = typeof(Components.Pages.Archive);

        // Tag canonical + paginated routes. Group posts by tag once so paginated
        // pages reflect the same descending-date ordering BlogContentResolver uses.
        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var post in posts)
        {
            foreach (var tag in post.Tags)
            {
                tagCounts.TryGetValue(tag, out var existing);
                tagCounts[tag] = existing + 1;
            }
        }

        var tagsPageSegment = _options.TagsPageUrl.Trim('/');
        foreach (var (tag, count) in tagCounts.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
        {
            var encoded = HttpUtility.UrlEncode(tag);
            var canonicalPath = new UrlPath($"/{tagsPageSegment}/{encoded}/");
            var outputFile = new FilePath($"{tagsPageSegment}/{encoded}/index.html");
            var canonicalRoute = new ContentRoute
            {
                CanonicalPath = canonicalPath,
                OutputFile = outputFile,
            };
            // Source value is informational — the static crawler issues
            // an HTTP GET which the Blazor router dispatches to Tag.razor
            // via its parameterized @page template.
            ContentSource tagSource = new RazorPageSource(
                tagComponentType.AssemblyQualifiedName ?? tagComponentType.FullName ?? tagComponentType.Name);
            yield return new DiscoveredItem(canonicalRoute, tagSource);

            var totalPages = (int)Math.Ceiling(count / (double)pageSize);
            for (var page = 2; page <= totalPages; page++)
            {
                var pagedPath = new UrlPath($"/{tagsPageSegment}/{encoded}/page/{page}/");
                var pagedOutput = new FilePath($"{tagsPageSegment}/{encoded}/page/{page}/index.html");
                yield return new DiscoveredItem(
                    new ContentRoute { CanonicalPath = pagedPath, OutputFile = pagedOutput },
                    tagSource);
            }
        }

        // Archive paginated routes. The canonical /archive route is emitted by
        // RazorPageContentService (its @page template has no parameters). Only
        // page 2..N need to come from here because /archive/page/{Page:int} is
        // parameterized and therefore skipped at automatic discovery time.
        if (posts.Count > pageSize)
        {
            var archiveTotalPages = (int)Math.Ceiling(posts.Count / (double)pageSize);
            ContentSource archiveSource = new RazorPageSource(
                archiveComponentType.AssemblyQualifiedName
                    ?? archiveComponentType.FullName
                    ?? archiveComponentType.Name);

            for (var page = 2; page <= archiveTotalPages; page++)
            {
                var pagedPath = new UrlPath($"/archive/page/{page}/");
                var pagedOutput = new FilePath($"archive/page/{page}/index.html");
                yield return new DiscoveredItem(
                    new ContentRoute { CanonicalPath = pagedPath, OutputFile = pagedOutput },
                    archiveSource);
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
    /// Build the RSS 2.0 XML document the <c>/rss.xml</c> endpoint returns.
    /// Exposed on the service so the minimal-API handler in
    /// <see cref="BlogSiteServiceExtensions.UseBlogSite"/> can share cached
    /// post metadata with <see cref="DiscoverAsync"/>.
    /// </summary>
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

    private async Task<ImmutableList<BlogPostDescriptor>> LoadPostsAsync()
    {
        var builder = ImmutableList.CreateBuilder<BlogPostDescriptor>();

        var contentRoot = Path.GetFullPath(
            Path.Combine(_options.ContentRootPath.Value, _options.BlogContentPath));
        if (!Directory.Exists(contentRoot))
        {
            return builder.ToImmutable();
        }

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
                var parsed = _parser.Parse<BlogSiteFrontMatter>(raw, file);
                fm = parsed.Metadata ?? new BlogSiteFrontMatter();
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

            builder.Add(new BlogPostDescriptor(route, fm, fm.Tags));
        }

        return builder.ToImmutable();
    }

    private sealed record BlogPostDescriptor(
        ContentRoute Route,
        BlogSiteFrontMatter FrontMatter,
        string[] Tags);
}