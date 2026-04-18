using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "My Pennington Site";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

var app = builder.Build();

app.UsePennington();

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

await app.RunOrBuildAsync(args);
