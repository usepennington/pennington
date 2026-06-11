namespace Pennington.Cli;

using System.CommandLine;

/// <summary>
/// A <c>diag</c> subcommand. Implementations register in DI as <see cref="IDiagCommand"/>;
/// the CLI discovers them and adds one System.CommandLine <see cref="Command"/> per
/// implementation under the <c>diag</c> group. Each command inspects the started app's
/// services and writes a human-readable text report. Optional packages (and hosts) add
/// their own inspection verbs by registering an implementation.
/// </summary>
public interface IDiagCommand
{
    /// <summary>Subcommand verb shown under <c>diag</c> (e.g. <c>toc</c>, <c>warnings</c>).</summary>
    string Name { get; }

    /// <summary>One-line description shown in <c>diag --help</c>.</summary>
    string Description { get; }

    /// <summary>
    /// Builds the System.CommandLine <see cref="Command"/> for this subcommand, wiring its options
    /// and an action that inspects <paramref name="services"/> (a fully started host's service
    /// provider) and writes to <paramref name="output"/>. The action returns the process exit code.
    /// </summary>
    Command Build(IServiceProvider services, TextWriter output);
}
