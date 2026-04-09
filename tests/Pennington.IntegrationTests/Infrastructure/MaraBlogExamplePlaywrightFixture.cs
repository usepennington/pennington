namespace Pennington.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using global::MonorailCss.Theme;
using Pennington.BlogSite;
using Pennington.MonorailCss;

public class MaraBlogExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "MaraBlogExample"));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = projectPath,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddBlogSite(() => new BlogSiteOptions
        {
            SiteTitle = "Mara Writes Code",
            Description = "Performance engineering for .NET",
            ContentRootPath = "Content",
            BlogContentPath = "Posts",
            BlogBaseUrl = "/blog",
            TagsPageUrl = "/topics",
            CanonicalBaseUrl = "https://mara.dev",
            AuthorName = "Mara Chen",
            ColorScheme = new AlgorithmicColorScheme
            {
                PrimaryHue = 25,
                BaseColorName = ColorNames.Zinc,
            },
            HeroContent = new HeroContent(
                "Performance engineer & writer",
                "I'm Mara. I help .NET teams ship faster software."),
            EnableRss = true,
            EnableSitemap = true,
        });

        _app = builder.Build();
        _app.UseBlogSite();

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
