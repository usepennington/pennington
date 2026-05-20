using GettingStartedNavigationExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

// Same Blazor host as the styling tutorial — markdown rendered through a
// catch-all @page, wrapped in MainLayout. AddPennington already registers
// NavigationBuilder, so the navigation menu added in this tutorial needs no
// extra service wiring here.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "My Pennington Site";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorName.Indigo,
        AccentColorName = ColorName.Pink,
        BaseColorName = ColorName.Slate,
    },
});

builder.Services.AddRazorComponents();

var app = builder.Build();

app.UsePennington();
app.UseMonorailCss();

app.UseAntiforgery();
app.MapRazorComponents<App>();

await app.RunOrBuildAsync(args);