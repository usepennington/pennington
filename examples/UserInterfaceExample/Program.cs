using Pennington.Infrastructure;
using Pennington.MonorailCss;
using UserInterfaceExample;
using UserInterfaceExample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Daily Life Hub";
    penn.SiteDescription = "Your everyday life, simplified";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocsFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
    });
});

builder.Services.AddMonorailCss();
builder.Services.AddTransient<ContentHelper>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UsePennington();

await app.RunOrBuildAsync(args);