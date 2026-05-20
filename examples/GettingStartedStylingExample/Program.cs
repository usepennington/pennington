using GettingStartedStylingExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

// Same Blazor host as the previous tutorial — markdown rendered through a
// catch-all @page in MarkdownPage.razor, wrapped in the styled MainLayout.
builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "My Styled Pennington Site";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });
});

// Register MonorailCSS. The NamedColorScheme picks which named palettes back
// the `primary`, `accent`, and `base` utility prefixes used throughout
// MainLayout.razor. Swap any ColorName constant to re-skin on the next request.
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

// /styles.css. The class collector scans response HTML on every request and
// keeps the stylesheet in sync with whatever utility classes show up.
app.UseMonorailCss();

app.UseAntiforgery();
app.MapRazorComponents<App>();

await app.RunOrBuildAsync(args);