namespace Penn.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Penn.Content;
using Penn.DocSite;

public class SearchExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "SearchExample"));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = projectPath,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Random Content Site",
            Description = "Random content site for demonstration purposes.",
            CanonicalBaseUrl = "https://mydocs.example.com",
            AdditionalRoutingAssemblies = [typeof(global::SearchExample.Services.RandomContentService).Assembly],
        });

        builder.Services.AddSingleton<global::SearchExample.Services.RandomContentService>();
        builder.Services.AddSingleton<IContentService>(provider =>
            provider.GetRequiredService<global::SearchExample.Services.RandomContentService>());

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
