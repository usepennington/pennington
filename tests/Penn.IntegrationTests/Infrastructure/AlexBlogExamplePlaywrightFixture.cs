namespace Penn.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Penn.BlogSite;

public class AlexBlogExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "AlexBlogExample"));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = projectPath,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddBlogSite(() => new BlogSiteOptions
        {
            SiteTitle = "Alex's Dev Blog",
            Description = "Notes on .NET, tools, and developer life",
            ContentRootPath = "Content",
            BlogContentPath = "Blog",
            BlogBaseUrl = "/blog",
            TagsPageUrl = "/tags",
            EnableRss = true,
            EnableSitemap = true,
            AuthorName = "Alex Chen",
            HeroContent = new HeroContent("Hi, I'm Alex", "I write about .NET, developer tooling, and the occasional deep dive into something unexpected."),
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
