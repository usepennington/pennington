using ForgePortalExample;
using ForgePortalExample.Components;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.MonorailCss;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "Forge";
    penn.SiteDescription = "Internal Developer Portal";
    penn.ContentRootPath = "Content";

    penn.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content/docs";
        md.BasePageUrl = "/docs";
        md.Section = "Documentation";
    });

    penn.AddMarkdownContent<BlogFrontMatter>(md =>
    {
        md.ContentPath = "Content/blog";
        md.BasePageUrl = "/blog";
        md.Section = "Blog";
    });

    penn.AddMarkdownContent<PageFrontMatter>(md =>
    {
        md.ContentPath = "Content/pages";
        md.BasePageUrl = "";
    });

    penn.Highlighting.AddHighlighter<PipelineHighlighter>();
});

builder.Services.AddMonorailCss();
builder.Services.AddTransient<ContentHelper>();
builder.Services.AddSingleton<ReleaseNotesContentService>();
builder.Services.AddSingleton<IContentService>(sp => sp.GetRequiredService<ReleaseNotesContentService>());
builder.Services.AddSingleton<IResponseProcessor, FeedbackWidgetProcessor>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UsePenn();

await app.RunOrBuildAsync(args);
