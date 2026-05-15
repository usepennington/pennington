using EventMicrositeExample;
using EventMicrositeExample.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pennington.Content;
using Pennington.Data;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Pipeline;
using Pennington.Routing;
using Pennington.Taxonomy;

var builder = WebApplication.CreateBuilder(args);

// Conference microsite — a bare AddPennington host that combines:
//   * AddDataFile<T>  — sponsors.yml + schedule.yml as typed values consumed from Razor pages
//   * AddTaxonomy<T>  — /topic/{slug}/ (single-valued) + /tag/{slug}/ (multi-valued)
//   * AddMarkdownContent<TalkFrontMatter> — one markdown file per talk under Content/talks/
//
// Every talk is the same Markdown source feeding both taxonomy axes.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Devcon Microsite";
    penn.ContentRootPath = "Content";

    // Talks live as one markdown file per talk under Content/talks/.
    penn.AddMarkdownContent<TalkFrontMatter>(md =>
    {
        md.ContentPath = "Content/talks";
        md.BasePageUrl = "/talks";
        md.SectionLabel = "Talks";
    });
});

builder.Services.AddMonorailCss();

// HtmlRenderer needs Blazor's component services. AddRazorComponents wires
// them; AddHttpContextAccessor lets the HtmlRenderer resolve cascading values.
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();

// Data files — typed values from YAML, hot-reloaded when the file changes.
builder.Services.AddDataFile<List<Sponsor>>("sponsors", "data/sponsors.yml");
builder.Services.AddDataFile<Schedule>("schedule", "data/schedule.yml");

// Two browse axes against the same TalkFrontMatter:
//  /topic — single-valued projection from `topic:` front-matter key
//  /tag   — multi-valued projection from the standard `tags:` array
builder.Services.AddTaxonomy<TalkFrontMatter, string>(opts =>
{
    opts.BaseUrl    = "/topic";
    opts.SelectKey  = fm => fm.Topic;
    opts.IndexPage  = typeof(TopicIndex);
    opts.TermPage   = typeof(TopicTerm);
});
builder.Services.AddTaxonomy<TalkFrontMatter, string>(opts =>
{
    opts.BaseUrl    = "/tag";
    opts.SelectKeys = fm => fm.Tags;
    opts.IndexPage  = typeof(TagIndex);
    opts.TermPage   = typeof(TagTerm);
});

var app = builder.Build();

app.UsePennington();
app.UseMonorailCss();

// Home page — uses both data files (sponsors strip + schedule preview).
app.MapGet("/", async (HtmlRenderer renderer, IDataFiles data) =>
{
    var parameters = new Dictionary<string, object?>
    {
        [nameof(Home.Sponsors)] = data.Get<List<Sponsor>>("sponsors"),
        [nameof(Home.Schedule)] = data.Get<Schedule>("schedule"),
    };
    return await RenderAsync<Home>(renderer, parameters);
});

// Standalone schedule page (also data-driven).
app.MapGet("/schedule/", async (HtmlRenderer renderer, IDataFiles data) =>
{
    var parameters = new Dictionary<string, object?>
    {
        [nameof(SchedulePage.Schedule)] = data.Get<Schedule>("schedule"),
    };
    return await RenderAsync<SchedulePage>(renderer, parameters);
});

// MapTaxonomy mounts /topic, /topic/{slug}, /tag, and /tag/{slug} from the two
// AddTaxonomy registrations above.
app.MapTaxonomy<TalkFrontMatter, string>();

// Catch-all that serves the AddMarkdownContent<TalkFrontMatter> sources at /talks/*.
// The bare-host pattern (see how-to/response-pipeline/razor-page-on-bare-host) walks
// IEnumerable<IContentService> and renders the first match — this version only
// handles MarkdownFileSource items (the taxonomy + data routes are mounted above).
app.MapGet("/{*path}", async (
    string? path,
    IEnumerable<IContentService> services,
    IContentParser parser,
    IContentRenderer renderer) =>
{
    var requested = new UrlPath(path ?? string.Empty).EnsureLeadingSlash();
    foreach (var service in services)
    {
        await foreach (var discovered in service.DiscoverAsync())
        {
            if (!discovered.Route.CanonicalPath.Matches(requested)) continue;
            if (discovered.Source is not MarkdownFileSource) continue;

            var parsed = await parser.ParseAsync(discovered);
            if (parsed.Value is not ParsedItem parsedItem) continue;

            var rendered = await renderer.RenderAsync(parsedItem);
            if (rendered.Value is not RenderedItem r) continue;

            var html = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <title>{{r.Metadata.Title}}</title>
                  <link rel="stylesheet" href="/styles.css" />
                </head>
                <body class="bg-base-50 text-base-900 dark:bg-base-950 dark:text-base-50">
                  <main class="mx-auto max-w-2xl px-6 py-12 prose dark:prose-invert">
                    {{r.Content.Html}}
                  </main>
                </body>
                </html>
                """;
            return Results.Content(html, "text/html");
        }
    }
    return Results.NotFound();
});

await app.RunOrBuildAsync(args);
return;

static async Task<IResult> RenderAsync<TComponent>(
    HtmlRenderer renderer,
    IDictionary<string, object?> parameters)
    where TComponent : IComponent
{
    var html = await renderer.Dispatcher.InvokeAsync(async () =>
    {
        var output = await renderer.RenderComponentAsync<TComponent>(
            ParameterView.FromDictionary(parameters));
        return output.ToHtmlString();
    });
    return Results.Content(html, "text/html");
}
