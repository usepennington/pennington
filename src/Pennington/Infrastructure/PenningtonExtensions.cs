namespace Pennington.Infrastructure;

using System.CommandLine;
using System.IO.Abstractions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Artifacts;
using Cli;
using Cli.Diag;
using Content;
using Favicon;
using Feeds;
using FrontMatter;
using Generation;
using Head;
using Highlighting;
using LlmsTxt;
using Localization;
using Markdig;
using Markdown;
using Markdown.Extensions;
using Markdown.Shortcodes;
using Mdazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Navigation;
using Pipeline;
using Routing;
using Search;
using SharpYaml.Serialization;
using SocialCards;
using StandardSite;
using Testably.Abstractions;

/// <summary>
/// DI and pipeline extensions that wire Pennington services into an ASP.NET Core host.
/// </summary>
public static class PenningtonExtensions
{
    private const string LocaleRoutingKey = "Pennington.LocaleRoutingAdded";

    /// <summary>Register all Pennington services.</summary>
    public static IServiceCollection AddPennington(this IServiceCollection services, Action<PenningtonOptions> configure)
    {
        var options = new PenningtonOptions();

        // Seed framework-default English translations before the user callback
        // runs so any `opts.Add("en", "pennington.notfound.title", ...)` call
        // inside ConfigurePennington takes precedence. Non-English translations
        // for these keys are the consumer's responsibility (TranslationOptions).
        options.Translations.Add("en", "pennington.notfound.title", "Not Found");
        options.Translations.Add("en", "pennington.notfound.body", "Page not found.");

        configure(options);

        // Register options
        services.AddSingleton(options);
        services.AddSingleton(options.Localization);

        // Mdazor: registers IComponentRegistry + Razor HtmlRenderer so Markdig can
        // inline-render components referenced from markdown. Consumers register
        // specific components with services.AddMdazorComponent<T>().
        services.AddMdazor();

        // Register output options from CLI args
        var args = Environment.GetCommandLineArgs();
        var outputOptions = OutputOptions.FromArgs(args.Length > 1 ? args[1..] : []);
        services.AddSingleton(outputOptions);

        // Build and diag run in-process: swap Kestrel for TestServer so the crawler dispatches
        // through the same middleware with no socket bind, dev-cert prompt, or random-port race.
        // Last-registered IServer wins, overriding the Kestrel registration CreateBuilder() adds.
        if (PenningtonCli.Current.IsHeadlessOneShot)
        {
            services.AddSingleton<IServer, TestServer>();
        }

        // One-shot commands (build/diag) and help/version requests print to stdout and exit, so the
        // host's lifetime chatter and dev-level Pennington logs are muted (the custom formatter also
        // strips the "info: Category[0]" prefix so output reads like a CLI tool). Only a real build
        // surfaces its own progress at Information; diag, --help, and --version keep stdout clean —
        // the explicit Pennington filter overrides hosts whose appsettings elevate it (e.g. the docs
        // site sets Pennington to Trace).
        if (PenningtonCli.Current.IsHeadlessOneShot || PenningtonCli.Current.IsHelpOrVersion)
        {
            services.AddLogging(b =>
            {
                b.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                if (PenningtonCli.Current.WritesOutput && !PenningtonCli.Current.IsHelpOrVersion)
                {
                    b.AddFilter("Pennington", LogLevel.Information);
                }
                else
                {
                    b.SetMinimumLevel(LogLevel.Warning);
                    b.AddFilter("Pennington", LogLevel.Warning);
                }

                b.AddConsole(o => o.FormatterName = BuildConsoleFormatter.FormatterName);
                b.AddConsoleFormatter<BuildConsoleFormatter, ConsoleFormatterOptions>();
            });
        }
        else
        {
            // Default WebHost ShutdownTimeout is 5s. The live-reload WebSocket
            // (LiveReloadServer) holds connections open across the dev session,
            // so Kestrel waits the full window on every Ctrl-C / IDE terminate
            // before forcing the socket closed — a 5–10s hang. 500ms is plenty
            // for a dev host to drain; anything still in flight gets cancelled.
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromMilliseconds(500));

            // Dev server: bind a free port instead of Kestrel's default localhost:5000, so
            // multiple sites can run at once and nothing collides with whatever already squats
            // 5000. We only do this when the user hasn't picked a port themselves — an explicit
            // endpoint takes effect only with no others configured, so we skip it when one is set
            // (see ShouldUseEphemeralPort) to leave their choice untouched. IConfiguration is read
            // at options-materialization time, after the host config (and Kestrel's own config
            // binding) is fully assembled. Kestrel disallows ListenLocalhost(0) — it can't promise
            // the same dynamic port on both loopbacks — so we pick the free port up front and bind
            // localhost to it, keeping the one-port, both-loopback, HTTP shape :5000 had.
            services.AddOptions<KestrelServerOptions>()
                .Configure<IConfiguration>((kestrel, config) =>
                {
                    if (ShouldUseEphemeralPort(config))
                    {
                        kestrel.ListenLocalhost(FindFreePort());
                    }
                });
        }

        // Core services
        // Build mode defaults StrictUnknownKeys to true so typo'd keys fail the build.
        // The user wins if they already flipped it on; flip-down for build is rare and
        // can still be done by registering a replacement options instance after AddPennington.
        if (PenningtonCli.Current.WritesOutput && !options.FrontMatter.StrictUnknownKeys)
        {
            options.FrontMatter.StrictUnknownKeys = true;
        }

