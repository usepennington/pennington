using MonorailCss.Theme;
using NorthwindHandbookExample;
using NorthwindHandbookExample.Components;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "Northwind Engineering Handbook";
    penn.SiteDescription = "How we build software at Northwind";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
    });

    penn.AddMarkdownContent<ChangelogFrontMatter>(md =>
    {
        md.ContentPath = "Content/changelog";
        md.BasePageUrl = "/changelog";
        md.Section = "Changelog";
    });
});

builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 220,
        BaseColorName = ColorNames.Stone,
    },
});

builder.Services.AddTransient<ContentHelper>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UsePenn();

await app.RunOrBuildAsync(args);
