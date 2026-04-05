using Penn.Infrastructure;
using Penn.Islands;
using Penn.MonorailCss;
using SpaNavigationExample;
using SpaNavigationExample.Components;
using SpaNavigationExample.Slots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Recipe Book";
    penn.SiteDescription = "A cookbook powered by SPA slots";
    penn.ContentRootPath = "Content";
    penn.AddMarkdownContent<RecipeFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
    });
});

builder.Services.AddMonorailCss();
builder.Services.AddTransient<ContentHelper>();

// SPA navigation
builder.Services.AddSpaNavigation();
builder.Services.AddScoped<ComponentRenderer>();
builder.Services.AddTransient<IIslandRenderer, RecipeContentSlotRenderer>();
builder.Services.AddTransient<IIslandRenderer, RecipeInfoSlotRenderer>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UseSpaNavigation();
app.UsePenn();
await app.RunOrBuildAsync(args);
