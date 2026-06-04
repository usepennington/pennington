using ExtensibilityLabExample;
using Markdig;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Markdown.Extensions;
using Pennington.Markdown.Shortcodes;
using Pennington.MonorailCss;
using Pennington.Pipeline;
using Pennington.Routing;
using Pennington.Taxonomy;

var builder = WebApplication.CreateBuilder(args);

// Bare AddPennington host. Each extension registration below is a fence
// target for the matching §2.3 Extensibility how-to — see Program.cs
// comments next to each service.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Extensibility Lab";
    penn.ContentRootPath = "Content";
    // Bare-host CanonicalBaseUrl lets the framework auto-emit
    // <link rel="canonical"> on every rendered page; DiagnosticsEmittingProcessor
    // (the in-project canonical-tag check) finds the tag and stays quiet.
    penn.CanonicalBaseUrl = "https://example.com";

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

    // 2.2.50 Tabbed-code class-name override — swap the default tab-*
    // classes for the lab-tabs-* variants styled via MonorailCssCustomization.
    TabbedCodeBlockStyling.ConfigureTabbedCodeBlocksOverride(penn);

    // 2.2.55 Site projection selector — the Lab's minimal HTML template wraps
    // content in <article>, so strip the surrounding chrome before indexing /
    // sidecar extraction. Shared by search and llms.txt.
    penn.SiteProjection.ContentSelector = "article";

    // 2.2.65 Markdig pipeline hook — register a custom [[wiki-link]] inline parser.
    // ConfigureMarkdownPipeline runs after every built-in extension (UseAdvancedExtensions
    // already supplies math, footnotes, definition lists, …), so add only the parser the
    // built-ins lack. AddIfNotAlready keeps the registration idempotent.
    penn.ConfigureMarkdownPipeline = (pipeline, _) =>
        pipeline.Extensions.AddIfNotAlready(new WikiLinkExtension());

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

// 2.3.15 Make the custom records discoverable — each ReleaseEntry is attached as
// DiscoveredItem.Metadata, so the engine treats them like markdown records. A browse-by-channel
// taxonomy walks them directly (no FileSource required); the `channel` search facet and
// per-page JSON-LD come from the same record. Taxonomy term pages render through Razor components,
// so the bare host opts into AddRazorComponents + AddHttpContextAccessor the way the how-to shows.
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTaxonomy<ReleaseEntry, string>(opts =>
{
    opts.BaseUrl = "/channel";
    opts.SelectKey = fm => fm.Channel;
    opts.IndexPage = typeof(ChannelIndex);
    opts.TermPage = typeof(ChannelTerm);
});

// 2.3.90 Emit-only IContentService — writes robots.txt and nothing else.
builder.Services.AddTransient<IContentService, RobotsTxtContentService>();

// 2.3.20 Code-block preprocessor — handles "linecount" fences.
builder.Services.AddSingleton<ICodeBlockPreprocessor, LineCountPreprocessor>();

// 2.3.25 Custom shortcode — turns <?# GitHubRepo "owner/repo" /?> into a link.
// The framework ships AssemblyVersionShortcode (<?# Version /?>) by default;
// any IShortcode registered after AddPennington joins the same dispatch table.
builder.Services.AddSingleton<IShortcode, GitHubRepoShortcode>();

// 2.3.40 Response processor — injects the feedback widget.
builder.Services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>();

// 2.3.45 Diagnostics-emitting response processor — injects DiagnosticContext
// and records a warning when a page lacks a canonical link; every diagnostic
// surfaces in the X-Pennington-Diagnostic header and the dev overlay.
builder.Services.AddScoped<IResponseProcessor, DiagnosticsEmittingProcessor>();

// 2.3.50 HTML response rewriter — PreParseAsync strips a sentinel comment,
// ApplyAsync lowercases marked anchor text.
builder.Services.AddSingleton<IHtmlResponseRewriter, AnchorLowercaseRewriter>();

// 2.2.30 MonorailCSS — utility CSS registers an endpoint the Playwright
// smoke test can fetch, and gives the injected chart/feedback HTML a
// place to theme. MonorailCssCustomization.BuildOptions pairs the color
// scheme with a CustomCssFrameworkSettings delegate fenced by
// how-to/configuration/monorail-css.
builder.Services.AddMonorailCss(_ => MonorailCssCustomization.BuildOptions());

var app = builder.Build();

app.UsePennington();
app.UseMonorailCss();

// Live HTTP handlers for the browse-by-channel taxonomy (/channel/ and /channel/{slug}/).
app.MapTaxonomy<ReleaseEntry, string>();

// Single catch-all fallback. Dispatches markdown via the pipeline and
// release notes via ReleaseNotesContentService so the route comes from
// one place — the discovery list — and phase-7 MapGet reuse does not
// duplicate fetches for the crawler.
app.MapGet("/{*path}", async (
    string? path,
    IPageResolver resolver,
    ReleaseNotesContentService releases) =>
{
    var trimmed = (path ?? string.Empty).Trim('/');
    var requested = new UrlPath("/" + trimmed + (trimmed.Length == 0 ? "" : "/"));

    // Release-notes index
    if (trimmed.Equals("releases", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Content(RenderReleaseIndex(releases), "text/html");
    }

    // Release-notes detail pages
    if (trimmed.StartsWith("releases/", StringComparison.OrdinalIgnoreCase))
    {
        var version = trimmed["releases/".Length..];
        var entry = releases.TryGet(version);
        if (entry is not null)
        {
            return Results.Content(RenderReleaseEntry(entry), "text/html");
        }
    }

    // Markdown pages — IPageResolver finds the matching content route and renders
    // it. Non-markdown sources (release notes, robots.txt) never resolve to a
    // rendered page, so they fall through to the 404.
    if (await resolver.ResolveAsync(requested) is { } page)
    {
        return Results.Content(RenderMarkdownPage(page), "text/html");
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
      <article id="main-content" data-spa-region="content">
        <h1>{{item.Metadata.Title}}</h1>
        {{item.Content.Html}}
      </article>
    </body>
    </html>
    """;

static string RenderReleaseIndex(ReleaseNotesContentService releases)
{
    var items = string.Join("\n", releases.Entries.Select(e =>
        $"""<li><a href="/releases/{e.Version}/">{e.Title}</a> <time datetime="{e.Date:yyyy-MM-dd}">{e.Date:yyyy-MM-dd}</time> ({e.Channel})</li>"""));
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
            <p><time datetime="{{entry.Date:yyyy-MM-dd}}">{{entry.Date:yyyy-MM-dd}}</time> · {{entry.Channel}}</p>
            <ul>{{highlights}}</ul>
            <p><a href="/releases/" data-lowercase>BACK TO INDEX</a></p>
          </article>
        </body>
        </html>
        """;
}

public partial class Program;