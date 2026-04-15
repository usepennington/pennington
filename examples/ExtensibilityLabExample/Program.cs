using ExtensibilityLabExample;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.Markdown.Extensions;
using Pennington.MonorailCss;
using Pennington.Pipeline;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

// Bare AddPennington host. Each extension registration below is a fence
// target for the matching §2.3 Extensibility how-to — see Program.cs
// comments next to each service.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Extensibility Lab";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
        // Don't walk into releases/ — those are JSON and owned by
        // ReleaseNotesContentService.
        md.ExcludePaths = ["releases"];
    });

    // 2.3.30 Custom highlighter — priority 100 wins for "pipeline" fences.
    penn.Highlighting.AddHighlighter(new PipelineHighlighter());

    // 2.3.60 Island renderer — name matches data-spa-island="chart".
    penn.Islands.Register<ChartIslandRenderer>("chart");

    // 2.2.50 Tabbed-code class-name override — swap the default tab-*
    // classes for the lab-tabs-* variants styled via MonorailCssCustomization.
    TabbedCodeBlockStyling.ConfigureTabbedCodeBlocksOverride(penn);

    // 2.2.60 llms.txt on bare host — DocSite auto-wires this; bare
    // AddPennington consumers must opt in explicitly.
    penn.AddLlmsTxt(LlmsTxtConfiguration.Configure);
});

// 2.3.10 Custom IContentService — JSON-backed release notes.
// Register the concrete type once, then forward the IContentService role to
// the same instance so the endpoint below can inject the concrete service
// without the DI container newing up a second copy.
builder.Services.AddSingleton<ReleaseNotesContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<ReleaseNotesContentService>());

// 2.3.20 Code-block preprocessor — handles "linecount" fences.
builder.Services.AddSingleton<ICodeBlockPreprocessor, LineCountPreprocessor>();

// 2.3.40 Response processor — injects the feedback widget.
builder.Services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>();

// 2.3.45 Diagnostics-emitting response processor — injects DiagnosticContext
// and records a warning when a page lacks a canonical link; every diagnostic
// surfaces in the X-Pennington-Diagnostic header and the dev overlay.
builder.Services.AddScoped<IResponseProcessor, DiagnosticsEmittingProcessor>();

// 2.3.50 HTML response rewriter — PreParseAsync strips a sentinel comment,
// ApplyAsync lowercases marked anchor text.
builder.Services.AddSingleton<IHtmlResponseRewriter, AnchorLowercaseRewriter>();

// 2.3.60 (cont.) — wire SPA navigation so /_spa-data/{slug}.json is
// served and ChartIslandRenderer actually gets invoked.
builder.Services.AddScoped<ComponentRenderer>();
builder.Services.AddSpaNavigation();

// 2.2.30 MonorailCSS — utility CSS registers an endpoint the Playwright
// smoke test can fetch, and gives the injected chart/feedback HTML a
// place to theme. MonorailCssCustomization.BuildOptions pairs the color
// scheme with a CustomCssFrameworkSettings delegate fenced by
// how-to/configuration/monorail-css.
builder.Services.AddMonorailCss(_ => MonorailCssCustomization.BuildOptions());

var app = builder.Build();

app.UsePennington();
app.UseMonorailCss();
app.UseSpaNavigation();

// Single catch-all fallback. Dispatches markdown via the pipeline and
// release notes via ReleaseNotesContentService so the route comes from
// one place — the discovery list — and phase-7 MapGet reuse does not
// duplicate fetches for the crawler.
app.MapGet("/{*path}", async (
    string? path,
    IEnumerable<IContentService> services,
    IContentParser parser,
    IContentRenderer renderer,
    ReleaseNotesContentService releases) =>
{
    var trimmed = (path ?? string.Empty).Trim('/');
    var requested = new UrlPath("/" + trimmed + (trimmed.Length == 0 ? "" : "/"));

    // Release-notes index
    if (trimmed.Equals("releases", StringComparison.OrdinalIgnoreCase))
        return Results.Content(RenderReleaseIndex(releases), "text/html");

    // Release-notes detail pages
    if (trimmed.StartsWith("releases/", StringComparison.OrdinalIgnoreCase))
    {
        var version = trimmed["releases/".Length..];
        var entry = releases.TryGet(version);
        if (entry is not null)
            return Results.Content(RenderReleaseEntry(entry), "text/html");
    }

    // Markdown pages
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

            return Results.Content(RenderMarkdownPage(renderedItem), "text/html");
        }
    }

    return Results.NotFound();
});

await app.RunOrBuildAsync(args);
return;

// Helpers live at the end so Program.cs reads top-to-bottom.

static string RenderMarkdownPage(RenderedItem item) => $$"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <title>{{item.Metadata.Title}}</title>
      <link rel="stylesheet" href="/styles.css" />
    </head>
    <body>
      <article id="main-content" data-spa-island="content">
        <h1>{{item.Metadata.Title}}</h1>
        {{item.Content.Html}}
      </article>
    </body>
    </html>
    """;

static string RenderReleaseIndex(ReleaseNotesContentService releases)
{
    var items = string.Join("\n", releases.Entries.Select(e =>
        $"""<li><a href="/releases/{e.Version}/">{e.Title}</a> <time datetime="{e.Date}">{e.Date}</time></li>"""));
    return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <title>Releases</title>
          <link rel="stylesheet" href="/styles.css" />
        </head>
        <body>
          <article data-extensibility-lab="release-index">
            <h1>Releases</h1>
            <ul>{{items}}</ul>
          </article>
        </body>
        </html>
        """;
}

static string RenderReleaseEntry(ReleaseEntry entry)
{
    var highlights = string.Join("\n", entry.Highlights.Select(h => $"<li>{h}</li>"));
    return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <title>{{entry.Title}}</title>
          <link rel="stylesheet" href="/styles.css" />
        </head>
        <body>
          <article data-extensibility-lab="release-entry" data-release-version="{{entry.Version}}">
            <h1>{{entry.Title}}</h1>
            <p><time datetime="{{entry.Date}}">{{entry.Date}}</time></p>
            <ul>{{highlights}}</ul>
            <p><a href="/releases/" data-lowercase>BACK TO INDEX</a></p>
          </article>
        </body>
        </html>
        """;
}

public partial class Program;