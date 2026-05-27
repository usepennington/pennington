namespace Pennington.Pipeline;

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Dom;
using Content;
using Infrastructure;
using LlmsTxt;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Routing;
using Search;

/// <summary>
/// Default <see cref="ISiteProjection"/>. Walks
/// <see cref="ContentServiceExtensions.CollectIndexableEntriesAsync"/> plus any
/// <c>WithLlmsTxtEntry</c> endpoint registrations, fetches post-pipeline HTML
/// for each route in parallel via <see cref="RenderedHtmlFetcher"/>, and folds
/// every result into a stable index-keyed array (deterministic ordering for
/// snapshot tests). Llms-only items have no HTTP route, so the projection
/// renders them in-process via <see cref="IContentRenderer"/> +
/// <see cref="XrefResolvingService"/> instead.
/// <para>
/// Registered as <see cref="IFileWatchAware"/> with
/// <see cref="FileWatchResponse.Refreshed"/> — file-watch invalidation marks just the
/// affected routes stale (via <see cref="IContentService.GetAffectedRoutes"/>) so the
/// next access re-fetches only those, reusing the cached HTML for everything else. A
/// <see cref="ContentChangeImpact.Wildcard"/> report (rename, folder-metadata edit)
/// falls back to a full rebuild.
/// </para>
/// </summary>
public sealed class SiteProjection : IFileWatchAware, ISiteProjection
{
    private readonly IEnumerable<IContentService> _contentServices;
    private readonly MetadataEnrichmentService _enrichment;
    private readonly IContentRenderer _renderer;
    private readonly XrefResolvingService _xrefResolver;
    private readonly RenderedHtmlFetcher _fetcher;
    private readonly HeadingSectionExtractor _extractor;
    private readonly SiteProjectionOptions _options;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly ILogger<SiteProjection> _logger;

    private readonly Lock _lock = new();
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private readonly AsyncLazy<bool> _seededLazy;

    // Cache state. Reads of _pages take the lock (or accept a possibly-stale snapshot)
    // since the immutable array reference is replaced atomically. Writes always under _lock.
    private ImmutableArray<RenderedPage> _pages = ImmutableArray<RenderedPage>.Empty;
    private FrozenDictionary<string, int> _routeIndex = FrozenDictionary<string, int>.Empty;

    // Pending stale set accumulated since last refresh. Protected by _lock.
    private readonly HashSet<string> _staleRoutes = new(StringComparer.OrdinalIgnoreCase);
    private bool _staleAll;

    /// <summary>Creates the projection; the corpus is materialized lazily on first access.</summary>
    public SiteProjection(
        IEnumerable<IContentService> contentServices,
        MetadataEnrichmentService enrichment,
        IContentRenderer renderer,
        XrefResolvingService xrefResolver,
        RenderedHtmlFetcher fetcher,
        HeadingSectionExtractor extractor,
        SiteProjectionOptions options,
        EndpointDataSource endpointDataSource,
        ILogger<SiteProjection> logger)
    {
        _contentServices = contentServices;
        _enrichment = enrichment;
        _renderer = renderer;
        _xrefResolver = xrefResolver;
        _fetcher = fetcher;
        _extractor = extractor;
        _options = options;
        _endpointDataSource = endpointDataSource;
        _logger = logger;
        _seededLazy = new AsyncLazy<bool>(SeedAsync);
    }

    /// <inheritdoc/>
    public FileWatchResponse OnFileChanged(FileChangeNotification change)
    {
        var wildcard = false;
        List<string>? affected = null;

        foreach (var service in _contentServices)
        {
            var routes = service.GetAffectedRoutes(change).AffectedRoutes;
            if (routes is null)
            {
                wildcard = true;
                break;
            }
            foreach (var route in routes.Value)
            {
                (affected ??= []).Add(Normalize(route.CanonicalPath.Value));
            }
        }

        if (!wildcard && (affected is null || affected.Count == 0))
        {
            return FileWatchResponse.Refreshed;
        }

        lock (_lock)
        {
            if (wildcard)
            {
                _staleAll = true;
                _staleRoutes.Clear();
            }
            else if (!_staleAll && affected is not null)
            {
                foreach (var key in affected)
                {
                    _staleRoutes.Add(key);
                }
            }
        }
        return FileWatchResponse.Refreshed;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<RenderedPage> GetPagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await _seededLazy;
        await RefreshIfStaleAsync(cancellationToken);

        ImmutableArray<RenderedPage> snapshot;
        lock (_lock)
        {
            snapshot = _pages;
        }
        foreach (var page in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return page;
        }
    }