        services.AddSingleton(options.FrontMatter);

        // YAML deserialization: register the built-in source-generated context and the provider
        // that dispatches each type to its context (or reflection). Satellite packages and users
        // add their own contexts via AddYamlContext.
        services.AddYamlContext(PenningtonYamlContext.Default);
        services.AddSingleton<PenningtonYamlContextProvider>();

        services.AddSingleton<FrontMatterParser>();
        services.AddFileWatched<FolderMetadataRegistry>();
        services.AddFileWatched<NavigationBuilder>();

        // File system abstraction
        services.TryAddSingleton<IFileSystem>(new RealFileSystem());

        // Wall clock — scheduled-publish (front matter Date in the future) compares
        // against this. Tests inject FakeTimeProvider to make boundaries deterministic.
        services.TryAddSingleton(TimeProvider.System);

        // File watching
        services.AddSingleton<IFileWatcher, FileWatcher>();
        services.AddSingleton<FileWatchDispatcher>();

        // Highlighting: register TextMate and Shell highlighters, then the service
        services.AddSingleton<TextMateLanguageRegistry>();
        services.AddSingleton<ICodeHighlighter, TextMateHighlighter>(sp =>
            new TextMateHighlighter(sp.GetRequiredService<TextMateLanguageRegistry>()));
        services.AddSingleton<ICodeHighlighter, ShellHighlighter>();
        foreach (var highlighter in options.Highlighting.Highlighters)
        {
            services.AddSingleton(highlighter);
        }
        services.AddSingleton(sp =>
            new HighlightingService(
                sp.GetServices<ICodeHighlighter>(),
                sp.GetService<IHttpContextAccessor>()));

        // Shared pipeline for fenced code blocks — used by Markdig's CodeHighlightRenderer
        // and by the <CodeBlock> Razor component so both emit identical HTML.
        services.AddTransient(sp =>
            new CodeBlockRenderingService(
                sp.GetRequiredService<HighlightingService>(),
                sp.GetServices<ICodeBlockPreprocessor>()));

        // Pre-render shortcode expander. Handlers register as IShortcode via DI; the
        // expander is transient so each render picks up the live set. Two ship built-in:
        // AssemblyVersionShortcode stamps the host app's version with <?# Version /?>, and
        // PackageVersionShortcode stamps Pennington's own NuGet version with <?# PackageVersion /?>.
        services.AddTransient(sp =>
            new ShortcodeExpander(
                sp.GetServices<IShortcode>(),
                sp.GetService<IHttpContextAccessor>()));
        services.AddSingleton<IShortcode, AssemblyVersionShortcode>();
        services.AddSingleton<IShortcode, PackageVersionShortcode>();

        // Markdown pipeline — includes highlighting, tabs, custom alerts, Mdazor,
        // and any consumer-supplied extensions via options.ConfigureMarkdownPipeline.
        services.AddSingleton(sp =>
            MarkdownPipelineFactory.CreateWithExtensions(
                sp,
                sp.GetRequiredService<CodeBlockRenderingService>(),
                tabOptions: options.TabbedCodeBlockOptions,
                configure: options.ConfigureMarkdownPipeline));

        // Relative-link resolver — factory-managed so it rebuilds its source → URL
        // index when content files change. Registered under both the concrete type
        // (for the transient renderer below) and the factory type.
        services.AddFileWatched<MarkdownLinkResolver>();

        // Content-format dispatch registry: maps a format key ("markdown", "cook", …) to the parser
        // and renderer factories the dispatchers resolve at request time.
        var formatRegistry = new ContentFormatRegistry();
        services.AddSingleton(formatRegistry);

        // Markdown renderer registered as its concrete type; every markdown source's format key maps
        // to it below (the dispatching IContentRenderer is registered after the format loops below).
        services.AddTransient(sp =>
            new MarkdownContentRenderer(
                sp.GetRequiredService<MarkdownPipeline>(),
                sp.GetService<MarkdownLinkResolver>(),
                sp.GetService<IFileSystem>(),
                sp.GetService<ShortcodeExpander>()));

