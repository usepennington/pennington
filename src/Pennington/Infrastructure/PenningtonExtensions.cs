namespace Pennington.Infrastructure;

using System.IO.Abstractions;
using Content;
using Feeds;
using FrontMatter;
using Generation;
using Highlighting;
using Islands;
using LlmsTxt;
using Localization;
using Markdig;
using Markdown;
using Markdown.Extensions;
using Mdazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Navigation;
using Pipeline;
using Routing;
using Search;
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

        // Register islands from the options API
        foreach (var (_, islandType) in options.Islands.RegisteredIslands)
        {
            services.AddTransient(typeof(IIslandRenderer), islandType);
        }

        // Component renderer for SPA islands. RazorIslandRenderer<T> depends on
        // this; registering it here (rather than in AddDocSite only) means bare
        // AddPennington hosts can consume RazorIslandRenderer without having to
        // hand-wire the dependency. See postmortem-ExtensibilityLabExample.md.
        services.AddScoped<ComponentRenderer>();

        // Core services
        services.AddSingleton<FrontMatterParser>();
        services.AddFileWatched<NavigationBuilder>();

        // File system abstraction
        services.TryAddSingleton<IFileSystem>(new RealFileSystem());

        // File watching
        services.AddSingleton<IFileWatcher, FileWatcher>();

        // Highlighting: register TextMate and Shell highlighters, then the service
        services.AddSingleton<TextMateLanguageRegistry>();
        services.AddSingleton<ICodeHighlighter, TextMateHighlighter>(sp =>
            new TextMateHighlighter(sp.GetRequiredService<TextMateLanguageRegistry>()));
        services.AddSingleton<ICodeHighlighter, ShellHighlighter>();
        foreach (var highlighter in options.Highlighting.Highlighters)
        {
            services.AddSingleton<ICodeHighlighter>(highlighter);
        }
        services.AddSingleton<HighlightingService>(sp =>
            new HighlightingService(sp.GetServices<ICodeHighlighter>()));

        // Markdown pipeline — includes highlighting, tabs, custom alerts, Mdazor,
        // and any consumer-supplied extensions via options.ConfigureMarkdownPipeline.
        services.AddSingleton<MarkdownPipeline>(sp =>
            MarkdownPipelineFactory.CreateWithExtensions(
                sp,
                sp.GetRequiredService<HighlightingService>(),
                tabOptions: options.TabbedCodeBlockOptions,
                preprocessors: sp.GetServices<ICodeBlockPreprocessor>(),
                configure: options.ConfigureMarkdownPipeline));

        // Relative-link resolver — factory-managed so it rebuilds its source → URL
        // index when content files change. Registered under both the concrete type
        // (for the transient renderer below) and the factory type.
        services.AddFileWatched<MarkdownLinkResolver>();

        services.AddTransient<IContentRenderer>(sp =>
            new MarkdownContentRenderer(
                sp.GetRequiredService<MarkdownPipeline>(),
                sp.GetService<MarkdownLinkResolver>()));

        // Register markdown content services for each configured source
        foreach (var source in options.MarkdownSources)
        {
            // Capture loop variable for closure
            var capturedSource = source;

            // Register the content service — resolve content path at activation time
            // using IWebHostEnvironment.ContentRootPath so tests and hosts both work.
            var frontMatterType = capturedSource.FrontMatterType ?? typeof(DocFrontMatter);
            var serviceType = typeof(MarkdownContentService<>).MakeGenericType(frontMatterType);

            services.AddSingleton(typeof(IContentService), sp =>
            {
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
                var fileWatcher = sp.GetRequiredService<IFileWatcher>();
                var localization = sp.GetRequiredService<LocalizationOptions>();
                return Activator.CreateInstance(serviceType, sourceOptions, parser, fileSystem, fileWatcher, localization)!;
            });

            // Register parser for the front matter type
            var parserType = typeof(MarkdownContentParser<>).MakeGenericType(frontMatterType);
            services.AddTransient(typeof(IContentParser), sp =>
            {
                var fmParser = sp.GetRequiredService<FrontMatterParser>();
                var fileSystem = sp.GetRequiredService<IFileSystem>();
                return Activator.CreateInstance(parserType, fmParser, fileSystem)!;
            });
        }

        // Register Razor page content service for @page component discovery
        if (options.AdditionalRoutingAssemblies.Length > 0)
        {
            var assemblies = options.AdditionalRoutingAssemblies;
            services.AddSingleton<IContentService>(sp =>
                new RazorPageContentService(
                    assemblies,
                    sp.GetRequiredService<IFileSystem>(),
                    sp.GetRequiredService<FrontMatterParser>(),
                    sp.GetRequiredService<ILogger<RazorPageContentService>>()));
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

        // Response processors.
        // Single HTML rewriting processor that walks the parsed document once
        // and runs every registered IHtmlResponseRewriter in Order.
        services.AddSingleton<XrefResolvingService>();
        services.AddSingleton<IHtmlResponseRewriter, XrefHtmlRewriter>();
        services.AddSingleton<IHtmlResponseRewriter, LocaleLinkHtmlRewriter>();
        services.AddSingleton<IHtmlResponseRewriter>(sp =>
            new BaseUrlHtmlRewriter(sp.GetRequiredService<OutputOptions>()));
        services.AddSingleton<IResponseProcessor, HtmlResponseRewritingProcessor>();
        services.AddSingleton<IResponseProcessor, LiveReloadScriptProcessor>();
        services.AddSingleton<IResponseProcessor, DiagnosticOverlayProcessor>();

        // Live reload
        services.AddSingleton<LiveReloadServer>();

        // Feed builders. When CanonicalBaseUrl is set (typically a fully-qualified
        // https://host/sub URL) we respect it verbatim — that's the correct form
        // per sitemap / RSS protocol. When CanonicalBaseUrl is not set but the
        // static build is targeting a sub-path (e.g. `build /sub/`), fall back
        // to OutputOptions.BaseUrl so the generated sitemap produces
        // `<loc>/sub/page/</loc>` instead of root-relative `<loc>/page/</loc>`.
        // Crawlers resolve the relative URL against the sitemap.xml's own URL,
        // so this still produces a reachable target.
        var explicitCanonicalBase = options.CanonicalBaseUrl;
        services.AddSingleton(sp =>
        {
            var effectiveBase = !string.IsNullOrEmpty(explicitCanonicalBase)
                ? new UrlPath(explicitCanonicalBase)
                : sp.GetRequiredService<Generation.OutputOptions>().BaseUrl;
            return new SitemapBuilder(effectiveBase);
        });
        services.AddSingleton(sp =>
        {
            var effectiveBase = !string.IsNullOrEmpty(explicitCanonicalBase)
                ? new UrlPath(explicitCanonicalBase)
                : sp.GetRequiredService<Generation.OutputOptions>().BaseUrl;
            return new RssFeedBuilder(effectiveBase);
        });

        // Shared helper for fetching post-pipeline rendered HTML from the running host.
        // Used by LlmsTxtService and SearchIndexService so their outputs reflect
        // Markdig extensions, Razor SSR, xref resolution, etc.
        services.AddHttpClient(RenderedHtmlFetcher.HttpClientName, (sp, client) =>
            {
                var server = sp.GetRequiredService<IServer>();
                var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
                if (addresses is null || addresses.Count == 0)
                {
                    throw new InvalidOperationException(
                        "RenderedHtmlFetcher requires the web host to be listening. " +
                        "IServerAddressesFeature has no addresses — is the app started yet?");
                }

                // Prefer http:// to avoid dev-cert trust issues.
                var baseAddress = addresses.FirstOrDefault(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    ?? addresses.First();
                client.BaseAddress = new Uri(baseAddress);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddSingleton<RenderedHtmlFetcher>();

        // Search index
        services.AddSingleton(options.SearchIndex);
        services.AddSingleton(sp => new SearchIndexBuilder(
            sp.GetRequiredService<SearchIndexOptions>().DefaultPriority));

        // Search index and sitemap services — factory-managed, trust IContentService for fresh data
        services.AddFileWatched<SearchIndexService>();
        services.AddFileWatched<SitemapService>();

        // llms.txt generation
        if (options.LlmsTxt is { } llmsTxtOptions)
        {
            services.AddSingleton(llmsTxtOptions);
            services.AddFileWatched<LlmsTxtService>();
            services.AddSingleton<IContentService, LlmsTxtContentService>();
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

        if (!options.Localization.IsMultiLocale) return app;

        // Guard against double-registration
        var appBuilder = (IApplicationBuilder)app;
        if (appBuilder.Properties.ContainsKey(LocaleRoutingKey)) return app;
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
            if (!Directory.Exists(contentPath)) continue;

            var requestPath = new UrlPath(source.BasePageUrl).EnsureLeadingSlash().RemoveTrailingSlash().Value;
            if (requestPath == "/") requestPath = "";
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
                        continue;

                    var localeContentPath = Path.Combine(contentPath, locale);
                    if (!Directory.Exists(localeContentPath)) continue;

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

        // Search index and sitemap endpoints (auto-discovered by static build).
        // One search-index file per configured locale so clients only fetch their
        // locale's documents. Use concrete URLs (not a {locale} route param) so
        // OutputGenerationService.DiscoverMapGetRoutes bakes each file.
        var localization = app.Services.GetRequiredService<LocalizationOptions>();
        var searchIndexLocales = localization.Locales.Count > 0
            ? (IEnumerable<string>)localization.Locales.Keys
            : [localization.DefaultLocale];
        foreach (var code in searchIndexLocales)
        {
            var capture = code;
            app.MapGet($"/search-index-{capture}.json", async (SearchIndexService service) =>
                Results.Content(await service.GetSearchIndexJsonAsync(capture), "application/json"));
        }
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

        if (args.Length > 0 && args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
        {
            await app.StartAsync();
            var generator = app.Services.GetRequiredService<OutputGenerationService>();
            var addresses = app.Urls.Any() ? app.Urls : ["http://localhost:5000"];
            var report = await generator.GenerateAsync(addresses.First());
            await app.StopAsync();

            report.WriteTo(Console.Out);
            if (report.HasErrors)
            {
                Environment.ExitCode = 1;
            }
        }
        else
        {
            await app.RunAsync();
        }
    }
}