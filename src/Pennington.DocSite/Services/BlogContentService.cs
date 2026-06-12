namespace Pennington.DocSite.Services;

using System.Collections.Immutable;
using System.Linq;
using Content;
using LlmsTxt;
using Pipeline;
using Routing;

/// <summary>
/// Surfaces the blog index route (<c>/blog</c>) the static build cannot otherwise discover, and
/// declares the <c>/blog/</c> llms.txt subtree. Browse-by-tag routes come from the registered
/// <c>AddTaxonomy&lt;BlogPostFrontMatter, string&gt;</c> axis; post pages from the markdown source;
/// post data, pagination, and RSS from the shared <see cref="BlogPostQuery"/>. Stateless — it reads
/// no files and holds no cache.
/// </summary>
public sealed class BlogContentService : IContentService, ILlmsSubtreeProvider
{
    /// <inheritdoc />
    public string DefaultSectionLabel => "";

    /// <inheritdoc />
    public int SearchPriority => 0;

    /// <inheritdoc />
    public IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        => new[] { RazorRoute("/blog", "blog/index.html", typeof(Components.Pages.Blog)) }
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc />
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc />
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);

    /// <summary>
    /// Declares <c>/blog/</c> as an llms.txt subtree so posts split out of the front-door
    /// <c>llms.txt</c> into a dedicated <c>/blog/llms.txt</c>. Always declared — the service is only
    /// registered when the content project has a blog folder.
    /// </summary>
    public Task<ImmutableList<LlmsSubtree>> GetLlmsSubtreesAsync()
        => Task.FromResult(ImmutableList.Create(new LlmsSubtree(
            routePrefix: "/blog/",
            title: "Blog",
            description: "Posts and announcements from the site blog.")));

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
}
