using Penn.Infrastructure;
using Penn.MonorailCss;
using UserInterfaceExample;
using UserInterfaceExample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
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
app.UsePenn();

await app.RunOrBuildAsync(args);
