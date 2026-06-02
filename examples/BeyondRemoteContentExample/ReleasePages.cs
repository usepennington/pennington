namespace BeyondRemoteContentExample;

using Pennington.Pipeline;
using Pennington.Routing;

/// <summary>Turns release records into the HTML the <c>MapGet</c> endpoint serves.</summary>
public static class ReleasePages
{
    /// <summary>
    /// Renders one release's markdown body to HTML through the same Markdig pipeline
    /// markdown files use, then wraps it in the <c>&lt;article&gt;</c> the projection
    /// selector targets. <see cref="EndpointSource"/> bodies are not auto-rendered, so
    /// the endpoint invokes <see cref="IContentRenderer"/> itself; the emitted headings
    /// are what let the build's self-fetch index the page for search and llms.txt.
    /// </summary>
    public static async Task<string> RenderDetailAsync(IContentRenderer renderer, ReleaseEntry entry)
    {
        var route = ContentRouteFactory.FromUrl(new UrlPath($"/releases/{entry.Version}/"));
        var parsed = new ParsedItem(route, new ReleaseFrontMatter(entry.Title), entry.BodyMarkdown ?? "");
        var rendered = await renderer.RenderAsync(parsed);

        var body = rendered.Value is RenderedItem item
            ? item.Content.Html
            : "<p>Release notes are unavailable.</p>";

        return Page(entry.Title, $"""
            <p class="meta">
              <time datetime="{entry.Date:yyyy-MM-dd}">{entry.Date:yyyy-MM-dd}</time>
              &middot; <a href="{entry.HtmlUrl}">View on GitHub</a>
            </p>
            {body}
            """);
    }

    /// <summary>Renders the index listing every release, newest first.</summary>
    public static string RenderIndex(IEnumerable<ReleaseEntry> entries)
    {
        var items = string.Join("\n", entries.Select(e =>
            $"""<li><a href="/releases/{e.Version}/">{e.Title}</a></li>"""));
        return Page("Releases", $"<ul>{items}</ul>");
    }

    // The page shell. Content sits inside <article> — the value set as
    // SiteProjection.ContentSelector in Program.cs — so the build's self-fetch indexes
    // the release body for search and llms.txt instead of the surrounding chrome.
    private static string Page(string title, string bodyHtml) => $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>{{title}}</title>
        </head>
        <body>
          <main>
            <article data-spa-region="content">
              <h1>{{title}}</h1>
              {{bodyHtml}}
            </article>
          </main>
        </body>
        </html>
        """;
}
