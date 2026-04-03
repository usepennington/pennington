namespace Penn.Infrastructure;

using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Generation;
using Penn.Highlighting;
using Penn.Markdown;
using Penn.Navigation;
using Penn.Pipeline;
using Penn.Routing;
using Penn.Search;
using Penn.Feeds;

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
        services.AddSingleton<HighlightingService>(sp =>
            new HighlightingService(sp.GetServices<ICodeHighlighter>()));

        // Markdown pipeline
        services.AddSingleton<MarkdownPipeline>(_ => MarkdownPipelineFactory.CreateDefault());
        services.AddTransient<IContentRenderer, MarkdownContentRenderer>();

        // Register markdown content services for each configured source
        foreach (var source in options.MarkdownSources)
        {
            var sourceOptions = new MarkdownContentServiceOptions
            {
                ContentPath = new FilePath(source.ContentPath),
                BasePageUrl = new UrlPath(source.BasePageUrl),
                Section = source.Section,
            };

            // Register the content service
            var frontMatterType = source.FrontMatterType ?? typeof(DocFrontMatter);
            var serviceType = typeof(MarkdownContentService<>).MakeGenericType(frontMatterType);

            services.AddSingleton(typeof(IContentService), sp =>
            {
                var parser = sp.GetRequiredService<FrontMatterParser>();
                return Activator.CreateInstance(serviceType, sourceOptions, parser)!;
            });

            // Register parser for the front matter type
            var parserType = typeof(MarkdownContentParser<>).MakeGenericType(frontMatterType);
            services.AddTransient(typeof(IContentParser), sp =>
            {
                var fmParser = sp.GetRequiredService<FrontMatterParser>();
                return Activator.CreateInstance(parserType, fmParser)!;
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

        // Response processors
        services.AddSingleton<IResponseProcessor>(sp =>
            new BaseUrlRewritingProcessor(sp.GetRequiredService<OutputOptions>()));

        // Feed builders
        var canonicalBase = new UrlPath(options.CanonicalBaseUrl ?? "/");
        services.AddSingleton(_ => new SitemapBuilder(canonicalBase));
        services.AddSingleton(_ => new RssFeedBuilder(canonicalBase));
        services.AddSingleton(_ => new SearchIndexBuilder());

        // Output generation
        services.AddTransient<OutputGenerationService>();

        return services;
    }

    /// <summary>Configure the Penn middleware pipeline.</summary>
    public static WebApplication UsePenn(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<PennOptions>();

        // Serve static files from content root
        var contentRoot = Path.Combine(Directory.GetCurrentDirectory(), options.ContentRootPath);
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
            var contentPath = Path.Combine(Directory.GetCurrentDirectory(), source.ContentPath);
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
            await generator.GenerateAsync(addresses.First());
            await app.StopAsync();
        }
        else
        {
            await app.RunAsync();
        }
    }
}
