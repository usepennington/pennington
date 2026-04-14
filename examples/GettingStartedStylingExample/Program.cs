using GettingStartedStylingExample;
using MonorailCss.Theme;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

// Register Pennington and a single markdown source, as in tutorial 1.1.20.
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

// Register MonorailCSS. The NamedColorScheme below picks which named Tailwind
// palettes resolve behind the `primary`, `accent`, `tertiary-one`,
// `tertiary-two`, and `base` utility prefixes. Change `PrimaryColorName` and
// the next stylesheet regeneration reflects the new palette.
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

// Map `/styles.css`. The endpoint returns the currently-collected utility
// classes as a real stylesheet; `CssClassCollectorProcessor` watches every
// rendered HTML response and adds any new class tokens it finds.
app.UseMonorailCss();

// Serve any URL by walking the configured IContentService instances, parsing
// the matching markdown file, rendering it, and wrapping the result in the
// shared layout (which is the only place utility classes live by default).
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
