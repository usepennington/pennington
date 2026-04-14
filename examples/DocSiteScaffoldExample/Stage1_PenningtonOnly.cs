namespace DocSiteScaffoldExample;

using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>
/// Stage 1 — the "bare Pennington" shape the reader arrives with from
/// tutorial 1. `AddPennington` registers the content pipeline and a
/// `MapGet` fallback walks the configured <see cref="IContentService"/>
/// instances to render pages. No DocSite template, no sidebar, no chrome.
/// Tutorial prose extracts the body of <see cref="Run"/> via
/// <c>xmldocid,bodyonly</c>. This class is never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>The pre-DocSite host — Pennington core with a plain fallback.</summary>
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddPennington(penn =>
        {
            penn.SiteTitle = "Scaffold Docs";
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

                    return Results.Content(renderedItem.Content.Html, "text/html");
                }
            }
            return Results.NotFound();
        });

        await app.RunOrBuildAsync(args);
    }
}
