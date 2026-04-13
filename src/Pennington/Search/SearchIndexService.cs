namespace Pennington.Search;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Content;
using Infrastructure;
using LlmsTxt;

/// <summary>
/// Generates per-locale search index JSON. Each configured locale gets its own
/// document bucket; the client fetches only the index for the active locale.
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
    private readonly AsyncLazy<IReadOnlyDictionary<string, string>> _indexLazy;
    private readonly LocalizationOptions _localization;

    public SearchIndexService(
        IServiceProvider serviceProvider,
        SearchIndexBuilder builder,
        SearchIndexOptions options,
        RenderedHtmlFetcher fetcher,
        LocalizationOptions localization,
        ILogger<SearchIndexService> logger)
    {
        _localization = localization;
        _indexLazy = new AsyncLazy<IReadOnlyDictionary<string, string>>(
            () => BuildAllAsync(serviceProvider, builder, options, fetcher, localization, logger));
    }

    /// <summary>
    /// Returns the JSON array of search documents for the given locale. Returns
    /// <c>"[]"</c> when the locale has no indexed content (or is not registered).
    /// </summary>
    public async Task<string> GetSearchIndexJsonAsync(string locale)
    {
        var all = await _indexLazy.Value;
        return all.TryGetValue(locale, out var json) ? json : "[]";
    }

    private static async Task<IReadOnlyDictionary<string, string>> BuildAllAsync(
        IServiceProvider sp,
        SearchIndexBuilder builder,
        SearchIndexOptions options,
        RenderedHtmlFetcher fetcher,
        LocalizationOptions localization,
        ILogger logger)
    {
        var groups = new Dictionary<string, List<SearchIndexDocument>>(StringComparer.OrdinalIgnoreCase);

        // Seed an empty bucket for every configured locale so registered-but-empty
        // locales serve "[]" rather than 404. Fall back to DefaultLocale for
        // single-locale sites that haven't called AddLocale.
        var configuredLocales = localization.Locales.Count > 0
            ? (IEnumerable<string>)localization.Locales.Keys
            : [localization.DefaultLocale];
        foreach (var code in configuredLocales) groups[code] = [];

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

                var locale = string.IsNullOrEmpty(toc.Route.Locale)
                    ? localization.DefaultLocale
                    : toc.Route.Locale;
                if (!groups.TryGetValue(locale, out var list))
                    groups[locale] = list = [];
                list.Add(builder.Build(toc, element.InnerHtml));
            }
        }

        return groups.ToDictionary(
            kv => kv.Key,
            kv => JsonSerializer.Serialize(kv.Value, SearchIndexJsonContext.Default.ListSearchIndexDocument),
            StringComparer.OrdinalIgnoreCase);
    }
}

[JsonSerializable(typeof(List<SearchIndexDocument>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SearchIndexJsonContext : JsonSerializerContext;