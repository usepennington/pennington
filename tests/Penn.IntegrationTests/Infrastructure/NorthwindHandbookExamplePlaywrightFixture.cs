namespace Penn.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using global::MonorailCss.Theme;
using Penn.FrontMatter;
using Penn.Infrastructure;
using Penn.MonorailCss;

public class NorthwindHandbookExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "NorthwindHandbookExample"));

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
            penn.SiteTitle = "Northwind Engineering Handbook";
            penn.SiteDescription = "How we build software at Northwind";
            penn.ContentRootPath = "Content";
            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "";
            });
            penn.AddMarkdownContent<global::NorthwindHandbookExample.ChangelogFrontMatter>(md =>
            {
                md.ContentPath = "Content/changelog";
                md.BasePageUrl = "/changelog";
                md.Section = "Changelog";
            });
        });
        builder.Services.AddMonorailCss(_ => new MonorailCssOptions
        {
            ColorScheme = new AlgorithmicColorScheme
            {
                PrimaryHue = 220,
                BaseColorName = ColorNames.Stone,
            },
        });
        builder.Services.AddTransient<global::NorthwindHandbookExample.ContentHelper>();

        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapRazorComponents<global::NorthwindHandbookExample.Components.App>();
        _app.UseMonorailCss();
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
