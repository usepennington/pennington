namespace PaginatedListingExample;

using Pennington.Content;
using Pennington.Pipeline;

/// <summary>One entry in the article listing.</summary>
/// <param name="Url">Canonical URL of the article.</param>
/// <param name="Title">Display title.</param>
public sealed record Article(string Url, string Title);

/// <summary>
/// Collects the markdown articles under <c>/articles/</c> from every registered
/// <see cref="IContentService"/> and serves them one page at a time. Injected by the
/// <c>ArticlesPage</c> Razor component. It is not registered as an <see cref="IContentService"/>,
/// so the plain <see cref="IEnumerable{T}"/> injection here is safe — only the discovery service
/// (which is in that set) has to resolve siblings lazily to avoid a cycle.
/// </summary>
public sealed class ArticleResolver(IEnumerable<IContentService> services)
{
    /// <summary>Returns the requested 1-based page of articles, ordered by URL.</summary>
    public async Task<PagedList<Article>> GetPagedAsync(int page, int pageSize)
    {
        var all = await CollectAsync();
        var skip = Math.Max(0, (page - 1) * pageSize);
        var items = all.Skip(skip).Take(pageSize).ToList();
        return new PagedList<Article>(items, page, pageSize, all.Count);
    }

    private async Task<List<Article>> CollectAsync()
    {
        var articles = new List<Article>();
        await foreach (var item in services.DiscoverAllAsync())
        {
            if (item.Source.Value is FileSource { IsMarkdown: true } &&
                item.Route.CanonicalPath.Value.StartsWith("/articles/"))
            {
                var url = item.Route.CanonicalPath.Value;
                articles.Add(new Article(url, item.Metadata?.Title ?? url));
            }
        }

        return articles.OrderBy(a => a.Url, StringComparer.Ordinal).ToList();
    }
}