        // Register markdown content services for each configured source
        for (var sourceIndex = 0; sourceIndex < options.MarkdownSources.Count; sourceIndex++)
        {
            // Capture loop variables for closure
            var capturedSource = options.MarkdownSources[sourceIndex];

            // Distinct dispatch key per source so two markdown sources with different front-matter
            // types route to their own parser instead of the last-registered one clobbering the rest.
            var formatKey = MarkdownFormat.SourceKey(sourceIndex);

            // Register the content service — resolve content path at activation time
            // using IWebHostEnvironment.ContentRootPath so tests and hosts both work.
            var frontMatterType = capturedSource.FrontMatterType ?? typeof(DocFrontMatter);
            var serviceType = typeof(MarkdownContentService<>).MakeGenericType(frontMatterType);

            // One instance per source, shared between the IContentService and IFileWatchAware
            // registrations so FileWatchDispatcher refreshes the very service that serves content.
            object? instance = null;
            var gate = new object();
            object Resolve(IServiceProvider sp)
            {
                if (instance is not null)
                {
                    return instance;
                }

                lock (gate)
                {
                    if (instance is not null)
                    {
                        return instance;
                    }

                    var env = sp.GetService<IWebHostEnvironment>();
                    var resolvedContentPath = FilePath.ResolveAgainstRoot(capturedSource.ContentPath, env?.ContentRootPath);

                    var sourceOptions = new MarkdownContentServiceOptions
                    {
                        ContentPath = new FilePath(resolvedContentPath),
                        BasePageUrl = new UrlPath(capturedSource.BasePageUrl),
                        SectionLabel = capturedSource.SectionLabel,
                        ExcludePaths = capturedSource.ExcludePaths,
                        ReserveNotFoundPage = capturedSource.ReserveNotFoundPage,
                        Format = formatKey,
                    };

                    instance = ActivatorUtilities.CreateInstance(sp, serviceType, sourceOptions);
                }
                return instance;
            }

            services.AddSingleton(typeof(IContentService), Resolve);
            services.AddSingleton(typeof(IFileWatchAware), Resolve);

            // Register this source's parser under its distinct format key (closing over the source's
            // front-matter type) so DispatchingContentParser routes each source's files to the right
            // parser. The renderer is shared across every markdown source — they all render the same.
            var parserType = typeof(MarkdownContentParser<>).MakeGenericType(frontMatterType);
            formatRegistry.Register(formatKey,
                parser: sp => (IContentParser)Activator.CreateInstance(
                    parserType,
                    sp.GetRequiredService<FrontMatterParser>(),
                    sp.GetRequiredService<IFileSystem>())!,
                renderer: sp => sp.GetRequiredService<MarkdownContentRenderer>());
        }

        // Register Razor page content service for @page component discovery.
        // The entry assembly is always scanned so a bare host's routable @page
        // components (e.g. a lone @page "/" landing page) are crawled and emitted
        // without the consumer having to populate AdditionalRoutingAssemblies.
        // The DocSite/BlogSite templates already prepend the entry assembly, so
        // the dedup in ResolveRoutingAssemblies makes this a no-op for them.
        var routingAssemblies = ResolveRoutingAssemblies(
            options.AdditionalRoutingAssemblies, Assembly.GetEntryAssembly());
        if (routingAssemblies.Length > 0)
        {
            services.AddSingleton<IContentService>(sp =>
                new RazorPageContentService(
                    routingAssemblies,
                    sp.GetRequiredService<IFileSystem>(),
                    sp.GetRequiredService<FrontMatterParser>(),
                    sp.GetRequiredService<ILogger<RazorPageContentService>>(),
                    sp.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>()?.ContentRootPath,
                    sp.GetRequiredService<TimeProvider>()));
        }

        // Custom content formats (AddContentFormat): a FileContentService<T> per source for
        // discovery, plus the consumer's parser/renderer registered in the dispatch registry.
        foreach (var format in options.ContentFormats)
        {
            var capturedFormat = format;
            var formatFrontMatterType = capturedFormat.FrontMatterType ?? typeof(DocFrontMatter);
            var serviceType = typeof(FileContentService<>).MakeGenericType(formatFrontMatterType);

            // One instance per source, shared between the IContentService and IFileWatchAware
            // registrations so FileWatchDispatcher refreshes the very service that serves content.
            object? instance = null;
            var gate = new object();
            object ResolveFormatService(IServiceProvider sp)
            {
                if (instance is not null)
                {
                    return instance;
                }

                lock (gate)
                {
                    if (instance is not null)
                    {
                        return instance;
                    }

                    var env = sp.GetService<IWebHostEnvironment>();
                    var resolvedContentPath = FilePath.ResolveAgainstRoot(capturedFormat.ContentPath, env?.ContentRootPath);

                    var fileOptions = new FileContentServiceOptions
                    {
                        ContentPath = new FilePath(resolvedContentPath),
                        Format = capturedFormat.Format,
                        BasePageUrl = new UrlPath(capturedFormat.BasePageUrl),
                        FilePattern = capturedFormat.FilePattern,
                        SectionLabel = capturedFormat.SectionLabel,
                        ExcludePaths = capturedFormat.ExcludePaths,
                    };

                    instance = ActivatorUtilities.CreateInstance(sp, serviceType, fileOptions);
                }
                return instance;
            }

            services.AddSingleton(typeof(IContentService), ResolveFormatService);
            services.AddSingleton(typeof(IFileWatchAware), ResolveFormatService);

            // The consumer's parser/renderer are resolved from DI by the registry factories below.
            if (capturedFormat.ParserType is { } cfParserType)
            {
                services.TryAddTransient(cfParserType);
            }
            if (capturedFormat.RendererType is { } cfRendererType)
            {
                services.TryAddTransient(cfRendererType);
            }

            formatRegistry.Register(capturedFormat.Format,
                parser: sp => (IContentParser)sp.GetRequiredService(capturedFormat.ParserType!),
                renderer: sp => (IContentRenderer)sp.GetRequiredService(capturedFormat.RendererType!));
        }

        // Dispatchers are the single IContentParser/IContentRenderer the pipeline resolves; they
        // route each item to its format's registered parser/renderer. The renderer is always
        // registered (a bare host never produces a ParsedItem to render); the parser is gated so a
        // truly bare host leaves IContentParser unregistered, and PageResolver/ContentPipeline run
        // parser-less via their optional-parser constructors.
        services.AddTransient<IContentRenderer>(sp =>
            new DispatchingContentRenderer(sp.GetRequiredService<ContentFormatRegistry>(), sp));
        if (options.MarkdownSources.Count > 0 || options.ContentFormats.Count > 0)
        {
            services.AddTransient<IContentParser>(sp =>
                new DispatchingContentParser(sp.GetRequiredService<ContentFormatRegistry>(), sp));
        }

