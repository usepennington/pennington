namespace Penn.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Penn.DocSite;
using Penn.Localization;

public class LocalizationExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "LocalizationExample"));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = projectPath,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddDocSite(_ => new DocSiteOptions
        {
            SiteTitle = "The Multilingual Tavern",
            Description = "A documentation site that speaks many tongues",
            ConfigureLocalization = loc =>
            {
                loc.DefaultLocale = "en";
                loc.AddLocale("en", new LocaleInfo("English"));
                loc.AddLocale("pl", new LocaleInfo("Pig Latin"));
                loc.AddLocale("sv", new LocaleInfo("Bork Bork", HtmlLang: "sv-chef"));
                loc.AddLocale("pi", new LocaleInfo("Pirate", HtmlLang: "en-pirate"));
                loc.AddLocale("kl", new LocaleInfo("Klingon", HtmlLang: "tlh"));
            },
        });

        _app = builder.Build();
        _app.UseDocSite();

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
