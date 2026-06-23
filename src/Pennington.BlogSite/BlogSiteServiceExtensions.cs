namespace Pennington.BlogSite;

using System.Reflection;
using Content;
using Infrastructure;
using Mdazor;
using Taxonomy;
using Pennington.Head;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MonorailCss;
using Pennington.UI.Components;
using Pennington.UI.Styling;
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

        // RSS alternate link — was literal markup in App.razor, now a head contributor.
        services.AddHeadContributor<BlogSiteHeadContributor>();

        services.AddPennington(penn =>
        {
            penn.SiteTitle = options.SiteTitle;
            penn.SiteDescription = options.SiteDescription;
            penn.CanonicalBaseUrl = options.CanonicalBaseUrl;
            penn.ContentRootPath = options.ContentRootPath;
            penn.MapSitemap = options.EnableSitemap;
            // Forwarded so StructuredDataHtmlRewriter can fill the author on a post's JSON-LD
            // when the front matter names none — mirrors the old per-page FallbackAuthorName.
            penn.StructuredDataAuthorName = options.AuthorName;
            penn.SocialCards = options.SocialCards;
            penn.StandardSite = options.StandardSite;
            penn.Favicons = options.Favicons;

            var blogContentPath = Path.Combine(options.ContentRootPath.Value, options.BlogContentPath);
            penn.AddMarkdownContent<BlogSiteFrontMatter>(md =>
            {
                md.ContentPath = blogContentPath;
                md.BasePageUrl = options.BlogBaseUrl;
                // The not-found body lives at the content root (Content/404.md), outside this
                // blog source; reserve it here too in case a host points the blog source at the
                // content root (BlogContentPath = "") so a 404.md never becomes a post route.
                md.ReserveNotFoundPage = true;
            });

            // The BlogSite ships Home/Archive/Tag/Tags/Blog Razor pages inside
            // Pennington.BlogSite.dll. RazorPageContentService must scan that
            // assembly to discover them, otherwise the generated site is missing
            // its root, archive, and tag listings. The entry assembly and any
            // explicit extras come from the shared routing set (BuildRoutingAssemblies)
            // so build discovery and the runtime router agree on what is routable.
            var blogSiteAssembly = typeof(BlogSiteServiceExtensions).Assembly;
            var routingAssemblies = BuildRoutingAssemblies(options);
            penn.AdditionalRoutingAssemblies = Array.IndexOf(routingAssemblies, blogSiteAssembly) >= 0
                ? routingAssemblies
                : [blogSiteAssembly, .. routingAssemblies];
        });

        // Source-generated YAML metadata for BlogSite's front-matter type (reflection fallback otherwise).
        services.AddYamlContext(BlogSiteYamlContext.Default);

        // Make Pennington.UI components available inline in markdown via Mdazor.
        // <CodeBlock> is intentionally excluded: markdown authors should use fenced
        // code blocks, not a component round-trip through Mdazor+Markdig.
        services.AddMdazorComponent<Badge>()
                .AddMdazorComponent<BigTable>()
                .AddMdazorComponent<Card>()
                .AddMdazorComponent<CardGrid>()
                .AddMdazorComponent<Checkpoint>()
                .AddMdazorComponent<LinkCard>()
                .AddMdazorComponent<Step>()
                .AddMdazorComponent<Steps>();

        // Re-invoke the user's factory per resolve (rather than reading the singleton snapshot)
        // so edits to Program.cs flow into the served stylesheet. The MonorailCSS option factory
        // is registered transient by AddMonorailCss, so this lambda runs on every /styles.css
        // request.
        services.AddMonorailCss(_ => BuildMonorailOptions(configureOptions()));

        // Pennington.UI components style themselves (inline defaults + a Variant param); a
        // per-instance *Class param Tailwind-merges over that base via ClassMerge, run through
        // BlogSite's own MonorailCSS framework so the palette and custom utilities define the
        // conflicts. Lazy: most renders pass no *Class, so the framework is only built on first use.
        var styleMerger = new Lazy<Func<string, string, string>>(
            () => MonorailCssService.CreateClassMerger(BuildMonorailOptions(configureOptions())));
        services.AddSingleton(new ClassMerge(
            (baseClasses, overrideClasses) => styleMerger.Value(baseClasses, overrideClasses)));

        // Not-found body resolver (content-root 404.md). Post listings, single-post rendering, and RSS
        // come from the shared core BlogPostQuery (registered by AddPennington) over the cached records.
        services.AddTransient<BlogContentResolver>();

        // Browse-by-tag via the core taxonomy subsystem. The @page Tags/Tag components render the index
        // and term pages with full chrome and search indexing; this axis supplies term data (via
        // TaxonomyAccessor), route discovery, and cross-references. MapTaxonomy is intentionally not
        // called — the @page routes serve the HTML.
        services.AddTaxonomy<BlogSiteFrontMatter, string>(tax =>
        {
            tax.BaseUrl = "/tags";
            tax.SelectKeys = fm => fm.Tags;
            tax.IndexPage = typeof(Components.Pages.Tags);
            tax.TermPage = typeof(Components.Pages.Tag);
        });

        // Emits the paginated archive routes the static crawler can't auto-discover and projects the
        // template-owned home-page record (for social-card generation).
        services.AddSingleton<IContentService, BlogSiteContentService>();

        return services;

        // Builds the MonorailCSS options BlogSite renders with. Shared by the stylesheet factory
        // and the component class merger (ClassMerge) so both reason about the same utility model.
        static MonorailCssOptions BuildMonorailOptions(BlogSiteOptions options) =>
            new()
            {
                ColorScheme = options.ColorScheme ?? new MonorailCssOptions().ColorScheme,
                ExtraStyles = options.ExtraStyles ?? string.Empty,
            };
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
        // The runtime Blazor router needs the same augmented assembly list the
        // static content service uses, otherwise a consumer's @page in the host
        // project is invisible at runtime — the bundled pages win or the route
        // 404s. The BlogSite library is already the App assembly, so it is
        // excluded here (passing it to AddAdditionalAssemblies double-registers).
        app.MapRazorComponents<Components.App>()
            .AddAdditionalAssemblies(BuildRoutingAssemblies(options));

        if (options.EnableRss)
        {
            // Static crawler picks this up via DiscoverMapGetRoutes, so /rss.xml
            // lands both in dev-server responses and in the generated output.
            app.MapGet("/rss.xml", async (BlogPostQuery posts) =>
                Results.Content(
                    await posts.GetRssXmlAsync<BlogSiteFrontMatter>(
                        options.SiteTitle, options.SiteDescription, options.CanonicalBaseUrl,
                        options.BlogBaseUrl, fm => fm.Author),
                    "application/xml"));
        }

        return app;
    }

    /// <summary>Runs the BlogSite: either serves the app or performs a static build, based on command-line args.</summary>
    public static async Task RunBlogSiteAsync(this WebApplication app, string[] args)
    {
        await app.RunOrBuildAsync(args);
    }

    /// <summary>
    /// Builds the routing set beyond the App assembly: the entry assembly plus any
    /// configured extras (deduped). Shared by build-time discovery and the runtime
    /// router so both agree on which <c>@page</c> components are routable.
    /// </summary>
    private static Assembly[] BuildRoutingAssemblies(BlogSiteOptions options)
    {
        var entry = Assembly.GetEntryAssembly();
        var seen = new HashSet<Assembly>();
        var result = new List<Assembly>(options.AdditionalRoutingAssemblies.Length + 1);

        if (entry is not null && seen.Add(entry))
        {
            result.Add(entry);
        }

        foreach (var asm in options.AdditionalRoutingAssemblies)
        {
            if (seen.Add(asm))
            {
                result.Add(asm);
            }
        }

        return result.ToArray();
    }
}