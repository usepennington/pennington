namespace Pennington.Cli;

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Single source of truth for run-mode detection and owner of the <c>build</c> / <c>diag</c>
/// command definitions. <see cref="Current"/> classifies the verb from the process command line
/// (<see cref="Environment.GetCommandLineArgs"/>) so DI-time wiring and run-time dispatch agree —
/// host <c>Program.cs</c> is unaffected.
/// </summary>
internal sealed class PenningtonCli
{
    /// <summary>Shared instance, classified from the current process command line.</summary>
    public static PenningtonCli Current { get; } = new(SliceProcessArgs());

    /// <summary>Classifies the run mode from the supplied param-style args (verb at index 0).</summary>
    public PenningtonCli(string[] args)
    {
        Mode = ClassifyMode(args);
        IsHelpOrVersion = args.Any(IsHelpOrVersionToken);
    }

    /// <summary>Run mode the CLI verb maps to; <see cref="PenningtonRunMode.Serve"/> when no known verb is present.</summary>
    public PenningtonRunMode Mode { get; }

    /// <summary>
    /// True when the command line is a help or version request. Such invocations print and exit
    /// without running the host, so they keep stdout clean like a <see cref="PenningtonRunMode.Diag"/> run.
    /// </summary>
    public bool IsHelpOrVersion { get; }

    /// <summary>True for <see cref="PenningtonRunMode.Build"/> or <see cref="PenningtonRunMode.Diag"/> — headless, in-process, no socket bind.</summary>
    public bool IsHeadlessOneShot => Mode is PenningtonRunMode.Build or PenningtonRunMode.Diag;

    /// <summary>True only for <see cref="PenningtonRunMode.Build"/> — strict front-matter keys and disk writes.</summary>
    public bool WritesOutput => Mode is PenningtonRunMode.Build;

    private static PenningtonRunMode ClassifyMode(string[] args)
    {
        if (args.Length == 0)
        {
            return PenningtonRunMode.Serve;
        }

        if (args[0].Equals("build", StringComparison.OrdinalIgnoreCase))
        {
            return PenningtonRunMode.Build;
        }

        if (args[0].Equals("diag", StringComparison.OrdinalIgnoreCase))
        {
            return PenningtonRunMode.Diag;
        }

        return PenningtonRunMode.Serve;
    }

    private static string[] SliceProcessArgs()
    {
        // GetCommandLineArgs()[0] is the executable; user args start at [1] — the same
        // slice OutputOptions.FromArgs and the legacy build-mode detector consume.
        var args = Environment.GetCommandLineArgs();
        return args.Length > 1 ? args[1..] : [];
    }

    /// <summary>
    /// Builds the <c>build</c> verb. Its options exist to document the build CLI in <c>--help</c>;
    /// the effective values come from <see cref="Generation.OutputOptions.FromArgs"/> at DI time, so
    /// the action wired by the caller ignores the parsed option/positional values. Unmatched tokens
    /// are tolerated to preserve the historical positional forms (<c>build /sub dist</c>).
    /// </summary>
    public static Command CreateBuildCommand()
    {
        var baseUrl = new Option<string>("--base-url")
        {
            Description = "Base URL the site is deployed under, e.g. /docs (also accepted positionally). Default: /",
        };
        var output = new Option<string>("--output")
        {
            Description = "Directory to write generated output to (also accepted positionally). Default: output",
        };

        var build = new Command("build", "Generate the static site to the output directory, then exit.");
        build.TreatUnmatchedTokensAsErrors = false;
        build.Options.Add(baseUrl);
        build.Options.Add(output);
        return build;
    }

    /// <summary>True when <paramref name="arg"/> is a help or version request token.</summary>
    public static bool IsHelpOrVersionToken(string arg) => arg is "--help" or "-h" or "-?" or "--version";

    /// <summary>
    /// Builds the <c>diag</c> command group from every registered <see cref="IDiagCommand"/>, each
    /// running against <paramref name="services"/> and writing to <paramref name="output"/>.
    /// </summary>
    public static Command BuildDiagGroup(IServiceProvider services, TextWriter output)
    {
        var diag = new Command("diag", "Inspect the site (read-only). Built for humans and AI assistants.");
        foreach (var command in services.GetServices<IDiagCommand>().OrderBy(c => c.Name, StringComparer.Ordinal))
        {
            diag.Subcommands.Add(command.Build(services, output));
        }

        return diag;
    }
}
