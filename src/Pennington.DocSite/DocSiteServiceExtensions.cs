namespace Pennington.DocSite;

using System.Reflection;
using Infrastructure;
using Islands;
using Mdazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MonorailCss;
using Pennington.UI.Components;

/// <summary>DI extension methods for registering and running the DocSite template.</summary>
public static class DocSiteServiceExtensions
{
    /// <summary>Registers DocSite services with the provided options.</summary>
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

            // llms.txt generation: markdown content uses the rendition channel; non-markdown
            // (Razor pages, API symbol pages) falls back to HTTP-fetching the rendered page
            // and scoping via ContentSelector to strip layout chrome. Both selectors default
            // to #main-content and are overridable through DocSiteOptions.
            var llmsSelector = options.LlmsTxtContentSelector ?? "#main-content";
            penn.AddLlmsTxt(opts => opts.ContentSelector ??= llmsSelector);
            penn.SearchIndex.ContentSelector ??= options.SearchIndexContentSelector ?? "#main-content";

            // Localization
            options.ConfigureLocalization?.Invoke(penn.Localization);

            // Scan the entry assembly (the app) plus any explicitly configured assemblies.
            // Dedup so a user who passes their entry assembly explicitly doesn't get the
            // same assembly enumerated twice by RazorPageContentService (which would emit
            // every @page route twice and trip the duplicate-route warning).
            penn.AdditionalRoutingAssemblies = BuildRoutingAssemblies(options);

            // Last: give the app a chance to wire additional content sources,
            // highlighters, islands, etc. Runs after DocSite's own defaults so
            // users can still override anything DocSite set above.
            options.ConfigurePennington?.Invoke(penn);
        });

        // Make Pennington.UI components available inline in markdown via Mdazor.
        // <CodeBlock> is intentionally excluded: markdown authors should use fenced
        // code blocks, not a component round-trip through Mdazor+Markdig.
        services.AddMdazorComponent<Badge>()
                .AddMdazorComponent<BigTable>()
                .AddMdazorComponent<Card>()
                .AddMdazorComponent<CardGrid>()
                .AddMdazorComponent<LinkCard>()
                .AddMdazorComponent<RenderedFixture>()
                .AddMdazorComponent<Step>()
                .AddMdazorComponent<Steps>();

        // MonorailCSS
        services.AddMonorailCss(sp =>
        {
            var options = sp.GetRequiredService<DocSiteOptions>();

            var monoOptions = new MonorailCssOptions
            {
                ColorScheme = options.ColorScheme ?? new MonorailCssOptions().ColorScheme,
                SyntaxTheme = options.SyntaxTheme ?? SyntaxTheme.Default,
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

    /// <summary>Wires DocSite middleware and Razor components into the request pipeline.</summary>
    public static WebApplication UseDocSite(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<DocSiteOptions>();

        app.UsePenningtonLocaleRouting();
        app.UseAntiforgery();
        app.UseStaticFiles();
        app.UseMonorailCss();
        app.UseSpaNavigation();
        // UsePennington wires the redirect middleware; call it before mapping
        // the Razor component endpoint so `redirectUrl:` pages short-circuit
        // with 301 instead of falling through to the catch-all page route.
        app.UsePennington();
        // The runtime Blazor router needs the same augmented assembly list the static
        // content service uses, otherwise a consumer's `@page` (and its `@layout`
        // directive) is invisible at runtime — the catch-all Pages.razor would win and
        // strip the layout by rendering via DynamicComponent.
        app.MapRazorComponents<Components.App>()
            .AddAdditionalAssemblies(BuildRoutingAssemblies(options));

        return app;
    }

    /// <summary>Runs the DocSite: either serves the app or performs a static build, based on command-line args.</summary>
    public static async Task RunDocSiteAsync(this WebApplication app, string[] args)
    {
        await app.RunOrBuildAsync(args);
    }

    private static Assembly[] BuildRoutingAssemblies(DocSiteOptions options)
    {
        var entry = Assembly.GetEntryAssembly();
        var seen = new HashSet<Assembly>();
        var result = new List<Assembly>(options.AdditionalRoutingAssemblies.Length + 1);

        if (entry is not null && seen.Add(entry))
            result.Add(entry);

        foreach (var asm in options.AdditionalRoutingAssemblies)
        {
            if (seen.Add(asm))
                result.Add(asm);
        }

        return result.ToArray();
    }
}