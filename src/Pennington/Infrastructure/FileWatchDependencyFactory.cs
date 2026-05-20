namespace Pennington.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ============================================================================
// DI Lifetime Audit (Pennington / DocSite / BlogSite)  [FR-6, 2026-05]
// ============================================================================
// Three categories drive the chosen lifetime:
//
//   FILE-READING   - Reads files directly. Use AddFileWatched<T> so a watcher
//                    event drops the instance and the next access rebuilds.
//                    AsyncLazy<> inside is the canonical one-time-init shim;
//                    it is naturally invalidated when the instance dies.
//
//   COMPOSING      - Wires/dispatches other services. Trust the deps; do not
//                    cache derived state. Transient (or singleton if stateless).
//                    When a composer DOES cache aggregated results pulled from
//                    IEnumerable<IContentService>, file-watch lifetime is the
//                    correct way to keep the cache fresh.
//
//   PURE TRANSFORM - Stateless input -> output. Singleton (cheap to share) or
//                    transient. No caching at this layer; let callers cache.
//
// ----------------------------------------------------------------------------
// Pennington core (PenningtonExtensions.AddPennington)
// ----------------------------------------------------------------------------
// FILE-READING:
//   MarkdownContentService<TFm>      singleton + internal AddPathWatch
//                                    [open-generic; can't use AddFileWatched]
//   RedirectContentService           singleton + internal SubscribeToChanges
//                                    [aliased as both concrete + IContentService]
//   MarkdownLinkResolver             AddFileWatched<>
//   XrefResolver                     AddFileWatched<>
//   SearchIndexService               AddFileWatched<>
//   SitemapService                   AddFileWatched<>
//   LlmsTxtService                   AddFileWatched<>
//   NavigationBuilder                AddFileWatched<>
//                                    [memo cache is per-instance; dropped on file change]
//
// COMPOSING:
//   HighlightingService              singleton (dispatches highlighters)
//   CodeBlockRenderingService        transient
//   MarkdownContentRenderer          transient (IContentRenderer)
//   ContentPipeline                  transient (IContentPipeline)
//   XrefResolvingService             transient
//   HtmlResponseRewritingProcessor   transient (carries IHtmlResponseRewriter list)
//   HttpDispatcher                   singleton (IInProcessHttpDispatcher)
//   RenderedHtmlFetcher              singleton
//   PenningtonStringLocalizerFactory singleton
//   OutputGenerationService          transient (build orchestrator)
//   LlmsTxtContentService            transient (wraps file-watched LlmsTxtService)
//   ContentResolver (DocSite)        transient
//   BlogContentResolver              AddFileWatched<>
//                                    [aggregates IContentService results - file-watched
//                                     lifetime keeps the post cache fresh]
//   BlogSiteContentService           AddFileWatched<>
//
// PURE TRANSFORM:
//   FrontMatterParser                singleton
//   TextMateLanguageRegistry         singleton
//   TextMateHighlighter              singleton (ICodeHighlighter)
//   ShellHighlighter                 singleton (ICodeHighlighter)
//   PlainTextHighlighter             constructed inside HighlightingService
//   MarkdownPipeline                 singleton (Markdig pipeline factory)
//   MarkdownContentParser<TFm>       transient (IContentParser, parses one file)
//   XrefHtmlRewriter                 transient
//   LocaleLinkHtmlRewriter           singleton
//   BaseUrlHtmlRewriter              singleton
//   BaseUrlCssResponseProcessor      singleton
//   LiveReloadScriptProcessor        singleton
//   DiagnosticOverlayProcessor       singleton
//   AuditDiagnosticProcessor         singleton (reads from AuditCache)
//   OverlapAuditor / XrefAuditor     transient (IBuildAuditor)
//   LinkAuditor                      transient (IRenderedAuditor)
//   SitemapBuilder / RssFeedBuilder  singleton (wraps CanonicalBaseUrl)
//   SearchIndexBuilder               singleton
//   CanonicalBaseUrl                 singleton (record)
//
// INFRASTRUCTURE / STATE-CONTAINER:
//   IFileSystem (RealFileSystem)     singleton
//   IFileWatcher (FileWatcher)       singleton
//   AuditCache                       singleton
//   LiveReloadServer                 singleton (subscribes to IFileWatcher)
//   AuditRunner                      hosted (subscribes to IFileWatcher)
//
// REQUEST-SCOPED:
//   DiagnosticContext                scoped
//   LocaleContext                    scoped
//
// OPTIONS / DATA RECORDS (singletons):
//   PenningtonOptions, LocalizationOptions, OutputOptions,
//   FrontMatterParserOptions, SearchIndexOptions, LlmsTxtOptions,
//   TranslationOptions, DocSiteOptions, BlogSiteOptions
//
// ----------------------------------------------------------------------------
// Notable design decisions:
//
// - RazorPageContentService is plain singleton with Lazy<> internals and no
//   file-watch subscription. @page routes are compile-time (a .razor edit
//   recompiles + restarts the host), so there's no live filesystem state to
//   refresh; sidecar metadata reloads on app restart only.
//
// - MarkdownContentService<TFm> can't use AddFileWatched<T> - it is open
//   generic over TFrontMatter and registered per AddMarkdownContent call with
//   a per-source ContentPath. Its internal AddPathWatch resets the AsyncLazy
//   fields on disk-scoped change, which is functionally equivalent to
//   instance replacement.
//
// - NavigationBuilder, XrefResolver, MarkdownLinkResolver, BlogContentResolver
//   are nominally COMPOSERS (they aggregate IContentService results) but
//   register as AddFileWatched because their aggregations are non-trivial to
//   recompute per call. The factory drop is the eviction signal.
//
// - HighlightingService (singleton) carries a ConcurrentDictionary of
//   "seen unknown languages" for FR-18 once-per-process Info dedup. This is
//   intentional process-lifetime state, not a file-derived cache.
//
// - When a composing service captures a file-watched dep (e.g. transient
//   IHtmlResponseRewriter pulling XrefResolver), the composer must be transient
//   so each resolution re-resolves the current file-watched instance. A
//   singleton composer would pin the first XrefResolver and never refresh.
//   See FileWatchedServiceExtensions for the AddTransient indirection that
//   gives every consumer the latest factory-managed instance.
// ============================================================================

