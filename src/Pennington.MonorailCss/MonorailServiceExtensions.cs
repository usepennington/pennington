using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MonorailCss;
using MonorailCss.Discovery;

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

        // The engine owns the CssFramework instance Pennington built from the user's
        // MonorailCssOptions, and is the only way Pennington's internals touch the framework.
        // Pennington deliberately does NOT register CssFramework in DI: a host that wants its
        // own CssFramework (for ad-hoc CompileUtilityClass calls, theme inspection, anything
        // outside Pennington's stylesheet pipeline) registers one themselves with whatever
        // settings they want, and there's no chance of shadowing or DI ordering surprises.
        // Components that specifically want Pennington's instance inject MonorailCssEngine
        // and read .Framework.
        services.AddSingleton<MonorailCssEngine>(sp =>
        {
            var options = sp.GetRequiredService<MonorailCssOptions>();
            return new MonorailCssEngine(MonorailCssService.BuildFramework(options));
        });

        services.AddSingleton<MonorailCssService>();

        services.AddMonorailClassDiscovery();

        // Hand Pennington's engine framework to the discovery pipeline so the candidate
        // parser, class registry, and stylesheet generator all share one theme. Pennington.UI
        // and the rest of the Pennington.* packages ride along automatically — Discovery
        // force-loads every non-BCL assembly the entry app references.
        services.AddSingleton<IConfigureOptions<MonorailDiscoveryOptions>>(sp =>
            new ConfigureNamedOptions<MonorailDiscoveryOptions>(Options.DefaultName, opts =>
            {
                opts.Framework = sp.GetRequiredService<MonorailCssEngine>().Framework;
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
}
