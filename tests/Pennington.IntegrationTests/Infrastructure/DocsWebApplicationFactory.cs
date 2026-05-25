namespace Pennington.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DocsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Set content root to docs project directory
        var docsProjectPath = Path.Combine(FindRepoRoot(), "docs", "Pennington.Docs");

        builder.UseContentRoot(docsProjectPath);

        // Ensure relative paths in the docs project's configuration resolve the same way
        // they would under `dotnet run` from the docs folder.
        Directory.SetCurrentDirectory(docsProjectPath);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }

    /// <summary>
    /// Orderly async teardown. Signals shutdown so subscribers can drain, then
    /// awaits base disposal under a bounded timeout. Cleanup exceptions are
    /// swallowed — on Windows, singleton disposal can race with file-system
    /// handles (MSBuildWorkspace, FileSystemWatcher) and surface as
    /// `Test Collection Cleanup Failure` even though no test assertion failed.
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        try
        {
            Services.GetService<IHostApplicationLifetime>()?.StopApplication();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DocsWebApplicationFactory: StopApplication threw: {ex}");
        }

        try
        {
            var disposeTask = base.DisposeAsync().AsTask();
            var completed = await Task.WhenAny(disposeTask, Task.Delay(TimeSpan.FromSeconds(15)));
            if (completed != disposeTask)
            {
                Console.Error.WriteLine("DocsWebApplicationFactory: base.DisposeAsync did not complete within 15s; abandoning wait.");
            }
            else
            {
                await disposeTask;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DocsWebApplicationFactory: base.DisposeAsync threw: {ex}");
        }

        GC.SuppressFinalize(this);
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
}