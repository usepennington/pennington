namespace GettingStartedStylingExample;

using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Stage 2 — register MonorailCSS in DI with <c>AddMonorailCss</c> and pick a
/// <c>NamedColorScheme</c>. The host does not yet map <c>/styles.css</c>, so
/// pages still render unstyled; this stage exists purely so the tutorial can
/// show the DI-side wiring in isolation before the endpoint is mounted in
/// <see cref="Stage3"/>. Tutorial prose extracts the body of <see cref="Run"/>
/// via <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage2
{
    /// <summary>Register MonorailCSS services and a NamedColorScheme.</summary>
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
                PrimaryColorName = ColorName.Indigo,
                AccentColorName = ColorName.Pink,
                TertiaryOneColorName = ColorName.Cyan,
                TertiaryTwoColorName = ColorName.Amber,
                BaseColorName = ColorName.Slate,
            },
        });

        var app = builder.Build();

        app.UsePennington();

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