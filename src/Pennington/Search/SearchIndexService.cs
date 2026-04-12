namespace Pennington.Search;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.LlmsTxt;

/// <summary>
/// Generates search index JSON for the /search-index.json endpoint.
/// Uses <see cref="AsyncLazy{T}"/> for lazy, thread-safe computation.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — no manual watcher subscription needed.
/// <para>
/// Iterates TOC entries (which covers both markdown and Razor @page content)
/// and fetches post-pipeline HTML via <see cref="RenderedHtmlFetcher"/>, so the
/// index reflects what users actually see rather than pre-render markdown.
/// </para>
/// </summary>
public sealed class SearchIndexService
{
    private readonly AsyncLazy<string> _indexLazy;

    public SearchIndexService(
        IServiceProvider serviceProvider,
        SearchIndexBuilder builder,
        SearchIndexOptions options,
        RenderedHtmlFetcher fetcher,
        ILogger<SearchIndexService> logger)
    {
        _indexLazy = new AsyncLazy<string>(() => BuildSearchIndexAsync(serviceProvider, builder, options, fetcher, logger));
    }

    public Task<string> GetSearchIndexJsonAsync() => _indexLazy.Value;

    private static async Task<string> BuildSearchIndexAsync(
        IServiceProvider sp,
        SearchIndexBuilder builder,
        SearchIndexOptions options,
        RenderedHtmlFetcher fetcher,
        ILogger logger)
    {
        var documents = new List<SearchIndexDocument>();
        var contentServices = sp.GetServices<IContentService>();

        foreach (var service in contentServices)
        {
            // Skip the LlmsTxtContentService to avoid indexing the llms.txt artifacts themselves.
            if (service is LlmsTxtContentService) continue;

            var tocItems = await service.GetIndexableEntriesAsync();
            foreach (var toc in tocItems)
            {
                if (toc.ExcludeFromSearch) continue;
                var element = await fetcher.FetchContentAsync(toc.Route.CanonicalPath.Value, options.ContentSelector);
                if (element is null)
                {
                    logger.LogWarning("SearchIndexService: failed to fetch {Path}, skipping", toc.Route.CanonicalPath.Value);
                    continue;
                }

                documents.Add(builder.Build(toc, element.InnerHtml));
            }
        }

        return JsonSerializer.Serialize(documents, SearchIndexJsonContext.Default.ListSearchIndexDocument);
    }
}

[JsonSerializable(typeof(List<SearchIndexDocument>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SearchIndexJsonContext : JsonSerializerContext;