        // Pipeline
        services.AddTransient<IContentPipeline, ContentPipeline>();

        // Single-URL resolver — walks the content services and returns the rendered
        // page matching a requested route. Transient so each resolution snapshots the
        // current service set, mirroring the pipeline registration above.
        services.AddTransient<IPageResolver, PageResolver>();

        // Shared blog read model (post listings, pagination, single-post render, RSS) over the
        // cached record registry + page resolver. Transient so it captures the current file-watched
        // ContentRecordRegistry; holds no cache of its own.
        services.AddTransient<Content.BlogPostQuery>();

        // Unified redirect map (from _redirects.yml + per-page redirectUrl front matter).
        // Registered as both a concrete service (for the middleware) and an IContentService
        // (so the build crawler hits the YAML-defined redirect sources and captures 301s).
        services.AddSingleton<RedirectContentService>();
        services.AddSingleton<IContentService>(sp => sp.GetRequiredService<RedirectContentService>());

        // Content-root static assets. The runtime mount in UsePennington serves the entire content
        // root (ServeUnknownFileTypes = false), including shared folders that no markdown source
        // owns (e.g. Content/assets/). Surfacing them as an IContentService routes them through the
        // same CollectContentToCopyAsync path the build copy and both link auditors already consume,
        // so the static build copies them and the auditors treat them as known — no dev/build
        // divergence. Registered last so per-source asset outputs win the build's output-path dedup.
        services.AddSingleton<IContentService>(sp =>
        {
            var penn = sp.GetRequiredService<PenningtonOptions>();
            var env = sp.GetService<IWebHostEnvironment>();
            var contentRoot = FilePath.ResolveAgainstRoot(penn.ContentRootPath.Value, env?.ContentRootPath);
            return new ContentRootAssetService(contentRoot, sp.GetRequiredService<IFileSystem>());
        });

        // Xref resolution — factory-managed, recreated on file changes
        services.AddFileWatched<XrefResolver>();

        // Response processors. The XrefResolvingService -> XrefHtmlRewriter ->
        // HtmlResponseRewritingProcessor chain transitively depends on the
        // file-watched XrefResolver, so every link in the chain is transient:
        // the middleware resolves IEnumerable<IResponseProcessor> per request
        // via InvokeAsync parameter injection, which rebuilds the chain with
        // the current XrefResolver each time.
        services.AddTransient<XrefResolvingService>();
        services.AddTransient<IHtmlResponseRewriter, XrefHtmlRewriter>();
        services.AddSingleton<IHtmlResponseRewriter, LocaleLinkHtmlRewriter>();
        services.AddSingleton<IHtmlResponseRewriter, FallbackLangHtmlRewriter>();
        // Canonical and structured-data head tags are now IHeadContributors registered by AddHead()
        // below (replacing the former CanonicalLinkHtmlRewriter / StructuredDataHtmlRewriter).
        services.AddSingleton<IHtmlResponseRewriter>(sp =>
            new BaseUrlHtmlRewriter(sp.GetRequiredService<OutputOptions>()));
        // Head composition: one rewriter (Order 25) reconciles every IHeadContributor into <head>.
        // Inert until contributors are registered, so this leaves head output byte-identical.
        services.AddHead();

        // Standard Site (AT Protocol) verification surface — registered only when configured. The
        // well-known artifact service and head links no-op on blank config (fail-safe).
        if (options.StandardSite is { } standardSite)
        {
            services.AddSingleton(standardSite);
            services.AddSingleton<IDiagCommand, DiagStandardSiteCommand>();
            services.AddTransient<StandardSiteUriResolver>();
            services.AddTransient<IArtifactContentService, WellKnownArtifactService>();
            services.AddHeadContributor<StandardSiteHeadContributor>();
        }

        // Favicon / icon links — registered only when configured. The contributor no-ops on an empty
        // icon list, and root-relative hrefs are sub-path prefixed by BaseUrlHtmlRewriter. No artifact
        // service: the icon files are user-provided static assets already served by ContentRootAssetService.
        if (options.Favicons is { } favicons)
        {
            services.AddSingleton(favicons);
            services.AddHeadContributor<FaviconHeadContributor>();
        }
        // Transient: this processor holds the IHtmlResponseRewriter list, which
        // includes XrefHtmlRewriter capturing the file-watched XrefResolver. A
        // singleton would pin the first-resolved (stale) resolver and share one
        // IBrowsingContext across concurrent requests — see the chain comment above.
        services.AddTransient<IResponseProcessor, HtmlResponseRewritingProcessor>();
        services.AddSingleton<IResponseProcessor>(sp =>
            new BaseUrlCssResponseProcessor(sp.GetRequiredService<OutputOptions>()));
        services.AddSingleton<IResponseProcessor, LiveReloadScriptProcessor>();
        services.AddSingleton<IResponseProcessor, DiagnosticOverlayProcessor>();
        services.AddSingleton<IResponseProcessor, NotFoundStatusProcessor>();
        services.AddSingleton<IResponseProcessor, AuditDiagnosticProcessor>();
        // Per-page link verification in dev mode. Replaces the corpus-wide LinkAuditor
        // self-fetch flood with a one-shot check of the response HTML for the current page.
        services.AddFileWatched<PageLinkVerifier>();
        services.AddSingleton<IResponseProcessor, PageLinkAuditProcessor>();

