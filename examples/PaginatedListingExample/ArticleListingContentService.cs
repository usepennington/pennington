namespace PaginatedListingExample;

using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using PaginatedListingExample.Components.Pages;
using Pennington.Content;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Emits the paginated <c>/articles/page/N/</c> routes so the static build crawls them. The
/// canonical <c>/articles</c> and <c>/articles/page/{Page:int}</c> URLs come from
/// <see cref="ArticlesPage"/>'s <c>@page</c> directives, but Pennington's automatic Razor route
/// discovery skips the parameterized template, so the numbered pages are declared here instead.
/// </summary>
/// <remarks>
/// Mirrors <c>SocialCardContentService</c>: this service is itself one of the registered
/// <see cref="IContentService"/> instances, so it resolves its siblings on demand from
/// <see cref="IServiceProvider"/> inside <see cref="DiscoverAsync"/> and excludes itself with
/// <c>!ReferenceEquals(s, this)</c>. Constructor-injecting <c>IEnumerable&lt;IContentService&gt;</c>
/// here would form a DI cycle and throw at startup.
/// </remarks>
public sealed class ArticleListingContentService(IServiceProvider serviceProvider) : IContentService
{
    private const int PageSize = 20;

    /// <inheritdoc/>
    public string DefaultSectionLabel => "";

    /// <inheritdoc/>
    public int SearchPriority => 0;

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        // Resolve siblings on demand rather than via a ctor IEnumerable<IContentService>: this
        // service is itself in that set, so the ctor injection would be a DI cycle. Exclude self
        // so the sibling walk can't recurse back into this discovery.
        var siblings = serviceProvider.GetServices<IContentService>()
            .Where(s => !ReferenceEquals(s, this))
            .ToList();

        var count = 0;
        await foreach (var item in siblings.DiscoverAllAsync())
        {
            if (item.Source.Value is FileSource { IsMarkdown: true } &&
                item.Route.CanonicalPath.Value.StartsWith("/articles/"))
            {
                count++;
            }
        }

        ContentSource source = new RazorPageSource(typeof(ArticlesPage).AssemblyQualifiedName!);
        var totalPages = (int)Math.Ceiling(count / (double)PageSize);
        for (var page = 2; page <= totalPages; page++)
        {
            yield return new DiscoveredItem(
                new ContentRoute
                {
                    CanonicalPath = new UrlPath($"/articles/page/{page}/"),
                    OutputFile = new FilePath($"articles/page/{page}/index.html"),
                },
                source);
        }
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        => Task.FromResult(ImmutableList<CrossReference>.Empty);
}