/// <summary>
/// Manages a cached service instance that auto-invalidates when watched files change.
/// </summary>
public sealed class FileWatchDependencyFactory<T> : IFileWatchAware, IDisposable where T : class, IFileWatchAware
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private T? _instance;
    private readonly Lock _lock = new();

    /// <summary>Initializes the factory. <see cref="FileWatchDispatcher"/> drives invalidation.</summary>
    public FileWatchDependencyFactory(IServiceProvider serviceProvider, ILogger<FileWatchDependencyFactory<T>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>Returns the cached instance, constructing one via DI on first access.</summary>
    public T GetInstance()
    {
        lock (_lock)
        {
            if (_instance is not null)
            {
                return _instance;
            }

            _logger.LogDebug("Creating new instance of {Type}", typeof(T).Name);
            _instance = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            return _instance;
        }
    }

    /// <summary>
    /// Consults the current instance (if one has been built) and drops it when the instance
    /// asks to be recreated. Called by <see cref="FileWatchDispatcher"/>.
    /// </summary>
    public FileWatchResponse OnFileChanged(FileChangeNotification change)
    {
        lock (_lock)
        {
            if (_instance is null)
            {
                return FileWatchResponse.Ignore;
            }

            var response = _instance.OnFileChanged(change);
            if (response == FileWatchResponse.Recreate)
            {
                InvalidateInstance();
            }
            return response;
        }
    }

    /// <summary>Disposes the current instance so the next <see cref="GetInstance"/> builds a fresh one. Caller holds <see cref="_lock"/>.</summary>
    private void InvalidateInstance()
    {
        if (_instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _instance = null;
        _logger.LogDebug("Invalidated instance of {Type}", typeof(T).Name);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _instance = null;
        }
    }
}