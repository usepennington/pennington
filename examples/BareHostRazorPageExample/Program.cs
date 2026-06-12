using System.Collections.Immutable;
using BareHostRazorPageExample;
using BareHostRazorPageExample.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Pipeline;
using Pennington.Routing;

var builder = WebApplication.CreateBuilder(args);

// Bare AddPennington host. This example demonstrates rendering a Razor
// component as the entire response body of a MapGet route — no DocSite, no
// markdown pipeline.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Bare-host Razor page";
    penn.ContentRootPath = "Content";
});

builder.Services.AddMonorailCss();

// HtmlRenderer needs Blazor's component services. AddRazorComponents wires
// them; AddHttpContextAccessor lets the HtmlRenderer resolve cascading values.
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();

// IContentService that publishes the per-status routes to the build crawler.
// On a bare host, EndpointSource is the case to use when a sibling MapGet
// produces the HTML — see how-to/extensibility/custom-content-service.
builder.Services.AddSingleton<StatusPagesContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<StatusPagesContentService>());

var app = builder.Build();

app.UsePennington();
app.UseMonorailCss();

app.MapGet("/status/{slug}/", (string slug, StatusPagesContentService statuses, HtmlRenderer renderer)
    => BareHostRenderer.RenderRazorPageAsync<StatusPage>(renderer, statuses.TryGet(slug) is { } entry
        ? new Dictionary<string, object?>
        {
            [nameof(StatusPage.Slug)] = entry.Slug,
            [nameof(StatusPage.Title)] = entry.Title,
            [nameof(StatusPage.Summary)] = entry.Summary,
            [nameof(StatusPage.Facts)] = entry.Facts,
        }
        : null));

await app.RunOrBuildAsync(args);

namespace BareHostRazorPageExample
{
    /// <summary>Renders a Razor component to a complete HTML response on a bare host.</summary>
    public static class BareHostRenderer
    {
        /// <summary>
        /// Renders <typeparamref name="TComponent"/> to HTML inside a request handler.
        /// <see cref="HtmlRenderer"/> runs the component on the request thread via
        /// <c>Dispatcher.InvokeAsync</c>, then returns the HTML as a string. The component
        /// owns the whole document, so the result is a complete HTML page ready to flush
        /// to the response; a null <paramref name="parameters"/> means no record matched
        /// and the route 404s.
        /// </summary>
        /// <typeparam name="TComponent">The component that renders the whole document.</typeparam>
        /// <param name="renderer">The Blazor <see cref="HtmlRenderer"/> from DI.</param>
        /// <param name="parameters">The component's <c>[Parameter]</c> values, or null to 404.</param>
        public static async Task<IResult> RenderRazorPageAsync<TComponent>(
            HtmlRenderer renderer,
            IDictionary<string, object?>? parameters)
            where TComponent : IComponent
        {
            if (parameters is null)
            {
                return Results.NotFound();
            }

            var html = await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await renderer.RenderComponentAsync<TComponent>(
                    ParameterView.FromDictionary(parameters));
                return output.ToHtmlString();
            });
            return Results.Content(html, "text/html");
        }
    }

    /// <summary>One status page entry — the data StatusPage.razor binds.</summary>
    public sealed record StatusEntry(
        string Slug,
        string Title,
        string Summary,
        IReadOnlyList<KeyValuePair<string, string>> Facts);

    /// <summary>
    /// Publishes one route per status entry so the build crawler discovers
    /// them. <see cref="EndpointSource"/> tells the pipeline that the HTML is
    /// produced by a sibling <c>MapGet</c>; the crawler still fetches each
    /// URL through the live pipeline at build time.
    /// </summary>
    public sealed class StatusPagesContentService : IContentService
    {
        private readonly ImmutableArray<StatusEntry> _entries =
        [
            new("intro", "Intro", "How this example wires HtmlRenderer.",
                [new("Stage", "Production"), new("Owner", "Docs team")]),
            new("verify", "Verify", "Confirm the rendered page reaches the browser.",
                [new("Stage", "Beta"), new("Owner", "Docs team")]),
        ];

        public string DefaultSectionLabel => "Status";
        public int SearchPriority => 30;

        public StatusEntry? TryGet(string slug) =>
            _entries.FirstOrDefault(e => e.Slug == slug);

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var entry in _entries)
            {
                var route = ContentRouteFactory.FromUrl(new UrlPath($"/status/{entry.Slug}/"));
                yield return new DiscoveredItem(route, new EndpointSource());
            }
            await Task.CompletedTask;
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
            => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
        {
            var builder = ImmutableList.CreateBuilder<ContentTocItem>();
            var order = 10;
            foreach (var entry in _entries)
            {
                builder.Add(new ContentTocItem(
                    Title: entry.Title,
                    Route: ContentRouteFactory.FromUrl(new UrlPath($"/status/{entry.Slug}/")),
                    Order: order,
                    HierarchyParts: ["status", entry.Slug],
                    SectionLabel: DefaultSectionLabel,
                    Locale: null));
                order += 10;
            }
            return Task.FromResult(builder.ToImmutable());
        }

        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
        {
            var builder = ImmutableList.CreateBuilder<CrossReference>();
            foreach (var entry in _entries)
            {
                var route = ContentRouteFactory.FromUrl(new UrlPath($"/status/{entry.Slug}/"));
                builder.Add(new CrossReference($"status-{entry.Slug}", entry.Title, route));
            }
            return Task.FromResult(builder.ToImmutable());
        }
    }
}