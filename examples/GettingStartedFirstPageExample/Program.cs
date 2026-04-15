using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

// Register Pennington and declare a single markdown source. Every file under
// Content/ becomes a page; the file path (minus `.md`) becomes its URL.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "My First Pennington Site";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

var app = builder.Build();

app.UsePennington();

// Serve any URL by walking the configured IContentService instances, parsing
// the matching markdown file, and rendering it through the pipeline. A tiny
// nav strip is built from NavigationBuilder so the tutorial can point at the
// auto-assembled list of pages as more files land on disk.
app.MapGet("/{*path}", async (
    string? path,
    IEnumerable<IContentService> services,
    IContentParser parser,
    IContentRenderer renderer,
    NavigationBuilder navigation) =>
{
    var requested = new UrlPath("/" + (path ?? string.Empty).Trim('/'));

    var tocItems = new List<Pennington.Content.ContentTocItem>();
    foreach (var service in services)
    {
        var entries = await service.GetIndexableEntriesAsync();
        tocItems.AddRange(entries);
    }
    var navTree = navigation.BuildTree(tocItems);
    var navHtml = string.Join(
        "",
        navTree.Select(item =>
            $"<li><a href=\"{item.Route.CanonicalPath.Value}\">{item.Title}</a></li>"));

    foreach (var service in services)
    {
        await foreach (var discovered in service.DiscoverAsync())
        {
            if (!discovered.Route.CanonicalPath.Matches(requested)) continue;

            var parsed = await parser.ParseAsync(discovered);
            if (parsed is not ParsedItem parsedItem) continue;

            var rendered = await renderer.RenderAsync(parsedItem);
            if (rendered is not RenderedItem renderedItem) continue;

            var html = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <title>{renderedItem.Metadata.Title}</title>
                </head>
                <body>
                  <nav><ul>{navHtml}</ul></nav>
                  <article>
                    <h1>{renderedItem.Metadata.Title}</h1>
                    {renderedItem.Content.Html}
                  </article>
                </body>
                </html>
                """;
            return Results.Content(html, "text/html");
        }
    }

    return Results.NotFound();
});

await app.RunOrBuildAsync(args);