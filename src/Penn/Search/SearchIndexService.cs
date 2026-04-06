namespace Penn.Search;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.Infrastructure;
using Penn.Pipeline;

/// <summary>
/// Generates and caches the search index JSON for the /search-index.json endpoint.
/// Invalidates when content files change.
/// </summary>
public sealed class SearchIndexService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SearchIndexBuilder _builder;
    private string? _cachedJson;

    public SearchIndexService(
        IServiceProvider serviceProvider,
        SearchIndexBuilder builder,
        IFileWatcher fileWatcher)
    {
        _serviceProvider = serviceProvider;
        _builder = builder;
        fileWatcher.SubscribeToChanges(() => _cachedJson = null);
    }

    public async Task<string> GetSearchIndexJsonAsync()
    {
        if (_cachedJson != null)
            return _cachedJson;

        var documents = new List<SearchIndexDocument>();
        var contentServices = _serviceProvider.GetServices<IContentService>();
        var parser = _serviceProvider.GetRequiredService<IContentParser>();
        var renderer = _serviceProvider.GetRequiredService<IContentRenderer>();

        foreach (var service in contentServices)
        {
            await foreach (var discovered in service.DiscoverAsync())
            {
                var parseResult = await parser.ParseAsync(discovered);
                if (parseResult is not ParsedItem parsed) continue;

                var renderResult = await renderer.RenderAsync(parsed);
                if (renderResult is not RenderedItem rendered) continue;

                var doc = _builder.Build(rendered);
                if (doc != null)
                    documents.Add(doc);
            }
        }

        _cachedJson = JsonSerializer.Serialize(documents, SearchIndexJsonContext.Default.ListSearchIndexDocument);
        return _cachedJson;
    }
}

[JsonSerializable(typeof(List<SearchIndexDocument>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SearchIndexJsonContext : JsonSerializerContext;
