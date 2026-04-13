namespace Pennington.DocSite;

using System.Reflection;
using Mdazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.MonorailCss;
using Pennington.UI.Components;

public static class DocSiteServiceExtensions
{
    public static IServiceCollection AddDocSite(this IServiceCollection services,
        Func<DocSiteOptions> configureOptions)
    {
        var options = configureOptions();
        services.AddSingleton(options);
        services.AddRazorComponents();

        // Pennington core
        services.AddPennington(penn =>
        {
            penn.SiteTitle = options.SiteTitle;
            penn.SiteDescription = options.Description;
            penn.CanonicalBaseUrl = options.CanonicalBaseUrl;
            penn.ContentRootPath = options.ContentRootPath.Value;

            penn.AddMarkdownContent<DocSiteFrontMatter>(md =>
            {
                md.ContentPath = options.ContentRootPath.Value;
                md.BasePageUrl = "/";
            });

            // llms.txt generation — the DocSite layout puts content inside <article id="main-content">
            // so both llms.txt output and the search index can scope extraction to that element.
            penn.AddLlmsTxt(opts => opts.ContentSelector ??= "#main-content");
            penn.SearchIndex.ContentSelector ??= "#main-content";

            // Localization
            options.ConfigureLocalization?.Invoke(penn.Localization);

            // Scan the entry assembly (the app) plus any explicitly configured assemblies
            var appAssembly = Assembly.GetEntryAssembly();
            var allAssemblies = appAssembly != null
                ? [appAssembly, .. options.AdditionalRoutingAssemblies]
                : options.AdditionalRoutingAssemblies;
            penn.AdditionalRoutingAssemblies = allAssemblies;
        });

        // Make Pennington.UI components available inline in markdown via Mdazor.
        services.AddMdazorComponent<Badge>()
                .AddMdazorComponent<BigTable>()
                .AddMdazorComponent<Card>()
                .AddMdazorComponent<CardGrid>()
                .AddMdazorComponent<CodeBlock>()
                .AddMdazorComponent<LinkCard>()
                .AddMdazorComponent<Step>()
                .AddMdazorComponent<Steps>();

        // MonorailCSS
        services.AddMonorailCss(sp =>
        {
            var options = sp.GetRequiredService<DocSiteOptions>();

            var monoOptions = new MonorailCssOptions
            {
                ColorScheme = options.ColorScheme ?? new MonorailCssOptions().ColorScheme,
                ExtraStyles = options.ExtraStyles ?? string.Empty,
            };

            return monoOptions;
        });

        // SPA navigation
        services.AddSpaNavigation();

        // Component renderer for SPA islands
        services.AddScoped<ComponentRenderer>();

        // Register the article island renderer
        services.AddTransient<IIslandRenderer, Slots.DocSiteArticleSlotRenderer>();

        // Content resolver
        services.AddTransient<Services.ContentResolver>();

        return services;
    }

    public static WebApplication UseDocSite(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<DocSiteOptions>();

        app.UsePenningtonLocaleRouting();
        app.UseAntiforgery();
        app.UseStaticFiles();
        app.MapRazorComponents<Components.App>()
            .AddAdditionalAssemblies(options.AdditionalRoutingAssemblies);
        app.UseMonorailCss();
        app.UseSpaNavigation();
        app.UsePennington();

        return app;
    }

    public static async Task RunDocSiteAsync(this WebApplication app, string[] args)
    {
        await app.RunOrBuildAsync(args);
    }
}
