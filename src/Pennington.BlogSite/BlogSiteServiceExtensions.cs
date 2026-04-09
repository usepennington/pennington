namespace Pennington.BlogSite;

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pennington.BlogSite.Services;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

public static class BlogSiteServiceExtensions
{
    public static IServiceCollection AddBlogSite(this IServiceCollection services,
        Func<BlogSiteOptions> configureOptions)
    {
        var options = configureOptions();
        services.AddSingleton(options);
        services.AddRazorComponents();

        services.AddPennington(penn =>
        {
            penn.SiteTitle = options.SiteTitle;
            penn.SiteDescription = options.Description;
            penn.CanonicalBaseUrl = options.CanonicalBaseUrl;
            penn.ContentRootPath = options.ContentRootPath;

            var blogContentPath = Path.Combine(options.ContentRootPath, options.BlogContentPath);
            penn.AddMarkdownContent<BlogSiteFrontMatter>(md =>
            {
                md.ContentPath = blogContentPath;
                md.BasePageUrl = options.BlogBaseUrl;
            });

            var appAssembly = Assembly.GetEntryAssembly();
            var allAssemblies = appAssembly != null
                ? [appAssembly, .. options.AdditionalRoutingAssemblies]
                : options.AdditionalRoutingAssemblies;
            penn.AdditionalRoutingAssemblies = allAssemblies;
        });

        services.AddMonorailCss(sp =>
        {
            var options = sp.GetRequiredService<BlogSiteOptions>();
            return new MonorailCssOptions
            {
                ColorScheme = options.ColorScheme ?? new MonorailCssOptions().ColorScheme,
                ExtraStyles = options.ExtraStyles ?? string.Empty,
            };
        });

        services.AddFileWatched<BlogContentResolver>();

        return services;
    }

    public static WebApplication UseBlogSite(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<BlogSiteOptions>();

        app.UseAntiforgery();
        app.UseStaticFiles();
        app.MapRazorComponents<Components.App>()
            .AddAdditionalAssemblies(options.AdditionalRoutingAssemblies);
        app.UseMonorailCss();
        app.UsePennington();

        return app;
    }

    public static async Task RunBlogSiteAsync(this WebApplication app, string[] args)
    {
        await app.RunOrBuildAsync(args);
    }
}
