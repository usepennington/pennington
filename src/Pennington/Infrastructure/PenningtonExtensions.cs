namespace Pennington.Infrastructure;

using System.IO.Abstractions;
using System.Reflection;
using Content;
using Feeds;
using FrontMatter;
using Generation;
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
using Microsoft.AspNetCore.TestHost;
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

        // Build mode: replace Kestrel with TestServer so the crawler dispatches
        // in-memory through the same middleware pipeline. No socket bind, no
        // dev-cert prompt, no random-port races in CI. Last-registered IServer
        // wins, so this overrides the Kestrel registration that
        // WebApplication.CreateBuilder() puts in place.
        if (PenningtonBuildMode.IsBuildMode())
        {
            services.AddSingleton<IServer, TestServer>();

            // Mute the host's "Application started / Hosting environment / Content root"
            // chatter — the build is in-process and the user wants Pennington's own
            // progress messages, not lifetime breadcrumbs. Pennington.* is bumped to
            // Information so the phase logs surface even when the host's appsettings
            // sets a Default of Warning. The custom formatter strips the
            // "info: Category[0]" prefix so the build reads like a CLI tool.
            services.AddLogging(b =>
            {
                b.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                b.AddFilter("Pennington", LogLevel.Information);
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
        }

        // Core services
        // Build mode defaults StrictUnknownKeys to true so typo'd keys fail the build.
        // The user wins if they already flipped it on; flip-down for build is rare and
        // can still be done by registering a replacement options instance after AddPennington.
        if (PenningtonBuildMode.IsBuildMode() && !options.FrontMatter.StrictUnknownKeys)
        {
            options.FrontMatter.StrictUnknownKeys = true;
        }

        services.AddSingleton(options.FrontMatter);

        // YAML deserialization: register the built-in source-generated context and the provider
        // that dispatches each type to its context (or reflection). Satellite packages and users
        // add their own contexts via AddPenningtonYamlContext.
        services.AddPenningtonYamlContext(PenningtonYamlContext.Default);
        services.AddSingleton<PenningtonYamlContextProvider>();

        services.AddSingleton<FrontMatterParser>();
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
        // expander is transient so each render picks up the live set. AssemblyVersionShortcode
        // ships built-in so authors can stamp the host app's version with <?# Version /?>.
        services.AddTransient(sp =>
            new ShortcodeExpander(
                sp.GetServices<IShortcode>(),
                sp.GetService<IHttpContextAccessor>()));
        services.AddSingleton<IShortcode, AssemblyVersionShortcode>();

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

        services.AddTransient<IContentRenderer>(sp =>
            new MarkdownContentRenderer(
                sp.GetRequiredService<MarkdownPipeline>(),
                sp.GetService<MarkdownLinkResolver>(),
                sp.GetService<IFileSystem>(),
                sp.GetService<ShortcodeExpander>()));

        // Register markdown content services for each configured source
        foreach (var source in options.MarkdownSources)
        {
            // Capture loop variable for closure
            var capturedSource = source;

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
                    var resolvedContentPath = Path.IsPathRooted(capturedSource.ContentPath)
                        ? capturedSource.ContentPath
                        : env != null
                            ? Path.Combine(env.ContentRootPath, capturedSource.ContentPath)
                            : capturedSource.ContentPath;

                    var sourceOptions = new MarkdownContentServiceOptions
                    {
                        ContentPath = new FilePath(resolvedContentPath),
                        BasePageUrl = new UrlPath(capturedSource.BasePageUrl),
                        SectionLabel = capturedSource.SectionLabel,
                        ExcludePaths = capturedSource.ExcludePaths,
                    };

                    var parser = sp.GetRequiredService<FrontMatterParser>();
                    var fileSystem = sp.GetRequiredService<IFileSystem>();
                    var localization = sp.GetRequiredService<LocalizationOptions>();
                    var clock = sp.GetRequiredService<TimeProvider>();
                    instance = Activator.CreateInstance(serviceType, sourceOptions, parser, fileSystem, localization, clock)!;
                }
                return instance;
            }

            services.AddSingleton(typeof(IContentService), Resolve);
            services.AddSingleton(typeof(IFileWatchAware), Resolve);

            // Register parser for the front matter type
            var parserType = typeof(MarkdownContentParser<>).MakeGenericType(frontMatterType);
            services.AddTransient(typeof(IContentParser), sp =>
            {
                var fmParser = sp.GetRequiredService<FrontMatterParser>();
                var fileSystem = sp.GetRequiredService<IFileSystem>();
                return Activator.CreateInstance(parserType, fmParser, fileSystem)!;
            });
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
                    sp.GetRequiredService<TimeProvider>()));
        }

        // Pipeline
        services.AddTransient<IContentPipeline, ContentPipeline>();

        // Unified redirect map (from _redirects.yml + per-page redirectUrl front matter).
        // Registered as both a concrete service (for the middleware) and an IContentService
        // (so the build crawler hits the YAML-defined redirect sources and captures 301s).
        services.AddSingleton<RedirectContentService>();
        services.AddSingleton<IContentService>(sp => sp.GetRequiredService<RedirectContentService>());

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
        services.AddSingleton<IHtmlResponseRewriter, CanonicalLinkHtmlRewriter>();
        services.AddSingleton<IHtmlResponseRewriter>(sp =>
            new BaseUrlHtmlRewriter(sp.GetRequiredService<OutputOptions>()));
        services.AddTransient<IResponseProcessor, HtmlResponseRewritingProcessor>();
        services.AddSingleton<IResponseProcessor>(sp =>
            new BaseUrlCssResponseProcessor(sp.GetRequiredService<OutputOptions>()));
        services.AddSingleton<IResponseProcessor, LiveReloadScriptProcessor>();
        services.AddSingleton<IResponseProcessor, DiagnosticOverlayProcessor>();
        services.AddSingleton<IResponseProcessor, NotFoundStatusProcessor>();
        services.AddSingleton<IResponseProcessor, AuditDiagnosticProcessor>();

        // Audit pipeline: AuditCache holds the latest snapshot from every IBuildAuditor;
        // AuditRunner refreshes it at startup and on file change. AuditDiagnosticProcessor
        // (registered above) feeds per-route entries into the dev overlay; the build report
        // copies the same snapshot in OutputGenerationService.
        services.AddSingleton<AuditCache>();
        services.AddSingleton<IAuditCache>(sp => sp.GetRequiredService<AuditCache>());
        services.AddHostedService<AuditRunner>();
        services.AddTransient<IBuildAuditor, OverlapAuditor>();
        services.AddTransient<IBuildAuditor, XrefAuditor>();
        services.AddTransient<IRenderedAuditor, LinkAuditor>();

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
        services.AddSingleton(sp => new RssFeedBuilder(
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
            sp.GetRequiredService<LocalizationOptions>()));
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

        // Search artifact and sitemap services — factory-managed, trust IContentService for fresh data
        services.AddFileWatched<SearchArtifactService>();
        services.AddFileWatched<SitemapService>();

        // Emit the sharded search artifacts into the static build (mirrors llms.txt);
        // transient so each resolution captures the current file-watched service.
        services.AddTransient<IContentEmitter, SearchArtifactEmitter>();

        // Derived-metadata enrichment: enrichers contribute non-authored fields
        // (reading time, …) merged into ParsedItem.Derived. Consumed by LlmsTxtService;
        // the seam is general, so it is wired regardless of llms.txt config.
        services.AddTransient<MetadataEnrichmentService>();
        services.AddTransient<IMetadataEnricher, ReadingTimeEnricher>();

        // llms.txt generation
        if (options.LlmsTxt is { } llmsTxtOptions)
        {
            services.AddSingleton(llmsTxtOptions);
            services.AddFileWatched<LlmsTxtService>();
            // Transient so each resolution captures the current file-watched
            // LlmsTxtService — a singleton here would pin the first instance.
            // Registered as IContentEmitter (not IContentService) to keep it
            // out of LlmsTxtService's own IEnumerable<IContentService>; the
            // build crawler picks it up via OutputGenerationService's emitter
            // pass.
            services.AddTransient<IContentEmitter, LlmsTxtContentService>();
        }

        // Per-request diagnostic context
        services.AddHttpContextAccessor();
        services.AddScoped<Diagnostics.DiagnosticContext>();

        // Locale context — scoped per request, populated by LocaleDetectionMiddleware
        services.AddScoped(sp =>
        {
            var localization = sp.GetRequiredService<LocalizationOptions>();
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
    public static IServiceCollection AddPenningtonYamlContext(this IServiceCollection services, YamlSerializerContext context)
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
    /// Adds locale detection and URL path rewriting middleware. Must be called
    /// <b>before</b> <c>MapRazorComponents</c> so that Blazor routing sees the
    /// locale-stripped path (e.g., <c>/gen-z/schedule</c> becomes <c>/schedule</c>).
    /// <para>
    /// Called automatically by <see cref="UsePennington"/> when it hasn't been called yet,
    /// but at that point it is too late for Blazor endpoint routing. Sites that use
    /// <c>@page</c> directives with locale prefixes must call this explicitly.
    /// </para>
    /// </summary>
    public static WebApplication UsePenningtonLocaleRouting(this WebApplication app)
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
            var contentPath = Path.IsPathRooted(source.ContentPath)
                ? source.ContentPath
                : Path.Combine(hostContentRoot, source.ContentPath);
            if (!Directory.Exists(contentPath))
            {
                logger?.LogWarning(
                    "Pennington: content path '{ContentPath}' does not exist. No content will be discovered from this source.",
                    contentPath);
            }
        }

        // Serve static files from content root
        var contentRoot = Path.IsPathRooted(options.ContentRootPath)
            ? options.ContentRootPath
            : Path.Combine(hostContentRoot, options.ContentRootPath);
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
            var contentPath = Path.IsPathRooted(source.ContentPath)
                ? source.ContentPath
                : Path.Combine(hostContentRoot, source.ContentPath);
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

        // Serve static files from locale subdirectories
        if (options.Localization.IsMultiLocale)
        {
            foreach (var source in options.MarkdownSources)
            {
                var contentPath = Path.IsPathRooted(source.ContentPath)
                    ? source.ContentPath
                    : Path.Combine(hostContentRoot, source.ContentPath);

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
        // callers that need that should call UsePenningtonLocaleRouting() explicitly.
        app.UsePenningtonLocaleRouting();

        // File-watch dispatcher: eagerly resolve so its constructor wires every
        // IFileWatchAware service's scopes and subscription to the file watcher.
        _ = app.Services.GetRequiredService<FileWatchDispatcher>();

        // Live reload: eagerly resolve so it subscribes to file watcher, then map WebSocket endpoint
        _ = app.Services.GetRequiredService<LiveReloadServer>();
        app.UsePenningtonLiveReload();

        // Response processing middleware
        app.UseMiddleware<ResponseProcessingMiddleware>();

        // Redirect middleware — checks the unified redirect map before any content
        // routing, returning 301 + meta-refresh body. The static build crawler sees
        // the same 301 and OutputGenerationService writes the body to disk, so dev
        // and publish share one code path for redirects.
        app.UseMiddleware<PenningtonRedirectMiddleware>();

        // Llms.txt subtree files and per-page sidecars. Runs as middleware so
        // /reference/api/llms.txt isn't claimed by the API-reference Razor route's
        // {slug} segment.
        if (app.Services.GetService<LlmsTxtOptions>() is not null)
        {
            app.UseMiddleware<LlmsTxtMiddleware>();
        }

        // Sharded search artifacts under /search/... served from the same in-memory
        // service the build emitter writes from. Middleware (not an endpoint) so the
        // {locale}/{prefix} files aren't claimed by content routes and don't depend
        // on the crawler, which can't bake parameterized routes.
        app.UseMiddleware<SearchArtifactMiddleware>();

        // Sitemap endpoint (auto-discovered and baked by the static build). The
        // sharded search index is emitted by SearchArtifactEmitter instead.
        app.MapGet("/sitemap.xml", async (SitemapService service) =>
            Results.Content(await service.GetSitemapXmlAsync(), "application/xml"));

        if (app.Services.GetService<LlmsTxtOptions>() is not null)
        {
            app.MapGet("/llms.txt", async (LlmsTxtService service) =>
                Results.Content(await service.GetLlmsTxtAsync(), "text/plain"));
        }

        return app;
    }

    /// <summary>Run in dev mode or build static site.</summary>
    public static async Task RunOrBuildAsync(this WebApplication app, string[] args)
    {
        StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);

        if (PenningtonBuildMode.IsBuildMode(args))
        {
            // Dispose the host when the one-shot build finishes so container singletons
            // are torn down — notably SolutionWorkspaceService, whose Dispose() disposes
            // the MSBuildWorkspace (terminating its BuildHost child and releasing mapped
            // assembly handles) and deletes its per-run temp build folder. Without this,
            // every build leaks a %TEMP%\Pennington_Build_* folder and leaves the
            // workspace untorn-down; the resulting litter and orphaned handles are what
            // intermittently starve the next build's metadata-reference resolution
            // (producing sparse API reference pages).
            await using (app)
            {
                await app.StartAsync();
                var generator = app.Services.GetRequiredService<OutputGenerationService>();
                var report = await generator.GenerateAsync();
                await app.StopAsync();

                report.WriteTo(Console.Out);
                if (report.HasErrors)
                {
                    Environment.ExitCode = 1;
                }
            }
        }
        else
        {
            await app.RunAsync();
        }
    }
}