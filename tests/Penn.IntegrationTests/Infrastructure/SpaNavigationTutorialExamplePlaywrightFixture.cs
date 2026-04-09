namespace Penn.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.Islands;
using Penn.MonorailCss;

public class SpaNavigationTutorialExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "SpaNavigationTutorialExample"));

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
            penn.SiteTitle = "SPA Navigation Tutorial";
            penn.SiteDescription = "Demonstrates SPA navigation with islands";
            penn.ContentRootPath = "Content";
            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "";
            });
        });

        builder.Services.AddMonorailCss();
        builder.Services.AddTransient<global::SpaNavigationTutorialExample.ContentHelper>();

        builder.Services.AddSpaNavigation();
        builder.Services.AddScoped<ComponentRenderer>();
        builder.Services.AddTransient<IIslandRenderer, global::SpaNavigationTutorialExample.Islands.ArticleIslandRenderer>();
        builder.Services.AddTransient<IIslandRenderer, global::SpaNavigationTutorialExample.Islands.NavIslandRenderer>();

        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapRazorComponents<global::SpaNavigationTutorialExample.Components.App>();
        _app.UseMonorailCss();
        _app.UseSpaNavigation();
        _app.UsePenn();

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
