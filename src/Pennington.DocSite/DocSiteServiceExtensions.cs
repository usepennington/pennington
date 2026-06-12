namespace Pennington.DocSite;

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

        // The blog activates only when the content project has a `blog` folder with at
        // least one markdown article. Checked here (DI time, against the process working
        // directory) so the second content source, the `/rss.xml` endpoint, and the
        // header link are all wired conditionally.
        var blogContentDir = Path.GetFullPath(
            Path.Combine(options.ContentRootPath.Value, "blog"));
        var hasBlog = Directory.Exists(blogContentDir)
            && Directory.EnumerateFiles(blogContentDir, "*.md", SearchOption.AllDirectories).Any();
        services.AddSingleton(new BlogFeature(hasBlog));

        // Site-invariant discovery meta (og:site_name, default card image, RSS alternate) — was
        // literal markup in App.razor, now a head contributor.
        services.AddHeadContributor<DocSiteHeadContributor>();

        // Pennington core
        services.AddPennington(penn =>
        {
            penn.SiteTitle = options.SiteTitle;
            penn.SiteDescription = options.SiteDescription;
            penn.CanonicalBaseUrl = options.CanonicalBaseUrl;
            penn.ContentRootPath = options.ContentRootPath;
            penn.SocialCards = options.SocialCards;
            penn.StandardSite = options.StandardSite;

            penn.AddMarkdownContent<DocSiteFrontMatter>(md =>
            {
                md.ContentPath = options.ContentRootPath.Value;
                md.BasePageUrl = "/";
                // A content-root 404.md is the not-found body, rendered on demand by the
                // catch-all (Pages.razor) — never a routable /404/ page or a nav/sitemap entry.
                md.ReserveNotFoundPage = true;
                // Carve the blog subtree out of the doc source so blog posts aren't
                // double-discovered as documentation pages.
                if (hasBlog)
                {
                    md.ExcludePaths = ["blog"];
                }
            });

            // Blog posts: a separate markdown source parsed as BlogPostFrontMatter.
            // BlogPostFrontMatter.SearchOnly keeps posts indexed for search/llms/sitemap
            // while excluding them from the documentation sidebar.
            if (hasBlog)
            {
                penn.AddMarkdownContent<BlogPostFrontMatter>(md =>
                {
                    md.ContentPath = Path.Combine(options.ContentRootPath.Value, "blog");
                    md.BasePageUrl = "/blog";
                });
            }

            // Shared corpus projection: search, llms.txt sidecars, and build-time link audit
            // all consume the same RenderedPage stream, so the chrome-stripping selector lives
            // in one place. Default to #main-content — the DocSite layout's article wrapper.
            penn.SiteProjection.ContentSelector ??= options.ContentSelector ?? "#main-content";
            penn.AddLlmsTxt();

            // Boost search results by content area. Each area's boost defaults to its position in the
            // list (earlier areas weigh more, so task-oriented docs lead over reference when matches
            // are comparable) and can be overridden per area via ContentArea.SearchBoost.
            if (penn.SearchIndex.AreaPriorities.Count == 0 && options.Areas.Count > 0)
            {
                var areaCount = options.Areas.Count;
                for (var i = 0; i < areaCount; i++)
                {
                    var area = options.Areas[i];
                    var boost = area.SearchBoost ?? (areaCount - i) * 2;
                    penn.SearchIndex.AreaPriorities[area.Slug] = penn.SearchIndex.DefaultPriority + boost;
                }
            }

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

        // Source-generated YAML metadata for DocSite's front-matter types (reflection fallback otherwise).
        services.AddYamlContext(DocSiteYamlContext.Default);

        // Make Pennington.UI components available inline in markdown via Mdazor.
        // <CodeBlock> is intentionally excluded: markdown authors should use fenced
        // code blocks, not a component round-trip through Mdazor+Markdig.
        services.AddMdazorComponent<Badge>()
                .AddMdazorComponent<BigTable>()
                .AddMdazorComponent<Card>()
                .AddMdazorComponent<CardGrid>()
                .AddMdazorComponent<Checkpoint>()
                .AddMdazorComponent<LinkCard>()
                .AddMdazorComponent<RenderedFixture>()
                .AddMdazorComponent<Step>()
                .AddMdazorComponent<Steps>();

        // MonorailCSS — re-invoke the user's factory per resolve (rather than reading the
        // singleton snapshot) so edits to Program.cs flow into the served stylesheet. The
        // MonorailCSS option factory is registered transient by
        // AddMonorailCss, so this lambda runs on every /styles.css request.
        services.AddMonorailCss(_ => BuildMonorailOptions(configureOptions()));

        // Component style registry: DocSite's skin over the Pennington.UI defaults, with the
        // user's Styles merged on top. Like the MonorailCss factory above, the overrides func
        // re-invokes the user's options factory per resolve so Styles edits flow under dotnet
        // run; unknown keys throw here at startup, listing the valid catalog. The class merger
        // resolves override-vs-skin conflicts through DocSite's own MonorailCSS framework — built
        // lazily on the first override merge, since most hosts set no Styles.
        var styleMerger = new Lazy<Func<string, string, string>>(
            () => MonorailCssService.CreateClassMerger(BuildMonorailOptions(configureOptions())));
        services.AddPenningtonStyles(
            DocSiteStyleSkin.Styles,
            () => configureOptions().Styles,
            (baseClasses, overrideClasses) => styleMerger.Value(baseClasses, overrideClasses));

        // Content resolver
        services.AddTransient<Services.DocSiteContentResolver>();

        // Blog services. Post listings, single-post rendering, and RSS all come from the shared core
        // BlogPostQuery (registered by AddPennington) reading the cached content-record snapshot.
        if (hasBlog)
        {
            // Browse-by-tag via the core taxonomy subsystem. The @page tag components
            // (BlogTags/BlogTagPage) render them with full chrome and search indexing while this
            // axis supplies term data (via TaxonomyAccessor), route discovery, and cross-references.
            // MapTaxonomy is intentionally not called — the @page routes serve the HTML.
            services.AddTaxonomy<BlogPostFrontMatter, string>(tax =>
            {
                tax.BaseUrl = "/blog/tags";
                tax.SelectKeys = fm => fm.Tags;
                tax.IndexPage = typeof(Components.Pages.BlogTags);
                tax.TermPage = typeof(Components.Pages.BlogTagPage);
            });

            // Emits the /blog index route for the static crawler and declares the /blog llms subtree.
            services.AddSingleton<IContentService, Services.BlogContentService>();
        }

        return services;

        // Builds the MonorailCSS options DocSite renders with: the user's color scheme, syntax
        // theme, prose extensions, and extra styles, plus a cross-platform default font stack when
        // the consumer supplies none. Shared by the stylesheet factory and the style registry's
        // class merger so both reason about the same utility model.
        static MonorailCssOptions BuildMonorailOptions(DocSiteOptions options)
        {
            // Sensible cross-platform sans-serif stack used when the consumer
            // doesn't supply DisplayFontFamily / BodyFontFamily. Both `font-display`
            // and `font-sans` utilities must resolve to something — the docs site
            // chrome (headers, body) leans on them.
            const string defaultFontStack =
                "-apple-system, BlinkMacSystemFont, 'avenir next', avenir, 'segoe ui', " +
                "'helvetica neue', 'Adwaita Sans', Cantarell, Ubuntu, roboto, noto, " +
                "helvetica, arial, sans-serif";
            const string defaultMonoStack =
                "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', " +
                "'Courier New', monospace";
            var displayFont = options.DisplayFontFamily ?? defaultFontStack;
            var bodyFont = options.BodyFontFamily ?? defaultFontStack;
            var monoFont = options.MonoFontFamily ?? defaultMonoStack;
            var userCustomization = options.CustomCssFrameworkSettings ?? (settings => settings);

            return new MonorailCssOptions
            {
                ColorScheme = options.ColorScheme ?? new MonorailCssOptions().ColorScheme,
                SyntaxTheme = options.SyntaxTheme ?? SyntaxTheme.Default,
                ExtraStyles = options.ExtraStyles ?? string.Empty,
                // Register fonts first so the user's CustomCssFrameworkSettings
                // hook still gets the last word if it wants to swap them.
                CustomCssFrameworkSettings = settings => userCustomization(settings with
                {
                    Theme = settings.Theme
                        .AddFontFamily("display", displayFont)
                        .AddFontFamily("sans", bodyFont)
                        .AddFontFamily("mono", monoFont),
                }),
                ExtendProseCustomization = options.ExtendProseCustomization ?? (prose => prose),
            };
        }
    }

    /// <summary>Wires DocSite middleware and Razor components into the request pipeline.</summary>
    public static WebApplication UseDocSite(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<DocSiteOptions>();

        app.UseLocaleRouting();
        app.UseAntiforgery();
        app.UseStaticFiles();
        app.UseMonorailCss();
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

        // RSS feed for the blog. The static crawler picks up MapGet routes, so /rss.xml
        // lands in both dev-server responses and the generated output.
        if (app.Services.GetRequiredService<BlogFeature>().Enabled)
        {
            app.MapGet("/rss.xml", async (BlogPostQuery posts) =>
                Results.Content(
                    await posts.GetRssXmlAsync<BlogPostFrontMatter>(
                        options.SiteTitle, options.SiteDescription, options.CanonicalBaseUrl, "/blog",
                        fm => fm.Author),
                    "application/xml"));
        }

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