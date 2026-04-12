using MultipleContentSourceExample;
using MultipleContentSourceExample.Components;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "My Little Content Engine";
    penn.SiteDescription = "An Inflexible Content Engine for .NET";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<ContentFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
        md.ExcludePaths = ["blog", "docs"];
    });

    penn.AddMarkdownContent<BlogFrontMatter>(md =>
    {
        md.ContentPath = "Content/blog";
        md.BasePageUrl = "/blog";
        md.Section = "blog";
    });

    penn.AddMarkdownContent<DocsFrontMatter>(md =>
    {
        md.ContentPath = "Content/docs";
        md.BasePageUrl = "/docs";
        md.Section = "docs";
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
