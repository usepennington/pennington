namespace Pennington.BlogSite;

using System.Reflection;
using Content;
using Infrastructure;
using Mdazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MonorailCss;
using Pennington.UI.Components;
using Services;

/// <summary>DI extension methods for registering and running the BlogSite template.</summary>
public static class BlogSiteServiceExtensions
{
    /// <summary>Registers BlogSite services with the provided options.</summary>
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

            // The BlogSite ships Home/Archive/Tag/Tags/Blog Razor pages inside
            // Pennington.BlogSite.dll. Without adding this assembly to the
            // routing set, RazorPageContentService cannot discover them and the
            // generated site is missing its root, archive, and tag listings.
            var blogSiteAssembly = typeof(BlogSiteServiceExtensions).Assembly;
            var appAssembly = Assembly.GetEntryAssembly();
            var assemblies = new List<Assembly> { blogSiteAssembly };
            if (appAssembly != null && appAssembly != blogSiteAssembly)
                assemblies.Add(appAssembly);
            foreach (var extra in options.AdditionalRoutingAssemblies)
            {
                if (!assemblies.Contains(extra))
                    assemblies.Add(extra);
            }
            penn.AdditionalRoutingAssemblies = assemblies.ToArray();
        });

        // Make Pennington.UI components available inline in markdown via Mdazor.
        // <CodeBlock> is intentionally excluded: markdown authors should use fenced
        // code blocks, not a component round-trip through Mdazor+Markdig.
        services.AddMdazorComponent<Badge>()
                .AddMdazorComponent<BigTable>()
                .AddMdazorComponent<Card>()
                .AddMdazorComponent<CardGrid>()
                .AddMdazorComponent<LinkCard>()
                .AddMdazorComponent<Step>()
                .AddMdazorComponent<Steps>();

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

        // Content service that yields per-tag routes and the /rss.xml file.
        // Reads markdown directly from disk instead of going through
        // BlogContentResolver so it doesn't take a circular dependency on the
        // still-initializing IContentService set during DiscoverAsync.
        services.AddFileWatched<BlogSiteContentService>();
        // Transient wrapper — AddSingleton here would capture the first
        // file-watched instance and never see a refresh, defeating the
        // AddFileWatched above.
        services.AddTransient<IContentService>(sp =>
            sp.GetRequiredService<BlogSiteContentService>());

        return services;
    }

    /// <summary>Wires BlogSite middleware, Razor components, and RSS endpoint into the request pipeline.</summary>
    public static WebApplication UseBlogSite(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<BlogSiteOptions>();

        app.UseAntiforgery();
        app.UseStaticFiles();
        app.UseMonorailCss();
        // UsePennington wires the redirect middleware; call it before mapping
        // the Razor component endpoint so `redirectUrl:` pages short-circuit
        // with 301 instead of falling through to the catch-all page route.
        app.UsePennington();
        app.MapRazorComponents<Components.App>()
            .AddAdditionalAssemblies(options.AdditionalRoutingAssemblies);

        if (options.EnableRss)
        {
            // Static crawler picks this up via DiscoverMapGetRoutes, so /rss.xml
            // lands both in dev-server responses and in the generated output.
            app.MapGet("/rss.xml", async (BlogSiteContentService service) =>
                Results.Content(await service.GetRssXmlAsync(), "application/xml"));
        }

        return app;
    }

    /// <summary>Runs the BlogSite: either serves the app or performs a static build, based on command-line args.</summary>
    public static async Task RunBlogSiteAsync(this WebApplication app, string[] args)
    {
        await app.RunOrBuildAsync(args);
    }
}