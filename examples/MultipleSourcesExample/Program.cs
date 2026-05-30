using MultipleSourcesExample;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Routing;
using BlogFrontMatter = MultipleSourcesExample.BlogFrontMatter;
using DocFrontMatter = Pennington.FrontMatter.DocFrontMatter;

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

// Opt the custom BlogFrontMatter into source-generated YAML metadata. DocFrontMatter is
// already covered by Pennington's built-in context; types with no context use reflection.
builder.Services.AddPenningtonYamlContext(BlogFrontMatterYamlContext.Default);

var app = builder.Build();

app.UsePennington();

// Single catch-all route that asks IPageResolver to render the first match.
// Both markdown sources surface as IContentService instances, so one resolver
// resolves /docs/* against the doc source and /blog/* against the blog source
// from one place.
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

await app.RunOrBuildAsync(args);

public partial class Program;