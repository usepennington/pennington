using Pennington.DocSite;

var builder = WebApplication.CreateBuilder(args);

// Swap the bare `AddPennington` host for the DocSite template. `AddDocSite`
// wires the full documentation experience on top of Pennington core — a
// Blazor-rendered layout with sidebar navigation, header, search surface,
// outline nav, dark-mode toggle — driven entirely from `DocSiteOptions`.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Scaffold Docs",
    Description = "A minimal DocSite scaffold built on AddDocSite.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Scaffold Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",
});

var app = builder.Build();

// `UseDocSite` mounts locale routing, antiforgery, static files, Razor
// component routing (`Pages.razor` owns `/{*fileName:nonfile}`), MonorailCSS,
// SPA navigation, and the core Pennington middleware in the right order.
app.UseDocSite();

// `RunDocSiteAsync` delegates to `RunOrBuildAsync`, so `dotnet run` serves live
// and `dotnet run -- build <baseUrl> <outputDir>` generates static HTML. Both
// positional args are optional (defaults: `/` and `output`).
await app.RunDocSiteAsync(args);