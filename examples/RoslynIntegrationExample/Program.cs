using Penn.Infrastructure;
using Penn.MonorailCss;
using Penn.Roslyn;
using RoslynIntegrationExample;
using RoslynIntegrationExample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Little Content Engine";
    penn.SiteDescription = "An Inflexible Content Engine for .NET";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<BlogFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
    });
});

builder.Services.AddPennRoslyn(options =>
{
    options.SolutionPath = "../../Penn.slnx";
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
