namespace Pennington.Content;

using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Abstractions;
using FrontMatter;
using Infrastructure;
using LlmsTxt;
using Microsoft.Extensions.Logging;
using Pipeline;
using Routing;
using SharpYaml;

/// <summary>
/// Discovers and provides markdown content from a directory.
/// When <see cref="LocalizationOptions"/> has multiple locales, also discovers
/// content from locale subdirectories (e.g., Content/fr/, Content/de/).
/// </summary>
/// <remarks>
/// File-reading service registered as a plain singleton (the open-generic shape over
/// <typeparamref name="TFrontMatter"/> and per-source <c>ContentPath</c> rule out
/// <c>AddFileWatched&lt;T&gt;()</c>). It implements <see cref="IFileWatchAware"/>: it declares
/// its content directory as a <see cref="FileWatchScope"/> and resets the cached metadata
/// <see cref="AsyncLazy{T}"/> when <see cref="FileWatchDispatcher"/> reports a change there.
/// </remarks>
public sealed class MarkdownContentService<TFrontMatter>
    : IContentService, IMarkdownContentSource, ILlmsSubtreeProvider, IFolderMetadataProvider, IFileWatchAware
    where TFrontMatter : IFrontMatter, new()
{
    /// <summary>Sidecar filename that, when dropped at any folder under the content root, declares folder metadata (display title, sort order, llms-subtree opt-in).</summary>
    public const string FolderMetadataSidecarFileName = "_meta.yml";

    /// <summary>File-name suffix that marks a markdown file as llms-only — emitted to the llms.txt sidecar but never as an HTML page.</summary>
    public const string LlmsOnlyFileSuffix = ".llms.md";

    private readonly MarkdownContentServiceOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly IFileSystem _fileSystem;
    private readonly LocalizationOptions _localization;
    private readonly TimeProvider _clock;
    private readonly ILogger<MarkdownContentService<TFrontMatter>>? _logger;
    private readonly string _absoluteContentPath;
    private readonly ImmutableArray<string> _normalizedExcludePaths;
    private readonly FileWatchScope _watchScope;

    // Metadata cache. _entries is keyed by absolute source-file path; each FileEntry holds
    // the routes the file generates (its own route + any fallback routes for non-default
    // locales that don't have their own copy) together with the parsed front matter.
    // _seededLazy ensures the initial scan + parse runs exactly once; mutations on
    // OnFileChanged update individual entries under _cacheLock.
    private readonly Lock _cacheLock = new();
    private readonly Dictionary<string, FileEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FolderMetadata> _folderMetadataByFile = new(StringComparer.OrdinalIgnoreCase);
    private AsyncLazy<bool> _seededLazy;
    private AsyncLazy<bool> _folderSeededLazy;

    /// <summary>Routes a single source file produces (its own route + fallback locale routes), with the parsed front matter shared across them.</summary>
    private sealed record FileEntry(
        ImmutableList<(ContentRoute Route, bool IsLlmsOnly)> Routes,
        TFrontMatter FrontMatter);

    /// <summary>
    /// Initializes the service and prepares lazy metadata loading. <see cref="FileWatchDispatcher"/>
    /// watches the content directory and drives cache invalidation through <see cref="OnFileChanged"/>.
    /// </summary>
    public MarkdownContentService(
        MarkdownContentServiceOptions options,
        FrontMatterParser parser,
        IFileSystem fileSystem,
        LocalizationOptions localization,
        TimeProvider? clock = null,
        ILogger<MarkdownContentService<TFrontMatter>>? logger = null)
    {
        _options = options;
        _parser = parser;
        _fileSystem = fileSystem;
        _localization = localization;
        _clock = clock ?? TimeProvider.System;
        _logger = logger;
        _absoluteContentPath = _fileSystem.Path.GetFullPath(options.ContentPath.Value);
        _normalizedExcludePaths = NormalizeExcludePaths(options.ExcludePaths);
        _watchScope = new FileWatchScope(_absoluteContentPath, "*.*", IncludeSubdirectories: true);
        _seededLazy = new AsyncLazy<bool>(SeedMetadataAsync);
        _folderSeededLazy = new AsyncLazy<bool>(SeedFolderMetadataAsync);
    }

    /// <inheritdoc/>
    public IReadOnlyList<FileWatchScope> WatchScopes => [_watchScope];

    /// <inheritdoc/>
    public ContentChangeImpact GetAffectedRoutes(FileChangeNotification change)
    {
        if (!_watchScope.Matches(change))
        {
            return ContentChangeImpact.None;
        }

        var fileName = _fileSystem.Path.GetFileName(change.FullPath) ?? "";

        // _meta.yml affects every page under the folder prefix; conservative wildcard
        // rather than walking _entries (which mutates concurrently with this call).
        if (string.Equals(fileName, FolderMetadataSidecarFileName, StringComparison.OrdinalIgnoreCase))
        {
            return ContentChangeImpact.Wildcard;
        }

        if (!fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            // Assets (images, css) — neither cache holds these.
            return ContentChangeImpact.None;
        }

        // Renames only carry the new path; we can't know the old route, so over-invalidate.
        if (change.ChangeType == WatcherChangeTypes.Renamed)
        {
            return ContentChangeImpact.Wildcard;
        }

        // Pass a predicate that always reports "no localized version exists" so every
        // candidate fallback route is included. Over-invalidating one or two routes per
        // locale is cheaper than scanning _entries mid-watcher-callback to learn which
        // locales actually have their own copy.
        var absolutePath = _fileSystem.Path.GetFullPath(change.FullPath);
        var locale = LocaleForPath(absolutePath);
        var file = new FilePath(absolutePath);
        return ContentChangeImpact.Routes(
            [.. RoutesForFile(file, locale, static (_, _) => false)]);
    }

    /// <summary>
    /// Surgically updates the discovery caches when a file under this source's content
    /// directory changes. A markdown edit re-parses just that file's front matter; a
    /// <c>_meta.yml</c> change re-reads just that sidecar; an asset change is a no-op.
    /// Renames (where the watcher only carries the new path) fall back to a full re-seed.
    /// Changes outside the scope are ignored.
    /// </summary>
    public FileWatchResponse OnFileChanged(FileChangeNotification change)
    {
        if (!_watchScope.Matches(change))
        {
            return FileWatchResponse.Ignore;
        }

        var fileName = _fileSystem.Path.GetFileName(change.FullPath) ?? "";
        if (string.Equals(fileName, FolderMetadataSidecarFileName, StringComparison.OrdinalIgnoreCase))
        {
            return ApplyFolderMetadataChange(change);
        }

        if (fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return ApplyMarkdownChange(change);
        }

        // Asset under the content tree (image, css, etc.) — neither cache holds these.
        return FileWatchResponse.Ignore;
    }

    private FileWatchResponse ApplyMarkdownChange(FileChangeNotification change)
    {
        var absolutePath = _fileSystem.Path.GetFullPath(change.FullPath);

        // Renamed events only carry the new path. Force a re-seed so a renamed-away
        // entry doesn't linger and a renamed-in entry gets picked up cleanly.
        if (change.ChangeType == WatcherChangeTypes.Renamed)
        {
            lock (_cacheLock)
            {
                _entries.Clear();
            }
            _seededLazy = new AsyncLazy<bool>(SeedMetadataAsync);
            return FileWatchResponse.Refreshed;
        }

        // Pre-seed mutations: the initial scan hasn't run yet, so it will see current
        // disk state when it does. Nothing to do.
        if (!_seededLazy.Task.IsCompletedSuccessfully)
        {
            return FileWatchResponse.Refreshed;
        }

        var locale = LocaleForPath(absolutePath);

        switch (change.ChangeType)
        {
            case WatcherChangeTypes.Deleted:
                lock (_cacheLock)
                {
                    _entries.Remove(absolutePath);
                    // Removing a non-default-locale file might restore a fallback the
                    // default-locale entry had previously dropped. Recompute it.
                    RebuildCorrespondingDefaultLocaleEntry(absolutePath, locale);
                }
                break;

            case WatcherChangeTypes.Created:
            case WatcherChangeTypes.Changed:
            default:
                // Parse outside the lock; assign under it.
                var parsed = TryParseFrontMatter(absolutePath);
                if (parsed is null)
                {
                    return FileWatchResponse.Refreshed;
                }

                lock (_cacheLock)
                {
                    if (_entries.TryGetValue(absolutePath, out var existing)
                        && change.ChangeType == WatcherChangeTypes.Changed)
                    {
                        // Content edit: routes unchanged, just refresh front matter.
                        _entries[absolutePath] = existing with { FrontMatter = parsed };
                    }
                    else
                    {
                        // Created (or Changed on a file we don't know yet): build a fresh entry.
                        _entries[absolutePath] = BuildEntry(
                            new FilePath(absolutePath), locale, parsed, HasLocalizedVersionFromCache);

                        // A new non-default-locale file supersedes the default-locale fallback.
                        RebuildCorrespondingDefaultLocaleEntry(absolutePath, locale);
                    }
                }
                break;
        }

        return FileWatchResponse.Refreshed;
    }

    private FileWatchResponse ApplyFolderMetadataChange(FileChangeNotification change)
    {
        var absolutePath = _fileSystem.Path.GetFullPath(change.FullPath);

        if (change.ChangeType == WatcherChangeTypes.Renamed)
        {
            lock (_cacheLock)
            {
                _folderMetadataByFile.Clear();
            }
            _folderSeededLazy = new AsyncLazy<bool>(SeedFolderMetadataAsync);
            return FileWatchResponse.Refreshed;
        }

        if (!_folderSeededLazy.Task.IsCompletedSuccessfully)
        {
            return FileWatchResponse.Refreshed;
        }

        if (change.ChangeType == WatcherChangeTypes.Deleted)
        {
            lock (_cacheLock)
            {
                _folderMetadataByFile.Remove(absolutePath);
            }
            return FileWatchResponse.Refreshed;
        }

        var metadata = TryReadFolderMetadata(absolutePath);
        lock (_cacheLock)
        {
            if (metadata is null)
            {
                _folderMetadataByFile.Remove(absolutePath);
            }
            else
            {
                _folderMetadataByFile[absolutePath] = metadata;
            }
        }
        return FileWatchResponse.Refreshed;
    }

    /// <summary>
    /// When a non-default-locale file is added or removed, the corresponding default-locale
    /// entry's fallback-route set may need to flip. Re-parse and rebuild just that entry's
    /// FileEntry; cheap (one front-matter parse) and keeps fallbacks correct without a full
    /// re-seed. Must be called under <see cref="_cacheLock"/>.
    /// </summary>
    private void RebuildCorrespondingDefaultLocaleEntry(string changedAbsolutePath, string changedLocale)
    {
        if (!_localization.IsMultiLocale
            || string.Equals(changedLocale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(changedLocale))
        {
            return;
        }

        var localeRoot = _fileSystem.Path.Combine(_absoluteContentPath, changedLocale);
        var relative = _fileSystem.Path.GetRelativePath(localeRoot, changedAbsolutePath).Replace('\\', '/');
        if (relative.StartsWith("..", StringComparison.Ordinal))
        {
            return;
        }

        var defaultPath = _fileSystem.Path.GetFullPath(
            _fileSystem.Path.Combine(_absoluteContentPath, relative));

        if (!_entries.TryGetValue(defaultPath, out var existing))
        {
            return;
        }

        _entries[defaultPath] = BuildEntry(
            new FilePath(defaultPath), _localization.DefaultLocale, existing.FrontMatter, HasLocalizedVersionFromCache);
    }

    /// <inheritdoc/>
    public string DefaultSectionLabel => _options.SectionLabel ?? "";

    /// <inheritdoc/>
    public int SearchPriority => _options.SearchPriority;

    /// <summary>
    /// Absolute filesystem path to the root of this service's content directory.
    /// </summary>
    public string AbsoluteContentRoot => _absoluteContentPath;

    /// <summary>
    /// URL prefix prepended to routes generated from this content directory.
    /// </summary>
    public UrlPath BasePageUrl => _options.BasePageUrl;

    /// <summary>
    /// Normalized forward-slash subtree paths (relative to the content root) that are skipped during discovery.
    /// </summary>
    public ImmutableArray<string> ExcludePaths => _normalizedExcludePaths;

    /// <summary>
    /// Normalizes user-provided exclude paths to forward-slash, lowercase, trimmed
    /// of leading/trailing slashes. Empty entries are dropped so a bare
    /// <c>""</c> or <c>"/"</c> doesn't accidentally exclude the entire root.
    /// </summary>
    private static ImmutableArray<string> NormalizeExcludePaths(ImmutableArray<string> raw)
    {
        if (raw.IsDefaultOrEmpty)
        {
            return ImmutableArray<string>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<string>(raw.Length);
        foreach (var entry in raw)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            var normalized = entry.Replace('\\', '/').Trim('/').ToLowerInvariant();
            if (normalized.Length == 0)
            {
                continue;
            }

            builder.Add(normalized);
        }
        return builder.ToImmutable();
    }

    /// <summary>
    /// True when <paramref name="relativePath"/> (forward-slash, relative to the content
    /// root) is inside one of the excluded subtrees. Matching is segment-based:
    /// exclude <c>"a/b"</c> covers <c>a/b</c>, <c>a/b/...</c> but not <c>a/bcd</c>.
    /// </summary>
    private bool IsRelativePathExcluded(string relativePath)
    {
        if (_normalizedExcludePaths.IsDefaultOrEmpty)
        {
            return false;
        }

        var normalized = relativePath.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
        foreach (var excluded in _normalizedExcludePaths)
        {
            if (normalized.Length == excluded.Length
                && normalized.Equals(excluded, StringComparison.Ordinal))
            {
                return true;
            }

            if (normalized.Length > excluded.Length
                && normalized.StartsWith(excluded, StringComparison.Ordinal)
                && normalized[excluded.Length] == '/')
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        // Walk the cached metadata rather than re-reading + re-parsing front matter per call —
        // every other channel on this service already does this (GetIndexableEntriesAsync,
        // GetContentTocEntriesAsync, etc.), and per-request callers (catch-all page handlers)
        // would otherwise pay an O(files) YAML re-parse on every hit.
        var metadata = await SnapshotMetadataAsync();
        foreach (var (route, frontMatter, isLlmsOnly) in metadata)
        {
            if (frontMatter.IsHiddenFromBuild(_clock))
            {
                continue;
            }

            // Pages with RedirectUrl front matter are surfaced as RedirectSource so the
            // unified redirect middleware handles them at dev time and the static build
            // crawler captures the 301 response (written as a meta-refresh HTML file).
            if (frontMatter is IRedirectable { RedirectUrl: { Length: > 0 } redirectTarget })
            {
                yield return new DiscoveredItem(route, new RedirectSource(new UrlPath(redirectTarget)));
                continue;
            }

            if (isLlmsOnly)
            {
                yield return new DiscoveredItem(route, new LlmsOnlySource(route.SourceFile!.Value)) { Metadata = frontMatter };
                continue;
            }

            yield return new DiscoveredItem(route, new MarkdownFileSource(route.SourceFile!.Value)) { Metadata = frontMatter };
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ParsedItem> ParseContentAsync()
    {
        var start = Stopwatch.GetTimestamp();
        var yielded = 0;
        foreach (var (route, sourceFile) in DiscoverRoutesWithFallbacks())
        {
            var item = await TryParseContentAsync(route, sourceFile);
            if (item is not null)
            {
                yielded++;
                yield return item;
            }
        }
        _logger?.LogDebug(
            "MarkdownContentService.ParseContentAsync: yielded {Count} items in {ElapsedMs:F1}ms ({ContentPath})",
            yielded, Stopwatch.GetElapsedTime(start).TotalMilliseconds, _absoluteContentPath);
    }

    /// <summary>
    /// Reads and parses one file with this service's <typeparamref name="TFrontMatter"/>.
    /// Returns <c>null</c> for unparseable files and for items that never reach the
    /// parsed-body channel — drafts/scheduled pages and redirects (surfaced as
    /// <see cref="RedirectSource"/>, not bodies) — mirroring <see cref="DiscoverAsync"/>.
    /// </summary>
    private async Task<ParsedItem?> TryParseContentAsync(ContentRoute route, FilePath sourceFile)
    {
        try
        {
            var content = await _fileSystem.File.ReadAllTextAsync(sourceFile.Value);
            var result = _parser.Parse<TFrontMatter>(content, sourceFile.Value);
            var metadata = result.Metadata ?? new TFrontMatter();

            if (metadata.IsHiddenFromBuild(_clock)
                || metadata is IRedirectable { RedirectUrl: { Length: > 0 } })
            {
                return null;
            }

            return new ParsedItem(route, metadata, result.Body);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var metadata = await SnapshotMetadataAsync();
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var entry in metadata)
        {
            // Llms-only pages are surfaced through GetIndexableEntriesAsync so
            // LlmsTxtService can pick them up, but they must stay out of nav
            // and the search index — drop them from the TOC channel.
            if (entry.IsLlmsOnly)
            {
                continue;
            }

            var toc = BuildTocItem(entry.Route, entry.FrontMatter, isLlmsOnly: false);
            if (toc is null)
            {
                continue;
            }

            builder.Add(toc);
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<ContentTocItem>> GetIndexableEntriesAsync()
    {
        var metadata = await SnapshotMetadataAsync();
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var entry in metadata)
        {
            var toc = BuildTocItem(entry.Route, entry.FrontMatter, entry.IsLlmsOnly);
            if (toc is null)
            {
                continue;
            }

            builder.Add(toc);
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Builds a single TOC item, applying draft/redirect/empty-title filters.
    /// Returns <c>null</c> when the entry should be dropped from the channel.
    /// Llms-only entries are forced to <see cref="ContentTocItem.ExcludeFromSearch"/>
    /// regardless of front matter — by definition the page has no human-facing URL
    /// for a search hit to point at.
    /// </summary>
    private ContentTocItem? BuildTocItem(ContentRoute route, TFrontMatter fm, bool isLlmsOnly)
    {
        if (fm.IsHiddenFromBuild(_clock))
        {
            return null;
        }
        // Redirects are transport-only — they shouldn't appear in navigation,
        // search results, or llms.txt (all of which iterate this TOC). The
        // engine emits a meta-refresh page at the route, which has no
        // meaningful title or body to index.
        if (fm is IRedirectable { RedirectUrl: { Length: > 0 } })
        {
            return null;
        }
        // Defensive: a parsed file with no title produces an empty search
        // entry (title="", body=""). This typically indicates a redirect
        // whose frontmatter type doesn't implement IRedirectable, or a
        // file with only frontmatter and no body. Either way, it's not
        // useful content — skip it.
        if (string.IsNullOrWhiteSpace(fm.Title))
        {
            return null;
        }

        var order = fm is IOrderable orderable ? orderable.Order : int.MaxValue;
        var sectionLabel = fm is ISectionable sectionable ? sectionable.SectionLabel : _options.SectionLabel;
        var excludeFromSearch = !fm.Search || isLlmsOnly;
        var excludeFromLlms = !fm.Llms;
        var hierarchyParts = route.CanonicalPath.Value
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        return new ContentTocItem(
            Title: fm.Title,
            Route: route,
            Order: order,
            HierarchyParts: hierarchyParts,
            SectionLabel: sectionLabel ?? DefaultSectionLabel,
            Locale: string.IsNullOrEmpty(route.Locale) ? null : route.Locale
        )
        {
            Description = fm.Description,
            Tags = fm is ITaggable taggable ? taggable.Tags : [],
            ExcludeFromSearch = excludeFromSearch,
            ExcludeFromLlms = excludeFromLlms,
            SearchOnly = fm.SearchOnly,
        };
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<DiscoveredItem>> GetRedirectSourcesAsync()
    {
        var metadata = await SnapshotMetadataAsync();
        var builder = ImmutableList.CreateBuilder<DiscoveredItem>();

        foreach (var (route, fm, _) in metadata)
        {
            if (fm.IsHiddenFromBuild(_clock))
            {
                continue;
            }

            if (fm is IRedirectable { RedirectUrl: { Length: > 0 } target })
            {
                builder.Add(new DiscoveredItem(route, new RedirectSource(new UrlPath(target))));
            }
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var metadata = await SnapshotMetadataAsync();
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var (route, fm, _) in metadata)
        {
            if (fm.Uid is { } uid && !string.IsNullOrEmpty(uid))
            {
                builder.Add(new CrossReference(uid, fm.Title, route));
            }
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
    {
        var contentPath = _absoluteContentPath;
        if (!_fileSystem.Directory.Exists(contentPath))
        {
            return Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        }

        var builder = ImmutableList.CreateBuilder<ContentToCopy>();
        var excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".md", ".mdx", ".razor", ".yml", ".yaml" };
        var localeSubfolders = GetNonDefaultLocaleSubfolders();
        // Matches the URL prefix ContentRouteFactory applies to routes from this
        // source. Post at `/blog/foo/bar/` with sibling `commits.png` references
        // `/blog/foo/bar/commits.png` in its rendered HTML; without this prefix
        // the file would be copied to `/foo/bar/commits.png` and 404.
        var outputPrefix = _options.BasePageUrl.Value.Trim('/');

        foreach (var file in _fileSystem.Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories))
        {
            var ext = _fileSystem.Path.GetExtension(file);
            if (excludedExtensions.Contains(ext))
            {
                continue;
            }

            var relativePath = _fileSystem.Path.GetRelativePath(contentPath, file).Replace('\\', '/');

            // Skip locale subfolders from the default locale scan — they get their own entries below
            if (IsInLocaleSubfolder(relativePath, localeSubfolders))
            {
                continue;
            }

            // Skip subtrees explicitly excluded via MarkdownContentServiceOptions.ExcludePaths
            // (typically because another content source owns that subtree).
            if (IsRelativePathExcluded(relativePath))
            {
                continue;
            }

            var outputPath = outputPrefix.Length == 0
                ? relativePath
                : $"{outputPrefix}/{relativePath}";
            builder.Add(new ContentToCopy(new FilePath(file), new FilePath(outputPath)));
        }

        // Also enumerate static files from non-default locale subdirectories
        foreach (var locale in localeSubfolders)
        {
            var localePath = _fileSystem.Path.Combine(contentPath, locale);
            if (!_fileSystem.Directory.Exists(localePath))
            {
                continue;
            }

            foreach (var file in _fileSystem.Directory.EnumerateFiles(localePath, "*.*", SearchOption.AllDirectories))
            {
                var ext = _fileSystem.Path.GetExtension(file);
                if (excludedExtensions.Contains(ext))
                {
                    continue;
                }

                var relativeToLocale = _fileSystem.Path.GetRelativePath(localePath, file).Replace('\\', '/');
                // Output at /{locale}{basePageUrl}/relative/path so they serve at
                // the locale-prefixed URL that matches ContentRouteFactory's output.
                var outputPath = outputPrefix.Length == 0
                    ? $"{locale}/{relativeToLocale}"
                    : $"{locale}/{outputPrefix}/{relativeToLocale}";
                builder.Add(new ContentToCopy(new FilePath(file), new FilePath(outputPath)));
            }
        }

        return Task.FromResult(builder.ToImmutable());
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <summary>
    /// Discovers markdown files. When multi-locale, discovers from the base path
    /// (default locale) and each locale subdirectory, tagging files with their locale.
    /// </summary>
    private List<(FilePath File, string Locale)> DiscoverFiles()
    {
        var contentPath = _absoluteContentPath;
        if (!_fileSystem.Directory.Exists(contentPath))
        {
            return [];
        }

        var localeSubfolders = GetNonDefaultLocaleSubfolders();
        var results = new List<(FilePath, string)>();

        // Default locale: enumerate base path, excluding locale subdirectories
        foreach (var file in _fileSystem.Directory.EnumerateFiles(contentPath, _options.FilePattern, SearchOption.AllDirectories))
        {
            var relativePath = _fileSystem.Path.GetRelativePath(contentPath, file).Replace('\\', '/');
            if (IsInLocaleSubfolder(relativePath, localeSubfolders))
            {
                continue;
            }

            if (IsRelativePathExcluded(relativePath))
            {
                continue;
            }

            var locale = _localization.IsMultiLocale ? _localization.DefaultLocale : _options.Locale;
            results.Add((new FilePath(file), locale));
        }

        // Non-default locales: enumerate each locale subdirectory
        foreach (var locale in localeSubfolders)
        {
            var localePath = _fileSystem.Path.Combine(contentPath, locale);
            if (!_fileSystem.Directory.Exists(localePath))
            {
                continue;
            }

            foreach (var file in _fileSystem.Directory.EnumerateFiles(localePath, _options.FilePattern, SearchOption.AllDirectories))
            {
                // ExcludePaths apply to the logical content tree, so match against the
                // path relative to the locale root — "changelog" in the default locale
                // also means "fr/changelog" in fr (without forcing users to list both).
                var localeRelative = _fileSystem.Path.GetRelativePath(localePath, file).Replace('\\', '/');
                if (IsRelativePathExcluded(localeRelative))
                {
                    continue;
                }

                results.Add((new FilePath(file), locale));
            }
        }

        return results;
    }

    /// <summary>
    /// Initial scan: discovers every markdown file, parses each one exactly once,
    /// and builds one <see cref="FileEntry"/> per source file (covering its own route plus
    /// any fallback locale routes the file generates). Runs at most once per service
    /// lifetime via <see cref="_seededLazy"/>; incremental edits go through <see cref="OnFileChanged"/>.
    /// </summary>
    private async Task<bool> SeedMetadataAsync()
    {
        var files = DiscoverFiles();

        // Precompute "which non-default-locale folders carry this relative path" so we
        // know whether a default-locale file needs a fallback route per locale. Building
        // this once up front is cheaper than consulting the dictionary mid-build (and
        // sidesteps the ordering issue where the default-locale file might be processed
        // before its non-default-locale siblings).
        var localizedPaths = BuildLocalizedPathLookup(files);
        bool HasLocalizedVersion(string locale, string relativePath) =>
            localizedPaths.TryGetValue(locale, out var set) && set.Contains(relativePath);

        foreach (var (file, locale) in files)
        {
            var parsed = await TryParseFrontMatterAsync(file.Value);
            if (parsed is null)
            {
                continue;
            }

            var entry = BuildEntry(file, locale, parsed, HasLocalizedVersion);
            lock (_cacheLock)
            {
                _entries[_fileSystem.Path.GetFullPath(file.Value)] = entry;
            }
        }

        return true;
    }

    private Dictionary<string, HashSet<string>> BuildLocalizedPathLookup(List<(FilePath File, string Locale)> files)
    {
        var lookup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        if (!_localization.IsMultiLocale)
        {
            return lookup;
        }

        foreach (var (file, locale) in files)
        {
            if (string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var contentRoot = GetContentRootForLocale(locale);
            var relativePath = _fileSystem.Path.GetRelativePath(contentRoot, file.Value).Replace('\\', '/');
            if (!lookup.TryGetValue(locale, out var set))
            {
                lookup[locale] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            set.Add(relativePath);
        }
        return lookup;
    }

    /// <summary>True when the file's path ends with the <c>.llms.md</c> suffix.</summary>
    private static bool IsLlmsOnlyFile(FilePath file) =>
        file.Value.EndsWith(LlmsOnlyFileSuffix, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public async Task<ImmutableList<LlmsSubtree>> GetLlmsSubtreesAsync()
    {
        var folderMetadata = await SnapshotFolderMetadataAsync();
        var builder = ImmutableList.CreateBuilder<LlmsSubtree>();
        foreach (var md in folderMetadata)
        {
            if (md.LlmsDescription is null || string.IsNullOrWhiteSpace(md.Title))
            {
                continue;
            }

            builder.Add(new LlmsSubtree(md.FolderUrlPrefix, md.Title, md.LlmsDescription));
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public Task<ImmutableList<FolderMetadata>> GetFolderMetadataAsync() => SnapshotFolderMetadataAsync();

    /// <summary>
    /// Initial scan: walks every <c>_meta.yml</c> under the content root and populates
    /// <see cref="_folderMetadataByFile"/>. Incremental edits go through <see cref="OnFileChanged"/>.
    /// </summary>
    private async Task<bool> SeedFolderMetadataAsync()
    {
        if (!_fileSystem.Directory.Exists(_absoluteContentPath))
        {
            return true;
        }

        foreach (var file in _fileSystem.Directory.EnumerateFiles(
            _absoluteContentPath, FolderMetadataSidecarFileName, SearchOption.AllDirectories))
        {
            var metadata = await TryReadFolderMetadataAsync(file);
            if (metadata is null)
            {
                continue;
            }

            lock (_cacheLock)
            {
                _folderMetadataByFile[_fileSystem.Path.GetFullPath(file)] = metadata;
            }
        }

        return true;
    }

    /// <summary>
    /// Reads and parses one <c>_meta.yml</c> sidecar into a <see cref="FolderMetadata"/>.
    /// Returns <c>null</c> when the file can't be read or parsed.
    /// </summary>
    private async Task<FolderMetadata?> TryReadFolderMetadataAsync(string file)
    {
        string content;
        try
        {
            content = await _fileSystem.File.ReadAllTextAsync(file);
        }
        catch
        {
            return null;
        }

        return BuildFolderMetadata(file, content);
    }

    /// <summary>Synchronous variant of <see cref="TryReadFolderMetadataAsync"/> for use inside <see cref="OnFileChanged"/>.</summary>
    private FolderMetadata? TryReadFolderMetadata(string file)
    {
        string content;
        try
        {
            content = _fileSystem.File.ReadAllText(file);
        }
        catch
        {
            return null;
        }

        return BuildFolderMetadata(file, content);
    }

    private FolderMetadata? BuildFolderMetadata(string file, string yamlContent)
    {
        FolderMetadataSidecar? sidecar;
        try
        {
            sidecar = YamlSerializer.Deserialize<FolderMetadataSidecar>(yamlContent, PenningtonYaml.ReflectionOptions);
        }
        catch
        {
            return null;
        }

        if (sidecar is null)
        {
            return null;
        }

        var basePrefix = NormalizeBasePageUrl(_options.BasePageUrl.Value);
        var folder = _fileSystem.Path.GetDirectoryName(file) ?? _absoluteContentPath;
        var relativeFolder = _fileSystem.Path
            .GetRelativePath(_absoluteContentPath, folder)
            .Replace('\\', '/')
            .Trim('/');

        // GetRelativePath returns "." when folder == base; treat that as the content root.
        if (relativeFolder == ".")
        {
            relativeFolder = "";
        }

        var prefix = relativeFolder.Length == 0
            ? basePrefix
            : basePrefix + relativeFolder + "/";

        return new FolderMetadata(
            FolderUrlPrefix: prefix,
            Title: string.IsNullOrWhiteSpace(sidecar.Title) ? null : sidecar.Title,
            Order: sidecar.Order,
            LlmsDescription: sidecar.Llms?.Description);
    }

    /// <summary>Awaits seeding, takes the cache lock briefly, and flattens <see cref="_entries"/> into the legacy (Route, FrontMatter, IsLlmsOnly) tuple list.</summary>
    private async Task<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter, bool IsLlmsOnly)>> SnapshotMetadataAsync()
    {
        await _seededLazy;
        lock (_cacheLock)
        {
            var builder = ImmutableList.CreateBuilder<(ContentRoute, TFrontMatter, bool)>();
            foreach (var entry in _entries.Values)
            {
                foreach (var (route, isLlmsOnly) in entry.Routes)
                {
                    builder.Add((route, entry.FrontMatter, isLlmsOnly));
                }
            }
            return builder.ToImmutable();
        }
    }

    /// <summary>Awaits seeding, takes the cache lock briefly, and snapshots the folder-metadata dictionary.</summary>
    private async Task<ImmutableList<FolderMetadata>> SnapshotFolderMetadataAsync()
    {
        await _folderSeededLazy;
        lock (_cacheLock)
        {
            return _folderMetadataByFile.Values.ToImmutableList();
        }
    }

    /// <summary>Parses front matter from a single file. Returns <c>null</c> when the file can't be read or parsed.</summary>
    private async Task<TFrontMatter?> TryParseFrontMatterAsync(string file)
    {
        try
        {
            var content = await _fileSystem.File.ReadAllTextAsync(file);
            var parsed = _parser.Parse<TFrontMatter>(content, file);
            return parsed.Metadata ?? new TFrontMatter();
        }
        catch
        {
            return default;
        }
    }

    /// <summary>Synchronous variant of <see cref="TryParseFrontMatterAsync"/> for use inside <see cref="OnFileChanged"/>.</summary>
    private TFrontMatter? TryParseFrontMatter(string file)
    {
        try
        {
            var content = _fileSystem.File.ReadAllText(file);
            var parsed = _parser.Parse<TFrontMatter>(content, file);
            return parsed.Metadata ?? new TFrontMatter();
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Builds a <see cref="FileEntry"/> from a parsed front matter — own route plus any
    /// fallback routes the file generates in non-default locales that don't have their own
    /// version (per the supplied predicate).
    /// </summary>
    private FileEntry BuildEntry(
        FilePath file,
        string locale,
        TFrontMatter frontMatter,
        Func<string, string, bool> hasLocalizedVersion)
    {
        var isLlmsOnly = IsLlmsOnlyFile(file);
        var routes = ImmutableList.CreateBuilder<(ContentRoute, bool)>();
        foreach (var route in RoutesForFile(file, locale, hasLocalizedVersion))
        {
            routes.Add((route, isLlmsOnly));
        }
        return new FileEntry(routes.ToImmutable(), frontMatter);
    }

    /// <summary>
    /// Yields the routes one source file generates: its own route, plus — for default-locale
    /// files in multi-locale mode — a fallback route per non-default locale that doesn't
    /// have its own copy of the file (as judged by <paramref name="hasLocalizedVersion"/>).
    /// </summary>
    private IEnumerable<ContentRoute> RoutesForFile(
        FilePath file,
        string locale,
        Func<string, string, bool> hasLocalizedVersion)
    {
        yield return CreateRouteForFile(file, locale);

        if (!_localization.IsMultiLocale)
        {
            yield break;
        }

        var isDefaultLocale = string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase);
        if (!isDefaultLocale)
        {
            yield break;
        }

        var contentRoot = GetContentRootForLocale(locale);
        var relativePath = _fileSystem.Path.GetRelativePath(contentRoot, file.Value).Replace('\\', '/');

        foreach (var otherLocale in GetNonDefaultLocaleSubfolders())
        {
            if (hasLocalizedVersion(otherLocale, relativePath))
            {
                continue;
            }

            var fallbackRoute = ContentRouteFactory.FromMarkdownFile(
                RoutePathFor(file), new FilePath(_absoluteContentPath), _options.BasePageUrl, otherLocale);
            fallbackRoute = fallbackRoute with { IsFallback = true };
            if (IsLlmsOnlyFile(file))
            {
                fallbackRoute = fallbackRoute with { SourceFile = file };
            }

            yield return fallbackRoute;
        }
    }

    /// <summary>
    /// "Does the non-default locale <paramref name="otherLocale"/> have its own copy of
    /// the relative-path <paramref name="relativePath"/>?" predicate that consults the
    /// live <see cref="_entries"/> dictionary. Caller must already hold <see cref="_cacheLock"/>.
    /// </summary>
    private bool HasLocalizedVersionFromCache(string otherLocale, string relativePath)
    {
        var localizedPath = _fileSystem.Path.GetFullPath(
            _fileSystem.Path.Combine(_absoluteContentPath, otherLocale, relativePath));
        return _entries.ContainsKey(localizedPath);
    }

    /// <summary>
    /// Determines the locale of a file by checking whether the first relative-path segment
    /// matches a configured locale subfolder. Returns the default locale otherwise.
    /// </summary>
    private string LocaleForPath(string absolutePath)
    {
        if (!_localization.IsMultiLocale)
        {
            return _options.Locale;
        }

        var relativePath = _fileSystem.Path
            .GetRelativePath(_absoluteContentPath, absolutePath)
            .Replace('\\', '/');
        var firstSegment = relativePath.Split('/')[0];

        foreach (var locale in _localization.Locales.Keys)
        {
            if (string.Equals(firstSegment, locale, StringComparison.OrdinalIgnoreCase))
            {
                return locale;
            }
        }
        return _localization.DefaultLocale;
    }

    private static string NormalizeBasePageUrl(string raw)
    {
        var trimmed = (raw ?? "").Trim('/');
        return trimmed.Length == 0 ? "/" : "/" + trimmed + "/";
    }

    private sealed class FolderMetadataSidecar
    {
        public string? Title { get; set; }
        public int? Order { get; set; }
        public LlmsBlock? Llms { get; set; }

        public sealed class LlmsBlock
        {
            public string? Description { get; set; }
        }
    }

    /// <summary>Returns locale codes for non-default locales when multi-locale is configured.</summary>
    private List<string> GetNonDefaultLocaleSubfolders()
    {
        if (!_localization.IsMultiLocale)
        {
            return [];
        }

        return _localization.Locales.Keys
            .Where(l => !string.Equals(l, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Creates a ContentRoute for a discovered file, handling default-locale tagging.
    /// Default locale files get no URL prefix but are tagged with the locale code.
    /// For <c>*.llms.md</c> files the canonical slug has the <c>.llms</c> marker
    /// stripped so the sidecar URL is <c>/_llms/{slug}.md</c>, not <c>/_llms/{slug}.llms.md</c>.
    /// </summary>
    private ContentRoute CreateRouteForFile(FilePath file, string locale)
    {
        var contentRoot = new FilePath(GetContentRootForLocale(locale));
        var isDefaultLocale = _localization.IsMultiLocale
            && string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase);
        var route = ContentRouteFactory.FromMarkdownFile(
            RoutePathFor(file), contentRoot, _options.BasePageUrl, isDefaultLocale ? "" : locale);
        if (isDefaultLocale)
        {
            route = route with { Locale = locale };
        }

        if (IsLlmsOnlyFile(file))
        {
            route = route with { SourceFile = file };
        }

        return route;
    }

    /// <summary>
    /// Returns the path the routing factory should use for slug computation.
    /// For <c>*.llms.md</c> files this strips the <c>.llms</c> marker so the
    /// canonical URL matches the bare slug. The factory only inspects the path
    /// lexically, so the virtual <c>.md</c> path doesn't need to exist on disk.
    /// </summary>
    private static FilePath RoutePathFor(FilePath file) =>
        IsLlmsOnlyFile(file)
            ? new FilePath(file.Value[..^LlmsOnlyFileSuffix.Length] + ".md")
            : file;

    /// <summary>
    /// Produces routes for all discovered files plus fallback entries for missing locale
    /// content. Used by <see cref="ParseContentAsync"/>; the metadata cache (built by
    /// <see cref="SeedMetadataAsync"/>) goes through the same <see cref="RoutesForFile"/>
    /// helper to keep the two channels consistent.
    /// </summary>
    private IEnumerable<(ContentRoute Route, FilePath SourceFile)> DiscoverRoutesWithFallbacks()
    {
        var files = DiscoverFiles();
        var localizedPaths = BuildLocalizedPathLookup(files);
        bool HasLocalizedVersion(string locale, string relativePath) =>
            localizedPaths.TryGetValue(locale, out var set) && set.Contains(relativePath);

        foreach (var (file, locale) in files)
        {
            foreach (var route in RoutesForFile(file, locale, HasLocalizedVersion))
            {
                yield return (route, file);
            }
        }
    }

    /// <summary>Returns the content root path for a given locale.</summary>
    private string GetContentRootForLocale(string locale)
    {
        if (!_localization.IsMultiLocale
            || string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            return _absoluteContentPath;
        }

        return _fileSystem.Path.Combine(_absoluteContentPath, locale);
    }

    private static bool IsInLocaleSubfolder(string relativePath, List<string> localeSubfolders)
    {
        if (localeSubfolders.Count == 0)
        {
            return false;
        }

        var firstSegment = relativePath.Split('/')[0];
        return localeSubfolders.Any(l => string.Equals(firstSegment, l, StringComparison.OrdinalIgnoreCase));
    }
}