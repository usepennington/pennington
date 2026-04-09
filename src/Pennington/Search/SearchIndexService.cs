namespace Pennington.Search;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;

/// <summary>
/// Generates search index JSON for the /search-index.json endpoint.
/// Uses <see cref="AsyncLazy{T}"/> for lazy, thread-safe computation.
/// When managed by <see cref="FileWatchDependencyFactory{T}"/>, the instance is
/// recreated on file changes — no manual watcher subscription needed.
/// </summary>
public sealed class SearchIndexService
{
    private readonly AsyncLazy<string> _indexLazy;

    public SearchIndexService(IServiceProvider serviceProvider, SearchIndexBuilder builder)
    {
        _indexLazy = new AsyncLazy<string>(() => BuildSearchIndexAsync(serviceProvider, builder));
    }

    public Task<string> GetSearchIndexJsonAsync() => _indexLazy.Value;

    private static async Task<string> BuildSearchIndexAsync(IServiceProvider sp, SearchIndexBuilder builder)
    {
        var documents = new List<SearchIndexDocument>();
        var contentServices = sp.GetServices<IContentService>();
        var parser = sp.GetRequiredService<IContentParser>();
        var renderer = sp.GetRequiredService<IContentRenderer>();

        foreach (var service in contentServices)
        {
            await foreach (var discovered in service.DiscoverAsync())
            {
                var parseResult = await parser.ParseAsync(discovered);
                if (parseResult is not ParsedItem parsed) continue;

                var renderResult = await renderer.RenderAsync(parsed);
                if (renderResult is not RenderedItem rendered) continue;

                var doc = builder.Build(rendered);
                if (doc != null)
                    documents.Add(doc);
            }
        }

        return JsonSerializer.Serialize(documents, SearchIndexJsonContext.Default.ListSearchIndexDocument);
    }
}

[JsonSerializable(typeof(List<SearchIndexDocument>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SearchIndexJsonContext : JsonSerializerContext;
