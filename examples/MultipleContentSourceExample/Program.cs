using MultipleContentSourceExample;
using MultipleContentSourceExample.Components;
using Penn.Infrastructure;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Little Content Engine";
    penn.SiteDescription = "An Inflexible Content Engine for .NET";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<ContentFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
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
app.UsePenn();

await app.RunOrBuildAsync(args);
