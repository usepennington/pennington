namespace Pennington.Content;

using System.Collections.Immutable;
using System.IO.Abstractions;
using FrontMatter;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Pipeline;
using Routing;

/// <summary>
/// Generic file-format content source: globs <see cref="FileContentServiceOptions.FilePattern"/>
/// under the content directory, parses each file's front matter into <typeparamref name="TFrontMatter"/>,
/// and emits <see cref="FileSource"/> discovered items tagged with the format key. Registered by
/// <c>AddContentFormat</c>; the format's registered parser/renderer turn the bodies into HTML.
/// Deliberately leaner than <see cref="MarkdownContentService{T}"/> — no locale fan-out, no
/// <c>.llms.md</c> handling, no <c>_meta.yml</c> folder metadata.
/// </summary>
/// <typeparam name="TFrontMatter">Front-matter type parsed from each file.</typeparam>
public sealed class FileContentService<TFrontMatter> : IContentService, IFileWatchAware
    where TFrontMatter : IFrontMatter, new()
{
    private readonly FileContentServiceOptions _options;
    private readonly FrontMatterParser _parser;
    private readonly IFileSystem _fileSystem;
    private readonly TimeProvider _clock;
    private readonly ILogger<FileContentService<TFrontMatter>>? _logger;
    private readonly string _absoluteContentPath;
    private readonly FileWatchScope _watchScope;
    private readonly Lock _gate = new();
    private AsyncLazy<ImmutableList<Entry>> _cache;

    private sealed record Entry(ContentRoute Route, TFrontMatter FrontMatter, string Body);

    /// <summary>Creates the service and prepares lazy discovery of the content directory.</summary>
    public FileContentService(
        FileContentServiceOptions options,
        FrontMatterParser parser,
        IFileSystem fileSystem,
        TimeProvider? clock = null,
        ILogger<FileContentService<TFrontMatter>>? logger = null)
    {
        _options = options;
        _parser = parser;
        _fileSystem = fileSystem;
        _clock = clock ?? TimeProvider.System;
        _logger = logger;
        _absoluteContentPath = _fileSystem.Path.GetFullPath(options.ContentPath.Value);
        _watchScope = new FileWatchScope(_absoluteContentPath, "*.*", IncludeSubdirectories: true);
        _cache = new AsyncLazy<ImmutableList<Entry>>(LoadAsync);
    }

    /// <inheritdoc/>
    public string DefaultSectionLabel => _options.SectionLabel ?? "";

    /// <inheritdoc/>
    public int SearchPriority => _options.SearchPriority;

    /// <inheritdoc/>
    public IReadOnlyList<FileWatchScope> WatchScopes => [_watchScope];

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change)
    {
        if (!_watchScope.Matches(change))
        {
            return FileWatchResponse.Ignore;
        }

        lock (_gate)
        {
            _cache = new AsyncLazy<ImmutableList<Entry>>(LoadAsync);
        }

        return FileWatchResponse.Refreshed;
    }

    /// <inheritdoc/>
    public ContentChangeImpact GetAffectedRoutes(FileChangeNotification change)
        => _watchScope.Matches(change) ? ContentChangeImpact.Wildcard : ContentChangeImpact.None;

    /// <inheritdoc/>
    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var entry in await SnapshotAsync())
        {
            if (entry.FrontMatter.IsHiddenFromBuild(_clock))
            {
                continue;
            }

            if (entry.FrontMatter is IRedirectable { RedirectUrl: { Length: > 0 } target })
            {
                yield return new DiscoveredItem(entry.Route, new RedirectSource(new UrlPath(target)));
                continue;
            }

            yield return new DiscoveredItem(entry.Route, new FileSource(entry.Route.SourceFile!.Value, _options.Format))
            {
                Metadata = entry.FrontMatter,
            };
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ParsedItem> ParseContentAsync()
    {
        foreach (var entry in await SnapshotAsync())
        {
            if (entry.FrontMatter.IsHiddenFromBuild(_clock)
                || entry.FrontMatter is IRedirectable { RedirectUrl: { Length: > 0 } })
            {
                continue;
            }

            yield return new ParsedItem(entry.Route, entry.FrontMatter, entry.Body) { Format = _options.Format };
        }
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var builder = ImmutableList.CreateBuilder<ContentTocItem>();
        foreach (var entry in await SnapshotAsync())
        {
            if (BuildTocItem(entry.Route, entry.FrontMatter) is { } toc)
            {
                builder.Add(toc);
            }
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        var builder = ImmutableList.CreateBuilder<CrossReference>();
        foreach (var entry in await SnapshotAsync())
        {
            if (entry.FrontMatter.Uid is { Length: > 0 } uid)
            {
                builder.Add(new CrossReference(uid, entry.FrontMatter.Title, entry.Route));
            }
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public async Task<ImmutableList<DiscoveredItem>> GetRedirectSourcesAsync()
    {
        var builder = ImmutableList.CreateBuilder<DiscoveredItem>();
        foreach (var entry in await SnapshotAsync())
        {
            if (!entry.FrontMatter.IsHiddenFromBuild(_clock)
                && entry.FrontMatter is IRedirectable { RedirectUrl: { Length: > 0 } target })
            {
                builder.Add(new DiscoveredItem(entry.Route, new RedirectSource(new UrlPath(target))));
            }
        }

        return builder.ToImmutable();
    }

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
        => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    /// <inheritdoc/>
    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
        => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    private ContentTocItem? BuildTocItem(ContentRoute route, TFrontMatter fm)
    {
        if (fm.IsHiddenFromBuild(_clock)
            || fm is IRedirectable { RedirectUrl: { Length: > 0 } }
            || string.IsNullOrWhiteSpace(fm.Title))
        {
            return null;
        }

        var order = fm is IOrderable orderable ? orderable.Order : int.MaxValue;
        var sectionLabel = fm is ISectionable sectionable ? sectionable.SectionLabel : _options.SectionLabel;
        var hierarchyParts = route.CanonicalPath.Value.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        return new ContentTocItem(
            Title: fm.Title,
            Route: route,
            Order: order,
            HierarchyParts: hierarchyParts,
            SectionLabel: sectionLabel ?? DefaultSectionLabel,
            Locale: null)
        {
            Description = fm.Description,
            Tags = fm is ITaggable taggable ? taggable.Tags : [],
            ExcludeFromSearch = !fm.Search,
            ExcludeFromLlms = !fm.Llms,
            SearchOnly = fm.SearchOnly,
        };
    }

    private Task<ImmutableList<Entry>> SnapshotAsync()
    {
        lock (_gate)
        {
            return _cache.Task;
        }
    }

    private async Task<ImmutableList<Entry>> LoadAsync()
    {
        var builder = ImmutableList.CreateBuilder<Entry>();
        if (!_fileSystem.Directory.Exists(_absoluteContentPath))
        {
            return builder.ToImmutable();
        }

        var excludes = _options.ExcludePaths
            .Select(p => p.Replace('\\', '/').Trim('/'))
            .Where(p => p.Length > 0)
            .ToImmutableArray();

        foreach (var file in _fileSystem.Directory.EnumerateFiles(
                     _absoluteContentPath, _options.FilePattern, SearchOption.AllDirectories))
        {
            var relative = _fileSystem.Path.GetRelativePath(_absoluteContentPath, file).Replace('\\', '/');
            if (IsExcluded(relative, excludes))
            {
                continue;
            }

            string content;
            try
            {
                content = await _fileSystem.File.ReadAllTextAsync(file);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "FileContentService: failed to read {File}", file);
                continue;
            }

            var result = _parser.Parse<TFrontMatter>(content, file);
            var frontMatter = result.Metadata ?? new TFrontMatter();
            var route = ContentRouteFactory.FromMarkdownFile(
                new FilePath(file), new FilePath(_absoluteContentPath), _options.BasePageUrl);
            builder.Add(new Entry(route, frontMatter, result.Body));
        }

        return builder.ToImmutable();
    }

    private static bool IsExcluded(string relativePath, ImmutableArray<string> excludes)
    {
        foreach (var exclude in excludes)
        {
            if (relativePath.Equals(exclude, StringComparison.OrdinalIgnoreCase)
                || relativePath.StartsWith(exclude + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
