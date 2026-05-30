using System.Text;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Routing;
using ScaleStressExample;

// Populate Content/ with 5000 generated markdown files on first launch.
// Subsequent runs are no-ops once the target count is on disk.
await CorpusGenerator.EnsureAsync(
    outputDir: "Content",
    corpusDir: "../_shared/Bramble/Content");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Scale Stress Example";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

var app = builder.Build();

app.UsePennington();

// Index: list every discovered document. Uses the cached TOC channel so the
// listing is cheap to render even with thousands of files.
app.MapGet("/", async (IEnumerable<IContentService> services) =>
{
    var sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\" /><title>Scale Stress Example</title></head><body>");
    sb.AppendLine("<h1>Scale Stress Example</h1>");

    var total = 0;
    var listSection = new StringBuilder("<ul>");
    foreach (var service in services)
    {
        var entries = await service.GetIndexableEntriesAsync();
        foreach (var entry in entries)
        {
            total++;
            var url = entry.Route.CanonicalPath.Value;
            listSection.Append("<li><a href=\"").Append(url).Append("\">")
                .Append(System.Net.WebUtility.HtmlEncode(entry.Title))
                .Append("</a> <code>").Append(url).Append("</code></li>");
        }
    }
    listSection.Append("</ul>");

    sb.Append("<p>").Append(total).Append(" documents.</p>");
    sb.Append(listSection);
    sb.AppendLine("</body></html>");
    return Results.Content(sb.ToString(), "text/html");
});

// Page: resolve the matching markdown file and render it.
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
          <p><a href="/">&larr; index</a></p>
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
