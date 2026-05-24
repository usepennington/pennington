namespace Pennington.Search;

using Content;
using DeweySearch;
using Infrastructure;
using Microsoft.Extensions.Logging;

/// <summary>
/// Builds the sharded search artifacts for every configured locale and exposes them as a
/// single <c>path -&gt; bytes</c> map — the one source of truth shared by the build-time
/// emitter (<see cref="SearchArtifactEmitter"/>) and the dev-time middleware
/// (<see cref="SearchArtifactMiddleware"/>).
/// <para>
/// Iterates indexable TOC entries (markdown and Razor <c>@page</c> content) and fetches
/// post-pipeline HTML via <see cref="RenderedHtmlFetcher"/>, so the index reflects what
/// users actually see. The corpus is grouped by locale and handed to the external DeweySearch
/// <see cref="IndexBuilder"/>; the resulting per-locale artifacts are laid out under
/// <c>search/{locale}/</c>. Computed lazily and recreated on file changes when managed by
/// <see cref="FileWatchDependencyFactory{T}"/>.
/// </para>
/// </summary>
public sealed class SearchArtifactService : IFileWatchAware
{
    private readonly AsyncLazy<IReadOnlyDictionary<string, byte[]>> _filesLazy;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>Creates the service; artifacts are computed lazily on first request.</summary>
    public SearchArtifactService(
        IEnumerable<IContentService> contentServices,
        SearchIndexBuilder corpusBuilder,
        HeadingSectionExtractor extractor,
        IndexBuilder indexBuilder,
        SearchIndexOptions options,
        RenderedHtmlFetcher fetcher,
        LocalizationOptions localization,
        ILogger<SearchArtifactService> logger)
    {
        _filesLazy = new AsyncLazy<IReadOnlyDictionary<string, byte[]>>(
            () => BuildAllAsync(contentServices, corpusBuilder, extractor, indexBuilder, options, fetcher, localization, logger));
    }

    /// <summary>Returns every artifact keyed by its relative output path (e.g. <c>search/en/index.json</c>).</summary>
    public async Task<IReadOnlyDictionary<string, byte[]>> GetArtifactFilesAsync() => await _filesLazy.Value;

    /// <summary>Returns the bytes for a single artifact path, or null when no artifact matches.</summary>
    public async Task<byte[]?> GetArtifactAsync(string relativePath)
    {
        var files = await _filesLazy.Value;
        return files.TryGetValue(relativePath, out var bytes) ? bytes : null;
    }

    private static async Task<IReadOnlyDictionary<string, byte[]>> BuildAllAsync(
        IEnumerable<IContentService> contentServices,
        SearchIndexBuilder corpusBuilder,
        HeadingSectionExtractor extractor,
        IndexBuilder indexBuilder,
        SearchIndexOptions options,
        RenderedHtmlFetcher fetcher,
        LocalizationOptions localization,
        ILogger logger)
    {
        var groups = new Dictionary<string, List<SearchDocument>>(StringComparer.OrdinalIgnoreCase);

        // Seed an empty bucket for every configured locale so registered-but-empty locales
        // still serve a valid (empty) manifest rather than 404.
        var configuredLocales = localization.Locales.Count > 0
            ? localization.Locales.Keys
            : [localization.DefaultLocale];
        foreach (var code in configuredLocales)
        {
            groups[code] = [];
        }

        var entries = (await contentServices.CollectIndexableEntriesAsync())
            .Where(toc => !toc.ExcludeFromSearch)
            .ToList();

        // Fetch + extract every page in parallel — this is the loop that drove build
        // parallelism to 1.77×. Each result lands in an index-keyed slot so the corpus
        // is folded in discovery order (deterministic artifact bytes). The fetch goes
        // through BuildHtmlCache, so the disk-write pass replays these renders.
        var perEntry = new (string Locale, List<SearchDocument> Docs)?[entries.Count];
        await Parallel.ForEachAsync(Enumerable.Range(0, entries.Count), async (i, ct) =>
        {
            var toc = entries[i];
            var element = await fetcher.FetchContentAsync(toc.Route.CanonicalPath.Value, options.ContentSelector, ct);
            if (element is null)
            {
                logger.LogWarning("SearchArtifactService: failed to fetch {Path}, skipping", toc.Route.CanonicalPath.Value);
                return;
            }

            var locale = string.IsNullOrEmpty(toc.Route.Locale)
                ? localization.DefaultLocale
                : toc.Route.Locale;

            // One record per heading section (plus a page-lead record) so results are
            // heading-level and deep-link to anchors. Code blocks are dropped inside the extractor.
            var docs = new List<SearchDocument>();
            foreach (var section in extractor.Extract(element, options.ExcludeCodeBlocks))
            {
                docs.Add(corpusBuilder.BuildSection(toc, section));
            }

            perEntry[i] = (locale, docs);
        });

        // Fold per-page results into locale buckets sequentially — the dictionary mutation
        // is not thread-safe, so it stays out of the parallel section.
        foreach (var entry in perEntry)
        {
            if (entry is not { } result)
            {
                continue;
            }

            if (!groups.TryGetValue(result.Locale, out var list))
            {
                groups[result.Locale] = list = [];
            }

            list.AddRange(result.Docs);
        }

        // Hand each locale's corpus to DeweySearch and lay its artifacts out under search/{locale}/.
        var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var (locale, docs) in groups)
        {
            var index = indexBuilder.Build(docs);
            var dir = $"search/{locale}";
            foreach (var (name, bytes) in index.ToFiles())
            {
                files[$"{dir}/{name}"] = bytes;
            }
        }

        return files;
    }
}
