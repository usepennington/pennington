using Pennington.DocSite;
using Pennington.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Swap the bare `AddPennington` host for the DocSite template. `AddDocSite`
// wires the full documentation experience on top of Pennington core — a
// Blazor-rendered layout with sidebar navigation, header, search surface,
// outline nav, dark-mode toggle — driven entirely from `DocSiteOptions`.
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "Scaffold Docs",
    Description = "A minimal DocSite scaffold showing AddDocSite and area routing.",
    GitHubUrl = "https://github.com/usepennington/pennington",
    HeaderContent = """<a href="/">Scaffold Docs</a>""",
    FooterContent = """<footer class="mt-16 py-8 text-center text-sm text-base-500">Built with Pennington DocSite.</footer>""",

    // Each area maps to a top-level folder under `Content/` and to a URL
    // prefix. The sidebar renders an area selector when more than one is
    // configured and only shows the TOC for the active area.
    Areas =
    [
        new ContentArea("Guides", "guides"),
        new ContentArea("Reference", "reference"),
    ],
});

var app = builder.Build();

// `UseDocSite` mounts locale routing, antiforgery, static files, Razor
// component routing (`Pages.razor` owns `/{*fileName:nonfile}`), MonorailCSS,
// SPA navigation, and the core Pennington middleware in the right order.
app.UseDocSite();

// `RunDocSiteAsync` delegates to `RunOrBuildAsync`, so `dotnet run` serves
// live and `dotnet run -- build <baseUrl>` generates static HTML.
await app.RunDocSiteAsync(args);
