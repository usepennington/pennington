namespace Pennington.Content;

using System.Collections.Immutable;
using System.IO.Abstractions;
using FrontMatter;
using Infrastructure;
using LlmsTxt;
using Pipeline;
using Routing;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    : IContentService, IMarkdownContentSource, ILlmsSubtreeProvider, IFileWatchAware
    where TFrontMatter : IFrontMatter, new()
{
    /// <summary>Sidecar filename that, when dropped at any folder under the content root, declares that folder as an llms.txt subtree.</summary>
    public const string LlmsSubtreeSidecarFileName = "_llms.yaml";

    /// <summary>File-name suffix that marks a markdown file as llms-only — emitted to the llms.txt sidecar but never as an HTML page.</summary>
    public const string LlmsOnlyFileSuffix = ".llms.md";

    private readonly MarkdownContentServiceOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly IFileSystem _fileSystem;
    private readonly LocalizationOptions _localization;
    private readonly TimeProvider _clock;
    private readonly string _absoluteContentPath;
    private readonly ImmutableArray<string> _normalizedExcludePaths;
    private readonly FileWatchScope _watchScope;
    private AsyncLazy<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter, bool IsLlmsOnly)>> _metadataLazy;
    private AsyncLazy<ImmutableList<LlmsSubtree>> _subtreesLazy;

    /// <summary>
    /// Initializes the service and prepares lazy metadata loading. <see cref="FileWatchDispatcher"/>
    /// watches the content directory and drives cache invalidation through <see cref="OnFileChanged"/>.
    /// </summary>
    public MarkdownContentService(
        MarkdownContentServiceOptions options,
        FrontMatterParser parser,
        IFileSystem fileSystem,
        LocalizationOptions localization,
        TimeProvider? clock = null)
    {
        _options = options;
        _parser = parser;
        _fileSystem = fileSystem;
        _localization = localization;
        _clock = clock ?? TimeProvider.System;
        _absoluteContentPath = _fileSystem.Path.GetFullPath(options.ContentPath.Value);
        _normalizedExcludePaths = NormalizeExcludePaths(options.ExcludePaths);
        _watchScope = new FileWatchScope(_absoluteContentPath, "*.*", IncludeSubdirectories: true);
        _metadataLazy = new AsyncLazy<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter, bool IsLlmsOnly)>>(LoadMetadataAsync);
        _subtreesLazy = new AsyncLazy<ImmutableList<LlmsSubtree>>(LoadSubtreesAsync);
    }

    /// <inheritdoc/>
    public IReadOnlyList<FileWatchScope> WatchScopes => [_watchScope];

    /// <summary>
    /// Resets the discovery caches when a file under this source's content directory changes, so
    /// TOC and discovery results pick up new, renamed, and deleted files. Changes elsewhere are
    /// ignored.
    /// </summary>
    public FileWatchResponse OnFileChanged(FileChangeNotification change)
    {
        if (!_watchScope.Matches(change))
        {
            return FileWatchResponse.Ignore;
        }

        _metadataLazy = new AsyncLazy<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter, bool IsLlmsOnly)>>(LoadMetadataAsync);
        _subtreesLazy = new AsyncLazy<ImmutableList<LlmsSubtree>>(LoadSubtreesAsync);
        return FileWatchResponse.Refreshed;
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
        foreach (var (route, sourceFile) in DiscoverRoutesWithFallbacks())
        {
            TFrontMatter? frontMatter = default;
            try
            {
                var content = await _fileSystem.File.ReadAllTextAsync(sourceFile.Value);
                var parsed = _parser.Parse<TFrontMatter>(content, sourceFile.Value);
                if (parsed.Metadata is { } metadata && metadata.IsHiddenFromBuild(_clock))
                {
                    continue;
                }

                frontMatter = parsed.Metadata;
            }
            catch
            {
                // If front matter can't be parsed, include the file as a markdown source.
            }

            // Pages with RedirectUrl front matter are surfaced as RedirectSource so the
            // unified redirect middleware handles them at dev time and the static build
            // crawler captures the 301 response (written as a meta-refresh HTML file).
            if (frontMatter is IRedirectable { RedirectUrl: { Length: > 0 } redirectTarget })
            {
                yield return new DiscoveredItem(route, new RedirectSource(new UrlPath(redirectTarget)));
                continue;
            }

            if (IsLlmsOnlyFile(sourceFile))
            {
                yield return new DiscoveredItem(route, new LlmsOnlySource(sourceFile)) { Metadata = frontMatter };
                continue;
            }

            yield return new DiscoveredItem(route, new MarkdownFileSource(sourceFile)) { Metadata = frontMatter };
        }
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var metadata = await _metadataLazy.Value;
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
        var metadata = await _metadataLazy.Value;
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
            ExcludeFromSearch = excludeFromSearch,
            ExcludeFromLlms = excludeFromLlms,
            SearchOnly = fm.SearchOnly,
        };
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<DiscoveredItem>> GetRedirectSourcesAsync()
    {
        var metadata = await _metadataLazy.Value;
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
        var metadata = await _metadataLazy.Value;
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

    private async Task<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter, bool IsLlmsOnly)>> LoadMetadataAsync()
    {
        var builder = ImmutableList.CreateBuilder<(ContentRoute, TFrontMatter, bool)>();

        foreach (var (route, sourceFile) in DiscoverRoutesWithFallbacks())
        {
            try
            {
                var content = await _fileSystem.File.ReadAllTextAsync(sourceFile.Value);
                var parsed = _parser.Parse<TFrontMatter>(content, sourceFile.Value);
                var fm = parsed.Metadata ?? new TFrontMatter();
                builder.Add((route, fm, IsLlmsOnlyFile(sourceFile)));
            }
            catch
            {
                // Skip files that can't be parsed for metadata
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>True when the file's path ends with the <c>.llms.md</c> suffix.</summary>
    private static bool IsLlmsOnlyFile(FilePath file) =>
        file.Value.EndsWith(LlmsOnlyFileSuffix, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public Task<ImmutableList<LlmsSubtree>> GetLlmsSubtreesAsync() => _subtreesLazy.Value;

    private async Task<ImmutableList<LlmsSubtree>> LoadSubtreesAsync()
    {
        if (!_fileSystem.Directory.Exists(_absoluteContentPath))
        {
            return ImmutableList<LlmsSubtree>.Empty;
        }

        var basePrefix = NormalizeBasePageUrl(_options.BasePageUrl.Value);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var builder = ImmutableList.CreateBuilder<LlmsSubtree>();

        foreach (var file in _fileSystem.Directory.EnumerateFiles(
            _absoluteContentPath, LlmsSubtreeSidecarFileName, SearchOption.AllDirectories))
        {
            string content;
            try
            {
                content = await _fileSystem.File.ReadAllTextAsync(file);
            }
            catch
            {
                continue;
            }

            LlmsSubtreeSidecar? sidecar;
            try
            {
                sidecar = deserializer.Deserialize<LlmsSubtreeSidecar?>(content);
            }
            catch
            {
                continue;
            }

            if (sidecar is null || string.IsNullOrWhiteSpace(sidecar.Title))
            {
                continue;
            }

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

            builder.Add(new LlmsSubtree(prefix, sidecar.Title, sidecar.Description ?? ""));
        }

        return builder.ToImmutable();
    }

    private static string NormalizeBasePageUrl(string raw)
    {
        var trimmed = (raw ?? "").Trim('/');
        return trimmed.Length == 0 ? "/" : "/" + trimmed + "/";
    }

    private sealed class LlmsSubtreeSidecar
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
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
    /// Produces routes for all discovered files plus fallback entries for missing locale content.
    /// Fallback entries point to default-locale source files and have <see cref="ContentRoute.IsFallback"/> set.
    /// </summary>
    private IEnumerable<(ContentRoute Route, FilePath SourceFile)> DiscoverRoutesWithFallbacks()
    {
        var files = DiscoverFiles();
        var defaultLocaleFiles = new List<(FilePath File, string RelativePath)>();
        var nonDefaultLocaleRelPaths = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (file, locale) in files)
        {
            var route = CreateRouteForFile(file, locale);
            yield return (route, file);

            if (!_localization.IsMultiLocale)
            {
                continue;
            }

            var contentRoot = GetContentRootForLocale(locale);
            var relativePath = _fileSystem.Path.GetRelativePath(contentRoot, file.Value).Replace('\\', '/');
            var isDefaultLocale = string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase);

            if (isDefaultLocale)
            {
                defaultLocaleFiles.Add((file, relativePath));
            }
            else
            {
                if (!nonDefaultLocaleRelPaths.TryGetValue(locale, out var paths))
                {
                    paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    nonDefaultLocaleRelPaths[locale] = paths;
                }
                paths.Add(relativePath);
            }
        }

        if (!_localization.IsMultiLocale)
        {
            yield break;
        }

        foreach (var locale in GetNonDefaultLocaleSubfolders())
        {
            var existing = nonDefaultLocaleRelPaths.GetValueOrDefault(locale)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (file, relativePath) in defaultLocaleFiles)
            {
                if (existing.Contains(relativePath))
                {
                    continue;
                }

                var route = ContentRouteFactory.FromMarkdownFile(
                    RoutePathFor(file), new FilePath(_absoluteContentPath), _options.BasePageUrl, locale);
                route = route with { IsFallback = true };
                if (IsLlmsOnlyFile(file))
                {
                    route = route with { SourceFile = file };
                }

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