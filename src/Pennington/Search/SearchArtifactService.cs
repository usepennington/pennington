namespace Pennington.Search;

using Content;
using DeweySearch;
using Infrastructure;
using Pipeline;

/// <summary>
/// Builds the sharded search artifacts for every configured locale and exposes them as a
/// single <c>path -&gt; bytes</c> map — the one source of truth shared by the build-time
/// emitter (<see cref="SearchArtifactEmitter"/>) and the dev-time middleware
/// (<see cref="SearchArtifactMiddleware"/>).
/// <para>
/// Folds over <see cref="ISiteProjection"/> — every page's post-pipeline HTML and
/// heading-split sections have already been produced once by the shared projection, so
/// this service is a pure mapping from <see cref="RenderedPage"/> + <see cref="HeadingSection"/>
/// to <see cref="SearchDocument"/>. The corpus is grouped by locale and handed to the
/// external DeweySearch <see cref="IndexBuilder"/>; the resulting per-locale artifacts are
/// laid out under <c>search/{locale}/</c>. Computed lazily and recreated on file changes
/// when managed by <see cref="FileWatchDependencyFactory{T}"/>.
/// </para>
/// </summary>
public sealed class SearchArtifactService : IFileWatchAware
{
    private readonly AsyncLazy<IReadOnlyDictionary<string, byte[]>> _filesLazy;

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;

    /// <summary>Creates the service; artifacts are computed lazily on first request.</summary>
    public SearchArtifactService(
        ISiteProjection projection,
        SearchIndexBuilder corpusBuilder,
        IndexBuilder indexBuilder,
        LocalizationOptions localization,
        ContentRecordRegistry recordRegistry)
    {
        _filesLazy = new AsyncLazy<IReadOnlyDictionary<string, byte[]>>(
            () => BuildAllAsync(projection, corpusBuilder, indexBuilder, localization, recordRegistry));
    }

    /// <summary>Returns every artifact keyed by its relative output path (e.g. <c>search/en/index.json</c>).</summary>
    public async Task<IReadOnlyDictionary<string, byte[]>> GetArtifactFilesAsync() => await _filesLazy;

    /// <summary>Returns the bytes for a single artifact path, or null when no artifact matches.</summary>
    public async Task<byte[]?> GetArtifactAsync(string relativePath)
    {
        var files = await _filesLazy;
        return files.TryGetValue(relativePath, out var bytes) ? bytes : null;
    }

    private static async Task<IReadOnlyDictionary<string, byte[]>> BuildAllAsync(
        ISiteProjection projection,
        SearchIndexBuilder corpusBuilder,
        IndexBuilder indexBuilder,
        LocalizationOptions localization,
        ContentRecordRegistry recordRegistry)
    {
        // Join the rendered corpus back to its records by canonical path so each page's custom
        // facets (IHasSearchFacets) ride along with the built-in section/tag/area dimensions.
        var records = await recordRegistry.GetSnapshotAsync();

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

        await foreach (var page in projection.GetPagesAsync())
        {
            // Endpoint entries and llms-only pages have no HTML body to index. The projection
            // already produced empty content for them; skip rather than emitting blank records.
            if (page.Toc.ExcludeFromSearch || page.Content is null)
            {
                continue;
            }

            var locale = string.IsNullOrEmpty(page.Route.Locale)
                ? localization.DefaultLocale
                : page.Route.Locale;

            if (!groups.TryGetValue(locale, out var list))
            {
                groups[locale] = list = [];
            }

            records.TryGetValue(page.Route.CanonicalPath.Value.Trim('/'), out var record);

            // One record per heading section (plus a page-lead record) so results are
            // heading-level and deep-link to anchors.
            foreach (var section in page.Sections.Value)
            {
                list.Add(corpusBuilder.BuildSection(page.Toc, section, record?.Metadata));
            }
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
