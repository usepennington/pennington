namespace Pennington.DocSite;

using System.Reflection;
using Mdazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;
using Islands;
using MonorailCss;
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

            // llms.txt generation and search index. The stock DocSite layout wraps
            // page body inside <article id="main-content">, so both outputs scope
            // extraction there by default. Both selectors are overridable via
            // DocSiteOptions when the layout has been customized.
            var llmsSelector = options.LlmsTxtContentSelector ?? "#main-content";
            penn.AddLlmsTxt(opts => opts.ContentSelector ??= llmsSelector);
            penn.SearchIndex.ContentSelector ??= options.SearchIndexContentSelector ?? "#main-content";

            // Localization
            options.ConfigureLocalization?.Invoke(penn.Localization);

            // Scan the entry assembly (the app) plus any explicitly configured assemblies
            var appAssembly = Assembly.GetEntryAssembly();
            var allAssemblies = appAssembly != null
                ? [appAssembly, .. options.AdditionalRoutingAssemblies]
                : options.AdditionalRoutingAssemblies;
            penn.AdditionalRoutingAssemblies = allAssemblies;

            // Last: give the app a chance to wire additional content sources,
            // highlighters, islands, etc. Runs after DocSite's own defaults so
            // users can still override anything DocSite set above.
            options.ConfigurePennington?.Invoke(penn);
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
                CustomCssFrameworkSettings = options.CustomCssFrameworkSettings ?? (settings => settings),
            };

            return monoOptions;
        });

        // SPA navigation
        services.AddSpaNavigation();

        // ComponentRenderer is registered by AddPennington (required by
        // RazorIslandRenderer<T> on any host, not just DocSite).

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