        // Audit pipeline: AuditCache holds the latest snapshot from every IBuildAuditor;
        // AuditRunner refreshes it at startup and on file change. AuditDiagnosticProcessor
        // (registered above) feeds per-route entries into the dev overlay; the build report
        // copies the same snapshot in OutputGenerationService.
        services.AddSingleton<AuditCache>();
        services.AddSingleton<IAuditCache>(sp => sp.GetRequiredService<AuditCache>());
        // Registered as a resolvable singleton (not just AddHostedService<T>) so the diag CLI
        // can await its initial pass via WaitForInitialPassAsync() before reading the cache.
        services.AddSingleton<AuditRunner>();
        services.AddHostedService(sp => sp.GetRequiredService<AuditRunner>());
        services.AddTransient<IBuildAuditor, OverlapAuditor>();
        services.AddTransient<IBuildAuditor, XrefAuditor>();
        services.AddTransient<IBuildAuditor, ClaimConflictAuditor>();
        services.AddTransient<IRenderedAuditor, LinkAuditor>();

        // Diagnostic CLI (`diag <sub>`): each command is discovered by PenningtonCli.BuildDiagGroup
        // and run against the started host. Read-only inspection for humans and AI assistants.
        services.AddSingleton<IDiagCommand, DiagInfoCommand>();
        services.AddSingleton<IDiagCommand, DiagTocCommand>();
        services.AddSingleton<IDiagCommand, DiagRoutesCommand>();
        services.AddSingleton<IDiagCommand, DiagWarningsCommand>();
        services.AddSingleton<IDiagCommand, DiagTranslationCommand>();
        services.AddSingleton<IDiagCommand, DiagFrontMatterCommand>();
        services.AddSingleton<IDiagCommand, DiagLlmsCommand>();

        // Live reload
        services.AddSingleton<LiveReloadServer>();

        // Effective canonical base. When PenningtonOptions.CanonicalBaseUrl is
        // set (typically a fully-qualified https://host/sub URL) we respect it
        // verbatim — that's the correct form per sitemap / RSS protocol. When
        // it is not set but the static build is targeting a sub-path (e.g.
        // `build /sub/`), fall back to OutputOptions.BaseUrl so generators
        // still produce `/sub/page/` instead of root-relative `/page/`.
        // Crawlers resolve relative URLs against the sitemap.xml's own URL, so
        // this still produces a reachable target.
        var explicitCanonicalBase = options.CanonicalBaseUrl;
        services.AddSingleton(sp =>
        {
            var effectiveBase = !string.IsNullOrEmpty(explicitCanonicalBase)
                ? new UrlPath(explicitCanonicalBase)
                : sp.GetRequiredService<OutputOptions>().BaseUrl;
            return new CanonicalBaseUrl(effectiveBase);
        });
        services.AddSingleton(sp => new SitemapBuilder(
            sp.GetRequiredService<CanonicalBaseUrl>().Value,
            sp.GetRequiredService<TimeProvider>()));

        // Render-once cache for the in-process crawl: the disk-write pass and the
        // search/llms.txt sidecars all self-fetch the same pages, so this collapses
        // 2–3 full-pipeline renders per URL down to one. Registered twice on purpose —
        // the same singleton is the long-lived store AND the IFileWatchAware that
        // FileWatchDispatcher clears on every content change (dev mode; one-shot builds
        // never evict).
        services.AddSingleton<BuildHtmlCache>();
        services.AddSingleton<IFileWatchAware>(sp => sp.GetRequiredService<BuildHtmlCache>());

        // In-memory dispatcher: routes self-fetches through TestServer (build mode +
        // integration tests) or Kestrel's listening socket (dev mode). Replaces the
        // old named-HttpClient + IServerAddressesFeature lookup. Wraps the client with
        // CachingHttpHandler so every consumer shares BuildHtmlCache.
        services.AddSingleton<IInProcessHttpDispatcher, HttpDispatcher>();

        // Shared helper for fetching post-pipeline rendered HTML from the running app.
        // Used by LlmsTxtService and SearchArtifactService so their outputs reflect
        // Markdig extensions, Razor SSR, xref resolution, etc.
        services.AddSingleton<RenderedHtmlFetcher>();

        // Sharded search index, built by the external DeweySearch engine. SearchIndexBuilder is the
        // Pennington-side corpus adapter (TOC + HTML -> DeweySearch.SearchDocument); DeweySearch.IndexBuilder
        // is the engine, configured from the host's search options.
        services.AddSingleton(options.SearchIndex);
        services.AddSingleton(sp => new SearchIndexBuilder(
            sp.GetRequiredService<SearchIndexOptions>(),
            sp.GetRequiredService<Localization.LocalizationOptions>()));
        services.AddSingleton<HeadingSectionExtractor>();
        services.AddSingleton(sp =>
        {
            var searchOptions = sp.GetRequiredService<SearchIndexOptions>();
            return new DeweySearch.IndexBuilder(new DeweySearch.IndexOptions
            {
                ShardPrefixLength = searchOptions.ShardPrefixLength,
                MaxEditDistance = searchOptions.MaxEditDistance,
                Synonyms = searchOptions.Synonyms,
            });
        });

