namespace Pennington.Search;

using System.Text.Json;
using System.Text.Json.Serialization;
using Content;
using Infrastructure;
using Microsoft.Extensions.Logging;

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
public sealed class SearchIndexService : IFileWatchAware
{
    private readonly AsyncLazy<IReadOnlyDictionary<string, string>> _indexLazy;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;
    private readonly LocalizationOptions _localization;

    /// <summary>Creates the service; the per-locale index is computed lazily on first request.</summary>
    public SearchIndexService(
        IEnumerable<IContentService> contentServices,
        SearchIndexBuilder builder,
        SearchIndexOptions options,
        RenderedHtmlFetcher fetcher,
        LocalizationOptions localization,
        ILogger<SearchIndexService> logger)
    {
        _localization = localization;
        _indexLazy = new AsyncLazy<IReadOnlyDictionary<string, string>>(
            () => BuildAllAsync(contentServices, builder, options, fetcher, localization, logger));
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
        IEnumerable<IContentService> contentServices,
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
            ? localization.Locales.Keys
            : [localization.DefaultLocale];
        foreach (var code in configuredLocales)
        {
            groups[code] = [];
        }

        foreach (var toc in await contentServices.CollectIndexableEntriesAsync())
        {
            if (toc.ExcludeFromSearch)
            {
                continue;
            }

            var element = await fetcher.FetchContentAsync(toc.Route.CanonicalPath.Value, options.ContentSelector);
            if (element is null)
            {
                logger.LogWarning("SearchIndexService: failed to fetch {Path}, skipping", toc.Route.CanonicalPath.Value);
                continue;
            }

            if (options.ExcludeCodeBlocks)
            {
                foreach (var pre in element.QuerySelectorAll("pre").ToList())
                {
                    pre.Remove();
                }
            }

            // Extract heading text before tag-stripping collapses the DOM.
            // All six levels are joined with spaces; client-side boost applies
            // uniformly — finer-grained per-level weights didn't pull their weight.
            var headings = string.Join(' ',
                element.QuerySelectorAll("h1, h2, h3, h4, h5, h6")
                    .Select(h => h.TextContent.Trim())
                    .Where(t => t.Length > 0));

            var locale = string.IsNullOrEmpty(toc.Route.Locale)
                ? localization.DefaultLocale
                : toc.Route.Locale;
            if (!groups.TryGetValue(locale, out var list))
            {
                groups[locale] = list = [];
            }

            list.Add(builder.Build(toc, element.InnerHtml, headings));
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