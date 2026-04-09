using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.MonorailCss;
using SpaNavigationExample;
using SpaNavigationExample.Components;
using SpaNavigationExample.Slots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "My Recipe Book";
    penn.SiteDescription = "A cookbook powered by SPA slots";
    penn.ContentRootPath = "Content";
    penn.AddMarkdownContent<RecipeFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
    });

    penn.Islands.Register<RecipeContentSlotRenderer>("content");
    penn.Islands.Register<RecipeInfoSlotRenderer>("recipe-info");
});

builder.Services.AddMonorailCss();
builder.Services.AddTransient<ContentHelper>();

// SPA navigation
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
