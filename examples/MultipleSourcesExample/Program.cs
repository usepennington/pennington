using MultipleSourcesExample;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;
using DocFrontMatter = Pennington.FrontMatter.DocFrontMatter;
using BlogFrontMatter = MultipleSourcesExample.BlogFrontMatter;

var builder = WebApplication.CreateBuilder(args);

// Bare AddPennington host with two AddMarkdownContent<T> calls pointed at
// different content roots and different front-matter types. Step 4 of
// how-to/configuration/multiple-sources fences RegisterDocSource here;
// step 5 fences RegisterBlogSource; step 6 (optional) fences the
// ExcludePaths variant. The overlap branch is wired via the
// MULTIPLE_SOURCES_OVERLAP env var so one project can demonstrate both
// shapes without duplicating the host.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Multiple Sources";
    penn.ContentRootPath = "Content";

    if (Environment.GetEnvironmentVariable("MULTIPLE_SOURCES_OVERLAP") == "1")
    {
        penn.AddMarkdownContent<DocFrontMatter>(ServiceConfiguration.RegisterOverlappingDocSource);
    }
    else
    {
        penn.AddMarkdownContent<DocFrontMatter>(ServiceConfiguration.RegisterDocSource);
    }

    penn.AddMarkdownContent<BlogFrontMatter>(ServiceConfiguration.RegisterBlogSource);
});

var app = builder.Build();

app.UsePennington();

// Single catch-all route that walks every registered IContentService and
// renders the first match. Both markdown sources surface as IContentService
// instances, so the loop resolves /docs/* against the doc source and /blog/*
// against the blog source from one place.
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
            if (discovered.Source is not MarkdownFileSource) continue;

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

public partial class Program;
