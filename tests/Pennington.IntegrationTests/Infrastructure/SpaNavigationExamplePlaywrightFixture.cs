namespace Pennington.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Pennington.Infrastructure;
using Islands;
using MonorailCss;

public class SpaNavigationExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "SpaNavigationExample"));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = projectPath,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddRazorComponents();
        builder.Services.AddPennington(penn =>
        {
            penn.SiteTitle = "My Recipe Book";
            penn.SiteDescription = "A cookbook powered by SPA slots";
            penn.ContentRootPath = "Content";
            penn.AddMarkdownContent<SpaNavigationExample.RecipeFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "";
            });
        });

        builder.Services.AddMonorailCss();
        builder.Services.AddTransient<SpaNavigationExample.ContentHelper>();

        // SPA navigation
        builder.Services.AddSpaNavigation();
        builder.Services.AddScoped<ComponentRenderer>();
        builder.Services.AddTransient<IIslandRenderer, SpaNavigationExample.Slots.RecipeContentSlotRenderer>();
        builder.Services.AddTransient<IIslandRenderer, SpaNavigationExample.Slots.RecipeInfoSlotRenderer>();

        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapRazorComponents<SpaNavigationExample.Components.App>();
        _app.UseMonorailCss();
        _app.UseSpaNavigation();
        _app.UsePennington();

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