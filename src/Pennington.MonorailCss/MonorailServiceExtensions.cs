using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MonorailCss.Discovery;
using Pennington.MonorailCss.Internal;

namespace Pennington.MonorailCss;

/// <summary>
/// Extension methods for registering and configuring MonorailCSS services.
/// </summary>
public static class MonorailServiceExtensions
{
    /// <summary>
    /// Registers MonorailCSS services and the runtime class-discovery pipeline.
    /// With no configuration, the discovery pipeline force-loads every non-BCL assembly the
    /// app references, scans each one's IL, watches the project's source files in development,
    /// and loads <c>wwwroot/app.css</c> as the source CSS prefix when present. The CSS endpoint
    /// served by <see cref="UseMonorailCss"/> regenerates whenever the class set changes.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="optionFactory">Optional factory for configuring MonorailCSS options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMonorailCss(this IServiceCollection services,
        Func<IServiceProvider, MonorailCssOptions>? optionFactory = null)
    {
        if (optionFactory == null)
        {
            services.AddSingleton(new MonorailCssOptions());
        }
        else
        {
            // Singleton (was Transient) so the framework + discovery share a single configured
            // instance for the lifetime of the app — required since the discovery service
            // validates candidates against this exact framework.
            services.AddSingleton(optionFactory);
        }

        services.AddSingleton<MonorailCssService>();

        services.AddMonorailClassDiscovery();

        // Seed the discovery pipeline with a framework built from Pennington's options so the
        // candidate parser, class registry, and stylesheet generator all share one theme until
        // discovery rebuilds the framework from source CSS (if any). Pennington.UI and the
        // rest of the Pennington.* packages ride along automatically — Discovery force-loads
        // every non-BCL assembly the entry app references.
        //
        // In development we also expand WatchSourceDirectories beyond the lone ContentRootPath
        // Discovery's defaults provide. PDBs of the loaded non-system assemblies tell us which
        // .csproj produced each one; under dotnet watch, the IL on disk goes stale on EnC
        // deltas, so the source-file watcher in those project dirs is what keeps the class
        // set fresh for .razor edits in referenced libraries (e.g., Pennington.DocSite,
        // Pennington.UI) that live outside the entry app's content root.
        services.AddSingleton<IConfigureOptions<MonorailDiscoveryOptions>>(sp =>
            new ConfigureNamedOptions<MonorailDiscoveryOptions>(Options.DefaultName, opts =>
            {
                var options = sp.GetRequiredService<MonorailCssOptions>();
                opts.Framework = MonorailCssService.BuildFramework(options);

                var hostEnv = sp.GetService<IHostEnvironment>();
                if (hostEnv?.IsDevelopment() != true && Environment.GetEnvironmentVariable("DOTNET_WATCH") != "1")
                {
                    return;
                }

                // Adding anything to WatchSourceDirectories disables Discovery's
                // ApplyEnvironmentDefaults fill-in of ContentRootPath, so re-add it ourselves.
                if (hostEnv != null && !string.IsNullOrEmpty(hostEnv.ContentRootPath))
                {
                    AddIfMissing(opts.WatchSourceDirectories, hostEnv.ContentRootPath);
                }

                foreach (var dir in SourceWatchProbe.GetProjectDirectories())
                {
                    AddIfMissing(opts.WatchSourceDirectories, dir);
                }
            }));

        return services;
    }

    /// <summary>
    /// Maps the MonorailCSS stylesheet endpoint. The endpoint pulls the current class set
    /// from the discovery pipeline registered in <see cref="AddMonorailCss"/>, generates CSS,
    /// and serves it.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="path">The URL path for the stylesheet endpoint. Defaults to "/styles.css".</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseMonorailCss(this WebApplication app, string path = "/styles.css")
    {
        if (app.Services.GetService<MonorailCssService>() is null)
        {
            throw new InvalidOperationException(
                "MonorailCssService is not registered. Please call AddMonorailCss() in ConfigureServices.");
        }

        app.MapGet(path, (MonorailCssService cssService) =>
            Results.Content(cssService.GetStyleSheet(), "text/css"));

        return app;
    }

    private static void AddIfMissing(List<string> list, string value)
    {
        var normalized = Path.TrimEndingDirectorySeparator(Path.GetFullPath(value));
        foreach (var existing in list)
        {
            if (string.Equals(
                    Path.TrimEndingDirectorySeparator(Path.GetFullPath(existing)),
                    normalized,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
        list.Add(normalized);
    }
}
