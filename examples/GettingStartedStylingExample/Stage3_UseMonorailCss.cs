namespace GettingStartedStylingExample;

using MonorailCss.Theme;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Stage 3 — the final wired state. Adds <c>app.UseMonorailCss()</c> on top of
/// <see cref="Stage2"/>'s DI registration, which mounts <c>/styles.css</c>.
/// The layout's <c>&lt;link rel="stylesheet" href="/styles.css"&gt;</c> now
/// fetches a real stylesheet populated with every utility class the response
/// collector has observed. This is identical to the top-level <c>Program.cs</c>.
/// Tutorial prose extracts the body of <see cref="Run"/> via
/// <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage3
{
    /// <summary>Run the fully styled host — AddMonorailCss + UseMonorailCss + utility layout.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddPennington(penn =>
        {
            penn.SiteTitle = "My Styled Pennington Site";
            penn.ContentRootPath = "Content";

            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "/";
            });
        });

        builder.Services.AddMonorailCss(_ => new MonorailCssOptions
        {
            ColorScheme = new NamedColorScheme
            {
                PrimaryColorName = ColorNames.Indigo,
                AccentColorName = ColorNames.Pink,
                TertiaryOneColorName = ColorNames.Cyan,
                TertiaryTwoColorName = ColorNames.Amber,
                BaseColorName = ColorNames.Slate,
            },
        });

        var app = builder.Build();

        app.UsePennington();
        app.UseMonorailCss();

        app.MapGet("/{*path}", async (
            string? path,
            IEnumerable<IContentService> services,
            IContentParser parser,
            IContentRenderer renderer,
            NavigationBuilder navigation) =>
        {
            var requested = new UrlPath("/" + (path ?? string.Empty).Trim('/'));

            var tocItems = new List<ContentTocItem>();
            foreach (var service in services)
            {
                var entries = await service.GetIndexableEntriesAsync();
                tocItems.AddRange(entries);
            }
            var navTree = navigation.BuildTree(tocItems);

            foreach (var service in services)
            {
                await foreach (var discovered in service.DiscoverAsync())
                {
                    if (!discovered.Route.CanonicalPath.Matches(requested)) continue;

                    var parsed = await parser.ParseAsync(discovered);
                    if (parsed is not ParsedItem parsedItem) continue;

                    var rendered = await renderer.RenderAsync(parsedItem);
                    if (rendered is not RenderedItem renderedItem) continue;

                    var html = Layout.Render(renderedItem.Metadata.Title, navTree, renderedItem.Content.Html);
                    return Results.Content(html, "text/html");
                }
            }

            return Results.NotFound();
        });

        await app.RunOrBuildAsync(args);
    }
}