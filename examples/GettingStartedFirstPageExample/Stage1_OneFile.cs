namespace GettingStartedFirstPageExample;

using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Stage 1 — a single markdown page (<c>Content/index.md</c>) is on disk.
/// The nav strip renders with exactly one entry, matching the page title from
/// front matter. Tutorial prose extracts the body of <see cref="Run"/> via
/// <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>Run a Pennington host with exactly one markdown page.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
            var navHtml = string.Join(
                "",
                navTree.Select(i =>
                    $"<li><a href=\"{i.Route.CanonicalPath.Value}\">{i.Title}</a></li>"));

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
    }
}