namespace Penn.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Playwright;
using Penn.Content;
using Penn.Infrastructure;
using Penn.MonorailCss;

public class RecipeExamplePlaywrightFixture : IAsyncLifetime
{
    private WebApplication? _app;
    public string BaseUrl { get; private set; } = "";
    public IBrowser Browser { get; private set; } = null!;
    private IPlaywright? _playwright;

    public async ValueTask InitializeAsync()
    {
        var projectPath = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "examples", "RecipeExample"));

        var recipePath = Path.Combine(projectPath, "recipes");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = projectPath,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddRazorComponents();

        builder.Services.AddPenn(penn =>
        {
            penn.SiteTitle = "Recipe Collection";
            penn.SiteDescription = "CookLang Recipe Website";
            penn.ContentRootPath = "recipes";
        });

        var recipeService = new global::RecipeExample.RecipeContentService(recipePath);
        builder.Services.AddSingleton<global::RecipeExample.IRecipeContentService>(recipeService);
        builder.Services.AddSingleton<IContentService>(recipeService);

        var imageService = new global::RecipeExample.ResponsiveImageContentService(recipePath);
        builder.Services.AddSingleton<global::RecipeExample.IResponsiveImageContentService>(imageService);
        builder.Services.AddSingleton<IContentService>(imageService);

        builder.Services.AddMonorailCss();

        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapRazorComponents<global::RecipeExample.Components.App>();
        _app.UseMonorailCss();
        _app.UsePenn();

        _app.MapGet("/images/{filename}-{size}.webp",
            async (string filename, string size, global::RecipeExample.IResponsiveImageContentService imgService) =>
            {
                var imageData = await imgService.ProcessImageAsync(filename, size);
                return imageData == null ? Results.NotFound() : Results.File(imageData, "image/webp");
            });

        await _app.StartAsync();
        BaseUrl = _app.Urls.First();

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright?.Dispose();
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
