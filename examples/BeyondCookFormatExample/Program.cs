using BeyondCookFormatExample;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

// CookContentRenderer renders a Razor component via Blazor's HtmlRenderer, which needs the
// component services AddRazorComponents registers.
builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Cook Format Example";
    penn.ContentRootPath = "Content";

    // A markdown landing page. Markdown and the custom "cook" format coexist on the
    // same site, both flowing through the dispatching parse -> render pipeline.
    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });

    // Register ".cook" (Cooklang) as a first-class content format. Recipes under recipes/
    // are discovered (by the built-in FileContentService), parsed by CookContentParser, and
    // rendered by CookContentRenderer — the same discover -> parse -> render path markdown uses.
    penn.AddContentFormat<CookFrontMatter>("cook", cook =>
    {
        cook.ContentPath = "recipes";
        cook.FilePattern = "*.cook";
        cook.BasePageUrl = "/recipes";
        cook.SectionLabel = "Recipes";
    })
    .UseParser<CookContentParser>()
    .UseRenderer<CookContentRenderer>();
});

var app = builder.Build();

app.UsePennington();

// One catch-all resolves any URL — markdown pages and cook recipes alike — through the
// dispatching pipeline, then wraps the rendered HTML in minimal chrome.
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
          <main>
            {page.Content.Html}
          </main>
        </body>
        </html>
        """;
    return Results.Content(html, "text/html");
});

await app.RunOrBuildAsync(args);

/// <summary>Exposed so an integration-test host factory can boot this site.</summary>
public partial class Program;
