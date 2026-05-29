namespace Pennington.Infrastructure;

using Cli;

/// <summary>
/// Legacy build-vs-serve detector, retained for backward compatibility. New code consults
/// <see cref="PenningtonCli.Current"/>, which also distinguishes the headless
/// <see cref="PenningtonRunMode.Diag"/> mode this boolean cannot represent. The convenience
/// members below give in-assembly call sites the three-state distinction without each one
/// taking a dependency on <see cref="PenningtonCli"/> directly.
/// </summary>
public static class PenningtonBuildMode
{
    /// <summary>
    /// Returns true when <paramref name="args"/> (param-style: verb at index 0) starts with the
    /// <c>build</c> verb. Used where a caller already holds a forwarded args array (e.g.
    /// <see cref="Generation.OutputOptions.FromArgs"/>).
    /// </summary>
    public static bool IsBuildMode(string[] args)
        => args.Length > 0 && args[0].Equals("build", StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns true when the current process was launched with the <c>build</c> verb.</summary>
    public static bool IsBuildMode() => PenningtonCli.Current.WritesOutput;

    /// <summary>
    /// True when the current process runs a headless one-shot command (<c>build</c> or
    /// <c>diag</c>) — neither serves a dev session, injects overlays/live-reload, nor binds a socket.
    /// </summary>
    internal static bool IsHeadlessOneShot => PenningtonCli.Current.IsHeadlessOneShot;

    /// <summary>True only when the current process performs a static build that writes output to disk.</summary>
    internal static bool WritesOutput => PenningtonCli.Current.WritesOutput;

    /// <summary>True when the current process was launched with a help or version flag.</summary>
    internal static bool IsHelpOrVersion => PenningtonCli.Current.IsHelpOrVersion;
}
