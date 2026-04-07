namespace Penn.Content;

using System.Collections.Immutable;
using System.IO.Abstractions;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Discovers and provides markdown content from a directory.
/// When <see cref="LocalizationOptions"/> has multiple locales, also discovers
/// content from locale subdirectories (e.g., Content/fr/, Content/de/).
/// </summary>
public sealed class MarkdownContentService<TFrontMatter> : IContentService
    where TFrontMatter : IFrontMatter, new()
{
    private readonly MarkdownContentServiceOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly IFileSystem _fileSystem;
    private readonly LocalizationOptions _localization;
    private readonly string _absoluteContentPath;
    private readonly AsyncLazy<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter)>> _metadataLazy;

    public MarkdownContentService(
        MarkdownContentServiceOptions options,
        FrontMatterParser parser,
        IFileSystem fileSystem,
        IFileWatcher fileWatcher,
        LocalizationOptions localization)
    {
        _options = options;
        _parser = parser;
        _fileSystem = fileSystem;
        _localization = localization;
        _absoluteContentPath = _fileSystem.Path.GetFullPath(options.ContentPath.Value);
        _metadataLazy = new AsyncLazy<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter)>>(LoadMetadataAsync);

        // Register content directory for watching so the central watcher
        // knows about it. The FileWatchDependencyFactory handles instance
        // recreation — no manual cache invalidation needed here.
        fileWatcher.AddPathWatch(_absoluteContentPath, "*.*", (_, _) => { });
    }

    public string DefaultSection => _options.Section ?? "";
    public int SearchPriority => _options.SearchPriority;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var (route, sourceFile) in DiscoverRoutesWithFallbacks())
        {
            yield return new DiscoveredItem(route, new MarkdownFileSource(sourceFile));
        }

        await Task.CompletedTask; // Satisfy async requirement
    }

    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var metadata = await _metadataLazy.Value;
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();

        foreach (var (route, fm) in metadata)
        {
            if (fm is IDraftable { IsDraft: true }) continue;

            var order = fm is IOrderable orderable ? orderable.Order : int.MaxValue;
            var section = fm is ISectionable sectionable ? sectionable.Section : _options.Section;
            var hierarchyParts = route.CanonicalPath.Value
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            builder.Add(new ContentTocItem(
                Title: fm.Title,
                Route: route,
                Order: order,
                HierarchyParts: hierarchyParts,
                Section: section ?? DefaultSection,
                Locale: string.IsNullOrEmpty(route.Locale) ? null : route.Locale
            ));
        }

        return builder.ToImmutable();
    }

    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var metadata = await _metadataLazy.Value;
        var builder = ImmutableList.CreateBuilder<CrossReference>();

        foreach (var (route, fm) in metadata)
        {
            if (fm is ICrossReferenceable { Uid: { } uid } && !string.IsNullOrEmpty(uid))
            {
                builder.Add(new CrossReference(uid, fm.Title, route));
            }
        }

        return builder.ToImmutable();
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
    {
        var contentPath = _absoluteContentPath;
        if (!_fileSystem.Directory.Exists(contentPath))
            return Task.FromResult(ImmutableList<ContentToCopy>.Empty);

        var builder = ImmutableList.CreateBuilder<ContentToCopy>();
        var excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".md", ".mdx", ".razor", ".yml", ".yaml" };
        var localeSubfolders = GetNonDefaultLocaleSubfolders();

        foreach (var file in _fileSystem.Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories))
        {
            var ext = _fileSystem.Path.GetExtension(file);
            if (excludedExtensions.Contains(ext)) continue;

            var relativePath = _fileSystem.Path.GetRelativePath(contentPath, file).Replace('\\', '/');

            // Skip locale subfolders from the default locale scan — they get their own entries below
            if (IsInLocaleSubfolder(relativePath, localeSubfolders)) continue;

            builder.Add(new ContentToCopy(new FilePath(file), new FilePath(relativePath)));
        }

        // Also enumerate static files from non-default locale subdirectories
        foreach (var locale in localeSubfolders)
        {
            var localePath = _fileSystem.Path.Combine(contentPath, locale);
            if (!_fileSystem.Directory.Exists(localePath)) continue;

            foreach (var file in _fileSystem.Directory.EnumerateFiles(localePath, "*.*", SearchOption.AllDirectories))
            {
                var ext = _fileSystem.Path.GetExtension(file);
                if (excludedExtensions.Contains(ext)) continue;

                var relativeToLocale = _fileSystem.Path.GetRelativePath(localePath, file).Replace('\\', '/');
                // Output at /{locale}/relative/path so they serve at the locale-prefixed URL
                builder.Add(new ContentToCopy(new FilePath(file), new FilePath($"{locale}/{relativeToLocale}")));
            }
        }

        return Task.FromResult(builder.ToImmutable());
    }

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
            return [];

        var localeSubfolders = GetNonDefaultLocaleSubfolders();
        var results = new List<(FilePath, string)>();

        // Default locale: enumerate base path, excluding locale subdirectories
        foreach (var file in _fileSystem.Directory.EnumerateFiles(contentPath, _options.FilePattern, SearchOption.AllDirectories))
        {
            var relativePath = _fileSystem.Path.GetRelativePath(contentPath, file).Replace('\\', '/');
            if (IsInLocaleSubfolder(relativePath, localeSubfolders)) continue;

            var locale = _localization.IsMultiLocale ? _localization.DefaultLocale : _options.Locale;
            results.Add((new FilePath(file), locale));
        }

        // Non-default locales: enumerate each locale subdirectory
        foreach (var locale in localeSubfolders)
        {
            var localePath = _fileSystem.Path.Combine(contentPath, locale);
            if (!_fileSystem.Directory.Exists(localePath)) continue;

            foreach (var file in _fileSystem.Directory.EnumerateFiles(localePath, _options.FilePattern, SearchOption.AllDirectories))
            {
                results.Add((new FilePath(file), locale));
            }
        }

        return results;
    }

    private async Task<ImmutableList<(ContentRoute Route, TFrontMatter FrontMatter)>> LoadMetadataAsync()
    {
        var builder = ImmutableList.CreateBuilder<(ContentRoute, TFrontMatter)>();

        foreach (var (route, sourceFile) in DiscoverRoutesWithFallbacks())
        {
            try
            {
                var content = await _fileSystem.File.ReadAllTextAsync(sourceFile.Value);
                var parsed = _parser.Parse<TFrontMatter>(content);
                var fm = parsed.Metadata ?? new TFrontMatter();
                builder.Add((route, fm));
            }
            catch
            {
                // Skip files that can't be parsed for metadata
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>Returns locale codes for non-default locales when multi-locale is configured.</summary>
    private List<string> GetNonDefaultLocaleSubfolders()
    {
        if (!_localization.IsMultiLocale) return [];
        return _localization.Locales.Keys
            .Where(l => !string.Equals(l, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Creates a ContentRoute for a discovered file, handling default-locale tagging.
    /// Default locale files get no URL prefix but are tagged with the locale code.
    /// </summary>
    private ContentRoute CreateRouteForFile(FilePath file, string locale)
    {
        var contentRoot = new FilePath(GetContentRootForLocale(locale));
        var isDefaultLocale = _localization.IsMultiLocale
            && string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase);
        var route = ContentRouteFactory.FromMarkdownFile(
            file, contentRoot, _options.BasePageUrl, isDefaultLocale ? "" : locale);
        if (isDefaultLocale)
            route = route with { Locale = locale };
        return route;
    }

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

            if (!_localization.IsMultiLocale) continue;

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

        if (!_localization.IsMultiLocale) yield break;

        foreach (var locale in GetNonDefaultLocaleSubfolders())
        {
            var existing = nonDefaultLocaleRelPaths.GetValueOrDefault(locale)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (file, relativePath) in defaultLocaleFiles)
            {
                if (existing.Contains(relativePath)) continue;

                var route = ContentRouteFactory.FromMarkdownFile(
                    file, new FilePath(_absoluteContentPath), _options.BasePageUrl, locale);
                route = route with { IsFallback = true };
                yield return (route, file);
            }
        }
    }

    /// <summary>Returns the content root path for a given locale.</summary>
    private string GetContentRootForLocale(string locale)
    {
        if (!_localization.IsMultiLocale
            || string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
            return _absoluteContentPath;

        return _fileSystem.Path.Combine(_absoluteContentPath, locale);
    }

    private static bool IsInLocaleSubfolder(string relativePath, List<string> localeSubfolders)
    {
        if (localeSubfolders.Count == 0) return false;
        var firstSegment = relativePath.Split('/')[0];
        return localeSubfolders.Any(l => string.Equals(firstSegment, l, StringComparison.OrdinalIgnoreCase));
    }
}
