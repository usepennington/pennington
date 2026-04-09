using MonorailCss.Theme;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using SpectreConsoleExample;
using SpectreConsoleExample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Spectre.Console Documentation";
    penn.SiteDescription = "Beautiful console applications with Spectre.Console";
    penn.ContentRootPath = "Content";

    // Console documentation
    penn.AddMarkdownContent<SpectreDocFrontMatter>(md =>
    {
        md.ContentPath = "Content/console";
        md.BasePageUrl = "/console";
        md.Section = "console";
    });

    // CLI documentation
    penn.AddMarkdownContent<SpectreDocFrontMatter>(md =>
    {
        md.ContentPath = "Content/cli";
        md.BasePageUrl = "/cli";
        md.Section = "cli";
    });

    // Blog
    penn.AddMarkdownContent<SpectreBlogFrontMatter>(md =>
    {
        md.ContentPath = "Content/blog";
        md.BasePageUrl = "/blog";
        md.Section = "blog";
    });
});

builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorNames.Sky,
        BaseColorName = ColorNames.Zinc,
        AccentColorName = ColorNames.Pink,
        TertiaryOneColorName = ColorNames.Indigo,
        TertiaryTwoColorName = ColorNames.Violet
    }
});

builder.Services.AddTransient<SpectreContentHelper>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UsePennington();

await app.RunOrBuildAsync(args);