        // Shared corpus projection — one walk, one fetch per route, one DOM parse, shared
        // across every site-wide aggregator (search, llms.txt, build-time link audit).
        // File-watched so a content edit recreates the cached array.
        services.AddSingleton(options.SiteProjection);
        services.AddFileWatched<Pipeline.ISiteProjection, Pipeline.SiteProjection>();

        // Route -> ContentRecord lookup aggregated from every content service. The discovery join:
        // search faceting and structured-data emission resolve a rendered route back to its typed
        // front matter through this. File-watched so it tracks content edits.
        services.AddFileWatched<Content.ContentRecordRegistry>();

        // Search artifact and sitemap services — factory-managed, trust IContentService for fresh data
        services.AddFileWatched<SearchArtifactService>();
        services.AddFileWatched<SitemapService>();

        // Artifact-tier façade serving /search/**.json in dev and emitting the same shards
        // into the static build; transient so each resolution captures the current
        // file-watched service.
        services.AddTransient<IArtifactContentService, SearchArtifactContentService>();

        // Derived-metadata enrichment: enrichers contribute non-authored fields
        // (reading time, …) merged into ParsedItem.Derived. Consumed by LlmsTxtService;
        // the seam is general, so it is wired regardless of llms.txt config.
        services.AddTransient<MetadataEnrichmentService>();
        services.AddTransient<IMetadataEnricher, ReadingTimeEnricher>();

        // llms.txt generation
        if (options.LlmsTxt is { } llmsTxtOptions)
        {
            services.AddSingleton(llmsTxtOptions);
            // Advertise the llms.txt index as a head alternate link (was literal template markup).
            services.AddHeadContributor<AlternateLinksHeadContributor>();
            services.AddFileWatched<LlmsTxtService>();
            // Transient so each resolution captures the current file-watched
            // LlmsTxtService — a singleton here would pin the first instance.
            // Registered on the artifact tier (never IContentService) so it stays
            // out of LlmsTxtService's own IEnumerable<IContentService> and out of
            // every request-path discovery walk.
            services.AddTransient<IArtifactContentService, LlmsArtifactContentService>();
        }

        // Social-card generation: discover one card route per content page (baked by the build
        // crawler) and render each on demand via MapSocialCards. The service emits EndpointSource
        // routes — the documented pairing with the MapGet endpoint that produces the bytes.
        if (options.SocialCards is { } socialCardOptions)
        {
            services.AddSingleton(socialCardOptions);
            // Singleton IContentService that resolves its sibling services on demand (like the
            // taxonomy service) — registered directly so build discovery enumerates its card routes.
            services.AddSingleton<IContentService>(sp =>
                new SocialCardContentService(sp, socialCardOptions));
            // Per-page og:image/twitter:image meta tags for every page with a content record —
            // template-agnostic, so cards tag markdown on any host and sidecar-backed Razor pages.
            services.AddHeadContributor<SocialCardHeadContributor>();
        }

        // Per-request diagnostic context
        services.AddHttpContextAccessor();
        services.AddScoped<Diagnostics.DiagnosticContext>();

        // Locale context — scoped per request, populated by LocaleDetectionMiddleware
        services.AddScoped(sp =>
        {
            var localization = sp.GetRequiredService<Localization.LocalizationOptions>();
            return new LocaleContext(localization);
        });

        // ASP.NET localization + Pennington's IStringLocalizer backed by TranslationOptions
        services.AddLocalization();
        services.AddSingleton(options.Translations);
        services.AddSingleton<IStringLocalizerFactory, PenningtonStringLocalizerFactory>();

        // Output generation
        services.AddTransient<OutputGenerationService>();

