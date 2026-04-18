namespace Pennington.IntegrationTests.Infrastructure;

using DocSite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Roslyn;

/// <summary>
/// Spins up the Pennington.Docs site under real Kestrel on a random port.
/// Use this when tests need the actual request pipeline (not TestServer) —
/// for example, endpoints that self-fetch other URLs via HttpClient.
/// </summary>
public sealed class DocsRealServerFixture : IAsyncLifetime
{
    private WebApplication? _app;

    public string BaseUrl { get; private set; } = "";
    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _app!.Services;

    public async ValueTask InitializeAsync()
    {
        var repoRoot = FindRepoRoot();
        var docsProjectPath = Path.Combine(repoRoot, "docs", "Pennington.Docs");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = docsProjectPath,
            EnvironmentName = "Testing",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services.AddDocSite(() => new DocSiteOptions
        {
            SiteTitle = "Pennington",
            Description = "A Content Engine for .NET",
        });

        builder.Services.AddPenningtonRoslyn(roslyn =>
        {
            roslyn.SolutionPath = Path.Combine(repoRoot, "Pennington.slnx");
        });

        _app = builder.Build();
        _app.UseDocSite();

        await _app.StartAsync();
        BaseUrl = _app.Urls.First();
        Client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Pennington.slnx")))
        {
            dir = dir.Parent;
        }

        if (dir == null)
        {
            throw new DirectoryNotFoundException(
                $"Could not locate Pennington.slnx walking up from {AppContext.BaseDirectory}.");
        }

        return dir.FullName;
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}