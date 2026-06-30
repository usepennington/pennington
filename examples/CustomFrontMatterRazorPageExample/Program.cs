using CustomFrontMatterRazorPageExample;
using CustomFrontMatterRazorPageExample.Components;
using Pennington.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Bare AddPennington host. Register the custom record as the source's front-matter
// type so pages under Content/symbols deserialize the `namespace` and `stability`
// keys into ApiFrontMatter.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Custom Front Matter Razor Page";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<ApiFrontMatter>(md =>
    {
        md.ContentPath = "Content/symbols";
        md.BasePageUrl = "/symbols";
    });
});

builder.Services.AddRazorComponents();

var app = builder.Build();

// UsePennington runs before MapRazorComponents so the catch-all `@page "/{*Path}"`
// doesn't swallow Pennington's redirect / sitemap / llms.txt routes.
app.UsePennington();
app.UseAntiforgery();
app.MapRazorComponents<App>();

await app.RunOrBuildAsync(args);
