using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

// 1. Register the Pennington content pipeline. Point ContentRootPath at the
//    folder of markdown files and declare one markdown source.
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

// 2. Wire the Pennington middleware (static files for Content/, live-reload,
//    response processing, and auto-registered endpoints like /sitemap.xml).
app.UsePennington();

// 3. Serve any URL by walking the configured IContentService instances, parsing
//    the matching markdown file, and rendering it through the pipeline. This
//    is deliberately minimal: in later tutorials the DocSite template provides
//    its own Razor layout and routing.
app.MapGet("/{*path}", async (
    string? path,
    IEnumerable<IContentService> services,
    IContentParser parser,
    IContentRenderer renderer) =>
{
    var requested = new UrlPath("/" + (path ?? string.Empty).Trim('/'));

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

// 4. Dev mode (`dotnet run`) serves live; build mode
//    (`dotnet run -- build <baseUrl> <outputDir>`) crawls the running app and
//    writes static HTML. Both args are optional; defaults are `/` and `output`.
await app.RunOrBuildAsync(args);