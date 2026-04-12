using MonorailCss.Theme;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using RecipeExample;
using RecipeExample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

var recipePath = Path.Combine(builder.Environment.ContentRootPath, "recipes");

builder.Services.AddPennington(penn =>
{
    penn.SiteTitle = "Recipe Collection";
    penn.SiteDescription = "CookLang Recipe Website";
    penn.ContentRootPath = "recipes";
    penn.AdditionalRoutingAssemblies = [typeof(Program).Assembly];
});

// Recipe content service
var recipeService = new RecipeContentService(recipePath);
builder.Services.AddSingleton<IRecipeContentService>(recipeService);
builder.Services.AddSingleton<IContentService>(recipeService);

// Responsive image service
var imageService = new ResponsiveImageContentService(recipePath);
builder.Services.AddSingleton<IResponsiveImageContentService>(imageService);
builder.Services.AddSingleton<IContentService>(imageService);

builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new NamedColorScheme
    {
        PrimaryColorName = ColorNames.Amber,
        AccentColorName = ColorNames.Sky,
        TertiaryOneColorName = ColorNames.Orange,
        TertiaryTwoColorName = ColorNames.Yellow,
        BaseColorName = ColorNames.Neutral
    },
});

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseMonorailCss();
app.UsePennington();

// Responsive image endpoint
app.MapGet("/images/{filename}-{size}.webp",
    async (string filename, string size, IResponsiveImageContentService imgService) =>
    {
        var imageData = await imgService.ProcessImageAsync(filename, size);
        return imageData == null ? Results.NotFound() : Results.File(imageData, "image/webp");
    });

await app.RunOrBuildAsync(args);
