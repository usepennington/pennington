namespace Pennington.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

public class UserInterfaceExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "UserInterfaceExample"));

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
            penn.SiteTitle = "Daily Life Hub";
            penn.SiteDescription = "Your everyday life, simplified";
            penn.ContentRootPath = "Content";

            penn.AddMarkdownContent<global::UserInterfaceExample.DocsFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "";
            });
        });
        builder.Services.AddMonorailCss();
        builder.Services.AddTransient<global::UserInterfaceExample.ContentHelper>();

        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapRazorComponents<global::UserInterfaceExample.Components.App>();
        _app.UseMonorailCss();
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
