namespace Pennington.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.MonorailCss;

public class ForgePortalExamplePlaywrightFixture : IAsyncLifetime
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
            "examples", "ForgePortalExample"));

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
            penn.SiteTitle = "Forge";
            penn.SiteDescription = "Internal Developer Portal";
            penn.ContentRootPath = "Content";

            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "Content/docs";
                md.BasePageUrl = "/docs";
                md.Section = "Documentation";
            });

            penn.AddMarkdownContent<BlogFrontMatter>(md =>
            {
                md.ContentPath = "Content/blog";
                md.BasePageUrl = "/blog";
                md.Section = "Blog";
            });

            penn.AddMarkdownContent<global::ForgePortalExample.PageFrontMatter>(md =>
            {
                md.ContentPath = "Content/pages";
                md.BasePageUrl = "";
            });

            penn.Highlighting.AddHighlighter<global::ForgePortalExample.PipelineHighlighter>();
        });

        builder.Services.AddMonorailCss();
        builder.Services.AddTransient<global::ForgePortalExample.ContentHelper>();
        builder.Services.AddSingleton<global::ForgePortalExample.ReleaseNotesContentService>();
        builder.Services.AddSingleton<IContentService>(sp => sp.GetRequiredService<global::ForgePortalExample.ReleaseNotesContentService>());
        builder.Services.AddSingleton<IResponseProcessor, global::ForgePortalExample.FeedbackWidgetProcessor>();

        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapRazorComponents<global::ForgePortalExample.Components.App>();
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
