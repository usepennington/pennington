namespace Pennington.DocSite;

using System.Reflection;
using Content;
using Infrastructure;
using Mdazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

        // The blog activates only when the content project has a `blog` folder with at
        // least one markdown article. Checked here (DI time, against the process working
        // directory) so the second content source, the `/rss.xml` endpoint, and the
        // header link are all wired conditionally.
        var blogContentDir = Path.GetFullPath(
            Path.Combine(options.ContentRootPath.Value, "blog"));
        var hasBlog = Directory.Exists(blogContentDir)
            && Directory.EnumerateFiles(blogContentDir, "*.md", SearchOption.AllDirectories).Any();
        services.AddSingleton(new BlogFeature(hasBlog));

        // Pennington core
        services.AddPennington(penn =>
        {
            penn.SiteTitle = options.SiteTitle;
            penn.SiteDescription = options.SiteDescription;
            penn.CanonicalBaseUrl = options.CanonicalBaseUrl;
            penn.ContentRootPath = options.ContentRootPath;

            penn.AddMarkdownContent<DocSiteFrontMatter>(md =>
            {
                md.ContentPath = options.ContentRootPath.Value;
                md.BasePageUrl = "/";
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
        services.AddMonorailCss(_ =>
        {
            var options = configureOptions();

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

            var monoOptions = new MonorailCssOptions
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

            return monoOptions;
        });

        // Content resolver
        services.AddTransient<Services.ContentResolver>();

        // Blog services. The resolver renders posts with their own front matter; the
        // content service yields the blog index, tag index, and per-tag routes for the
        // static build and produces the RSS feed. Aliased to IContentService through a
        // transient indirection so each resolve sees the current file-watched instance.
        services.AddTransient<Services.BlogPostResolver>();
        if (hasBlog)
        {
            services.AddFileWatched<Services.BlogContentService>();
            services.AddTransient<IContentService>(sp =>
                sp.GetRequiredService<Services.BlogContentService>());
        }

        return services;
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
            app.MapGet("/rss.xml", async (Services.BlogContentService service) =>
                Results.Content(await service.GetRssXmlAsync(), "application/xml"));
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