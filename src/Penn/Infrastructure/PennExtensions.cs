namespace Penn.Infrastructure;

using System.IO.Abstractions;
using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Generation;
using Penn.Highlighting;
using Penn.Markdown;
using Penn.Navigation;
using Penn.Pipeline;
using Penn.Markdown.Extensions;
using Penn.Routing;
using Penn.Search;
using Penn.Feeds;
using Testably.Abstractions;

public static class PennExtensions
{
    /// <summary>Register all Penn services.</summary>
    public static IServiceCollection AddPenn(this IServiceCollection services, Action<PennOptions> configure)
    {
        var options = new PennOptions();
        configure(options);

        // Register options
        services.AddSingleton(options);

        // Register output options from CLI args
        var args = Environment.GetCommandLineArgs();
        var outputOptions = OutputOptions.FromArgs(args.Length > 1 ? args[1..] : []);
        services.AddSingleton(outputOptions);

        // Core services
        services.AddSingleton<FrontMatterParser>();
        services.AddSingleton<NavigationBuilder>();

        // File system abstraction
        services.TryAddSingleton<IFileSystem>(new RealFileSystem());

        // File watching
        services.AddSingleton<IFileWatcher, FileWatcher>();

        // Highlighting: register TextMate and Shell highlighters, then the service
        services.AddSingleton<TextMateLanguageRegistry>();
        services.AddSingleton<ICodeHighlighter, TextMateHighlighter>(sp =>
            new TextMateHighlighter(sp.GetRequiredService<TextMateLanguageRegistry>()));
        services.AddSingleton<ICodeHighlighter, ShellHighlighter>();
        services.AddSingleton<HighlightingService>(sp =>
            new HighlightingService(sp.GetServices<ICodeHighlighter>()));

        // Markdown pipeline — includes highlighting, tabs, custom alerts, and preprocessors
        services.AddSingleton<MarkdownPipeline>(sp =>
            MarkdownPipelineFactory.CreateWithExtensions(
                sp.GetRequiredService<HighlightingService>(),
                preprocessors: sp.GetServices<ICodeBlockPreprocessor>()));
        services.AddTransient<IContentRenderer, MarkdownContentRenderer>();

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
                    Section = capturedSource.Section,
                };

                var parser = sp.GetRequiredService<FrontMatterParser>();
                var fileSystem = sp.GetRequiredService<IFileSystem>();
                var fileWatcher = sp.GetRequiredService<IFileWatcher>();
                return Activator.CreateInstance(serviceType, sourceOptions, parser, fileSystem, fileWatcher)!;
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
            services.AddSingleton<IContentService>(
                new RazorPageContentService(options.AdditionalRoutingAssemblies));
        }

        // Pipeline
        services.AddTransient<IContentPipeline, ContentPipeline>();

        // Xref resolution
        services.AddSingleton<XrefResolver>();

        // Response processors
        services.AddSingleton<IResponseProcessor>(sp =>
            new BaseUrlRewritingProcessor(sp.GetRequiredService<OutputOptions>()));
        services.AddSingleton<XrefResolvingService>();
        services.AddSingleton<IResponseProcessor, XrefResolvingProcessor>();
        services.AddSingleton<IResponseProcessor, LiveReloadScriptProcessor>();
        services.AddSingleton<IResponseProcessor, DiagnosticOverlayProcessor>();

        // Live reload (only does work when DOTNET_WATCH is set)
        services.AddSingleton<LiveReloadServer>();

        // Feed builders
        var canonicalBase = new UrlPath(options.CanonicalBaseUrl ?? "/");
        services.AddSingleton(_ => new SitemapBuilder(canonicalBase));
        services.AddSingleton(_ => new RssFeedBuilder(canonicalBase));
        services.AddSingleton(_ => new SearchIndexBuilder());

        // Per-request diagnostic context
        services.AddHttpContextAccessor();
        services.AddScoped<Diagnostics.DiagnosticContext>();

        // Output generation
        services.AddTransient<OutputGenerationService>();

        return services;
    }

    /// <summary>Configure the Penn middleware pipeline.</summary>
    public static WebApplication UsePenn(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<PennOptions>();
        var hostContentRoot = app.Environment.ContentRootPath;

        // Validate content paths at startup
        var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("Penn");
        foreach (var source in options.MarkdownSources)
        {
            var contentPath = Path.IsPathRooted(source.ContentPath)
                ? source.ContentPath
                : Path.Combine(hostContentRoot, source.ContentPath);
            if (!Directory.Exists(contentPath))
            {
                logger?.LogWarning(
                    "Penn: content path '{ContentPath}' does not exist. No content will be discovered from this source.",
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

        // Live reload: eagerly resolve so it subscribes to file watcher, then map WebSocket endpoint
        _ = app.Services.GetRequiredService<LiveReloadServer>();
        app.UsePennLiveReload();

        // Response processing middleware
        app.UseMiddleware<ResponseProcessingMiddleware>();

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
