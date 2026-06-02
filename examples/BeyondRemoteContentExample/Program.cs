using BeyondRemoteContentExample;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;

var builder = WebApplication.CreateBuilder(args);

// Typed HttpClient for the GitHub Releases API. The User-Agent is mandatory —
// GitHub answers 403 without one — and the short timeout stops a slow API from
// stalling the build (GitHubReleasesClient falls back to a fixture on timeout).
builder.Services.AddHttpClient<GitHubReleasesClient>(client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Pennington-Remote-Content-Example");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Remote Release Notes";
    penn.CanonicalBaseUrl = "https://example.com";

    // The release pages are rendered by the MapGet endpoint below and wrapped in
    // <article>. Point the projection selector at that element so the self-fetch
    // that builds the search index and llms.txt sees the release body, not the
    // surrounding page chrome.
    penn.SiteProjection.ContentSelector = "article";
});

// The remote service is process-lifetime: it fetches once and caches the result for
// the life of the build. Register the concrete type, then forward IContentService to
// the same instance so the endpoint and the pipeline share one cache.
builder.Services.AddSingleton<GitHubReleasesContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<GitHubReleasesContentService>());

var app = builder.Build();

app.UsePennington();

// Convenience landing — not a discovered route, so the static build does not write it.
app.MapGet("/", () => Results.Redirect("/releases/"));

// One catch-all under /releases/ serves both the index and each detail page. The
// build crawler discovers every /releases/{version}/ route (EndpointSource) and
// fetches it here through the live pipeline — the same path dev mode uses.
app.MapGet("/releases/{*rest}", async (
    string? rest,
    GitHubReleasesContentService releases,
    IContentRenderer renderer) =>
{
    var slug = (rest ?? string.Empty).Trim('/');
    if (slug.Length == 0)
    {
        return Results.Content(ReleasePages.RenderIndex(await releases.GetEntriesAsync()), "text/html");
    }

    var entry = await releases.TryGetAsync(slug);
    return entry is null
        ? Results.NotFound()
        : Results.Content(await ReleasePages.RenderDetailAsync(renderer, entry), "text/html");
});

await app.RunOrBuildAsync(args);

/// <summary>Exposed so an integration-test host factory can boot this site.</summary>
public partial class Program;