    /// <inheritdoc/>
    public async Task<RenderedPage?> GetPageAsync(UrlPath canonicalPath, CancellationToken cancellationToken = default)
    {
        await _seededLazy;
        await RefreshIfStaleAsync(cancellationToken);

        var key = Normalize(canonicalPath.Value);
        ImmutableArray<RenderedPage> snapshot;
        FrozenDictionary<string, int> index;
        lock (_lock)
        {
            snapshot = _pages;
            index = _routeIndex;
        }
        return index.TryGetValue(key, out var i) ? snapshot[i] : null;
    }

    private async Task<bool> SeedAsync()
    {
        var (pages, routeIndex) = await BuildOrRefreshAsync(
            reusable: ImmutableDictionary<string, RenderedPage>.Empty,
            staleRoutes: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            staleAll: true,
            CancellationToken.None);

        lock (_lock)
        {
            _pages = pages;
            _routeIndex = routeIndex;
        }
        return true;
    }

    private async Task RefreshIfStaleAsync(CancellationToken cancellationToken)
    {
        // Cheap pre-check without taking the gate.
        bool hasWork;
        lock (_lock)
        {
            hasWork = _staleAll || _staleRoutes.Count > 0;
        }
        if (!hasWork) return;

        await _refreshGate.WaitAsync(cancellationToken);
        try
        {
            HashSet<string> staleRoutes;
            bool staleAll;
            ImmutableDictionary<string, RenderedPage> reusable;
            lock (_lock)
            {
                staleAll = _staleAll;
                staleRoutes = new HashSet<string>(_staleRoutes, StringComparer.OrdinalIgnoreCase);
                _staleAll = false;
                _staleRoutes.Clear();
                if (!staleAll && staleRoutes.Count == 0)
                {
                    return;
                }
                // Snapshot the current cache so the build can reuse unchanged entries.
                var builder = ImmutableDictionary.CreateBuilder<string, RenderedPage>(StringComparer.OrdinalIgnoreCase);
                foreach (var (key, idx) in _routeIndex)
                {
                    builder[key] = _pages[idx];
                }
                reusable = builder.ToImmutable();
            }

            var (pages, routeIndex) = await BuildOrRefreshAsync(reusable, staleRoutes, staleAll, cancellationToken);

            lock (_lock)
            {
                _pages = pages;
                _routeIndex = routeIndex;
            }
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private async Task<(ImmutableArray<RenderedPage> Pages, FrozenDictionary<string, int> Index)> BuildOrRefreshAsync(
        ImmutableDictionary<string, RenderedPage> reusable,
        IReadOnlySet<string> staleRoutes,
        bool staleAll,
        CancellationToken cancellationToken)
    {
        // 1) Build the ParsedItem map so MarkdownOrigin pages carry enriched front-matter
        //    and derived metadata, and so Llms-only items can be rendered in-process.
        var parsedByPath = new Dictionary<string, ParsedItem>(StringComparer.OrdinalIgnoreCase);
        var parseStart = Stopwatch.GetTimestamp();
        await foreach (var parsed in _contentServices.ParseAllContentAsync(cancellationToken))
        {
            var enriched = await _enrichment.EnrichAsync(parsed);
            parsedByPath[Normalize(enriched.Route.CanonicalPath.Value)] = enriched;
        }
        var parseElapsed = Stopwatch.GetElapsedTime(parseStart);

        // 2) Source-type map to distinguish LlmsOnlySource (in-process render) from regular markdown.
        var sourceByPath = new Dictionary<string, ContentSource>(StringComparer.OrdinalIgnoreCase);
        var discoverStart = Stopwatch.GetTimestamp();
        await foreach (var discovered in _contentServices.DiscoverAllAsync(cancellationToken))
        {
            sourceByPath[Normalize(discovered.Route.CanonicalPath.Value)] = discovered.Source;
        }
        var discoverElapsed = Stopwatch.GetElapsedTime(discoverStart);

        // 3) Renderable TOC entries.
        var tocStart = Stopwatch.GetTimestamp();
        var tocItems = await _contentServices.CollectIndexableEntriesAsync();
        var tocElapsed = Stopwatch.GetElapsedTime(tocStart);

        // 4) Endpoint entries opted in via WithLlmsTxtEntry.
        var endpointEntries = CollectEndpointEntries(_endpointDataSource).ToList();

        var total = tocItems.Count + endpointEntries.Count;
        var results = new RenderedPage?[total];

        var refreshed = 0;
        var reused = 0;

        var renderStart = Stopwatch.GetTimestamp();
        await Parallel.ForEachAsync(
            Enumerable.Range(0, tocItems.Count),
            new ParallelOptions { CancellationToken = cancellationToken },
            async (i, ct) =>
            {
                var toc = tocItems[i];
                var key = Normalize(toc.Route.CanonicalPath.Value);

                if (!staleAll
                    && !staleRoutes.Contains(key)
                    && reusable.TryGetValue(key, out var cached))
                {
                    // Carry forward the cached render. Swap in the freshly-collected TOC so
                    // title/order edits propagate even when the rendered HTML is unchanged.
                    results[i] = cached with { Toc = toc };
                    Interlocked.Increment(ref reused);
                    return;
                }

                Interlocked.Increment(ref refreshed);
                results[i] = await RenderOneAsync(toc, key, parsedByPath, sourceByPath, ct);
            });

        for (var j = 0; j < endpointEntries.Count; j++)
        {
            var (toc, url) = endpointEntries[j];
            var key = Normalize(toc.Route.CanonicalPath.Value);
            if (!staleAll && !staleRoutes.Contains(key) && reusable.TryGetValue(key, out var cached))
            {
                results[tocItems.Count + j] = cached with { Toc = toc };
                reused++;
                continue;
            }

            refreshed++;
            results[tocItems.Count + j] = new RenderedPage(
                Route: toc.Route,
                Toc: toc,
                Origin: new EndpointOrigin(url),
                Html: "",
                Content: null,
                Sections: new Lazy<IReadOnlyList<HeadingSection>>(() => []));
        }

        var renderElapsed = Stopwatch.GetElapsedTime(renderStart);

        if (staleAll || staleRoutes.Count > 0 || reusable.Count != tocItems.Count + endpointEntries.Count)
        {
            _logger.LogDebug(
                "SiteProjection refresh: {Refreshed} refreshed, {Reused} reused "
                + "(parse {ParseMs:F1}ms, discover {DiscoverMs:F1}ms, toc {TocMs:F1}ms, render {RenderMs:F1}ms, "
                + "staleAll={StaleAll}, stale={StaleCount}, prevSize={Prev}, newSize={New})",
                refreshed, reused,
                parseElapsed.TotalMilliseconds, discoverElapsed.TotalMilliseconds,
                tocElapsed.TotalMilliseconds, renderElapsed.TotalMilliseconds,
                staleAll, staleRoutes.Count, reusable.Count, total);
        }

        var pagesBuilder = ImmutableArray.CreateBuilder<RenderedPage>(total);
        var indexBuilder = new Dictionary<string, int>(total, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < total; i++)
        {
            if (results[i] is { } page)
            {
                var key = Normalize(page.Route.CanonicalPath.Value);
                if (indexBuilder.ContainsKey(key))
                {
                    // Defensive: deduplicate if two TOC items end up at the same canonical path.
                    continue;
                }
                indexBuilder[key] = pagesBuilder.Count;
                pagesBuilder.Add(page);
            }
        }

        return (pagesBuilder.ToImmutable(), indexBuilder.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));
    }

    private async Task<RenderedPage?> RenderOneAsync(
        ContentTocItem toc,
        string key,
        IReadOnlyDictionary<string, ParsedItem> parsedByPath,
        IReadOnlyDictionary<string, ContentSource> sourceByPath,
        CancellationToken ct)
    {
        sourceByPath.TryGetValue(key, out var source);
        try
        {
            // Llms-only items have a route but no HTTP page — render in-process.
            if (source is LlmsOnlySource && parsedByPath.TryGetValue(key, out var llmsParsed))
            {
                var rendered = await RenderInProcessAsync(_renderer, _xrefResolver, llmsParsed);
                if (rendered is null)
                {
                    _logger.LogWarning("SiteProjection: failed to render llms-only item {Path}", toc.Route.CanonicalPath.Value);
                    return null;
                }
                var (html, element) = rendered.Value;
                return new RenderedPage(
                    Route: toc.Route,
                    Toc: toc,
                    Origin: new MarkdownOrigin(llmsParsed),
                    Html: html,
                    Content: element,
                    Sections: BuildSectionsLazy(_extractor, element));
            }

            var fetched = await _fetcher.FetchContentAsync(toc.Route.CanonicalPath.Value, _options.ContentSelector, ct);
            if (fetched is null)
            {
                return null;
            }

            PageOrigin? origin = null;
            if (parsedByPath.TryGetValue(key, out var parsed))
            {
                origin = new MarkdownOrigin(parsed);
            }

            return new RenderedPage(
                Route: toc.Route,
                Toc: toc,
                Origin: origin,
                Html: fetched.OuterHtml,
                Content: fetched,
                Sections: BuildSectionsLazy(_extractor, fetched));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SiteProjection: failed to project {Path}, skipping", toc.Route.CanonicalPath.Value);
            return null;
        }
    }

    private static Lazy<IReadOnlyList<HeadingSection>> BuildSectionsLazy(
        HeadingSectionExtractor extractor, IElement element)
        => new(() => extractor.Extract(element, excludeCodeBlocks: true));

    private static async Task<(string Html, IElement Element)?> RenderInProcessAsync(
        IContentRenderer renderer, XrefResolvingService xrefResolver, ParsedItem parsed)
    {
        var result = await renderer.RenderAsync(parsed);
        if (result.Value is not RenderedItem rendered)
        {
            return null;
        }

        var resolved = await xrefResolver.ResolveAsync(rendered.Content.Html);

        // Per-page browsing context — IBrowsingContext is not safe for concurrent OpenAsync.
        var browsingContext = BrowsingContext.New(Configuration.Default);
        var document = await browsingContext.OpenAsync(req => req.Content(resolved));
        var element = document.Body;
        if (element is null)
        {
            return null;
        }

        return (resolved, element);
    }

    /// <summary>
    /// Walks the registered endpoints looking for routes opted into llms.txt
    /// via <see cref="LlmsTxtEndpointExtensions.WithLlmsTxtEntry{TBuilder}"/>
    /// and yields synthetic TOC items keyed off the endpoint URL. Skips
    /// endpoints whose URL contains a route parameter — the projection needs a
    /// concrete URL, not a pattern.
    /// </summary>
    private static IEnumerable<(ContentTocItem Toc, string Url)> CollectEndpointEntries(EndpointDataSource endpointDataSource)
    {
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint route)
            {
                continue;
            }

            var meta = route.Metadata.GetMetadata<LlmsTxtEntryMetadata>();
            if (meta is null)
            {
                continue;
            }

            var rawText = route.RoutePattern.RawText;
            if (string.IsNullOrWhiteSpace(rawText) || rawText.Contains('{'))
            {
                continue;
            }

            var url = rawText.StartsWith('/') ? rawText : "/" + rawText;
            var canonicalPath = new UrlPath(url);
            var contentRoute = new ContentRoute
            {
                CanonicalPath = canonicalPath,
                OutputFile = new FilePath(url.TrimStart('/')),
            };
            var hierarchyParts = url.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            yield return (
                new ContentTocItem(
                    Title: meta.Title,
                    Route: contentRoute,
                    Order: int.MaxValue,
                    HierarchyParts: hierarchyParts,
                    SectionLabel: null,
                    Locale: null)
                {
                    Description = meta.Description,
                },
                url);
        }
    }

    private static string Normalize(string canonicalPath) => canonicalPath.Trim('/');
}
