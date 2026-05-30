using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
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

// 3. Serve any URL by asking IPageResolver to find the matching markdown file,
//    parse it, and render it through the pipeline. The resolver collapses the
//    discover->parse->render loop into one call; this host only decides what to
//    do with the result. In later tutorials the DocSite template provides its
//    own Razor layout and routing.
app.MapGet("/{*path}", async (string? path, IPageResolver resolver) =>
{
    var requested = new UrlPath(path ?? string.Empty).EnsureLeadingSlash();

    if (await resolver.ResolveAsync(requested) is not { } page)
    {
        return Results.NotFound();
    }

    var html = $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <title>{page.Metadata.Title}</title>
        </head>
        <body>
          <article>
            <h1>{page.Metadata.Title}</h1>
            {page.Content.Html}
          </article>
        </body>
        </html>
        """;
    return Results.Content(html, "text/html");
});

// 4. Dev mode (`dotnet run`) serves live; build mode
//    (`dotnet run -- build <baseUrl> <outputDir>`) crawls the running app and
//    writes static HTML. Both args are optional; defaults are `/` and `output`.
await app.RunOrBuildAsync(args);