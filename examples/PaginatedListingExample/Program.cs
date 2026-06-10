using PaginatedListingExample;
using PaginatedListingExample.Components;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

// Bare AddPennington host with one markdown source under Content/articles/. The articles
// themselves are plain markdown; the paginated /articles listing on top of them is the
// recipe this example backs (how-to/discovery/pagination).
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Paginated Listing";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

// ArticleResolver collects the markdown articles and serves them one page at a time. It is a
// plain service (not an IContentService), so injecting IEnumerable<IContentService> here is safe.
builder.Services.AddTransient<ArticleResolver>();

// The custom listing service that emits the numbered /articles/page/N/ routes for the crawler.
// Registered as IContentService, so it resolves siblings lazily inside DiscoverAsync.
builder.Services.AddTransient<IContentService, ArticleListingContentService>();

// Blazor static rendering backs the ArticlesPage @page component.
builder.Services.AddRazorComponents();

var app = builder.Build();

// UsePennington first so its routes register before the Blazor and catch-all routes.
app.UsePennington();
app.UseAntiforgery();

// The listing page (/articles, /articles/page/N/) is a routed Razor component.
app.MapRazorComponents<App>();

// Individual articles (/articles/{slug}/) are markdown — resolve and render them through the
// pipeline. ArticlesPage's @page routes win for the listing URLs before this fallback runs.
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
