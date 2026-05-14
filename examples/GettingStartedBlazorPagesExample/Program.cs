using GettingStartedBlazorPagesExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Same Pennington wiring as the minimal-site tutorial: register the content
//    pipeline and point one markdown source at Content/.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "My First Pennington Site";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

// 2. Add Blazor Server's static-rendering services. This is what unlocks
//    `MapRazorComponents<App>()` below.
builder.Services.AddRazorComponents();

var app = builder.Build();

// 3. Order matters: UsePennington registers redirect routes, llms.txt, and
//    sitemap endpoints. The Blazor catch-all `@page "/{*Path}"` would swallow
//    those routes if MapRazorComponents ran first.
app.UsePennington();

// 4. Antiforgery middleware is required by MapRazorComponents — Blazor's
//    routed components opt into the [RequireAntiforgeryToken] metadata even
//    when no form ships in the page.
app.UseAntiforgery();

// 5. Hand routing to Blazor. Components/App.razor's <Router> finds the
//    matching @page component (in this project: Components/Pages/MarkdownPage.razor).
app.MapRazorComponents<App>();

await app.RunOrBuildAsync(args);
