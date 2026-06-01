using Pennington.DocSite;
using Pennington.Tui;

var builder = WebApplication.CreateBuilder(args);

// Same DocSite host as the other "Beyond" tutorials — the only addition is the
// AddTui call below, which registers a dev-time full-screen terminal
// dashboard. When this example is launched with `dotnet run -- build`, the TUI
// hosted service detects build mode and no-ops, so the static build runs
// exactly as it would without the package reference.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Beyond TUI",
    Description = "Running a dev-time terminal dashboard alongside the Pennington host.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Beyond TUI</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",
});

// Opt in to the dev-time dashboard. No extra configuration is required for the
// happy path — defaults run the dry-run validator once at startup and again
// (debounced) whenever a watched file changes.
builder.Services.AddTui();

var app = builder.Build();

app.UseDocSite();

await app.RunDocSiteAsync(args);