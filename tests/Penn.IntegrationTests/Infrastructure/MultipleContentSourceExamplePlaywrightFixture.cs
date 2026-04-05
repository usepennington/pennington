namespace Penn.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Penn.Infrastructure;
using Penn.MonorailCss;

public class MultipleContentSourceExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "MultipleContentSourceExample"));

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
            penn.SiteTitle = "My Little Content Engine";
            penn.SiteDescription = "An Inflexible Content Engine for .NET";
            penn.ContentRootPath = "Content";

            penn.AddMarkdownContent<global::MultipleContentSourceExample.ContentFrontMatter>(md =>
            {
                md.ContentPath = "Content";
                md.BasePageUrl = "";
            });

            penn.AddMarkdownContent<global::MultipleContentSourceExample.BlogFrontMatter>(md =>
            {
                md.ContentPath = "Content/blog";
                md.BasePageUrl = "/blog";
                md.Section = "blog";
            });

            penn.AddMarkdownContent<global::MultipleContentSourceExample.DocsFrontMatter>(md =>
            {
                md.ContentPath = "Content/docs";
                md.BasePageUrl = "/docs";
                md.Section = "docs";
            });
        });
        builder.Services.AddMonorailCss();
        builder.Services.AddTransient<global::MultipleContentSourceExample.ContentHelper>();

        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapRazorComponents<global::MultipleContentSourceExample.Components.App>();
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
