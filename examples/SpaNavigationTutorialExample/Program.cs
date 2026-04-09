using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.MonorailCss;
using SpaNavigationTutorialExample;
using SpaNavigationTutorialExample.Components;
using SpaNavigationTutorialExample.Islands;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "SPA Navigation Tutorial";
    penn.SiteDescription = "Demonstrates SPA navigation with islands";
    penn.ContentRootPath = "Content";
    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
    });

    penn.Islands.Register<ArticleIslandRenderer>("article");
    penn.Islands.Register<NavIslandRenderer>("nav");
});

builder.Services.AddMonorailCss();
builder.Services.AddTransient<ContentHelper>();

builder.Services.AddSpaNavigation();
builder.Services.AddScoped<ComponentRenderer>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UseSpaNavigation();
app.UsePennington();

await app.RunOrBuildAsync(args);
