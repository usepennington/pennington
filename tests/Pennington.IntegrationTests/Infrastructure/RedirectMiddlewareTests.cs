namespace Pennington.IntegrationTests.Infrastructure;

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Pennington.DocSite;

/// <summary>
/// Covers the bug Agent 3 hit: `redirectUrl:` pages rendered as normal DocSite
/// pages because <c>MapRazorComponents&lt;App&gt;()</c> was wired before
/// <c>UsePennington()</c> — the catch-all endpoint matched first and the redirect
/// middleware never ran. With the ordering fix, a YAML-defined redirect
/// short-circuits with 301 + meta-refresh.
/// </summary>
public class RedirectMiddlewareTests : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient _client = null!;
    private string _tempRoot = "";

    public async ValueTask InitializeAsync()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Pennington-redirect-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Content"));
        await File.WriteAllTextAsync(
            Path.Combine(_tempRoot, "Content", "_redirects.yml"),
            """
            redirects:
              /old-guide: /new-guide/
            """);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = _tempRoot,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Redirect Test",
            SiteDescription = "Redirect middleware integration test",
            ContentRootPath = new Routing.FilePath(Path.Combine(_tempRoot, "Content")),
        });

        _app = builder.Build();
        _app.UseDocSite();

        await _app.StartAsync();
        _client = new HttpClient
        {
            BaseAddress = new Uri(_app.Urls.First()),
            // The redirect middleware returns 301; don't auto-follow so the
            // test can assert on the intermediate response.
        };
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        _client.Dispose();
        _client = new HttpClient(handler) { BaseAddress = new Uri(_app.Urls.First()) };
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public async Task RedirectUrl_Returns301_WithMetaRefreshBody()
    {
        var response = await _client.GetAsync("/old-guide", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.MovedPermanently);
        response.Headers.Location?.ToString().ShouldBe("/new-guide/");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldContain("""<meta http-equiv="refresh" content="0;url=/new-guide/">""");
    }
}