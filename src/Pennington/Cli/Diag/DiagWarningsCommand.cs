namespace Pennington.Cli.Diag;

using System.CommandLine;
using Diagnostics;
using Generation;
using Microsoft.Extensions.DependencyInjection;

/// <summary><c>diag warnings</c> — the site's current diagnostics, grouped by severity.</summary>
internal sealed class DiagWarningsCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "warnings";

    /// <inheritdoc/>
    public string Description => "List current diagnostics: broken links, broken xrefs, translation gaps, and structure.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var severityOption = new Option<string>("--severity")
        {
            Description = "Minimum severity to show: error, warning, or info (this level and above).",
        };

        var command = new Command(Name, Description);
        command.Options.Add(severityOption);
        command.SetAction(async (parseResult, _) =>
        {
            var threshold = ParseThreshold(parseResult.GetValue(severityOption));

            // The audit pass (structural auditors plus the rendered broken-link crawl) runs once at
            // startup in this headless run; wait for it, then read the cache it populated.
            await services.GetRequiredService<AuditRunner>().WaitForInitialPassAsync();
            var diagnostics = services.GetRequiredService<IAuditCache>().Diagnostics;

            // The exit code reflects whether the site has any errors at all, independent of the
            // display filter, so `diag warnings` is a meaningful CI/agent gate.
            var hasError = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            WriteReport(output, diagnostics.Where(d => Rank(d.Severity) >= threshold).ToList());
            return hasError ? 1 : 0;
        });
        return command;
    }

    private static int Rank(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Error => 2,
        DiagnosticSeverity.Warning => 1,
        _ => 0,
    };

    private static int ParseThreshold(string? severity) => severity?.ToLowerInvariant() switch
    {
        "error" => 2,
        "warning" => 1,
        _ => 0,
    };

    private static void WriteReport(TextWriter output, IReadOnlyList<BuildDiagnostic> diagnostics)
    {
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        var warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();
        var infos = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ToList();

        output.WriteLine($"Diagnostics — {errors.Count} error{P(errors.Count)}, {warnings.Count} warning{P(warnings.Count)}, {infos.Count} info");
        output.WriteLine();

        if (errors.Count > 0)
        {
            output.WriteLine("ERRORS");
            foreach (var diagnostic in errors)
            {
                WriteDetail(output, diagnostic);
            }

            output.WriteLine();
        }

        if (warnings.Count > 0)
        {
            output.WriteLine("WARNINGS");
            foreach (var diagnostic in warnings)
            {
                WriteBrief(output, diagnostic);
            }

            output.WriteLine();
        }

        if (infos.Count > 0)
        {
            output.WriteLine("INFO");
            foreach (var diagnostic in infos)
            {
                WriteBrief(output, diagnostic);
            }

            output.WriteLine();
        }

        if (diagnostics.Count == 0)
        {
            output.WriteLine("No diagnostics.");
        }
    }

    private static void WriteDetail(TextWriter output, BuildDiagnostic diagnostic)
    {
        if (diagnostic.Route is { } route)
        {
            output.WriteLine($"  {route.CanonicalPath}");
            output.WriteLine($"    {diagnostic.Message}");
            if (route.SourceFile is { } routeSource)
            {
                output.WriteLine($"    Source: {routeSource}");
            }
            else if (diagnostic.SourceFile is { } diagSource)
            {
                output.WriteLine($"    {diagSource}");
            }
        }
        else if (diagnostic.SourceFile is { } sourceFile)
        {
            output.WriteLine($"  {sourceFile}");
            output.WriteLine($"    {diagnostic.Message}");
        }
        else
        {
            output.WriteLine($"  {diagnostic.Message}");
        }

        if (diagnostic.Exception is { } ex)
        {
            output.WriteLine($"    Exception: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void WriteBrief(TextWriter output, BuildDiagnostic diagnostic)
    {
        if (diagnostic.Route is { } route)
        {
            output.WriteLine($"  {route.CanonicalPath}: {diagnostic.Message}");
        }
        else if (diagnostic.SourceFile is { } sourceFile)
        {
            output.WriteLine($"  {sourceFile}: {diagnostic.Message}");
        }
        else
        {
            output.WriteLine($"  {diagnostic.Message}");
        }
    }

    private static string P(int count) => count == 1 ? "" : "s";
}
