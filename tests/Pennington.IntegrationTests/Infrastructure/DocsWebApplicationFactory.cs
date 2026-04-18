namespace Pennington.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

public class DocsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Set content root to docs project directory
        var docsProjectPath = Path.Combine(FindRepoRoot(), "docs", "Pennington.Docs");

        builder.UseContentRoot(docsProjectPath);

        // Ensure relative paths in the docs project's configuration (e.g., RoslynOptions.SolutionPath = "../../Pennington.slnx")
        // resolve the same way they would under `dotnet run` from the docs folder.
        Directory.SetCurrentDirectory(docsProjectPath);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
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