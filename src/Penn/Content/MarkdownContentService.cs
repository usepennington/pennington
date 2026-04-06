namespace Penn.Content;

using System.Collections.Immutable;
using System.IO.Abstractions;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.Pipeline;
using Penn.Routing;

/// <summary>
/// Discovers and provides markdown content from a directory.
/// </summary>
public sealed class MarkdownContentService<TFrontMatter> : IContentService
    where TFrontMatter : IFrontMatter, new()
{
    private readonly MarkdownContentServiceOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly IFileSystem _fileSystem;
    private readonly string _absoluteContentPath;
    private List<(ContentRoute Route, TFrontMatter FrontMatter)>? _cachedMetadata;

    public MarkdownContentService(MarkdownContentServiceOptions options, FrontMatterParser parser, IFileSystem fileSystem, IFileWatcher fileWatcher)
    {
        _options = options;
        _parser = parser;
        _fileSystem = fileSystem;
        _absoluteContentPath = _fileSystem.Path.GetFullPath(options.ContentPath.Value);

        // Watch markdown content directory for changes to enable hot reload
        fileWatcher.AddPathWatch(
            _absoluteContentPath,
            "*.*",
            (_, _) => _cachedMetadata = null);
    }

    public string DefaultSection => _options.Section ?? "";
    public int SearchPriority => _options.SearchPriority;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        var files = DiscoverFiles();
        foreach (var file in files)
        {
            var route = ContentRouteFactory.FromMarkdownFile(
                file, new FilePath(_absoluteContentPath), _options.BasePageUrl, _options.Locale);
            var source = new ContentSource(new MarkdownFileSource(file));
            yield return new DiscoveredItem(route, source);
        }

        await Task.CompletedTask; // Satisfy async requirement
    }

    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var metadata = await GetCachedMetadataAsync();
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
                Locale: string.IsNullOrEmpty(_options.Locale) ? null : _options.Locale
            ));
        }

        return builder.ToImmutable();
    }

    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var metadata = await GetCachedMetadataAsync();
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

        foreach (var file in _fileSystem.Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories))
        {
            var ext = _fileSystem.Path.GetExtension(file);
            if (excludedExtensions.Contains(ext)) continue;

            var relativePath = _fileSystem.Path.GetRelativePath(contentPath, file).Replace('\\', '/');
            builder.Add(new ContentToCopy(new FilePath(file), new FilePath(relativePath)));
        }

        return Task.FromResult(builder.ToImmutable());
    }

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    private List<FilePath> DiscoverFiles()
    {
        var contentPath = _absoluteContentPath;
        if (!_fileSystem.Directory.Exists(contentPath))
            return [];

        return _fileSystem.Directory.EnumerateFiles(contentPath, _options.FilePattern, SearchOption.AllDirectories)
            .Select(f => new FilePath(f))
            .ToList();
    }

    private async Task<List<(ContentRoute Route, TFrontMatter FrontMatter)>> GetCachedMetadataAsync()
    {
        if (_cachedMetadata != null) return _cachedMetadata;

        var result = new List<(ContentRoute, TFrontMatter)>();

        foreach (var file in DiscoverFiles())
        {
            var route = ContentRouteFactory.FromMarkdownFile(
                file, new FilePath(_absoluteContentPath), _options.BasePageUrl, _options.Locale);

            try
            {
                var content = await _fileSystem.File.ReadAllTextAsync(file.Value);
                var parsed = _parser.Parse<TFrontMatter>(content);
                var fm = parsed.Metadata ?? new TFrontMatter();
                result.Add((route, fm));
            }
            catch
            {
                // Skip files that can't be parsed for metadata
            }
        }

        _cachedMetadata = result;
        return result;
    }
}
