namespace SubPathDeployableExample;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Generation;
using Pennington.Infrastructure;

/// <summary>
/// Addressable wrappers around Pennington's build-time entry points. The
/// GitHub Pages how-to fences these methods via <c>csharp:xmldocid,bodyonly</c>
/// so readers see small, copy-pasteable snippets rather than chasing extension
/// methods across the core library.
/// </summary>
public static class BuildHost
{
    /// <summary>
    /// Run in dev mode (no args) or generate a static site (<c>build [baseUrl]</c>).
    /// Body fence target: drops straight into the tail of a minimal
    /// <c>Program.cs</c> for sites that want to distinguish between dev and
    /// build without relying on the default <c>RunOrBuildAsync</c> extension.
    /// </summary>
    public static async Task RunOrBuildAsync(WebApplication app, string[] args)
    {
        if (args.Length > 0 && args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
        {
            await app.StartAsync();
            var generator = app.Services.GetRequiredService<OutputGenerationService>();
            var addresses = app.Urls.Any() ? app.Urls : ["http://localhost:5000"];
            var report = await generator.GenerateAsync(addresses.First());
            await app.StopAsync();

            PrintBuildReport(report);
        }
        else
        {
            await app.RunAsync();
        }
    }

    /// <summary>
    /// Write a <see cref="BuildReport"/> to stdout and set a non-zero process
    /// exit code when it contains errors or broken links. Useful when a CI
    /// workflow needs stricter failure semantics than the default
    /// <see cref="PenningtonExtensions.RunOrBuildAsync"/>, e.g. failing the
    /// main-branch build on broken xrefs while allowing warnings on feature
    /// branches.
    /// </summary>
    public static void PrintBuildReport(BuildReport report)
    {
        report.WriteTo(Console.Out);
        if (report.HasErrors)
        {
            Environment.ExitCode = 1;
        }
    }
}