        return services;
    }

    /// <summary>
    /// Register a source-generated <see cref="YamlSerializerContext"/> so the types it covers
    /// deserialize without reflection (NativeAOT/trim-friendly). Types not covered by any
    /// registered context fall back to reflection. Satellite templates call this for their own
    /// front-matter records; end users call it for theirs.
    /// </summary>
    public static IServiceCollection AddYamlContext(this IServiceCollection services, YamlSerializerContext context)
    {
        services.AddSingleton<YamlSerializerContext>(context);
        return services;
    }

    /// <summary>
    /// Builds the assembly set scanned for routable <c>@page</c> components,
    /// always including the entry assembly (deduped) so a bare host's pages are
    /// discovered without explicit <see cref="PenningtonOptions.AdditionalRoutingAssemblies"/>.
    /// </summary>
    internal static Assembly[] ResolveRoutingAssemblies(Assembly[] configured, Assembly? entryAssembly)
    {
        if (entryAssembly is null || Array.IndexOf(configured, entryAssembly) >= 0)
        {
            return configured;
        }

        return [.. configured, entryAssembly];
    }

    /// <summary>
    /// True when the dev server should bind an OS-assigned ephemeral port — i.e. the user
    /// hasn't already chosen one. <c>urls</c> is the sink for <c>ASPNETCORE_URLS</c>, the
    /// <c>--urls</c> argument, and <c>UseUrls</c>; <c>Kestrel:Endpoints</c> covers the
    /// appsettings-endpoints form (which isn't surfaced under <c>urls</c>).
    /// </summary>
    internal static bool ShouldUseEphemeralPort(IConfiguration config) =>
        string.IsNullOrEmpty(config[WebHostDefaults.ServerUrlsKey])
        && !config.GetSection("Kestrel:Endpoints").GetChildren().Any();

    /// <summary>
    /// Asks the OS for a free loopback TCP port by binding port 0 and reading the assignment back.
    /// The listener is closed before returning, so Kestrel can claim the port itself.
    /// </summary>
    private static int FindFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    /// Adds locale detection and URL path rewriting middleware. Must be called
    /// <b>before</b> <c>MapRazorComponents</c> so that Blazor routing sees the
    /// locale-stripped path (e.g., <c>/gen-z/schedule</c> becomes <c>/schedule</c>).
    /// <para>
    /// Called automatically by <see cref="UsePennington"/> when it hasn't been called yet,
    /// but at that point it is too late for Blazor endpoint routing. Sites that use
    /// <c>@page</c> directives with locale prefixes must call this explicitly.
    /// </para>
    /// </summary>
    public static WebApplication UseLocaleRouting(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<PenningtonOptions>();

        if (!options.Localization.IsMultiLocale)
        {
            return app;
        }

        // Guard against double-registration
        var appBuilder = (IApplicationBuilder)app;
        if (appBuilder.Properties.ContainsKey(LocaleRoutingKey))
        {
            return app;
        }

        appBuilder.Properties[LocaleRoutingKey] = true;

        var cultureProvider = new PenningtonUrlRequestCultureProvider(options.Localization);
        var defaultCulture = cultureProvider.MapToCultureName(options.Localization.DefaultLocale);

        var cultures = options.Localization.Locales.Keys
            .Select(l => cultureProvider.MapToCultureName(l))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(n => new System.Globalization.CultureInfo(n))
            .ToList();

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultCulture),
            SupportedCultures = cultures,
            SupportedUICultures = cultures,
            RequestCultureProviders =
            [
                cultureProvider,
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider(),
            ],
        });

        app.UseMiddleware<LocaleDetectionMiddleware>();

        // Explicitly place routing AFTER locale detection. In .NET 8+, routing is
        // implicitly placed at the start of the pipeline; calling UseRouting()
        // explicitly overrides that so endpoint matching sees the rewritten path.
        app.UseRouting();

        return app;
    }

    /// <summary>Configure the Pennington middleware pipeline.</summary>
    public static WebApplication UsePennington(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<PenningtonOptions>();
        var hostContentRoot = app.Environment.ContentRootPath;

        // Validate content paths at startup
        var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("Pennington");
        foreach (var source in options.MarkdownSources)
        {
            var contentPath = FilePath.ResolveAgainstRoot(source.ContentPath, hostContentRoot);
            if (!Directory.Exists(contentPath))
            {
                logger?.LogWarning(
                    "Pennington: content path '{ContentPath}' does not exist. No content will be discovered from this source.",
                    contentPath);
            }
        }
        foreach (var format in options.ContentFormats)
        {
            var contentPath = FilePath.ResolveAgainstRoot(format.ContentPath, hostContentRoot);
            if (!Directory.Exists(contentPath))
            {
                logger?.LogWarning(
                    "Pennington: content path '{ContentPath}' for format '{Format}' does not exist. No content will be discovered from this source.",
                    contentPath, format.Format);
            }
        }

        // Serve static files from content root
        var contentRoot = FilePath.ResolveAgainstRoot(options.ContentRootPath.Value, hostContentRoot);
        if (Directory.Exists(contentRoot))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(contentRoot),
                RequestPath = "",
                ServeUnknownFileTypes = false,
            });
        }

        // Serve static files from each content source directory
        foreach (var source in options.MarkdownSources)
        {
            var contentPath = FilePath.ResolveAgainstRoot(source.ContentPath, hostContentRoot);
            if (!Directory.Exists(contentPath))
            {
                continue;
            }

            var requestPath = new UrlPath(source.BasePageUrl).EnsureLeadingSlash().RemoveTrailingSlash().Value;
            if (requestPath == "/")
            {
                requestPath = "";
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(contentPath),
                RequestPath = requestPath,
                ServeUnknownFileTypes = true,
            });
        }

        // Serve static files (sibling assets) from each custom content-format source directory.
        foreach (var format in options.ContentFormats)
        {
            var contentPath = FilePath.ResolveAgainstRoot(format.ContentPath, hostContentRoot);
            if (!Directory.Exists(contentPath))
            {
                continue;
            }

            var requestPath = new UrlPath(format.BasePageUrl).EnsureLeadingSlash().RemoveTrailingSlash().Value;
            if (requestPath == "/")
            {
                requestPath = "";
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(contentPath),
                RequestPath = requestPath,
                ServeUnknownFileTypes = true,
            });
        }

        // Serve static files from locale subdirectories
        if (options.Localization.IsMultiLocale)
        {
            foreach (var source in options.MarkdownSources)
            {
                var contentPath = FilePath.ResolveAgainstRoot(source.ContentPath, hostContentRoot);

                foreach (var locale in options.Localization.Locales.Keys)
                {
                    if (string.Equals(locale, options.Localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var localeContentPath = Path.Combine(contentPath, locale);
                    if (!Directory.Exists(localeContentPath))
                    {
                        continue;
                    }

                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(localeContentPath),
                        RequestPath = $"/{locale}",
                        ServeUnknownFileTypes = true,
                    });
                }
            }
        }

        // Locale detection — ensure it's registered (idempotent).
        // For Blazor @page routing this must run before MapRazorComponents;
        // callers that need that should call UseLocaleRouting() explicitly.
        app.UseLocaleRouting();

        // File-watch dispatcher: eagerly resolve so its constructor wires every
        // IFileWatchAware service's scopes and subscription to the file watcher.
        _ = app.Services.GetRequiredService<FileWatchDispatcher>();

        // Live reload: eagerly resolve so it subscribes to file watcher, then map WebSocket endpoint
        _ = app.Services.GetRequiredService<LiveReloadServer>();
        app.UseLiveReload();

        // Corpus-fetch tripwire: RenderedHtmlFetcher stamps every projection-issued self-fetch
        // with this header; entering the scope here (before response processing, so it spans
        // every processor and the endpoint render) lets ISiteProjection fail fast with a
        // descriptive exception — instead of the b719d73 task-cycle deadlock — if anything on
        // this request's render path awaits the projection.
        app.Use(async (context, next) =>
        {
            if (context.Request.Headers.ContainsKey(CorpusFetchScope.HeaderName))
            {
                using (CorpusFetchScope.EnterCorpusFetch())
                {
                    await next(context);
                }

                return;
            }

            await next(context);
        });

        // Response processing middleware
        app.UseMiddleware<ResponseProcessingMiddleware>();

        // Redirect middleware — checks the unified redirect map before any content
        // routing, returning 301 + meta-refresh body. The static build crawler sees
        // the same 301 and OutputGenerationService writes the body to disk, so dev
        // and publish share one code path for redirects.
        app.UseMiddleware<PenningtonRedirectMiddleware>();

        // Artifact router: serves every registered IArtifactContentService's claimed territory
        // (llms.txt files, search shards, well-known files, book PDFs) from the same resolver
        // the static build writes from. One middleware instead of per-feature path sniffing.
        // Runs before endpoint routing so a claimed path like /reference/api/llms.txt can't be
        // captured by a content route's {slug} segment, and falls through on a resolver miss so
        // real content under a claimed prefix keeps working.
        app.UseMiddleware<ArtifactRouterMiddleware>();

        // Sitemap endpoint (auto-discovered and baked by the static build). The
        // sharded search index and llms.txt files go through the artifact tier instead.
        // Gated so a host can opt out (e.g. BlogSiteOptions.EnableSitemap = false);
        // when false the crawler also skips /sitemap.xml since MapGetRouteDiscovery
        // walks the live EndpointDataSource.
        if (app.Services.GetRequiredService<PenningtonOptions>().MapSitemap)
        {
            app.MapGet("/sitemap.xml", async (SitemapService service) =>
                Results.Content(await service.GetSitemapXmlAsync(), "application/xml"));
        }

        // On-demand social-card rendering. The catch-all {**slug} route is skipped by the build's
        // MapGet discovery (it is parameterized); the concrete card routes come from
        // SocialCardContentService and are fetched through this endpoint during the crawl.
        if (app.Services.GetService<SocialCardOptions>() is not null)
        {
            app.MapSocialCards();
        }

        return app;
    }

    /// <summary>
    /// Runs the host: serves live (no verb), builds the static site (<c>build</c>), or runs a
    /// diagnostic command (<c>diag &lt;sub&gt;</c>). Everything flows through one System.CommandLine
    /// pipeline, so <c>--help</c> / <c>--version</c> work at the root and every subcommand. Build and
    /// diag run one-shot against a started in-memory host that is disposed afterward; serve hands off
    /// to <see cref="WebApplication.RunAsync"/>.
    /// </summary>
    public static async Task RunOrBuildAsync(this WebApplication app, string[] args)
    {
        StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);

        var root = new RootCommand("Pennington site host. Run with no command to start the dev server.");
        root.TreatUnmatchedTokensAsErrors = false;
        root.SetAction(async (_, _) =>
        {
            await app.RunAsync();
            return 0;
        });

        var build = PenningtonCli.CreateBuildCommand();
        build.SetAction(async (_, _) =>
        {
            var report = await app.Services.GetRequiredService<OutputGenerationService>().GenerateAsync();
            report.WriteTo(Console.Out);
            return report.HasErrors ? 1 : 0;
        });
        root.Subcommands.Add(build);
        root.Subcommands.Add(PenningtonCli.BuildDiagGroup(app.Services, Console.Out));

        var parseResult = root.Parse(args);

        // build/diag are one-shot commands that inspect a started, in-memory host; --help/--version
        // just print, and serve starts the host itself via RunAsync — none of those need the wrapper.
        if (!args.Any(PenningtonCli.IsHelpOrVersionToken)
            && PenningtonCli.Current.Mode is PenningtonRunMode.Build or PenningtonRunMode.Diag)
        {
            // Dispose the host when the one-shot command finishes so container singletons are torn
            // down — notably SolutionWorkspaceService, whose Dispose() terminates the MSBuildWorkspace
            // (releasing its BuildHost child and mapped assembly handles) and deletes its per-run temp
            // build folder. Without this, every build leaks a %TEMP%\Pennington_Build_* folder and
            // leaves orphaned handles that intermittently starve the next build's metadata-reference
            // resolution (producing sparse API reference pages).
            await using (app)
            {
                await app.StartAsync();
                Environment.ExitCode = await parseResult.InvokeAsync();
                await app.StopAsync();
            }
        }
        else
        {
            Environment.ExitCode = await parseResult.InvokeAsync();
        }
    }
}