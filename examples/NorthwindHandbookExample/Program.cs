using System.Collections.Immutable;
using MonorailCss.Theme;
using NorthwindHandbookExample;
using NorthwindHandbookExample.Components;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Northwind Engineering Handbook";
    penn.SiteDescription = "How we build software at Northwind";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
        // The `changelog/` subtree is owned by the ChangelogFrontMatter source
        // registered below — carve it out here so both sources don't both emit
        // routes for /changelog/v*.
        md.ExcludePaths = ImmutableArray.Create("changelog");
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
app.UsePennington();

await app.RunOrBuildAsync(args);
